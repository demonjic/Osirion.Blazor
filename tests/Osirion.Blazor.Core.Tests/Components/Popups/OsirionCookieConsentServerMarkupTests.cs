using System.Text.Json;
using System.Text.RegularExpressions;
using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Handlers;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Components.Popups;

/// <summary>
/// Guard tests: they pass before the browser evaluated consent mode exists and pin the server rendered banner byte for
/// byte, so adding that mode cannot change what existing sites send. Two parts of the output depend on the build machine
/// and are normalized on both sides: line endings follow the checkout, and the scoped CSS attribute is a hash of the
/// stylesheet's path, which differs between Windows and Unix builds.
/// </summary>
public partial class OsirionCookieConsentServerMarkupTests : BunitContext
{
    private readonly DefaultHttpContext _httpContext = new();

    public OsirionCookieConsentServerMarkupTests()
    {
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns(_httpContext);
        var antiforgeryStateProvider = Substitute.For<AntiforgeryStateProvider>();
        antiforgeryStateProvider.GetAntiforgeryToken()
            .Returns(new AntiforgeryRequestToken("test-token", "__RequestVerificationToken"));

        Services.AddSingleton(Options.Create(new ThemingOptions()));
        Services.AddSingleton(httpContextAccessor);
        Services.AddSingleton(antiforgeryStateProvider);
        SetRendererInfo(new RendererInfo("Static", false));
    }

    [Fact]
    public void OsirionCookieConsent_FirstVisit_MarkupIsUnchanged()
    {
        Services.GetRequiredService<BunitNavigationManager>().NavigateTo("http://localhost/products/access?a=1");

        var cut = Render<OsirionCookieConsent>();

        Normalize(cut.Markup).ShouldBe(Normalize(FirstVisitMarkup));
    }

    [Fact]
    public void OsirionCookieConsent_ReopenedPanel_MarkupIsUnchanged()
    {
        ReopenWithAnalyticsAccepted();

        var cut = Render<OsirionCookieConsent>();

        Normalize(cut.Markup).ShouldBe(Normalize(ReopenedPanelMarkup));
    }

    [Fact]
    public void OsirionCookieConsent_EveryOption_MarkupIsUnchanged()
    {
        ReopenWithAnalyticsAccepted();

        var cut = Render<OsirionCookieConsent>(parameters => parameters
            .Add(p => p.PolicyLink, "/privacy")
            .Add(p => p.Icon, (RenderFragment)(builder => builder.AddMarkupContent(0, "<svg class=\"i\"></svg>")))
            .Add(p => p.ShowDeclineButton, false)
            .Add(p => p.Position, "top")
            .Add(p => p.Class, "extra")
            .Add(p => p.ConsentExpiryDays, 30)
            .AddUnmatched("id", "consent"));

        Normalize(cut.Markup).ShouldBe(Normalize(EveryOptionMarkup));
    }

    private void ReopenWithAnalyticsAccepted()
    {
        var consent = new CookieConsentData
        {
            Version = "1.0",
            ConsentType = "save-preferences",
            Categories = new Dictionary<string, bool> { ["necessary"] = true, ["analytics"] = true },
        };
        _httpContext.Request.QueryString = new QueryString("?customize-cookies=true");
        _httpContext.Request.Headers.Cookie = "osirion_cookie_consent=" + Uri.EscapeDataString(JsonSerializer.Serialize(consent));
        Services.GetRequiredService<BunitNavigationManager>().NavigateTo("http://localhost/docs?customize-cookies=true");
    }

    // The expected markup names the scope attribute b-scope. Only the first scope id found is renamed, so markup that
    // carried a second one would still fail the comparison.
    private static string Normalize(string markup)
    {
        var lf = markup.ReplaceLineEndings("\n");
        var scope = ScopeAttribute().Match(lf);
        return scope.Success ? lf.Replace(scope.Value, ScopePlaceholder, StringComparison.Ordinal) : lf;
    }

    private const string ScopePlaceholder = "b-scope";

    [GeneratedRegex(@"(?<= )b-[a-z0-9]{10}(?=[ />])")]
    private static partial Regex ScopeAttribute();

    private const string FirstVisitMarkup = """
<form method="post" action="/api/cookie-consent" class="osirion-cookie-consent-form" b-scope><input type="hidden" name="__RequestVerificationToken" value="test-token" />
        <input type="hidden" name="expiryDays" value="365" b-scope />
        <input type="hidden" name="returnUrl" value="/products/access?a=1" b-scope />
        <div class="osirion-cookie-consent osirion-cookie-consent-bottom" role="region" aria-label="Cookie Consent" b-scope><div class="osirion-cookie-consent-container" b-scope><div class="osirion-cookie-consent-content" b-scope><div class="osirion-cookie-consent-message" b-scope><div class="osirion-cookie-consent-text" b-scope><h3 class="osirion-cookie-consent-title" b-scope>Cookie Consent</h3>
                            <p class="osirion-cookie-consent-description" b-scope>This site would like to use optional cookies in addition to the ones it needs to work. Nothing optional is set until you choose.</p></div></div>

                    <div class="osirion-cookie-consent-actions" b-scope><button type="submit" name="consent" value="declined" class="osirion-button osirion-button-danger" b-scope>Decline</button><button type="submit" name="consent" value="customize" class="osirion-button osirion-button-outlined osirion-button-outlined-primary osirion-cookie-consent-customize-button" b-scope>Customize</button><button type="submit" name="consent" value="accepted" class="osirion-button osirion-button-primary" b-scope>Accept All</button></div></div></div></div></form>
""";

    private const string ReopenedPanelMarkup = """
<form method="post" action="/api/cookie-consent" class="osirion-cookie-consent-form" b-scope><input type="hidden" name="__RequestVerificationToken" value="test-token" />
        <input type="hidden" name="expiryDays" value="365" b-scope />
        <input type="hidden" name="returnUrl" value="/docs?customize-cookies=true" b-scope />
        <div class="osirion-cookie-consent osirion-cookie-consent-bottom osirion-cookie-consent-customizing" role="region" aria-label="Cookie Consent" b-scope><div class="osirion-cookie-consent-container" b-scope><div class="osirion-cookie-consent-content" b-scope><div class="osirion-cookie-consent-message" b-scope><div class="osirion-cookie-consent-text" b-scope><h3 class="osirion-cookie-consent-title" b-scope>Cookie Consent</h3>
                            <p class="osirion-cookie-consent-description" b-scope>This site would like to use optional cookies in addition to the ones it needs to work. Nothing optional is set until you choose.</p></div></div>

                    <div class="osirion-cookie-consent-actions" b-scope><button type="submit" name="consent" value="declined" class="osirion-button osirion-button-danger" b-scope>Decline</button><button type="submit" name="consent" value="customize" class="osirion-button osirion-button-outlined osirion-button-outlined-primary osirion-cookie-consent-customize-button" b-scope>Customize</button><button type="submit" name="consent" value="accepted" class="osirion-button osirion-button-primary" b-scope>Accept All</button></div></div><div class="osirion-cookie-consent-customize" b-scope><h4 class="osirion-cookie-consent-customize-title" b-scope>Cookie Preferences</h4>

                        <div class="osirion-cookie-consent-categories" b-scope><div class="osirion-cookie-consent-category" b-scope><label class="osirion-cookie-consent-category-label" b-scope><input type="checkbox" name="category_necessary" value="true" checked disabled class="osirion-checkbox" b-scope />
                                        <div class="osirion-cookie-consent-category-info" b-scope><span class="osirion-cookie-consent-category-name" b-scope>Necessary<span class="osirion-cookie-consent-required" b-scope> (Required)</span></span>
                                            <p class="osirion-cookie-consent-category-description" b-scope>These cookies are essential for the website to function properly.</p></div></label></div><div class="osirion-cookie-consent-category" b-scope><label class="osirion-cookie-consent-category-label" b-scope><input type="checkbox" name="category_analytics" value="true" checked class="osirion-checkbox" b-scope />
                                        <div class="osirion-cookie-consent-category-info" b-scope><span class="osirion-cookie-consent-category-name" b-scope>Analytics</span>
                                            <p class="osirion-cookie-consent-category-description" b-scope>These cookies help us understand how visitors interact with our website.</p></div></label></div><div class="osirion-cookie-consent-category" b-scope><label class="osirion-cookie-consent-category-label" b-scope><input type="checkbox" name="category_marketing" value="true" class="osirion-checkbox" b-scope />
                                        <div class="osirion-cookie-consent-category-info" b-scope><span class="osirion-cookie-consent-category-name" b-scope>Marketing</span>
                                            <p class="osirion-cookie-consent-category-description" b-scope>These cookies are used to show you relevant ads based on your interests.</p></div></label></div><div class="osirion-cookie-consent-category" b-scope><label class="osirion-cookie-consent-category-label" b-scope><input type="checkbox" name="category_preferences" value="true" class="osirion-checkbox" b-scope />
                                        <div class="osirion-cookie-consent-category-info" b-scope><span class="osirion-cookie-consent-category-name" b-scope>Preferences</span>
                                            <p class="osirion-cookie-consent-category-description" b-scope>These cookies remember your settings and preferences.</p></div></label></div></div>

                        <div class="osirion-cookie-consent-customize-actions" b-scope><button type="submit" name="consent" value="save-preferences" class="osirion-button osirion-button-outlined osirion-button-outlined-primary" b-scope>Save Preferences</button></div></div></div></div></form>
""";

    private const string EveryOptionMarkup = """
<form method="post" action="/api/cookie-consent" class="osirion-cookie-consent-form" id="consent" b-scope><input type="hidden" name="__RequestVerificationToken" value="test-token" />
        <input type="hidden" name="expiryDays" value="30" b-scope />
        <input type="hidden" name="returnUrl" value="/docs?customize-cookies=true" b-scope />
        <div class="osirion-cookie-consent osirion-cookie-consent-top osirion-cookie-consent-customizing extra" role="region" aria-label="Cookie Consent" b-scope><div class="osirion-cookie-consent-container" b-scope><div class="osirion-cookie-consent-content" b-scope><div class="osirion-cookie-consent-message" b-scope><div class="osirion-cookie-consent-icon" b-scope><svg class="i"></svg></div><div class="osirion-cookie-consent-text" b-scope><h3 class="osirion-cookie-consent-title" b-scope>Cookie Consent</h3>
                            <p class="osirion-cookie-consent-description" b-scope>This site would like to use optional cookies in addition to the ones it needs to work. Nothing optional is set until you choose. <a href="/privacy" class="osirion-cookie-consent-link" b-scope>Learn more</a></p></div></div>

                    <div class="osirion-cookie-consent-actions" b-scope><button type="submit" name="consent" value="customize" class="osirion-button osirion-button-outlined osirion-button-outlined-primary osirion-cookie-consent-customize-button" b-scope>Customize</button><button type="submit" name="consent" value="accepted" class="osirion-button osirion-button-primary" b-scope>Accept All</button></div></div><div class="osirion-cookie-consent-customize" b-scope><h4 class="osirion-cookie-consent-customize-title" b-scope>Cookie Preferences</h4>

                        <div class="osirion-cookie-consent-categories" b-scope><div class="osirion-cookie-consent-category" b-scope><label class="osirion-cookie-consent-category-label" b-scope><input type="checkbox" name="category_necessary" value="true" checked disabled class="osirion-checkbox" b-scope />
                                        <div class="osirion-cookie-consent-category-info" b-scope><span class="osirion-cookie-consent-category-name" b-scope>Necessary<span class="osirion-cookie-consent-required" b-scope> (Required)</span></span>
                                            <p class="osirion-cookie-consent-category-description" b-scope>These cookies are essential for the website to function properly.</p></div></label></div><div class="osirion-cookie-consent-category" b-scope><label class="osirion-cookie-consent-category-label" b-scope><input type="checkbox" name="category_analytics" value="true" checked class="osirion-checkbox" b-scope />
                                        <div class="osirion-cookie-consent-category-info" b-scope><span class="osirion-cookie-consent-category-name" b-scope>Analytics</span>
                                            <p class="osirion-cookie-consent-category-description" b-scope>These cookies help us understand how visitors interact with our website.</p></div></label></div><div class="osirion-cookie-consent-category" b-scope><label class="osirion-cookie-consent-category-label" b-scope><input type="checkbox" name="category_marketing" value="true" class="osirion-checkbox" b-scope />
                                        <div class="osirion-cookie-consent-category-info" b-scope><span class="osirion-cookie-consent-category-name" b-scope>Marketing</span>
                                            <p class="osirion-cookie-consent-category-description" b-scope>These cookies are used to show you relevant ads based on your interests.</p></div></label></div><div class="osirion-cookie-consent-category" b-scope><label class="osirion-cookie-consent-category-label" b-scope><input type="checkbox" name="category_preferences" value="true" class="osirion-checkbox" b-scope />
                                        <div class="osirion-cookie-consent-category-info" b-scope><span class="osirion-cookie-consent-category-name" b-scope>Preferences</span>
                                            <p class="osirion-cookie-consent-category-description" b-scope>These cookies remember your settings and preferences.</p></div></label></div></div>

                        <div class="osirion-cookie-consent-customize-actions" b-scope><button type="submit" name="consent" value="save-preferences" class="osirion-button osirion-button-outlined osirion-button-outlined-primary" b-scope>Save Preferences</button></div></div></div></div></form>
""";
}
