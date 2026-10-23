using BlazorJSComponents;
using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Osirion.Blazor.Components;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Components.Popups;

/// <summary>
/// The banner with <see cref="ConsentEvaluation.Browser"/>: the same markup for every visitor, so a page exported to static
/// files carries it, and a script decides in the browser whether to show it.
/// </summary>
public class OsirionCookieConsentBrowserTests : BunitContext
{
    private readonly DefaultHttpContext _httpContext = new();

    public OsirionCookieConsentBrowserTests()
    {
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns(_httpContext);
        var antiforgeryStateProvider = Substitute.For<AntiforgeryStateProvider>();
        antiforgeryStateProvider.GetAntiforgeryToken()
            .Returns(new AntiforgeryRequestToken("test-token", "__RequestVerificationToken"));

        Services.AddSingleton(Options.Create(new ThemingOptions()));
        Services.AddSingleton(httpContextAccessor);
        Services.AddSingleton(antiforgeryStateProvider);
        Services.AddJSComponents();
        JSInterop.Mode = JSRuntimeMode.Loose;
        SetRendererInfo(new RendererInfo("Static", false));
        Services.GetRequiredService<BunitNavigationManager>().NavigateTo("http://localhost/docs");
    }

    [Fact]
    public void OsirionCookieConsent_Browser_RendersTheBannerHiddenAndWithoutAForm()
    {
        var cut = RenderInBrowserMode();

        var banner = cut.Find("[data-osirion-consent-banner]");
        banner.TagName.ShouldBe("DIV");
        banner.HasAttribute("hidden").ShouldBeTrue();
        banner.ClassList.ShouldContain("osirion-cookie-consent-form");
        cut.FindAll("form").ShouldBeEmpty();
        cut.FindAll("input[type=hidden]").ShouldBeEmpty();
        cut.Markup.ShouldNotContain("test-token");
        cut.Markup.ShouldNotContain("__RequestVerificationToken");
    }

    [Fact]
    public void OsirionCookieConsent_Browser_EveryButtonIsTypeButtonAndNamesItsChoice()
    {
        var cut = RenderInBrowserMode();

        var buttons = cut.FindAll("button");
        buttons.Select(button => button.GetAttribute("type")).ShouldAllBe(type => type == "button");
        buttons.Select(button => button.GetAttribute("value")).ShouldBe(["declined", "customize", "accepted", "save-preferences"]);
    }

    [Fact]
    public void OsirionCookieConsent_Browser_RendersThePanelHiddenWithTheDefaultChoices()
    {
        var cut = RenderInBrowserMode();

        var panel = cut.Find(".osirion-cookie-consent-customize");
        panel.HasAttribute("hidden").ShouldBeTrue();
        cut.Find("button[value=customize]").GetAttribute("aria-expanded").ShouldBe("false");
        cut.Find(".osirion-cookie-consent").ClassList.ShouldNotContain("osirion-cookie-consent-customizing");
        var boxes = cut.FindAll("input[type=checkbox]");
        boxes.Select(box => (box.GetAttribute("name"), box.HasAttribute("checked"), box.HasAttribute("disabled"))).ShouldBe(
        [
            ("category_necessary", true, true),
            ("category_analytics", false, false),
            ("category_marketing", false, false),
            ("category_preferences", false, false),
        ]);
    }

    // The static file is the same for every visitor, so nothing in the request may change the banner. (The script
    // loader's instance counter after it differs per render, so the banner element is compared.)
    [Fact]
    public void OsirionCookieConsent_Browser_IgnoresTheRequestCookieAndQuery()
    {
        var first = RenderInBrowserMode().Find("[data-osirion-consent-banner]").OuterHtml;
        _httpContext.Request.Headers.Cookie = "osirion_cookie_consent=" + Uri.EscapeDataString(
            "{\"Version\":\"1.0\",\"ConsentType\":\"accepted\",\"Categories\":{\"necessary\":true,\"analytics\":true}}");
        _httpContext.Request.QueryString = new QueryString("?customize-cookies=true");

        var withConsent = RenderInBrowserMode().Find("[data-osirion-consent-banner]").OuterHtml;

        withConsent.ShouldBe(first);
    }

    [Fact]
    public void OsirionCookieConsent_Browser_WithoutAnHttpContext_StillRendersTheBanner()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton(Options.Create(new ThemingOptions()));
        context.Services.AddJSComponents();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.SetRendererInfo(new RendererInfo("Static", false));

        var cut = context.Render<OsirionCookieConsent>(p => p.Add(c => c.Evaluation, ConsentEvaluation.Browser));

        cut.Find("[data-osirion-consent-banner]").HasAttribute("hidden").ShouldBeTrue();
    }

    [Fact]
    public void OsirionCookieConsent_Browser_LoadsTheConsentScriptFromAFile()
    {
        var cut = RenderInBrowserMode();

        cut.Find("bl-script").GetAttribute("src").ShouldBe("./_content/Osirion.Blazor.Core/js/osirion-consent.js");
        cut.FindAll("script").Where(script => script.GetAttribute("type") is null or "module" or "text/javascript")
            .ShouldBeEmpty();
        cut.FindAll("*").SelectMany(element => element.Attributes)
            .Where(attribute => attribute.Name.StartsWith("on", StringComparison.OrdinalIgnoreCase))
            .ShouldBeEmpty();
    }

    [Theory]
    [InlineData(90, "90")]
    [InlineData(3650, "3650")]
    [InlineData(0, "365")]
    [InlineData(3651, "365")]
    public void OsirionCookieConsent_Browser_WritesTheExpiryTheEndpointWouldApply(int days, string written)
    {
        var cut = RenderInBrowserMode(p => p.Add(c => c.ConsentExpiryDays, days));

        cut.Find("[data-osirion-consent-banner]").GetAttribute("data-osirion-consent-days").ShouldBe(written);
    }

    // Without the panel there is nothing for the button to open, and no page to reload into.
    [Fact]
    public void OsirionCookieConsent_BrowserWithoutCustomizationPanel_LeavesOutTheCustomizeButton()
    {
        var cut = RenderInBrowserMode(p => p.Add(c => c.ShowCustomizationPanel, false));

        cut.FindAll("button").Select(button => button.GetAttribute("value")).ShouldBe(["declined", "accepted"]);
        cut.FindAll(".osirion-cookie-consent-customize").ShouldBeEmpty();
    }

    [Fact]
    public void OsirionCookieConsent_Browser_AppliesTheClassAndUnmatchedAttributesOnce()
    {
        var cut = RenderInBrowserMode(p => p.Add(c => c.Class, "extra").AddUnmatched("id", "consent-banner"));

        cut.FindAll("#consent-banner").ShouldHaveSingleItem().HasAttribute("data-osirion-consent-banner").ShouldBeTrue();
        cut.Find(".osirion-cookie-consent").ClassList.ShouldContain("extra");
        cut.Find(".osirion-cookie-consent").GetAttribute("aria-label").ShouldBe("Cookie Consent");
    }

    [Fact]
    public void OsirionCookieConsent_ServerEvaluation_RendersTheSameMarkupAsTheDefault()
    {
        var byDefault = Render<OsirionCookieConsent>().Markup;

        var server = Render<OsirionCookieConsent>(p => p.Add(c => c.Evaluation, ConsentEvaluation.Server)).Markup;

        server.ShouldBe(byDefault);
        server.ShouldContain("<form method=\"post\"");
    }

    private IRenderedComponent<OsirionCookieConsent> RenderInBrowserMode(
        Action<ComponentParameterCollectionBuilder<OsirionCookieConsent>>? configure = null) =>
        Render<OsirionCookieConsent>(p =>
        {
            p.Add(c => c.Evaluation, ConsentEvaluation.Browser);
            configure?.Invoke(p);
        });
}
