using Osirion.Blazor.Cms.Infrastructure.Utilities;
using Shouldly;

namespace Osirion.Blazor.Cms.Infrastructure.Tests.Utilities;

public class UrlGeneratorTests
{
    [Theory]
    [InlineData("blog/post.md", "post", null, "blog/post")]
    [InlineData(@"\blog\post.md\", "post", null, "blog/post")]
    [InlineData("post.md", "post", null, "post")]
    [InlineData("content/blog/post.md", "post", "content", "blog/post")]
    [InlineData("content", "post", "content", "post")]
    [InlineData("contentx/post.md", "post", "content", "contentx/post")]
    [InlineData("content/blog/post.md", "post", "/", "content/blog/post")]
    [InlineData("content/blog/post.md", "post", "  ", "content/blog/post")]
    [InlineData("content/blog/post.md", "post", "other", "content/blog/post")]
    [InlineData("blog/_index.md", "blog", null, "blog")]
    [InlineData("docs/guides/_INDEX.md", "guides", null, "docs/guides")]
    [InlineData("_index.md", "home", null, "home")]
    public void GenerateUrl_WithoutLocalization_BuildsPathFromDirectoryAndSlug(
        string path, string slug, string? skipSegment, string expected)
    {
        var result = UrlGenerator.GenerateUrl("", path, slug, false, null, skipSegment);

        result.ShouldBe(expected);
    }

    [Fact]
    public void GenerateUrl_WithoutLocalization_KeepsLocaleSegment()
    {
        var result = UrlGenerator.GenerateUrl("", "en/blog/post.md", "post", false, null);

        result.ShouldBe("en/blog/post");
    }

    [Theory]
    [InlineData("en/blog/post.md", "blog/post")]
    [InlineData("en-US/blog/post.md", "blog/post")]
    [InlineData("en/post.md", "post")]
    [InlineData("blog/post.md", "blog/post")]
    public void GenerateUrl_WithLocalization_RemovesLeadingLocale(string path, string expected)
    {
        var result = UrlGenerator.GenerateUrl("", path, "post", true, null);

        result.ShouldBe(expected);
    }

    [Fact]
    public void GenerateUrl_WithSupportedLocales_RemovesOnlySupportedLocale()
    {
        var supported = new List<string> { "de" };

        var unsupported = UrlGenerator.GenerateUrl("", "en/blog/post.md", "post", true, supported);
        var german = UrlGenerator.GenerateUrl("", "de/blog/post.md", "post", true, supported);

        unsupported.ShouldBe("en/blog/post");
        german.ShouldBe("blog/post");
    }

    [Fact]
    public void GenerateUrl_WithSkipSegmentAndLocale_RemovesBoth()
    {
        var result = UrlGenerator.GenerateUrl("", "content/en/blog/post.md", "post", true, null, "content");

        result.ShouldBe("blog/post");
    }

    [Theory]
    [InlineData("Hello, World!", "hello-world")]
    [InlineData("a  --  b", "a-b")]
    [InlineData("-x-", "x")]
    [InlineData("  ", "untitled")]
    [InlineData("", "untitled")]
    public void GenerateSlug_NormalizesText(string text, string expected)
    {
        var result = UrlGenerator.GenerateSlug(text);

        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("a/b/c.md", "a/b")]
    [InlineData("c.md", "")]
    public void GetDirectoryPath_ReturnsPartBeforeLastSlash(string filePath, string expected)
    {
        var result = UrlGenerator.GetDirectoryPath(filePath);

        result.ShouldBe(expected);
    }

    [Fact]
    public void RemoveLocaleFromPath_WhenLocalizationDisabled_ReturnsPathUnchanged()
    {
        var result = UrlGenerator.RemoveLocaleFromPath("content/en/blog", "content", false, null);

        result.ShouldBe("content/en/blog");
    }

    [Theory]
    [InlineData("content/en/blog", "content", "blog")]
    [InlineData("content/en/blog", "/content/", "blog")]
    [InlineData("content", "content", "")]
    [InlineData("contentx/en/blog", "content", "contentx/en/blog")]
    [InlineData("en/blog", "", "blog")]
    [InlineData("blog/post", "", "blog/post")]
    [InlineData("content/blog", "content", "blog")]
    public void RemoveLocaleFromPath_WhenLocalizationEnabled_RemovesContentPathAndLocale(
        string path, string contentPath, string expected)
    {
        var result = UrlGenerator.RemoveLocaleFromPath(path, contentPath, true, null);

        result.ShouldBe(expected);
    }

    [Fact]
    public void ExtractLocaleFromPath_WhenLocalizationDisabled_ReturnsDefaultLocale()
    {
        var result = UrlGenerator.ExtractLocaleFromPath("content/de/blog", "content", false, null, "en");

        result.ShouldBe("en");
    }

    [Theory]
    [InlineData("content/de/blog", "content", "de")]
    [InlineData("content/de-AT/blog", @"\content", "de-AT")]
    [InlineData("content", "content", "en")]
    [InlineData("contentx/de/blog", "content", "en")]
    [InlineData("fr/blog", "", "fr")]
    [InlineData("blog/post", "", "en")]
    public void ExtractLocaleFromPath_WhenLocalizationEnabled_ReturnsLocaleOrDefault(
        string path, string contentPath, string expected)
    {
        var result = UrlGenerator.ExtractLocaleFromPath(path, contentPath, true, null, "en");

        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("en", true)]
    [InlineData("en-US", true)]
    [InlineData("e1", false)]
    [InlineData("english", false)]
    [InlineData("en_US", false)]
    [InlineData("1n-US", false)]
    [InlineData("en-1S", false)]
    public void IsValidLocale_WithoutSupportedLocales_ChecksLanguageCodeShape(string locale, bool expected)
    {
        UrlGenerator.IsValidLocale(locale, null).ShouldBe(expected);
        UrlGenerator.IsValidLocale(locale, []).ShouldBe(expected);
    }

    [Theory]
    [InlineData("en", true)]
    [InlineData("EN", true)]
    [InlineData("de", false)]
    [InlineData("english", true)]
    public void IsValidLocale_WithSupportedLocales_ChecksTheListIgnoringCase(string locale, bool expected)
    {
        var supported = new List<string> { "en", "English" };

        UrlGenerator.IsValidLocale(locale, supported).ShouldBe(expected);
    }

    [Theory]
    [InlineData(@"\a\b\", "a/b")]
    [InlineData("/a/b/", "a/b")]
    [InlineData("a", "a")]
    public void NormalizePath_UsesForwardSlashesWithoutOuterSlashes(string path, string expected)
    {
        UrlGenerator.NormalizePath(path).ShouldBe(expected);
    }

    [Theory]
    [InlineData("/custom/place", "custom/place")]
    [InlineData("custom/place", "custom/place")]
    [InlineData("/custom/place/", "custom/place")]
    [InlineData(@"\custom\place", "custom/place")]
    public void GenerateUrl_WithPermalink_UsesItNormalizedWithoutLocaleFolder(string permalink, string expected)
    {
        // Fails before the fix: the permalink parameter was never read, so the address came from the path and slug.
        var withoutLocalization = UrlGenerator.GenerateUrl(permalink, "blog/post.md", "post", false, null);
        var withLocalization = UrlGenerator.GenerateUrl(permalink, "de/blog/post.md", "post", true, ["de"]);

        withoutLocalization.ShouldBe(expected);
        withLocalization.ShouldBe(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void GenerateUrl_WithBlankPermalink_BuildsTheAddressFromPathAndSlug(string permalink)
    {
        UrlGenerator.GenerateUrl(permalink, "blog/post.md", "post", false, null).ShouldBe("blog/post");
    }

    // permalink: "/" was ignored, so a home page could not be content; it is the site root now.
    [Theory]
    [InlineData("/")]
    [InlineData("//")]
    [InlineData(" / ")]
    public void GenerateUrl_RootPermalink_IsTheSiteRoot(string permalink)
    {
        UrlGenerator.GenerateUrl(permalink, "en/home.md", "home", true, ["en", "sr"]).ShouldBe(string.Empty);
    }

    [Theory]
    [InlineData("misc/my_index.md", "my-index", "misc/my-index")]
    [InlineData("misc/_index.md.bak", "bak", "misc/bak")]
    [InlineData("misc/x_index.md", "x-index", "misc/x-index")]
    [InlineData("_index.md/post.md", "post", "_index.md/post")]
    public void GenerateUrl_FileNameOnlyContainingIndexName_IsNotAFolderIndex(string path, string slug, string expected)
    {
        // Fails before the fix: any path containing "_index.md" was taken for a folder index, so misc/my_index.md got "misc".
        UrlGenerator.GenerateUrl("", path, slug, false, null).ShouldBe(expected);
    }

    [Fact]
    public void GenerateUrl_IndexFileWithPermalink_KeepsTheFolderAddress()
    {
        // The folder's own permalink is applied to the folder by the directory metadata, not to its index file here.
        UrlGenerator.GenerateUrl("/elsewhere", "blog/_index.md", "blog", false, null).ShouldBe("blog");
    }
}
