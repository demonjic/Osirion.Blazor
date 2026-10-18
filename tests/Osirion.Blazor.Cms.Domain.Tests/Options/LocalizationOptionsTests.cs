using Microsoft.Extensions.Configuration;
using Osirion.Blazor.Cms.Domain.Options.Configuration;
using Shouldly;

namespace Osirion.Blazor.Cms.Domain.Tests.Options;

public class LocalizationOptionsTests
{
    private static IConfigurationSection Section(params string[] cultures) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(cultures.Select((culture, index) =>
                KeyValuePair.Create($"Localization:SupportedCultures:{index}", (string?)culture)))
            .Build()
            .GetSection("Localization");

    [Fact]
    public void Bind_ConfiguredCultures_ReplaceTheDefault()
    {
        // Binding appends to a list that already has items, so the old non-empty default turned ["sr-Latn-RS"] into
        // en-US, sr-Latn-RS.
        var options = new LocalizationOptions();

        Section("sr-Latn-RS").Bind(options);

        options.SupportedCultures.ShouldBe(["sr-Latn-RS"]);
        options.GetEffectiveSupportedCultures().ShouldBe(["sr-Latn-RS"]);
    }

    [Fact]
    public void Bind_ConfiguredCulturesWithTheDefault_AreNotDuplicated()
    {
        var options = new LocalizationOptions();

        Section("en-US", "fr-FR").Bind(options);

        options.SupportedCultures.ShouldBe(["en-US", "fr-FR"]);
    }

    [Fact]
    public void GetEffectiveSupportedCultures_NothingConfigured_IsAmericanEnglish()
    {
        new LocalizationOptions().GetEffectiveSupportedCultures().ShouldBe(["en-US"]);
    }

    [Fact]
    public void GetEffectiveSupportedCultures_AssignedEmptyList_StaysEmpty()
    {
        new LocalizationOptions { SupportedCultures = [] }.GetEffectiveSupportedCultures().ShouldBeEmpty();
    }

    [Fact]
    public void DefaultProperties_HaveExpectedValues()
    {
        // Arrange
        var options = new LocalizationOptions();

        // Assert
        options.SupportedCultures.ShouldNotBeNull();
        options.SupportedCultures.ShouldBeEmpty();
        options.GetEffectiveSupportedCultures().ShouldBe(["en-US"]);
        options.DefaultCulture.ShouldBe("en-US");
        options.EnableCultureFallback.ShouldBeTrue();
        options.ShowCultureSelector.ShouldBeTrue();
        options.CultureDisplayFormat.ShouldBe("NativeName");
        options.RequireCulturePrefix.ShouldBeFalse();
    }

    [Fact]
    public void Properties_CanBeSet_AndRetrieved()
    {
        // Arrange
        var options = new LocalizationOptions
        {
            DefaultCulture = "fr-FR",
            EnableCultureFallback = false,
            ShowCultureSelector = false,
            CultureDisplayFormat = "EnglishName",
            RequireCulturePrefix = true,
            SupportedCultures = new List<string> { "en-US", "fr-FR", "de-DE" }
        };

        // Assert
        options.DefaultCulture.ShouldBe("fr-FR");
        options.EnableCultureFallback.ShouldBeFalse();
        options.ShowCultureSelector.ShouldBeFalse();
        options.CultureDisplayFormat.ShouldBe("EnglishName");
        options.RequireCulturePrefix.ShouldBeTrue();
        options.SupportedCultures.Count.ShouldBe(3);
        options.SupportedCultures.ShouldContain("en-US");
        options.SupportedCultures.ShouldContain("fr-FR");
        options.SupportedCultures.ShouldContain("de-DE");
    }

    [Fact]
    public void AddSupportedCultures_AddsCulturesToTheList()
    {
        // Arrange
        var options = new LocalizationOptions();

        // Act
        options.AddSupportedCultures("fr-FR", "de-DE");

        // Assert
        options.SupportedCultures.Count.ShouldBe(3); // Including default en-US
        options.SupportedCultures.ShouldContain("en-US");
        options.SupportedCultures.ShouldContain("fr-FR");
        options.SupportedCultures.ShouldContain("de-DE");
    }

    [Fact]
    public void AddSupportedCultures_SkipsDuplicates()
    {
        // Arrange
        var options = new LocalizationOptions();

        // Act
        options.AddSupportedCultures("en-US", "en-US", "fr-FR");

        // Assert
        options.SupportedCultures.Count.ShouldBe(2);
        options.SupportedCultures.ShouldContain("en-US");
        options.SupportedCultures.ShouldContain("fr-FR");
    }

    [Fact]
    public void SetDefaultCulture_UpdatesDefaultCulture()
    {
        // Arrange
        var options = new LocalizationOptions();

        // Act
        options.SetDefaultCulture("fr-FR");

        // Assert
        options.DefaultCulture.ShouldBe("fr-FR");
    }

    [Fact]
    public void SetDefaultCulture_AddsToSupportedCulturesIfNotPresent()
    {
        // Arrange
        var options = new LocalizationOptions
        {
            SupportedCultures = new List<string> { "en-US" }
        };

        // Act
        options.SetDefaultCulture("fr-FR");

        // Assert
        options.DefaultCulture.ShouldBe("fr-FR");
        options.SupportedCultures.Count.ShouldBe(2);
        options.SupportedCultures.ShouldContain("en-US");
        options.SupportedCultures.ShouldContain("fr-FR");
    }

    [Fact]
    public void SetDefaultCulture_DoesNotAddDuplicatesToSupportedCultures()
    {
        // Arrange
        var options = new LocalizationOptions
        {
            SupportedCultures = new List<string> { "en-US", "fr-FR" }
        };

        // Act
        options.SetDefaultCulture("fr-FR");

        // Assert
        options.DefaultCulture.ShouldBe("fr-FR");
        options.SupportedCultures.Count.ShouldBe(2);
        options.SupportedCultures.ShouldContain("en-US");
        options.SupportedCultures.ShouldContain("fr-FR");
    }

    [Fact]
    public void SetDefaultCulture_AlwaysAllowsEnUS()
    {
        // Arrange
        var options = new LocalizationOptions
        {
            SupportedCultures = new List<string> { "fr-FR" }
        };

        // Act
        options.SetDefaultCulture("en-US");

        // Assert
        options.DefaultCulture.ShouldBe("en-US");
        options.SupportedCultures.Count.ShouldBe(2);
        options.SupportedCultures.ShouldContain("en-US");
        options.SupportedCultures.ShouldContain("fr-FR");
    }
}