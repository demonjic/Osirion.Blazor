using System.Text.Json;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Osirion.Blazor.Analytics.Components;
using Osirion.Blazor.Analytics.Extensions;
using Osirion.Blazor.Analytics.Options;
using Osirion.Blazor.Analytics.Providers;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Handlers;
using Shouldly;

namespace Osirion.Blazor.Analytics.Tests.Internal;

/// <summary>
/// Trackers with <see cref="ConsentEvaluation.Browser"/>: a page exported to static files is the same for every visitor,
/// so every tracker renders its scripts held and the consent script runs them after consent.
/// </summary>
public class TrackerBrowserConsentTests : BunitContext
{
    private const string WebsiteId = "94db1cb1-74f4-4a40-ad6c-962362670409";
    private const string ModuleSource = "./_content/Osirion.Blazor.Core/js/osirion-consent.js";

    public static TheoryData<string> Trackers => ["ga4", "clarity", "matomo", "plausible", "umami", "yandex"];

    public static TheoryData<string, bool> TrackersWithAndWithoutTheGate => new()
    {
        { "ga4", false }, { "ga4", true }, { "clarity", false }, { "clarity", true }, { "matomo", false }, { "matomo", true },
        { "plausible", false }, { "plausible", true }, { "umami", false }, { "umami", true }, { "yandex", false }, { "yandex", true },
    };

    // Guard test: passes before the browser mode exists and pins the server markup to what each provider builds.
    [Theory]
    [MemberData(nameof(TrackersWithAndWithoutTheGate))]
    public void Tracker_ServerEvaluation_RendersExactlyWhatTheProviderBuilds(string tracker, bool consentGate)
    {
        Register(analytics =>
        {
            ConfigureEveryProvider(analytics);
            if (consentGate)
            {
                analytics.RequireConsent();
            }
        });
        var context = new AnalyticsScriptContext { ConsentGateActive = consentGate };

        var markup = RenderTracker(tracker, consentGate ? RequestWithAnalyticsConsent(true) : null);

        markup.ShouldBe(BuildWithProvider(tracker, context));
    }

    [Theory]
    [MemberData(nameof(Trackers))]
    public void Tracker_BrowserEvaluation_HoldsEveryScriptWithItsContentUnchanged(string tracker)
    {
        Register(analytics => ConfigureEveryProvider(analytics).RequireConsent(o => o.Evaluation = ConsentEvaluation.Browser));
        var consented = ParseScripts(BuildWithProvider(tracker, new AnalyticsScriptContext { ConsentGateActive = true }));

        var scripts = ParseScripts(RenderTracker(tracker));

        scripts[^1].GetAttribute("type").ShouldBe("module");
        scripts[^1].GetAttribute("src").ShouldBe(ModuleSource);
        var held = scripts[..^1];
        held.ShouldAllBe(script => script.GetAttribute("type") == "text/plain");
        held.ShouldAllBe(script => script.GetAttribute("data-osirion-consent") == "analytics");
        held.Select(Describe).ShouldBe(consented.Select(Describe));
    }

    // What a browser fetches or runs before consent: only the consent module from the site itself.
    [Theory]
    [MemberData(nameof(Trackers))]
    public void Tracker_BrowserEvaluation_RunsNothingButTheConsentModuleBeforeConsent(string tracker)
    {
        Register(analytics => ConfigureEveryProvider(analytics).RequireConsent(o => o.Evaluation = ConsentEvaluation.Browser));

        var elements = Parse(RenderTracker(tracker)).OfType<IElement>().ToList();

        elements.ShouldAllBe(element => element.LocalName == "script");
        elements.Where(element => element.GetAttribute("type") != "text/plain").Select(element => element.GetAttribute("src"))
            .ShouldBe([ModuleSource]);
    }

    // The page exported to static files is the same for every visitor, whatever the request carried.
    [Theory]
    [MemberData(nameof(Trackers))]
    public void Tracker_BrowserEvaluation_IgnoresTheRequestConsentCookie(string tracker)
    {
        Register(analytics => ConfigureEveryProvider(analytics).RequireConsent(o => o.Evaluation = ConsentEvaluation.Browser));

        var withoutRequest = RenderTracker(tracker);
        var declined = RenderTracker(tracker, RequestWithAnalyticsConsent(false));
        var accepted = RenderTracker(tracker, RequestWithAnalyticsConsent(true));

        withoutRequest.ShouldNotBeEmpty();
        declined.ShouldBe(withoutRequest);
        accepted.ShouldBe(withoutRequest);
    }

    [Fact]
    public void Tracker_BrowserEvaluation_WritesTheConfiguredCategory()
    {
        Register(analytics => analytics.AddGA4(o => o.MeasurementId = "G-TEST1234")
            .RequireConsent(o => { o.Evaluation = ConsentEvaluation.Browser; o.Category = "statistics"; }));

        var held = ParseScripts(RenderTracker("ga4"))[..^1];

        held.ShouldAllBe(script => script.GetAttribute("data-osirion-consent") == "statistics");
    }

    // The scripts run only after consent, as with the server gate, so the providers write the consented variant.
    [Fact]
    public void Tracker_BrowserEvaluation_PassesTheConsentGateToTheProviders()
    {
        Register(analytics => analytics
            .AddGA4(o => o.MeasurementId = "G-TEST1234")
            .AddMatomo(o => { o.SiteId = "7"; o.TrackerUrl = "https://m.example/"; o.RequireConsent = true; })
            .RequireConsent(o => o.Evaluation = ConsentEvaluation.Browser));

        RenderTracker("ga4").ShouldContain("gtag('consent', 'default'");
        RenderTracker("matomo").ShouldNotContain("requireConsent");
    }

    [Theory]
    [MemberData(nameof(Trackers))]
    public void Tracker_BrowserEvaluationWithoutTheGate_RendersTheScriptsLive(string tracker)
    {
        Services.Configure<AnalyticsConsentOptions>(o => o.Evaluation = ConsentEvaluation.Browser);
        Register(analytics => ConfigureEveryProvider(analytics));

        RenderTracker(tracker).ShouldBe(BuildWithProvider(tracker, AnalyticsScriptContext.Default));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Tracker_BrowserEvaluationWithoutACategory_RendersNothing(string category)
    {
        Register(analytics => analytics.AddGA4(o => o.MeasurementId = "G-TEST1234")
            .RequireConsent(o => { o.Evaluation = ConsentEvaluation.Browser; o.Category = category; }));

        RenderTracker("ga4").ShouldBeEmpty();
    }

    [Fact]
    public void AnalyticsConsentOptions_Evaluation_DefaultsToServer()
    {
        new AnalyticsConsentOptions().Evaluation.ShouldBe(ConsentEvaluation.Server);
    }

    [Fact]
    public void Configuration_EvaluationBrowser_IsBoundAndHoldsTheTrackers()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Osirion:Analytics:Consent:RequireConsent"] = "true",
            ["Osirion:Analytics:Consent:Evaluation"] = "Browser",
            ["Osirion:Analytics:GA4:MeasurementId"] = "G-TEST1234",
        }).Build();
        Services.AddOsirionAnalytics(configuration);
        SetRendererInfo(new RendererInfo("Static", false));

        Services.GetRequiredService<IOptions<AnalyticsConsentOptions>>().Value.Evaluation.ShouldBe(ConsentEvaluation.Browser);
        ParseScripts(RenderTracker("ga4"))[0].GetAttribute("type").ShouldBe("text/plain");
    }

    // A site's own provider is held the same way, and markup that would load before consent is left out.
    [Fact]
    public void CustomTracker_BrowserEvaluation_HoldsItsScriptAndLeavesOutItsPixel()
    {
        Register(analytics => analytics.RequireConsent(o => o.Evaluation = ConsentEvaluation.Browser));
        var provider = Substitute.For<IAnalyticsProvider>();
        provider.ShouldRender.Returns(true);
        provider.GetScript(Arg.Any<AnalyticsScriptContext>()).Returns(
            "<script async src=\"https://t.example/t.js\"></script><noscript><img src=\"https://t.example/p.gif\"></noscript>");

        var markup = Render<CustomTracker>(p => p.Add(c => c.TestProvider, provider)).Markup;

        markup.ShouldNotContain("p.gif");
        var held = ParseScripts(markup)[0];
        held.GetAttribute("src").ShouldBe("https://t.example/t.js");
        held.GetAttribute("type").ShouldBe("text/plain");
        provider.Received().GetScript(Arg.Is<AnalyticsScriptContext>(c => c.ConsentGateActive));
    }

    private void Register(Action<IAnalyticsBuilder> configure)
    {
        Services.AddOsirionAnalytics(configure);
        SetRendererInfo(new RendererInfo("Static", false));
    }

    private static IAnalyticsBuilder ConfigureEveryProvider(IAnalyticsBuilder analytics) => analytics
        .AddGA4(o => o.MeasurementId = "G-TEST1234")
        .AddClarity(o => o.SiteId = "clarity1")
        .AddMatomo(o => { o.SiteId = "7"; o.TrackerUrl = "https://m.example/"; })
        .AddPlausible(o => o.ScriptUrl = "https://plausible.io/js/pa-AbC123.js")
        .AddUmami(o => o.SiteId = WebsiteId)
        .AddYandexMetrica(o => o.CounterId = "99");

    private string RenderTracker(string tracker, HttpContext? request = null) => tracker switch
    {
        "ga4" => RenderMarkup<GA4Tracker>(request),
        "clarity" => RenderMarkup<ClarityTracker>(request),
        "matomo" => RenderMarkup<MatomoTracker>(request),
        "plausible" => RenderMarkup<PlausibleTracker>(request),
        "umami" => RenderMarkup<UmamiTracker>(request),
        "yandex" => RenderMarkup<YandexMetricaTracker>(request),
        _ => throw new ArgumentOutOfRangeException(nameof(tracker)),
    };

    private string RenderMarkup<TTracker>(HttpContext? request) where TTracker : AnalyticsTracker =>
        Render<TTracker>(parameters =>
        {
            if (request is not null)
            {
                parameters.AddCascadingValue(request);
            }
        }).Markup;

    private string BuildWithProvider(string tracker, AnalyticsScriptContext context) => tracker switch
    {
        "ga4" => Provider<GA4Provider>().GetScript(Provider<GA4Provider>().CreateOptionsCopy(), context),
        "clarity" => Provider<ClarityProvider>().GetScript(Provider<ClarityProvider>().CreateOptionsCopy(), context),
        "matomo" => Provider<MatomoProvider>().GetScript(Provider<MatomoProvider>().CreateOptionsCopy(), context),
        "plausible" => Provider<PlausibleProvider>().GetScript(Provider<PlausibleProvider>().CreateOptionsCopy(), context),
        "umami" => Provider<UmamiProvider>().GetScript(Provider<UmamiProvider>().CreateOptionsCopy(), context),
        "yandex" => Provider<YandexMetricaProvider>().GetScript(Provider<YandexMetricaProvider>().CreateOptionsCopy(), context),
        _ => throw new ArgumentOutOfRangeException(nameof(tracker)),
    };

    private TProvider Provider<TProvider>() where TProvider : notnull => Services.GetRequiredService<TProvider>();

    private static DefaultHttpContext RequestWithAnalyticsConsent(bool analytics)
    {
        var consent = new CookieConsentData
        {
            Version = "1.0",
            ConsentType = analytics ? "accepted" : "declined",
            Categories = new Dictionary<string, bool> { ["necessary"] = true, ["analytics"] = analytics },
        };
        var context = new DefaultHttpContext();
        context.Request.Headers.Cookie = "osirion_cookie_consent=" + Uri.EscapeDataString(JsonSerializer.Serialize(consent));
        return context;
    }

    // A script as it runs: its own type, its other attributes and its text, without the attributes that hold it.
    private static string Describe(IElement script)
    {
        var type = script.GetAttribute("data-osirion-type") ?? (script.GetAttribute("type") == "text/plain" ? null : script.GetAttribute("type"));
        var attributes = script.Attributes
            .Where(attribute => attribute.Name is not ("type" or "data-osirion-consent" or "data-osirion-type"))
            .Select(attribute => $"{attribute.Name}={attribute.Value}");
        return $"type={type}|{string.Join("|", attributes)}|{script.TextContent}";
    }

    private static INodeList Parse(string html)
    {
        var parser = new HtmlParser(new HtmlParserOptions { IsScripting = true });
        return parser.ParseFragment(html, parser.ParseDocument(string.Empty).Body!);
    }

    private static IElement[] ParseScripts(string html) =>
        Parse(html).OfType<IElement>().Where(element => element.LocalName == "script").ToArray();

    public sealed class CustomTracker : AnalyticsTracker
    {
        [Parameter]
        public IAnalyticsProvider? TestProvider { get; set; }

        protected override IAnalyticsProvider? Provider => TestProvider;

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (ShouldRenderTracker)
            {
                builder.AddContent(0, TrackerMarkup);
            }
        }
    }
}
