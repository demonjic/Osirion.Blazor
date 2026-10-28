using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Osirion.Blazor.Core.Extensions;
using Osirion.Blazor.Core.Localization;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Localization;

public class OsirionLocalePathsTests
{
    private static readonly CultureInfo Serbian = CultureInfo.GetCultureInfo("sr-Latn");
    private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en");

    private readonly IOsirionLocalePaths _paths = LocalePaths.Create();

    [Fact]
    public void Cultures_AreResolvedInTheConfiguredOrder()
    {
        _paths.Cultures.Select(culture => (culture.Culture.Name, culture.Prefix, culture.IsDefault)).ShouldBe(
        [
            ("en", "en", true),
            ("sr-Latn", "sr", false),
            ("de", "de", false),
        ]);
        _paths.DefaultCulture.ShouldBeSameAs(_paths.Cultures[0]);
    }

    [Fact]
    public void Cultures_DisplayNameDefaultsToTheCapitalizedNativeName()
    {
        _paths.Cultures[0].DisplayName.ShouldBe("English");
        _paths.Cultures[1].DisplayName.ShouldStartWith("Srpski", Case.Sensitive);
        _paths.Cultures[2].DisplayName.ShouldBe("Deutsch");
    }

    [Fact]
    public void Cultures_DefaultCultureNotListed_IsAddedFirst()
    {
        var paths = Create(options =>
        {
            options.DefaultCulture = "en";
            options.Cultures.Add(new LocalizationCulture { Name = "sr-Latn" });
        });

        paths.Cultures.Select(culture => culture.Culture.Name).ShouldBe(["en", "sr-Latn"]);
        paths.DefaultCulture.IsDefault.ShouldBeTrue();
        paths.DefaultCulture.DisplayName.ShouldBe("English");
    }

    [Fact]
    public void Cultures_PrefixDefaultsToTheNameInLowerCase()
    {
        var paths = Create(options =>
        {
            options.DefaultCulture = "en";
            options.Cultures.Add(new LocalizationCulture { Name = "sr-Latn" });
        });

        paths.Cultures[1].Prefix.ShouldBe("sr-latn");
    }

    [Theory]
    [InlineData("/sr/contact", "sr-Latn", "/contact")]
    [InlineData("/SR/contact", "sr-Latn", "/contact")]
    [InlineData("/sr", "sr-Latn", "/")]
    [InlineData("/sr/", "sr-Latn", "/")]
    [InlineData("/en/contact", "en", "/contact")]
    [InlineData("/sr//example.com", "sr-Latn", "/example.com")]
    [InlineData("/sr/\\example.com", "sr-Latn", "/example.com")]
    public void TryGetCulture_PrefixedPath_GivesTheCultureAndTheRest(string path, string culture, string remainder)
    {
        _paths.TryGetCulture(path, out var found, out var rest).ShouldBeTrue();

        found.Culture.Name.ShouldBe(culture);
        rest.ShouldBe(remainder);
    }

    [Theory]
    [InlineData("/contact")]
    [InlineData("/srpski/contact")]
    [InlineData("/")]
    [InlineData("sr/contact")]
    [InlineData("")]
    [InlineData(null)]
    public void TryGetCulture_PathWithoutPrefix_FindsNone(string? path)
    {
        _paths.TryGetCulture(path, out var found, out _).ShouldBeFalse();

        found.ShouldBeNull();
    }

    [Theory]
    [InlineData("/contact", "/sr/contact")]
    [InlineData("/", "/sr")]
    [InlineData("/de/contact", "/sr/contact")]
    [InlineData("/en/contact", "/sr/contact")]
    [InlineData("/contact?tab=2#form", "/sr/contact?tab=2#form")]
    [InlineData("/de?tab=2", "/sr?tab=2")]
    public void Localize_ToAnotherCulture_PutsItsPrefixFirst(string sitePath, string expected)
    {
        _paths.Localize(sitePath, Serbian).ShouldBe(expected);
    }

    [Theory]
    [InlineData("/contact", "/contact")]
    [InlineData("/sr/contact?tab=2", "/contact?tab=2")]
    [InlineData("/sr", "/")]
    public void Localize_ToTheDefaultCulture_LeavesNoPrefix(string sitePath, string expected)
    {
        _paths.Localize(sitePath, English).ShouldBe(expected);
    }

    [Theory]
    [InlineData("https://example.com/contact")]
    [InlineData("//example.com/contact")]
    [InlineData("/\\example.com")]
    [InlineData("mailto:office@example.com")]
    [InlineData("tel:+381111234567")]
    [InlineData("contact")]
    [InlineData("#form")]
    [InlineData("")]
    public void Localize_NotASitePath_IsUnchanged(string url)
    {
        _paths.Localize(url, Serbian).ShouldBe(url);
    }

    [Theory]
    [InlineData("sr-Latn-RS")]
    [InlineData("sr-Latn")]
    public void Localize_SpecificCulture_FallsBackThroughItsParents(string name)
    {
        _paths.Localize("/contact", CultureInfo.GetCultureInfo(name)).ShouldBe("/sr/contact");
    }

    [Fact]
    public void Localize_ParentOfAConfiguredCulture_IsMatchedOnlyThroughItsOwnParents()
    {
        // sr is the parent of sr-Latn, not the other way round, so it falls back to the default culture.
        _paths.Localize("/contact", CultureInfo.GetCultureInfo("sr")).ShouldBe("/contact");
    }

    [Fact]
    public void Localize_CultureTheSiteDoesNotHave_GivesTheDefaultCulture()
    {
        _paths.Localize("/sr/contact", CultureInfo.GetCultureInfo("fr-FR")).ShouldBe("/contact");
    }

    [Theory]
    [InlineData("/sr/contact?tab=2", "/contact?tab=2")]
    [InlineData("/SR/contact", "/contact")]
    [InlineData("/sr", "/")]
    [InlineData("/contact", "/contact")]
    [InlineData("/sr//example.com", "/example.com")]
    [InlineData("https://example.com/sr/contact", "https://example.com/sr/contact")]
    public void WithoutLocale_RemovesAPrefix(string sitePath, string expected)
    {
        _paths.WithoutLocale(sitePath).ShouldBe(expected);
    }

    [Fact]
    public void LocalizeForCurrentCulture_UsesTheCurrentUiCulture()
    {
        var previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("sr-Latn-RS");

            _paths.LocalizeForCurrentCulture("/contact").ShouldBe("/sr/contact");
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    private static IOsirionLocalePaths Create(Action<OsirionLocalizationOptions> configure) =>
        new ServiceCollection().AddOsirionLocalization(configure).BuildServiceProvider().GetRequiredService<IOsirionLocalePaths>();
}
