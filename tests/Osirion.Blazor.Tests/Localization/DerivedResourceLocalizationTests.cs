using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Osirion.Blazor.Extensions;
using Osirion.Blazor.Tests.Fakes;
using Shouldly;

namespace Osirion.Blazor.Tests.Localization;

/// <summary>The resources the tests read: English, Serbian Cyrillic, and one Serbian Latin value written by hand.</summary>
public sealed class DerivedStrings;

/// <summary>
/// <see cref="DerivedResourceLocalizationExtensions.AddOsirionDerivedResourceLocalization"/> over real resource files.
/// </summary>
public sealed class DerivedResourceLocalizationTests : IDisposable
{
    private static readonly Dictionary<string, string?> Site = new()
    {
        ["Osirion:Site:Name"] = "Example",
        ["Osirion:Site:Locales:sr:Name"] = "Пример",
        ["Osirion:Site:Locales:sr-Latn:DerivedFrom"] = "sr",
        ["Osirion:Site:Locales:sr-Latn:Transliteration"] = "serbian-cyrillic-to-latin",
    };

    private readonly CultureInfo _culture = CultureInfo.CurrentCulture;
    private readonly CultureInfo _uiCulture = CultureInfo.CurrentUICulture;

    public void Dispose()
    {
        CultureInfo.CurrentCulture = _culture;
        CultureInfo.CurrentUICulture = _uiCulture;
    }

    [Fact]
    public void Indexer_DerivedCulture_TransliteratesTheSourceString()
    {
        var localizer = Localizer(Site, derived: true);
        UseCulture("sr-Latn");

        localizer["Greeting"].Value.ShouldBe("Dobro došli");
        localizer["Greeting"].ResourceNotFound.ShouldBeFalse();
    }

    [Fact]
    public void Indexer_DerivedCultureWithItsOwnValue_ReturnsItsOwnValue()
    {
        var localizer = Localizer(Site, derived: true);
        UseCulture("sr-Latn");

        localizer["Brand"].Value.ShouldBe("Primer d.o.o.");
    }

    [Fact]
    public void Indexer_SpecificCultureOfTheDerivedOne_IsDerivedToo()
    {
        var localizer = Localizer(Site, derived: true);
        UseCulture("sr-Latn-RS");

        localizer["Greeting"].Value.ShouldBe("Dobro došli");
        localizer["Brand"].Value.ShouldBe("Primer d.o.o.");
    }

    [Fact]
    public void Indexer_WithArguments_KeepsThePlaceholdersAndFormatsInTheCurrentCulture()
    {
        var localizer = Localizer(Site, derived: true);
        UseCulture("sr-Latn");
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        localizer["Items", 3, 1200].Value.ShouldBe("3 stavki u 1,200 korpi, {primer}");
    }

    [Fact]
    public void Indexer_StringOnlyInTheInvariantResources_IsReturnedAsTheSourceHasIt()
    {
        var localizer = Localizer(Site, derived: true);
        UseCulture("sr-Latn");

        localizer["OnlyEnglish"].Value.ShouldBe("Only in English");
    }

    [Fact]
    public void Indexer_SourceCulture_IsUnchanged()
    {
        var localizer = Localizer(Site, derived: true);
        UseCulture("sr");

        localizer["Greeting"].Value.ShouldBe("Добро дошли");
    }

    [Fact]
    public void Indexer_WithoutTheCall_IsTheResourceLocalizersAnswer()
    {
        var localizer = Localizer(Site, derived: false);
        UseCulture("sr-Latn");

        // The resource localizer falls back to the parent culture, sr, so the Cyrillic string stays
        localizer["Greeting"].Value.ShouldBe("Добро дошли");
    }

    [Fact]
    public void Indexer_NoDerivedEntry_IsTheResourceLocalizersAnswer()
    {
        var site = new Dictionary<string, string?>(Site);
        site.Remove("Osirion:Site:Locales:sr-Latn:DerivedFrom");
        site.Remove("Osirion:Site:Locales:sr-Latn:Transliteration");
        var localizer = Localizer(site, derived: true);
        UseCulture("sr-Latn");

        localizer["Greeting"].Value.ShouldBe("Добро дошли");
    }

    [Fact]
    public void GetAllStrings_DerivedCulture_HoldsOwnValuesAndTransliteratedSourceStrings()
    {
        var localizer = Localizer(Site, derived: true);
        UseCulture("sr-Latn");

        var strings = localizer.GetAllStrings(includeParentCultures: true).ToDictionary(value => value.Name, value => value.Value);

        strings["Brand"].ShouldBe("Primer d.o.o.");
        strings["Greeting"].ShouldBe("Dobro došli");
        strings["OnlyEnglish"].ShouldBe("Only in English");
    }

    [Fact]
    public void AddOsirionDerivedResourceLocalization_CalledTwice_WrapsTheFactoryOnce()
    {
        var services = Services(Site);
        services.AddOsirionDerivedResourceLocalization();
        services.AddOsirionDerivedResourceLocalization();

        services.Count(descriptor => descriptor.ServiceType == typeof(IStringLocalizerFactory)).ShouldBe(1);
        var localizer = services.BuildServiceProvider().GetRequiredService<IStringLocalizer<DerivedStrings>>();
        UseCulture("sr-Latn");
        localizer["Greeting"].Value.ShouldBe("Dobro došli");
    }

    private static IStringLocalizer<DerivedStrings> Localizer(Dictionary<string, string?> site, bool derived)
    {
        var services = Services(site);
        if (derived)
        {
            services.AddOsirionDerivedResourceLocalization();
        }

        return services.BuildServiceProvider().GetRequiredService<IStringLocalizer<DerivedStrings>>();
    }

    private static IServiceCollection Services(Dictionary<string, string?> site)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(site).Build();
        var services = HostServices.Create(configuration);
        services.AddOsirion(configuration);
        services.AddLocalization();
        return services;
    }

    private static void UseCulture(string name) => CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(name);
}
