using Osirion.Blazor.Cms.Domain.Options;
using Shouldly;

namespace Osirion.Blazor.Cms.Domain.Tests.Options;

public class ContentProviderOptionsDerivedLocalesTests
{
    [Fact]
    public void DerivedLocales_ByDefault_IsEmptyAndLeavesTheLocalesAsTheyAre()
    {
        var options = new FileSystemOptions { SupportedLocales = ["en", "sr"] };

        options.DerivedLocales.ShouldBeEmpty();
        options.GetEffectiveSupportedLocales().ShouldBeSameAs(options.SupportedLocales);
    }

    [Fact]
    public void GetEffectiveSupportedLocales_DerivedLocale_IsAddedAfterTheConfiguredOnes()
    {
        var options = new FileSystemOptions
        {
            SupportedLocales = ["en", "sr"],
            DerivedLocales = [new DerivedLocaleOptions { Locale = "sr-Latn", From = "sr" }],
        };

        options.GetEffectiveSupportedLocales().ShouldBe(["en", "sr", "sr-Latn"]);
        options.SupportedLocales.ShouldBe(["en", "sr"]);
    }

    [Fact]
    public void GetEffectiveSupportedLocales_DerivedLocaleAlreadyListed_IsNotRepeated()
    {
        var options = new FileSystemOptions
        {
            SupportedLocales = ["sr", "sr-latn"],
            DerivedLocales = [new DerivedLocaleOptions { Locale = "sr-Latn", From = "sr" }],
        };

        options.GetEffectiveSupportedLocales().ShouldBe(["sr", "sr-latn"]);
    }

    [Fact]
    public void GetEffectiveSupportedLocales_ListConfiguredEmpty_StaysEmptySoAnyLocaleFolderIsAccepted()
    {
        var options = new FileSystemOptions
        {
            SupportedLocales = [],
            DerivedLocales = [new DerivedLocaleOptions { Locale = "sr-Latn", From = "sr" }],
        };

        options.GetEffectiveSupportedLocales().ShouldBeEmpty();
    }
}
