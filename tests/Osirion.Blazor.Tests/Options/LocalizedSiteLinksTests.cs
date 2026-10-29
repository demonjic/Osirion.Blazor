using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Osirion.Blazor.Core.Localization;
using Osirion.Blazor.Extensions;
using Osirion.Blazor.Options;
using Osirion.Blazor.Tests.Fakes;
using Shouldly;

namespace Osirion.Blazor.Tests.Options;

/// <summary>
/// <see cref="OsirionSiteOptions.ForCulture(CultureInfo, IOsirionLocalePaths?)"/>: the site's addresses under the
/// request's culture prefix, and a <c>Locales</c> entry that writes only texts.
/// </summary>
public class LocalizedSiteLinksTests
{
    private static readonly Dictionary<string, string?> Site = new()
    {
        ["Osirion:Site:Name"] = "Example",
        ["Osirion:Site:Menu:0:Text"] = "Docs",
        ["Osirion:Site:Menu:0:Href"] = "/docs",
        ["Osirion:Site:Menu:1:Text"] = "Guides",
        ["Osirion:Site:Menu:1:Items:0:Text"] = "Start",
        ["Osirion:Site:Menu:1:Items:0:Href"] = "/guides/start",
        ["Osirion:Site:Menu:2:Text"] = "Blog",
        ["Osirion:Site:Menu:2:Href"] = "https://blog.example.org/",
        ["Osirion:Site:CallToAction:Text"] = "Book a call",
        ["Osirion:Site:CallToAction:Href"] = "/contact",
        ["Osirion:Site:Announcement:Id"] = "launch",
        ["Osirion:Site:Announcement:Text"] = "New release",
        ["Osirion:Site:Announcement:LinkText"] = "Read",
        ["Osirion:Site:Announcement:LinkUrl"] = "/blog/release",
        ["Osirion:Site:FooterSections:0:Title"] = "Company",
        ["Osirion:Site:FooterSections:0:Links:0:Text"] = "About",
        ["Osirion:Site:FooterSections:0:Links:0:Href"] = "/about",
        ["Osirion:Site:FooterSections:0:Links:1:Text"] = "Write to us",
        ["Osirion:Site:FooterSections:0:Links:1:Href"] = "mailto:office@example.org",
        ["Osirion:Site:FooterSections:0:Links:2:Text"] = "Call us",
        ["Osirion:Site:FooterSections:0:Links:2:Href"] = "tel:+381111234567",
        ["Osirion:Site:FooterSections:0:Links:3:Text"] = "Top",
        ["Osirion:Site:FooterSections:0:Links:3:Href"] = "#top",
        ["Osirion:Site:LegalLinks:0:Text"] = "Privacy",
        ["Osirion:Site:LegalLinks:0:Href"] = "/privacy",
        ["Osirion:Site:OwnerUrl"] = "/about",
        ["Osirion:Site:SocialLinks:0:Network"] = "github",
        ["Osirion:Site:SocialLinks:0:Href"] = "https://github.com/example",
    };

    private static readonly Dictionary<string, string?> Localization = new()
    {
        ["Osirion:Localization:DefaultCulture"] = "en",
        ["Osirion:Localization:Cultures:0:Name"] = "sr",
        ["Osirion:Localization:Cultures:1:Name"] = "de",
    };

    [Fact]
    public void ForCulture_LocalizationRegistered_PutsEverySiteAddressUnderThePrefix()
    {
        var site = Version("de", Localization);

        site.HomeUrl.ShouldBe("/de");
        site.SearchUrl.ShouldBe("/de/search");
        site.Menu[0].Href.ShouldBe("/de/docs");
        site.Menu[1].Items[0].Href.ShouldBe("/de/guides/start");
        site.CallToAction!.Href.ShouldBe("/de/contact");
        site.Announcement!.LinkUrl.ShouldBe("/de/blog/release");
        site.FooterSections[0].Links[0].Href.ShouldBe("/de/about");
        site.LegalLinks[0].Href.ShouldBe("/de/privacy");
        site.OwnerUrl.ShouldBe("/de/about");
    }

    [Fact]
    public void ForCulture_LocalizationRegistered_LeavesOtherAddressesAsTheyAre()
    {
        var site = Version("de", Localization);

        site.Menu[2].Href.ShouldBe("https://blog.example.org/");
        site.FooterSections[0].Links.Skip(1).Select(link => link.Href).ShouldBe(["mailto:office@example.org", "tel:+381111234567", "#top"]);
        site.SocialLinks[0].Href.ShouldBe("https://github.com/example");
    }

    [Fact]
    public void ForCulture_DefaultCulture_KeepsTheAddressesWithoutAPrefix()
    {
        var site = Version("en", Localization);

        site.HomeUrl.ShouldBe("/");
        site.Menu[0].Href.ShouldBe("/docs");
        site.CallToAction!.Href.ShouldBe("/contact");
    }

    [Fact]
    public void ForCulture_EntryWithOnlyTexts_TakesTheSiteAddressesByPosition()
    {
        var values = new Dictionary<string, string?>(Localization)
        {
            ["Osirion:Site:Locales:sr:Menu:0:Text"] = "Dokumentacija",
            ["Osirion:Site:Locales:sr:Menu:1:Text"] = "Vodiči",
            ["Osirion:Site:Locales:sr:Menu:1:Items:0:Text"] = "Početak",
            ["Osirion:Site:Locales:sr:Menu:2:Text"] = "Blog",
            ["Osirion:Site:Locales:sr:CallToAction:Text"] = "Zakažite poziv",
            ["Osirion:Site:Locales:sr:Announcement:Id"] = "launch",
            ["Osirion:Site:Locales:sr:Announcement:Text"] = "Novo izdanje",
            ["Osirion:Site:Locales:sr:Announcement:LinkText"] = "Pročitajte",
            ["Osirion:Site:Locales:sr:FooterSections:0:Title"] = "Kompanija",
            ["Osirion:Site:Locales:sr:FooterSections:0:Links:0:Text"] = "O nama",
            ["Osirion:Site:Locales:sr:LegalLinks:0:Text"] = "Privatnost",
        };

        var site = Version("sr", values);

        site.Menu.Select(item => (item.Text, item.Href)).ShouldBe(
            [("Dokumentacija", "/sr/docs"), ("Vodiči", null), ("Blog", "https://blog.example.org/")]);
        site.Menu[1].Items.ShouldHaveSingleItem().Href.ShouldBe("/sr/guides/start");
        site.Menu[1].Items[0].Text.ShouldBe("Početak");
        site.CallToAction!.Text.ShouldBe("Zakažite poziv");
        site.CallToAction.Href.ShouldBe("/sr/contact");
        site.Announcement!.LinkUrl.ShouldBe("/sr/blog/release");
        site.FooterSections[0].Title.ShouldBe("Kompanija");
        site.FooterSections[0].Links.ShouldHaveSingleItem().Href.ShouldBe("/sr/about");
        site.LegalLinks.ShouldHaveSingleItem().Href.ShouldBe("/sr/privacy");
    }

    [Fact]
    public void ForCulture_AddressWrittenInTheEntry_IsKeptAsWritten()
    {
        var values = new Dictionary<string, string?>(Localization)
        {
            ["Osirion:Site:Locales:sr:HomeUrl"] = "/sr/pocetna",
            ["Osirion:Site:Locales:sr:Menu:0:Text"] = "Dokumentacija",
            ["Osirion:Site:Locales:sr:Menu:0:Href"] = "/dokumentacija",
            ["Osirion:Site:Locales:sr:CallToAction:Text"] = "Pišite",
            ["Osirion:Site:Locales:sr:CallToAction:Href"] = "mailto:office@example.org",
            ["Osirion:Site:Locales:sr:LegalLinks:0:Text"] = "Privatnost",
            ["Osirion:Site:Locales:sr:LegalLinks:0:Href"] = "/privatnost",
        };

        var site = Version("sr", values);

        site.HomeUrl.ShouldBe("/sr/pocetna");
        site.Menu.ShouldHaveSingleItem().Href.ShouldBe("/dokumentacija");
        site.CallToAction!.Href.ShouldBe("mailto:office@example.org");
        site.LegalLinks.ShouldHaveSingleItem().Href.ShouldBe("/privatnost");
    }

    // Guards the behavior without Osirion:Localization: the addresses are used as written.
    [Fact]
    public void ForCulture_WithoutLocalization_UsesTheAddressesAsWritten()
    {
        var site = Version("sr", new Dictionary<string, string?> { ["Osirion:Site:Locales:sr:Name"] = "Primer" });

        site.Name.ShouldBe("Primer");
        site.HomeUrl.ShouldBe("/");
        site.Menu[0].Href.ShouldBe("/docs");
        site.CallToAction!.Href.ShouldBe("/contact");
        site.LegalLinks[0].Href.ShouldBe("/privacy");
    }

    [Fact]
    public void ForCulture_WithoutLocalizationOrEntry_ReturnsTheOptionsThemselves()
    {
        var options = Build([]);

        options.ForCulture(CultureInfo.GetCultureInfo("de")).ShouldBeSameAs(options);
        options.ForCulture(CultureInfo.GetCultureInfo("de"), localePaths: null).ShouldBeSameAs(options);
    }

    [Fact]
    public void Build_EntryMenuItemWithoutAnAddressBeyondTheSiteMenu_FailsNamingTheItem()
    {
        var values = new Dictionary<string, string?>
        {
            ["Osirion:Site:Locales:sr:Menu:0:Text"] = "Dokumentacija",
            ["Osirion:Site:Locales:sr:Menu:1:Text"] = "Vodiči",
            ["Osirion:Site:Locales:sr:Menu:2:Text"] = "Blog",
            ["Osirion:Site:Locales:sr:Menu:3:Text"] = "Extra",
        };

        Should.Throw<OptionsValidationException>(() => Build(values)).Failures
            .ShouldContain("Osirion:Site:Locales:sr:Menu:3 needs an Href or Items.");
    }

    [Fact]
    public void Build_LanguagePickerWithoutLocalization_FailsNamingTheKey()
    {
        var values = new Dictionary<string, string?> { ["Osirion:Site:LanguagePicker"] = "Header" };

        Should.Throw<OptionsValidationException>(() => Build(values)).Failures.ShouldContain(
            "Osirion:Site:LanguagePicker needs the site's cultures: add an Osirion:Localization section, or call UseLocalization.");
    }

    private static OsirionSiteOptions Version(string culture, Dictionary<string, string?> values)
    {
        var provider = Provider(values);
        var options = provider.GetRequiredService<IOptions<OsirionSiteOptions>>().Value;
        return options.ForCulture(CultureInfo.GetCultureInfo(culture), provider.GetService<IOsirionLocalePaths>());
    }

    private static OsirionSiteOptions Build(Dictionary<string, string?> values) =>
        Provider(values).GetRequiredService<IOptions<OsirionSiteOptions>>().Value;

    private static ServiceProvider Provider(Dictionary<string, string?> values)
    {
        var all = new Dictionary<string, string?>(Site);
        foreach (var (key, value) in values)
        {
            all[key] = value;
        }

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(all).Build();
        var services = HostServices.Create(configuration);
        services.AddOsirion(configuration);
        return services.BuildServiceProvider();
    }
}
