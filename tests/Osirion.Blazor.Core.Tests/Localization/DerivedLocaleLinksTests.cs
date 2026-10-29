using Microsoft.Extensions.DependencyInjection;
using Osirion.Blazor.Core.Extensions;
using Osirion.Blazor.Core.Localization;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Localization;

public class DerivedLocaleLinksTests
{
    [Theory]
    [InlineData("/sr/contact", "/sr-latn/contact")]
    [InlineData("/SR/contact?tab=2#form", "/sr-latn/contact?tab=2#form")]
    [InlineData("/sr", "/sr-latn")]
    [InlineData("/contact", "/contact")]
    [InlineData("/en/contact", "/en/contact")]
    [InlineData("/sr/feed.xml", "/sr/feed.xml")]
    [InlineData("https://example.org/sr/contact", "https://example.org/sr/contact")]
    [InlineData("mailto:office@example.org", "mailto:office@example.org")]
    [InlineData("#top", "#top")]
    public void Move_WithoutLocalePaths_UsesTheDerivedCultureInLowerCase(string address, string expected)
    {
        DerivedLocaleLinks.Move(address, "sr", "sr-Latn").ShouldBe(expected);
    }

    [Theory]
    [InlineData("/sr/contact", "/sr-lat/contact")]
    [InlineData("/SR/contact?tab=2", "/sr-lat/contact?tab=2")]
    [InlineData("/contact", "/contact")]
    [InlineData("/hr/kontakt", "/hr/kontakt")]
    [InlineData("/sr/logo.png", "/sr/logo.png")]
    public void Move_WithLocalePaths_UsesTheConfiguredPrefix(string address, string expected)
    {
        var paths = LocalePaths("en", ("sr", null), ("sr-Latn", "sr-lat"), ("hr", null));

        DerivedLocaleLinks.Move(address, "sr", "sr-Latn", paths).ShouldBe(expected);
    }

    [Theory]
    [InlineData("/contact", "/sr-latn/contact")]
    [InlineData("/", "/sr-latn")]
    [InlineData("/en/contact", "/en/contact")]
    public void Move_SourceIsTheDefaultCulture_MovesPathsWithoutAPrefix(string address, string expected)
    {
        var paths = LocalePaths("sr", ("sr-Latn", null), ("en", null));

        DerivedLocaleLinks.Move(address, "sr", "sr-Latn", paths).ShouldBe(expected);
    }

    [Fact]
    public void Move_DerivedCultureNotAmongTheLocalePaths_FallsBackToTheCultureName()
    {
        var paths = LocalePaths("en", ("sr", null));

        DerivedLocaleLinks.Move("/sr/contact", "sr", "sr-Latn", paths).ShouldBe("/sr-latn/contact");
    }

    private static IOsirionLocalePaths LocalePaths(string defaultCulture, params (string Name, string? Prefix)[] cultures)
    {
        var services = new ServiceCollection().AddOsirionLocalization(options =>
        {
            options.DefaultCulture = defaultCulture;
            foreach (var (name, prefix) in cultures)
            {
                options.Cultures.Add(new LocalizationCulture { Name = name, Prefix = prefix });
            }
        });

        return services.BuildServiceProvider().GetRequiredService<IOsirionLocalePaths>();
    }
}
