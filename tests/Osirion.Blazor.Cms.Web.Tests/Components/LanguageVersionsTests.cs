using Microsoft.Extensions.DependencyInjection;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.ValueObjects;
using Osirion.Blazor.Cms.Web.Components.Seo;
using Osirion.Blazor.Core.Extensions;
using Osirion.Blazor.Core.Localization;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.Components;

public class LanguageVersionsTests
{
    private readonly IOsirionLocalePaths _localePaths = new ServiceCollection()
        .AddOsirionLocalization(localization =>
        {
            localization.DefaultCulture = "en";
            localization.Cultures.Add(new LocalizationCulture { Name = "sr-Cyrl", Prefix = "sr" });
            localization.Cultures.Add(new LocalizationCulture { Name = "sr-Latn", Prefix = "lat" });
            localization.Cultures.Add(new LocalizationCulture { Name = "de" });
        })
        .BuildServiceProvider()
        .GetRequiredService<IOsirionLocalePaths>();

    [Theory]
    [InlineData(null, "en")]
    [InlineData("", "en")]
    [InlineData("  ", "en")]
    [InlineData("sr-Latn", "sr-Latn")]
    [InlineData("SR-LATN", "sr-Latn")]
    [InlineData("sr", "sr-Cyrl")]
    [InlineData("lat", "sr-Latn")]
    [InlineData("sr-Latn-RS", "sr-Latn")]
    [InlineData("de-AT", "de")]
    [InlineData("en-GB", "en")]
    public void CultureOf_MatchesByNamePrefixOrParent(string? locale, string expected)
    {
        LanguageVersions.CultureOf(_localePaths, locale)!.Culture.Name.ShouldBe(expected);
    }

    [Theory]
    [InlineData("fr")]
    [InlineData("fr-CA")]
    [InlineData("not-a-culture-name")]
    public void CultureOf_LocaleTheSiteDoesNotHave_IsNull(string locale)
    {
        LanguageVersions.CultureOf(_localePaths, locale).ShouldBeNull();
    }

    [Fact]
    public void ForPage_ListsEveryCultureInOrderThenXDefault()
    {
        var links = LanguageVersions.ForPage(_localePaths, "/contact?page=2", path => "https://example.com" + path);

        links.ShouldBe([
            ("en", "https://example.com/contact?page=2"),
            ("sr-Cyrl", "https://example.com/sr/contact?page=2"),
            ("sr-Latn", "https://example.com/lat/contact?page=2"),
            ("de", "https://example.com/de/contact?page=2"),
            ("x-default", "https://example.com/contact?page=2"),
        ]);
    }

    [Fact]
    public void Ordered_OneVersion_IsEmpty()
    {
        var versions = new Dictionary<OsirionLocaleCulture, string> { [_localePaths.DefaultCulture] = "https://example.com/" };

        LanguageVersions.Ordered(_localePaths, versions).ShouldBeEmpty();
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("index, follow", false)]
    [InlineData("noindex", true)]
    [InlineData("NoIndex, follow", true)]
    public void IsNoIndex_ReadsTheRobotsFrontMatter(string robots, bool expected)
    {
        var item = ContentItem.Create("id", "Title", "<p>Body</p>", "en/post.md", "test");
        item.Metadata = new FrontMatter { SeoProperties = new SeoMetadata { Robots = robots } };

        LanguageVersions.IsNoIndex(item).ShouldBe(expected);
    }

    [Fact]
    public void IsNoIndex_ItemWithoutFrontMatter_IsIndexable()
    {
        LanguageVersions.IsNoIndex(ContentItem.Create("id", "Title", "<p>Body</p>", "en/post.md", "test")).ShouldBeFalse();
    }
}
