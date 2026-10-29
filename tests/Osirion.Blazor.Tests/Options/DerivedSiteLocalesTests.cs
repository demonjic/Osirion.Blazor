using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Osirion.Blazor.Extensions;
using Osirion.Blazor.Options;
using Osirion.Blazor.Tests.Fakes;
using Shouldly;

namespace Osirion.Blazor.Tests.Options;

/// <summary>
/// A <c>Osirion:Site:Locales</c> entry with <c>DerivedFrom</c>: the source culture's version, transliterated.
/// </summary>
public class DerivedSiteLocalesTests
{
    private static readonly Dictionary<string, string?> Site = new()
    {
        ["Osirion:Site:Name"] = "Example",
        ["Osirion:Site:Menu:0:Text"] = "Docs",
        ["Osirion:Site:Menu:0:Href"] = "/docs",
        ["Osirion:Site:Locales:sr:Name"] = "Пример",
        ["Osirion:Site:Locales:sr:HomeUrl"] = "/sr",
        ["Osirion:Site:Locales:sr:Menu:0:Text"] = "Документација",
        ["Osirion:Site:Locales:sr:Menu:0:Href"] = "/sr/docs",
        ["Osirion:Site:Locales:sr:Menu:0:Description"] = "Водичи и примери",
        ["Osirion:Site:Locales:sr:Menu:1:Text"] = "Блог",
        ["Osirion:Site:Locales:sr:Menu:1:Href"] = "https://blog.example.org/sr/",
        ["Osirion:Site:Locales:sr:CallToAction:Text"] = "Закажите позив",
        ["Osirion:Site:Locales:sr:CallToAction:Href"] = "/sr/kontakt",
        ["Osirion:Site:Locales:sr:CallToAction:FooterSection"] = "Контакт",
        ["Osirion:Site:Locales:sr:Announcement:Id"] = "launch",
        ["Osirion:Site:Locales:sr:Announcement:Text"] = "Ново издање",
        ["Osirion:Site:Locales:sr:Announcement:LinkText"] = "Прочитајте",
        ["Osirion:Site:Locales:sr:Announcement:LinkUrl"] = "/sr/blog/izdanje",
        ["Osirion:Site:Locales:sr:FooterDescription"] = "Компанија за софтвер",
        ["Osirion:Site:Locales:sr:FooterSections:0:Title"] = "Контакт",
        ["Osirion:Site:Locales:sr:FooterSections:0:Links:0:Text"] = "Пишите нам",
        ["Osirion:Site:Locales:sr:FooterSections:0:Links:0:Href"] = "mailto:office@example.org",
        ["Osirion:Site:Locales:sr:LegalLinks:0:Text"] = "Приватност",
        ["Osirion:Site:Locales:sr:LegalLinks:0:Href"] = "/sr/privatnost",
        ["Osirion:Site:Locales:sr:Copyright"] = "© 2026 <a href=\"/sr\">Пример</a>",
        ["Osirion:Site:Locales:sr:Labels:SkipLink"] = "Пређи на садржај",
        ["Osirion:Site:Locales:sr-Latn:DerivedFrom"] = "sr",
        ["Osirion:Site:Locales:sr-Latn:Transliteration"] = "serbian-cyrillic-to-latin",
    };

    [Fact]
    public void ForCulture_DerivedVersion_TransliteratesEveryText()
    {
        var latin = Build(Site).ForCulture(CultureInfo.GetCultureInfo("sr-Latn"));

        latin.Name.ShouldBe("Primer");
        latin.Menu[0].Text.ShouldBe("Dokumentacija");
        latin.Menu[0].Description.ShouldBe("Vodiči i primeri");
        latin.CallToAction!.Text.ShouldBe("Zakažite poziv");
        latin.CallToAction.FooterSection.ShouldBe("Kontakt");
        latin.Announcement!.Text.ShouldBe("Novo izdanje");
        latin.Announcement.LinkText.ShouldBe("Pročitajte");
        latin.Announcement.Id.ShouldBe("launch");
        latin.FooterDescription.ShouldBe("Kompanija za softver");
        latin.FooterSections[0].Title.ShouldBe("Kontakt");
        latin.FooterSections[0].Links[0].Text.ShouldBe("Pišite nam");
        latin.LegalLinks[0].Text.ShouldBe("Privatnost");
        latin.Labels.SkipLink.ShouldBe("Pređi na sadržaj");
        latin.Copyright.ShouldBe("© 2026 <a href=\"/sr-latn\">Primer</a>");
    }

    // A new label must be transliterated too, or a derived version shows it in the source script.
    [Fact]
    public void ForCulture_DerivedVersion_TransliteratesEveryLabel()
    {
        var cookieConsent = new SiteCookieConsentLabels
        {
            Categories = { ["analytics"] = new SiteCookieCategoryLabels { Name = "Аналитика", Description = "Опис" } },
        };
        var labels = new SiteLabels { CookieConsent = cookieConsent };
        foreach (var property in TextProperties<SiteLabels>())
        {
            property.SetValue(labels, "Ознака " + property.Name);
        }

        foreach (var property in TextProperties<SiteCookieConsentLabels>())
        {
            property.SetValue(cookieConsent, "Ознака " + property.Name);
        }

        var services = HostServices.Create();
        services.AddOsirion(osirion => osirion.UseSite(site =>
        {
            site.Name = "Example";
            site.Locales["sr"] = new SiteLocaleOptions { Labels = labels };
            site.Locales["sr-Latn"] = new SiteLocaleOptions { DerivedFrom = "sr", Transliteration = "serbian-cyrillic-to-latin" };
        }));
        var latin = services.BuildServiceProvider().GetRequiredService<IOptions<OsirionSiteOptions>>().Value
            .ForCulture(CultureInfo.GetCultureInfo("sr-Latn"));

        foreach (var property in TextProperties<SiteLabels>())
        {
            property.GetValue(latin.Labels).ShouldBe("Oznaka " + property.Name, property.Name);
        }

        foreach (var property in TextProperties<SiteCookieConsentLabels>())
        {
            property.GetValue(latin.Labels.CookieConsent).ShouldBe("Oznaka " + property.Name, property.Name);
        }

        latin.Labels.CookieConsent!.Categories["analytics"].Name.ShouldBe("Analitika");
        latin.Labels.CookieConsent.Categories["analytics"].Description.ShouldBe("Opis");

        // Every label is a text or one of the groups checked above, so a new kind of label needs a decision here too.
        typeof(SiteLabels).GetProperties().Where(property => property.PropertyType != typeof(string)).Select(property => property.Name)
            .ShouldBe([nameof(SiteLabels.CookieConsent)]);
    }

    private static IEnumerable<System.Reflection.PropertyInfo> TextProperties<T>() =>
        typeof(T).GetProperties().Where(property => property.PropertyType == typeof(string));

    [Fact]
    public void ForCulture_DerivedVersion_MovesPageLinksAndKeepsOtherAddresses()
    {
        var latin = Build(Site).ForCulture(CultureInfo.GetCultureInfo("sr-Latn"));

        latin.HomeUrl.ShouldBe("/sr-latn");
        latin.Menu[0].Href.ShouldBe("/sr-latn/docs");
        latin.Menu[1].Href.ShouldBe("https://blog.example.org/sr/");
        latin.CallToAction!.Href.ShouldBe("/sr-latn/kontakt");
        latin.Announcement!.LinkUrl.ShouldBe("/sr-latn/blog/izdanje");
        latin.FooterSections[0].Links[0].Href.ShouldBe("mailto:office@example.org");
        latin.LegalLinks[0].Href.ShouldBe("/sr-latn/privatnost");
    }

    [Fact]
    public void ForCulture_DerivedVersion_KeepsTheSourceUnchanged()
    {
        var serbian = Build(Site).ForCulture(CultureInfo.GetCultureInfo("sr"));

        serbian.Name.ShouldBe("Пример");
        serbian.Menu[0].Href.ShouldBe("/sr/docs");
    }

    [Fact]
    public void ForCulture_KeyWrittenInTheDerivedEntry_Wins()
    {
        var site = Build(new Dictionary<string, string?>(Site)
        {
            ["Osirion:Site:Locales:sr-Latn:Name"] = "Primer d.o.o.",
            ["Osirion:Site:Locales:sr-Latn:Menu:0:Text"] = "Uputstva",
            ["Osirion:Site:Locales:sr-Latn:Menu:0:Href"] = "/sr-latn/uputstva",
        });

        var latin = site.ForCulture(CultureInfo.GetCultureInfo("sr-Latn"));

        latin.Name.ShouldBe("Primer d.o.o.");
        latin.Menu.ShouldHaveSingleItem().Text.ShouldBe("Uputstva");
        latin.FooterDescription.ShouldBe("Kompanija za softver");
    }

    [Fact]
    public void ForCulture_LocalizationRegistered_UsesTheConfiguredPrefix()
    {
        var site = Build(new Dictionary<string, string?>(Site)
        {
            ["Osirion:Localization:DefaultCulture"] = "en",
            ["Osirion:Localization:Cultures:0:Name"] = "sr",
            ["Osirion:Localization:Cultures:1:Name"] = "sr-Latn",
            ["Osirion:Localization:Cultures:1:Prefix"] = "lat",
        });

        site.ForCulture(CultureInfo.GetCultureInfo("sr-Latn")).Menu[0].Href.ShouldBe("/lat/docs");
    }

    [Fact]
    public void ForCulture_SourceIsTheDefaultCultureWithoutAnEntry_DerivesFromTheSiteValues()
    {
        var site = Build(new Dictionary<string, string?>
        {
            ["Osirion:Site:Name"] = "Пример",
            ["Osirion:Site:Menu:0:Text"] = "Блог",
            ["Osirion:Site:Menu:0:Href"] = "/blog",
            ["Osirion:Site:Locales:sr-Latn:DerivedFrom"] = "sr",
            ["Osirion:Site:Locales:sr-Latn:Transliteration"] = "serbian-cyrillic-to-latin",
            ["Osirion:Localization:DefaultCulture"] = "sr",
            ["Osirion:Localization:Cultures:0:Name"] = "sr-Latn",
        });

        var latin = site.ForCulture(CultureInfo.GetCultureInfo("sr-Latn"));

        latin.Name.ShouldBe("Primer");
        latin.Menu[0].Href.ShouldBe("/sr-latn/blog");
    }

    [Fact]
    public void Build_NoDerivedEntry_LeavesTheVersionsAsConfigured()
    {
        var values = new Dictionary<string, string?>(Site);
        values.Remove("Osirion:Site:Locales:sr-Latn:DerivedFrom");
        values.Remove("Osirion:Site:Locales:sr-Latn:Transliteration");

        var site = Build(values);

        site.Locales.Keys.ShouldBe(["sr"]);
        site.ForCulture(CultureInfo.GetCultureInfo("sr-Latn")).Name.ShouldBe("Пример");
    }

    [Theory]
    [InlineData("DerivedFrom", "xx-not-a-culture", "Osirion:Site:Locales:sr-Latn:DerivedFrom 'xx-not-a-culture' is not a culture name, such as sr.")]
    [InlineData("DerivedFrom", "sr-Latn", "Osirion:Site:Locales:sr-Latn:DerivedFrom cannot be the entry's own culture.")]
    [InlineData("DerivedFrom", "hr", "Osirion:Site:Locales:sr-Latn:DerivedFrom 'hr' has no entry in Osirion:Site:Locales.")]
    [InlineData("Transliteration", "unknown", "Osirion:Site:Locales:sr-Latn:Transliteration 'unknown' is not a registered transliterator (registered: serbian-cyrillic-to-latin).")]
    [InlineData("Transliteration", "", "Osirion:Site:Locales:sr-Latn:Transliteration is required with DerivedFrom.")]
    public void Build_InvalidDerivedEntry_FailsNamingTheKey(string key, string value, string expected)
    {
        var values = new Dictionary<string, string?>(Site) { [$"Osirion:Site:Locales:sr-Latn:{key}"] = value };

        Should.Throw<OptionsValidationException>(() => Build(values)).Failures.ShouldContain(expected);
    }

    [Fact]
    public void Build_DerivedFromADerivedVersion_FailsAsAChain()
    {
        var values = new Dictionary<string, string?>(Site)
        {
            ["Osirion:Site:Locales:sr-Latn-RS:DerivedFrom"] = "sr-Latn",
            ["Osirion:Site:Locales:sr-Latn-RS:Transliteration"] = "serbian-cyrillic-to-latin",
        };

        Should.Throw<OptionsValidationException>(() => Build(values)).Failures
            .ShouldContain("Osirion:Site:Locales:sr-Latn-RS:DerivedFrom 'sr-Latn' is itself derived; derive from a version written by hand.");
    }

    [Fact]
    public void Build_TwoVersionsDerivedFromEachOther_FailsAsAChain()
    {
        var values = new Dictionary<string, string?>
        {
            ["Osirion:Site:Name"] = "Example",
            ["Osirion:Site:Locales:sr:DerivedFrom"] = "sr-Latn",
            ["Osirion:Site:Locales:sr:Transliteration"] = "serbian-cyrillic-to-latin",
            ["Osirion:Site:Locales:sr-Latn:DerivedFrom"] = "sr",
            ["Osirion:Site:Locales:sr-Latn:Transliteration"] = "serbian-cyrillic-to-latin",
        };

        Should.Throw<OptionsValidationException>(() => Build(values)).Failures.Count(failure => failure.Contains("is itself derived")).ShouldBe(2);
    }

    [Fact]
    public void Build_DerivedCultureWithoutARoute_FailsWhenLocalizationIsRegistered()
    {
        var values = new Dictionary<string, string?>(Site)
        {
            ["Osirion:Localization:DefaultCulture"] = "en",
            ["Osirion:Localization:Cultures:0:Name"] = "sr",
        };

        Should.Throw<OptionsValidationException>(() => Build(values)).Failures
            .ShouldContain("Osirion:Site:Locales:sr-Latn is not a culture of Osirion:Localization, so no address serves the derived version.");
    }

    // The breadcrumbs of the structured data name the home page with the label of the request's version, the derived
    // version included, and fall back to the SEO options (null) when the site sets none.
    [Fact]
    public void BreadcrumbHomeText_ReadsTheLabelOfEachVersion()
    {
        var values = new Dictionary<string, string?>(Site)
        {
            ["Osirion:Site:Locales:sr:Labels:Home"] = "Почетна",
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        var services = HostServices.Create(configuration);
        services.AddOsirion(configuration);

        var homeText = services.BuildServiceProvider().GetRequiredService<Cms.Web.Options.IBreadcrumbHomeText>();

        homeText.For(CultureInfo.GetCultureInfo("sr")).ShouldBe("Почетна");
        homeText.For(CultureInfo.GetCultureInfo("sr-Latn")).ShouldBe("Početna");
        homeText.For(CultureInfo.GetCultureInfo("en")).ShouldBeNull();
    }

    private static OsirionSiteOptions Build(Dictionary<string, string?> values)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        var services = HostServices.Create(configuration);
        services.AddOsirion(configuration);
        return services.BuildServiceProvider().GetRequiredService<IOptions<OsirionSiteOptions>>().Value;
    }
}
