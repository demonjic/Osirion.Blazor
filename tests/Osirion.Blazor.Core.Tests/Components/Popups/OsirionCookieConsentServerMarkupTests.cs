using System.Text.Json;
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
/// byte, so adding that mode cannot change what existing sites send. Line endings follow the checkout, so both sides
/// are compared with LF.
/// </summary>
public class OsirionCookieConsentServerMarkupTests : BunitContext
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

    private static string Normalize(string markup) => markup.ReplaceLineEndings("\n");

    private const string FirstVisitMarkup = """
<form method="post" action="/api/cookie-consent" class="osirion-cookie-consent-form" b-rsessv0pak><input type="hidden" name="__RequestVerificationToken" value="test-token" />
        <input type="hidden" name="expiryDays" value="365" b-rsessv0pak />
        <input type="hidden" name="returnUrl" value="/products/access?a=1" b-rsessv0pak />
        <div class="osirion-cookie-consent osirion-cookie-consent-bottom" role="region" aria-label="Cookie Consent" b-rsessv0pak><div class="osirion-cookie-consent-container" b-rsessv0pak><div class="osirion-cookie-consent-content" b-rsessv0pak><div class="osirion-cookie-consent-message" b-rsessv0pak><div class="osirion-cookie-consent-text" b-rsessv0pak><h3 class="osirion-cookie-consent-title" b-rsessv0pak>Cookie Consent</h3>
                            <p class="osirion-cookie-consent-description" b-rsessv0pak>This site would like to use optional cookies in addition to the ones it needs to work. Nothing optional is set until you choose.</p></div></div>

                    <div class="osirion-cookie-consent-actions" b-rsessv0pak><button type="submit" name="consent" value="declined" class="osirion-button osirion-button-danger" b-rsessv0pak>Decline</button><button type="submit" name="consent" value="customize" class="osirion-button osirion-button-outlined osirion-button-outlined-primary" style="color:inherit; border-color:inherit;" b-rsessv0pak>Customize</button><button type="submit" name="consent" value="accepted" class="osirion-button osirion-button-primary" b-rsessv0pak>Accept All</button></div></div></div></div></form>
""";

    private const string ReopenedPanelMarkup = """
<form method="post" action="/api/cookie-consent" class="osirion-cookie-consent-form" b-rsessv0pak><input type="hidden" name="__RequestVerificationToken" value="test-token" />
        <input type="hidden" name="expiryDays" value="365" b-rsessv0pak />
        <input type="hidden" name="returnUrl" value="/docs?customize-cookies=true" b-rsessv0pak />
        <div class="osirion-cookie-consent osirion-cookie-consent-bottom osirion-cookie-consent-customizing" role="region" aria-label="Cookie Consent" b-rsessv0pak><div class="osirion-cookie-consent-container" b-rsessv0pak><div class="osirion-cookie-consent-content" b-rsessv0pak><div class="osirion-cookie-consent-message" b-rsessv0pak><div class="osirion-cookie-consent-text" b-rsessv0pak><h3 class="osirion-cookie-consent-title" b-rsessv0pak>Cookie Consent</h3>
                            <p class="osirion-cookie-consent-description" b-rsessv0pak>This site would like to use optional cookies in addition to the ones it needs to work. Nothing optional is set until you choose.</p></div></div>

                    <div class="osirion-cookie-consent-actions" b-rsessv0pak><button type="submit" name="consent" value="declined" class="osirion-button osirion-button-danger" b-rsessv0pak>Decline</button><button type="submit" name="consent" value="customize" class="osirion-button osirion-button-outlined osirion-button-outlined-primary" style="color:inherit; border-color:inherit;" b-rsessv0pak>Customize</button><button type="submit" name="consent" value="accepted" class="osirion-button osirion-button-primary" b-rsessv0pak>Accept All</button></div></div><div class="osirion-cookie-consent-customize" b-rsessv0pak><h4 class="osirion-cookie-consent-customize-title" b-rsessv0pak>Cookie Preferences</h4>

                        <div class="osirion-cookie-consent-categories" b-rsessv0pak><div class="osirion-cookie-consent-category" b-rsessv0pak><label class="osirion-cookie-consent-category-label" b-rsessv0pak><input type="checkbox" name="category_necessary" value="true" checked disabled class="osirion-checkbox" b-rsessv0pak />
                                        <div class="osirion-cookie-consent-category-info" b-rsessv0pak><span class="osirion-cookie-consent-category-name" b-rsessv0pak>Necessary<span class="osirion-cookie-consent-required" b-rsessv0pak> (Required)</span></span>
                                            <p class="osirion-cookie-consent-category-description" b-rsessv0pak>These cookies are essential for the website to function properly.</p></div></label></div><div class="osirion-cookie-consent-category" b-rsessv0pak><label class="osirion-cookie-consent-category-label" b-rsessv0pak><input type="checkbox" name="category_analytics" value="true" checked class="osirion-checkbox" b-rsessv0pak />
                                        <div class="osirion-cookie-consent-category-info" b-rsessv0pak><span class="osirion-cookie-consent-category-name" b-rsessv0pak>Analytics</span>
                                            <p class="osirion-cookie-consent-category-description" b-rsessv0pak>These cookies help us understand how visitors interact with our website.</p></div></label></div><div class="osirion-cookie-consent-category" b-rsessv0pak><label class="osirion-cookie-consent-category-label" b-rsessv0pak><input type="checkbox" name="category_marketing" value="true" class="osirion-checkbox" b-rsessv0pak />
                                        <div class="osirion-cookie-consent-category-info" b-rsessv0pak><span class="osirion-cookie-consent-category-name" b-rsessv0pak>Marketing</span>
                                            <p class="osirion-cookie-consent-category-description" b-rsessv0pak>These cookies are used to show you relevant ads based on your interests.</p></div></label></div><div class="osirion-cookie-consent-category" b-rsessv0pak><label class="osirion-cookie-consent-category-label" b-rsessv0pak><input type="checkbox" name="category_preferences" value="true" class="osirion-checkbox" b-rsessv0pak />
                                        <div class="osirion-cookie-consent-category-info" b-rsessv0pak><span class="osirion-cookie-consent-category-name" b-rsessv0pak>Preferences</span>
                                            <p class="osirion-cookie-consent-category-description" b-rsessv0pak>These cookies remember your settings and preferences.</p></div></label></div></div>

                        <div class="osirion-cookie-consent-customize-actions" b-rsessv0pak><button type="submit" name="consent" value="save-preferences" class="osirion-button osirion-button-outlined osirion-button-outlined-primary" b-rsessv0pak>Save Preferences</button></div></div></div></div></form>
""";

    private const string EveryOptionMarkup = """
<form method="post" action="/api/cookie-consent" class="osirion-cookie-consent-form" id="consent" b-rsessv0pak><input type="hidden" name="__RequestVerificationToken" value="test-token" />
        <input type="hidden" name="expiryDays" value="30" b-rsessv0pak />
        <input type="hidden" name="returnUrl" value="/docs?customize-cookies=true" b-rsessv0pak />
        <div class="osirion-cookie-consent osirion-cookie-consent-top osirion-cookie-consent-customizing extra" role="region" aria-label="Cookie Consent" b-rsessv0pak><div class="osirion-cookie-consent-container" b-rsessv0pak><div class="osirion-cookie-consent-content" b-rsessv0pak><div class="osirion-cookie-consent-message" b-rsessv0pak><div class="osirion-cookie-consent-icon" b-rsessv0pak><svg class="i"></svg></div><div class="osirion-cookie-consent-text" b-rsessv0pak><h3 class="osirion-cookie-consent-title" b-rsessv0pak>Cookie Consent</h3>
                            <p class="osirion-cookie-consent-description" b-rsessv0pak>This site would like to use optional cookies in addition to the ones it needs to work. Nothing optional is set until you choose. <a href="/privacy" class="osirion-cookie-consent-link" b-rsessv0pak>Learn more</a></p></div></div>

                    <div class="osirion-cookie-consent-actions" b-rsessv0pak><button type="submit" name="consent" value="customize" class="osirion-button osirion-button-outlined osirion-button-outlined-primary" style="color:inherit; border-color:inherit;" b-rsessv0pak>Customize</button><button type="submit" name="consent" value="accepted" class="osirion-button osirion-button-primary" b-rsessv0pak>Accept All</button></div></div><div class="osirion-cookie-consent-customize" b-rsessv0pak><h4 class="osirion-cookie-consent-customize-title" b-rsessv0pak>Cookie Preferences</h4>

                        <div class="osirion-cookie-consent-categories" b-rsessv0pak><div class="osirion-cookie-consent-category" b-rsessv0pak><label class="osirion-cookie-consent-category-label" b-rsessv0pak><input type="checkbox" name="category_necessary" value="true" checked disabled class="osirion-checkbox" b-rsessv0pak />
                                        <div class="osirion-cookie-consent-category-info" b-rsessv0pak><span class="osirion-cookie-consent-category-name" b-rsessv0pak>Necessary<span class="osirion-cookie-consent-required" b-rsessv0pak> (Required)</span></span>
                                            <p class="osirion-cookie-consent-category-description" b-rsessv0pak>These cookies are essential for the website to function properly.</p></div></label></div><div class="osirion-cookie-consent-category" b-rsessv0pak><label class="osirion-cookie-consent-category-label" b-rsessv0pak><input type="checkbox" name="category_analytics" value="true" checked class="osirion-checkbox" b-rsessv0pak />
                                        <div class="osirion-cookie-consent-category-info" b-rsessv0pak><span class="osirion-cookie-consent-category-name" b-rsessv0pak>Analytics</span>
                                            <p class="osirion-cookie-consent-category-description" b-rsessv0pak>These cookies help us understand how visitors interact with our website.</p></div></label></div><div class="osirion-cookie-consent-category" b-rsessv0pak><label class="osirion-cookie-consent-category-label" b-rsessv0pak><input type="checkbox" name="category_marketing" value="true" class="osirion-checkbox" b-rsessv0pak />
                                        <div class="osirion-cookie-consent-category-info" b-rsessv0pak><span class="osirion-cookie-consent-category-name" b-rsessv0pak>Marketing</span>
                                            <p class="osirion-cookie-consent-category-description" b-rsessv0pak>These cookies are used to show you relevant ads based on your interests.</p></div></label></div><div class="osirion-cookie-consent-category" b-rsessv0pak><label class="osirion-cookie-consent-category-label" b-rsessv0pak><input type="checkbox" name="category_preferences" value="true" class="osirion-checkbox" b-rsessv0pak />
                                        <div class="osirion-cookie-consent-category-info" b-rsessv0pak><span class="osirion-cookie-consent-category-name" b-rsessv0pak>Preferences</span>
                                            <p class="osirion-cookie-consent-category-description" b-rsessv0pak>These cookies remember your settings and preferences.</p></div></label></div></div>

                        <div class="osirion-cookie-consent-customize-actions" b-rsessv0pak><button type="submit" name="consent" value="save-preferences" class="osirion-button osirion-button-outlined osirion-button-outlined-primary" b-rsessv0pak>Save Preferences</button></div></div></div></div></form>
""";
}
