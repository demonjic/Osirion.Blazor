using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Osirion.Blazor.Core.Extensions;
using Osirion.Blazor.Core.Localization;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Localization;

public class OsirionLocalizationOptionsTests
{
    [Fact]
    public void GetErrors_ValidOptions_IsEmpty()
    {
        var options = new OsirionLocalizationOptions();
        LocalePaths.Configure(options);

        options.GetErrors().ShouldBeEmpty();
    }

    [Fact]
    public void GetErrors_DefaultCultureOnly_IsEmpty()
    {
        new OsirionLocalizationOptions { DefaultCulture = "en" }.GetErrors().ShouldBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void GetErrors_MissingDefaultCulture_NamesTheKey(string defaultCulture)
    {
        new OsirionLocalizationOptions { DefaultCulture = defaultCulture }.GetErrors()
            .ShouldBe(["Osirion:Localization:DefaultCulture is required."]);
    }

    [Fact]
    public void GetErrors_UnknownDefaultCulture_NamesTheKey()
    {
        new OsirionLocalizationOptions { DefaultCulture = "xx-not-a-culture" }.GetErrors()
            .ShouldBe(["Osirion:Localization:DefaultCulture 'xx-not-a-culture' is not a known culture name."]);
    }

    [Theory]
    [InlineData("xx-not-a-culture")]
    [InlineData("iv")]
    public void GetErrors_UnknownOrInvariantCultureName_NamesTheKey(string name)
    {
        var options = new OsirionLocalizationOptions { DefaultCulture = "en" };
        options.Cultures.Add(new LocalizationCulture { Name = name });

        options.GetErrors().ShouldBe([$"Osirion:Localization:Cultures:0:Name '{name}' is not a known culture name."]);
    }

    [Fact]
    public void GetErrors_MissingCultureName_NamesTheKey()
    {
        var options = new OsirionLocalizationOptions { DefaultCulture = "en" };
        options.Cultures.Add(new LocalizationCulture());

        options.GetErrors().ShouldBe(["Osirion:Localization:Cultures:0:Name is required."]);
    }

    [Fact]
    public void GetErrors_CultureListedTwiceInAnotherLetterCase_NamesTheSecondEntry()
    {
        var options = new OsirionLocalizationOptions { DefaultCulture = "en" };
        options.Cultures.Add(new LocalizationCulture { Name = "sr-Latn" });
        options.Cultures.Add(new LocalizationCulture { Name = "SR-latn", Prefix = "srl" });

        options.GetErrors().ShouldBe(["Osirion:Localization:Cultures:1:Name 'SR-latn' is listed more than once."]);
    }

    [Fact]
    public void GetErrors_TwoCulturesWithOnePrefix_NamesTheSecondEntry()
    {
        var options = new OsirionLocalizationOptions { DefaultCulture = "en" };
        options.Cultures.Add(new LocalizationCulture { Name = "sr-Latn", Prefix = "sr" });
        options.Cultures.Add(new LocalizationCulture { Name = "sr-Cyrl", Prefix = "SR" });

        options.GetErrors().ShouldBe(["Osirion:Localization:Cultures:1:Prefix 'sr' is the prefix of another culture."]);
    }

    [Fact]
    public void GetErrors_PrefixOfTheImpliedDefaultCulture_IsTaken()
    {
        var options = new OsirionLocalizationOptions { DefaultCulture = "en" };
        options.Cultures.Add(new LocalizationCulture { Name = "en-GB", Prefix = "en" });

        options.GetErrors().ShouldBe(["Osirion:Localization:Cultures:0:Prefix 'en' is the prefix of another culture."]);
    }

    [Theory]
    [InlineData("sr latn")]
    [InlineData("sr--latn")]
    [InlineData("-sr")]
    [InlineData("sr/latn")]
    [InlineData("sr\n")]
    public void GetErrors_PrefixThatIsNotAPathSegment_NamesTheKey(string prefix)
    {
        var options = new OsirionLocalizationOptions { DefaultCulture = "en" };
        options.Cultures.Add(new LocalizationCulture { Name = "sr-Latn", Prefix = prefix });

        options.GetErrors().ShouldHaveSingleItem().ShouldStartWith("Osirion:Localization:Cultures:0:Prefix '");
    }

    [Fact]
    public void GetErrors_UpperCasePrefix_IsAcceptedInLowerCase()
    {
        var options = new OsirionLocalizationOptions { DefaultCulture = "en" };
        options.Cultures.Add(new LocalizationCulture { Name = "sr-Latn", Prefix = "SR" });

        options.GetErrors().ShouldBeEmpty();
    }

    [Fact]
    public void AddOsirionLocalization_InvalidSection_FailsAtStartupNamingTheKey()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Osirion:Localization:Cultures:0:Name"] = "sr" })
            .Build();
        var provider = new ServiceCollection().AddOsirionLocalization(configuration).BuildServiceProvider();

        var exception = Should.Throw<OptionsValidationException>(() => provider.GetRequiredService<IOsirionLocalePaths>());

        exception.Failures.ShouldContain("Osirion:Localization:DefaultCulture is required.");
    }

    [Fact]
    public void AddOsirionLocalization_InvalidOptions_FailTheStartupValidation()
    {
        var provider = new ServiceCollection()
            .AddOsirionLocalization(options => options.DefaultCulture = "xx-not-a-culture")
            .BuildServiceProvider();

        var exception = Should.Throw<OptionsValidationException>(() => provider.GetRequiredService<IStartupValidator>().Validate());

        exception.Failures.ShouldBe(["Osirion:Localization:DefaultCulture 'xx-not-a-culture' is not a known culture name."]);
    }
}
