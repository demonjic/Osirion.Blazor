using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Osirion.Blazor.Core.Localization;
using Osirion.Blazor.Extensions;
using Shouldly;

namespace Osirion.Blazor.Tests.Extensions;

public class OsirionLocalizationBuilderTests
{
    [Fact]
    public void AddOsirion_LocalizationSection_RegistersTheLocalePaths()
    {
        var provider = BuildProvider(new()
        {
            ["Osirion:Localization:DefaultCulture"] = "en",
            ["Osirion:Localization:Cultures:0:Name"] = "sr-Latn",
            ["Osirion:Localization:Cultures:0:Prefix"] = "sr",
        });

        var paths = provider.GetRequiredService<IOsirionLocalePaths>();

        paths.Cultures.Select(culture => culture.Prefix).ShouldBe(["en", "sr"]);
        paths.WithoutLocale("/sr/contact").ShouldBe("/contact");
    }

    [Fact]
    public void AddOsirion_InvalidLocalizationSection_FailsNamingTheKey()
    {
        var provider = BuildProvider(new() { ["Osirion:Localization:Cultures:0:Name"] = "sr-Latn" });

        var exception = Should.Throw<OptionsValidationException>(() => provider.GetRequiredService<IOsirionLocalePaths>());

        exception.Failures.ShouldBe(["Osirion:Localization:DefaultCulture is required."]);
    }

    [Fact]
    public void AddOsirion_WithoutLocalizationSection_RegistersNoLocalePaths()
    {
        var provider = BuildProvider(new() { ["Osirion:Site:Name"] = "A site" });

        provider.GetService<IOsirionLocalePaths>().ShouldBeNull();
    }

    [Fact]
    public void UseLocalization_Code_ConfiguresTheCultures()
    {
        var services = new ServiceCollection();

        services.AddOsirion(osirion => osirion.UseLocalization(localization =>
        {
            localization.DefaultCulture = "en";
            localization.Cultures.Add(new LocalizationCulture { Name = "de" });
        }));

        services.BuildServiceProvider().GetRequiredService<IOsirionLocalePaths>()
            .Cultures.Select(culture => culture.Culture.Name).ShouldBe(["en", "de"]);
    }

    private static ServiceProvider BuildProvider(Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        return new ServiceCollection().AddOsirion(configuration).BuildServiceProvider();
    }
}
