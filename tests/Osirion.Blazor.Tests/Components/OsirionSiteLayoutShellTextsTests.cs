using System.Globalization;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Osirion.Blazor.Components;
using Osirion.Blazor.Extensions;
using Shouldly;

namespace Osirion.Blazor.Tests.Components;

/// <summary>
/// The submenu toggle's name and the cookie consent banner's texts, in the language of each version: the site's own,
/// a version's entry, a version derived by transliteration, and a culture without an entry.
/// </summary>
public sealed class OsirionSiteLayoutShellTextsTests : BunitContext
{
    private static readonly Dictionary<string, string?> Site = new()
    {
        ["Osirion:Site:Name"] = "Example",
        ["Osirion:Site:ShowCookieConsent"] = "true",

        // Evaluated in the browser, the banner carries its category panel on every page
        ["Osirion:Analytics:Consent:Evaluation"] = "Browser",
        ["Osirion:Site:Menu:0:Text"] = "Products",
        ["Osirion:Site:Menu:0:Items:0:Text"] = "Access",
        ["Osirion:Site:Menu:0:Items:0:Href"] = "/access",
        ["Osirion:Site:Locales:sr:Menu:0:Text"] = "Производи",
        ["Osirion:Site:Locales:sr:Menu:0:Items:0:Text"] = "Приступ",
        ["Osirion:Site:Locales:sr:Labels:ToggleSubmenu"] = "Отвори подмени {0}",
        ["Osirion:Site:Locales:sr:Labels:CookieConsent:Title"] = "Колачићи",
        ["Osirion:Site:Locales:sr:Labels:CookieConsent:AcceptButton"] = "Прихвати све",
        ["Osirion:Site:Locales:sr:Labels:CookieConsent:Required"] = "(обавезно)",
        ["Osirion:Site:Locales:sr:Labels:CookieConsent:Categories:necessary:Name"] = "Неопходни",
        ["Osirion:Site:Locales:sr-Latn:DerivedFrom"] = "sr",
        ["Osirion:Site:Locales:sr-Latn:Transliteration"] = "serbian-cyrillic-to-latin",
    };

    private readonly CultureInfo _uiCulture = CultureInfo.CurrentUICulture;

    protected override void Dispose(bool disposing)
    {
        CultureInfo.CurrentUICulture = _uiCulture;
        base.Dispose(disposing);
    }

    // Guards the site without the new labels: the English name and banner as before.
    [Fact]
    public void Layout_SiteLanguage_KeepsTheEnglishToggleNameAndBanner()
    {
        var cut = RenderLayout("en");

        cut.Find("input.osirion-submenu-toggler").GetAttribute("aria-label").ShouldBe("Toggle Products submenu");
        cut.Find(".osirion-cookie-consent-title").TextContent.ShouldBe("Cookie Consent");
        cut.Find(".osirion-cookie-consent-required").TextContent.ShouldBe(" (Required)");
        cut.Find(".osirion-cookie-consent-category-name").TextContent.ShouldStartWith("Necessary");
    }

    [Fact]
    public void Layout_VersionWithItsTexts_NamesTheToggleAndWritesTheBannerInItsLanguage()
    {
        var cut = RenderLayout("sr");

        cut.Find("input.osirion-submenu-toggler").GetAttribute("aria-label").ShouldBe("Отвори подмени Производи");
        cut.Find(".osirion-cookie-consent-title").TextContent.ShouldBe("Колачићи");
        cut.Find("button[value='accepted']").TextContent.Trim().ShouldBe("Прихвати све");
        cut.Find(".osirion-cookie-consent-required").TextContent.ShouldBe(" (обавезно)");
        cut.Find(".osirion-cookie-consent-category-name").TextContent.ShouldStartWith("Неопходни");
        cut.Find("input[name='category_necessary']").ShouldNotBeNull();

        // A text the version leaves out keeps the English default
        cut.Find("button[value='declined']").TextContent.Trim().ShouldBe("Decline");
    }

    [Fact]
    public void Layout_DerivedVersion_TransliteratesTheToggleNameAndTheBanner()
    {
        var cut = RenderLayout("sr-Latn");

        cut.Find("input.osirion-submenu-toggler").GetAttribute("aria-label").ShouldBe("Otvori podmeni Proizvodi");
        cut.Find(".osirion-cookie-consent-title").TextContent.ShouldBe("Kolačići");
        cut.Find(".osirion-cookie-consent-required").TextContent.ShouldBe(" (obavezno)");
        cut.Find(".osirion-cookie-consent-category-name").TextContent.ShouldStartWith("Neophodni");
    }

    [Fact]
    public void Layout_CultureWithoutAnEntry_UsesTheSiteTexts()
    {
        var cut = RenderLayout("de");

        cut.Find("input.osirion-submenu-toggler").GetAttribute("aria-label").ShouldBe("Toggle Products submenu");
        cut.Find(".osirion-cookie-consent-title").TextContent.ShouldBe("Cookie Consent");
    }

    private IRenderedComponent<OsirionSiteLayout> RenderLayout(string culture)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(Site).Build();
        Services.AddSingleton<IConfiguration>(configuration);
        Services.AddSingleton<IHttpContextAccessor>(new HttpContextAccessor { HttpContext = new DefaultHttpContext() });
        var antiforgery = Substitute.For<AntiforgeryStateProvider>();
        antiforgery.GetAntiforgeryToken().Returns(new AntiforgeryRequestToken("token", "__RequestVerificationToken"));
        Services.AddSingleton(antiforgery);
        Services.AddOsirion(configuration);
        JSInterop.Mode = JSRuntimeMode.Loose;
        SetRendererInfo(new RendererInfo("Static", false));
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);

        return Render<OsirionSiteLayout>(p => p.Add(c => c.Body, (RenderFragment)(builder => builder.AddContent(0, "Page body"))));
    }
}
