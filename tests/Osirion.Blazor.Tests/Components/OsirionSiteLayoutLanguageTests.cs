using System.Globalization;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Osirion.Blazor.Components;
using Osirion.Blazor.Extensions;
using Shouldly;

namespace Osirion.Blazor.Tests.Components;

/// <summary>The shell on a site with language versions: localized addresses and the language picker.</summary>
public sealed class OsirionSiteLayoutLanguageTests : BunitContext
{
    private static readonly Dictionary<string, string?> Site = new()
    {
        ["Osirion:Site:Name"] = "Example",
        ["Osirion:Site:Menu:0:Text"] = "Docs",
        ["Osirion:Site:Menu:0:Href"] = "/docs",
        ["Osirion:Site:CallToAction:Text"] = "Book a call",
        ["Osirion:Site:CallToAction:Href"] = "/contact",
        ["Osirion:Site:LegalLinks:0:Text"] = "Privacy",
        ["Osirion:Site:LegalLinks:0:Href"] = "/privacy",
        ["Osirion:Site:Locales:sr:Labels:Language"] = "Jezik",
        ["Osirion:Site:Locales:sr:Menu:0:Text"] = "Dokumentacija",
    };

    private static readonly Dictionary<string, string?> Localization = new()
    {
        ["Osirion:Localization:DefaultCulture"] = "en",
        ["Osirion:Localization:Cultures:0:Name"] = "en",
        ["Osirion:Localization:Cultures:1:Name"] = "sr",
        ["Osirion:Localization:Cultures:2:Name"] = "de",
    };

    private readonly CultureInfo _uiCulture = CultureInfo.CurrentUICulture;

    protected override void Dispose(bool disposing)
    {
        CultureInfo.CurrentUICulture = _uiCulture;
        base.Dispose(disposing);
    }

    [Fact]
    public void Layout_SerbianRequest_WritesTheSiteAddressesUnderThePrefix()
    {
        var cut = RenderLayout("sr", Localization);

        var menu = cut.Find("nav[aria-label='Main navigation']");
        menu.QuerySelector("a.osirion-site-call-to-action")!.GetAttribute("href").ShouldBe("/sr/contact");
        menu.QuerySelectorAll("a").Select(link => link.GetAttribute("href")).ShouldContain("/sr/docs");
        menu.QuerySelectorAll("a").Select(link => link.GetAttribute("href")).ShouldContain("/sr");
        cut.Find("a.osirion-site-footer-brand").GetAttribute("href").ShouldBe("/sr");
        cut.Find(".osirion-footer-bottom-links a").GetAttribute("href").ShouldBe("/sr/privacy");
    }

    // Guards the behavior without Osirion:Localization: the addresses are written as configured.
    [Fact]
    public void Layout_WithoutLocalization_WritesTheAddressesAsConfigured()
    {
        var cut = RenderLayout("sr", []);

        cut.Find("a.osirion-site-call-to-action").GetAttribute("href").ShouldBe("/contact");
        cut.Find("a.osirion-site-footer-brand").GetAttribute("href").ShouldBe("/");
        cut.Find(".osirion-footer-bottom-links a").GetAttribute("href").ShouldBe("/privacy");
        cut.FindAll(".osirion-language-picker").ShouldBeEmpty();
    }

    [Fact]
    public void Layout_PickerInTheHeader_IsACompactDisclosureAmongTheMenuActions()
    {
        var cut = RenderLayout("sr", new(Localization) { ["Osirion:Site:LanguagePicker"] = "Header" });

        var picker = cut.Find(".osirion-menu-actions nav.osirion-language-picker.osirion-language-picker-compact");
        picker.GetAttribute("aria-label").ShouldBe("Jezik");
        picker.QuerySelector("details > summary").ShouldNotBeNull();
        cut.FindAll("footer .osirion-language-picker").ShouldBeEmpty();
    }

    [Fact]
    public void Layout_PickerInTheFooter_IsARowOfLinksBelowTheBottomRow()
    {
        var cut = RenderLayout("en", new(Localization) { ["Osirion:Site:LanguagePicker"] = "Footer" });

        var picker = cut.Find(".osirion-footer-extra nav.osirion-language-picker");
        picker.ClassList.ShouldNotContain("osirion-language-picker-compact");
        picker.GetAttribute("aria-label").ShouldBe("Language");
        picker.QuerySelectorAll("a").Length.ShouldBe(3);
        cut.FindAll(".osirion-menu-actions .osirion-language-picker").ShouldBeEmpty();
    }

    [Fact]
    public void Layout_PickerInBoth_RendersTwoPickers()
    {
        var cut = RenderLayout("en", new(Localization) { ["Osirion:Site:LanguagePicker"] = "Both" });

        cut.FindAll("nav.osirion-language-picker").Count.ShouldBe(2);
    }

    // Guards the default: no picker and no extra footer row.
    [Fact]
    public void Layout_PickerNotConfigured_RendersNoPickerAndNoFooterExtra()
    {
        var cut = RenderLayout("en", Localization);

        cut.FindAll(".osirion-language-picker").ShouldBeEmpty();
        cut.FindAll(".osirion-footer-extra").ShouldBeEmpty();
    }

    private IRenderedComponent<OsirionSiteLayout> RenderLayout(string culture, Dictionary<string, string?> values)
    {
        var all = new Dictionary<string, string?>(Site);
        foreach (var (key, value) in values)
        {
            all[key] = value;
        }

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(all).Build();
        Services.AddSingleton<IConfiguration>(configuration);
        Services.AddSingleton<IHttpContextAccessor>(new HttpContextAccessor { HttpContext = new DefaultHttpContext() });
        Services.AddOsirion(configuration);
        JSInterop.Mode = JSRuntimeMode.Loose;
        SetRendererInfo(new RendererInfo("Static", false));
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);

        return Render<OsirionSiteLayout>(p => p.Add(c => c.Body, (RenderFragment)(builder => builder.AddContent(0, "Page body"))));
    }
}
