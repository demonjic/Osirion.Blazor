using Osirion.Blazor.Cms.Web.Components.Seo;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.Components.Seo;

public class SeoTextTests
{
    [Fact]
    public void FirstNonBlank_SkipsNullEmptyAndWhiteSpace()
    {
        SeoText.FirstNonBlank(null, "", "  ", "value", "other").ShouldBe("value");
    }

    [Fact]
    public void FirstNonBlank_AllBlank_ReturnsNull()
    {
        SeoText.FirstNonBlank(null, " ").ShouldBeNull();
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(" ", " ")]
    [InlineData("Short title", "Short title")]
    [InlineData("Eleven char", "Eleven char")]
    [InlineData("Twelve chars long", "Twelve...")]
    [InlineData("Supercalifragilistic", "Supercalifr...")]
    public void TruncateTitle_CutsAtTheLastWordBeforeTheLimit(string? title, string? expected)
    {
        SeoText.TruncateTitle(title, 11).ShouldBe(expected);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("Fits in the limit.", "Fits in the limit.")]
    [InlineData("One sentence. Then more words that go past the limit of sixty characters", "One sentence.")]
    [InlineData("Words without any full stop that go on and on and on past the sixty character limit", "Words without any full stop that go on and on and on past...")]
    [InlineData("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx...")]
    public void TruncateDescription_PrefersASentenceEndThenAWordBoundary(string? description, string? expected)
    {
        SeoText.TruncateDescription(description, 60).ShouldBe(expected);
    }

    [Fact]
    public void TruncateDescription_PeriodFarFromTheCut_CutsAtTheWordInstead()
    {
        var description = "Hi. " + string.Join(" ", Enumerable.Repeat("word", 30));

        SeoText.TruncateDescription(description, 60).ShouldBe("Hi. " + string.Join(" ", Enumerable.Repeat("word", 11)) + "...");
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(" ", null)]
    [InlineData("https://cdn.example.com/a.png", "https://cdn.example.com/a.png")]
    [InlineData("HTTP://cdn.example.com/a.png", "HTTP://cdn.example.com/a.png")]
    [InlineData("/images/a.png", "https://site.test/images/a.png")]
    [InlineData("images/a.png", "https://site.test/images/a.png")]
    public void NormalizeImageUrl_MakesRelativeUrlsAbsolute(string? url, string? expected)
    {
        SeoText.NormalizeImageUrl(url, "https://site.test").ShouldBe(expected);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("https://www.example.com/", "example.com")]
    [InlineData("https://WWW.example.com/", "example.com")]
    [InlineData("https://blog.example.com/", "blog.example.com")]
    public void ExtractSiteName_ReturnsTheHostWithoutWww(string url, string expected)
    {
        SeoText.ExtractSiteName(url).ShouldBe(expected);
    }

    // Fails before the fix for a tag with a script: sr-Latn-RS became sr_Latn_RS, which is not an Open Graph locale.
    [Theory]
    [InlineData(null, "en_US")]
    [InlineData("", "en_US")]
    [InlineData("en-US", "en_US")]
    [InlineData("en-us", "en_US")]
    [InlineData("sr-Latn-RS", "sr_RS")]
    [InlineData("sr-latn-rs", "sr_RS")]
    [InlineData("sr_Cyrl_RS", "sr_RS")]
    [InlineData("zh-Hant-TW", "zh_TW")]
    [InlineData("es-419", "es_419")]
    [InlineData("sr-Latn", "sr")]
    [InlineData("zh-yue-HK", "zh_HK")]
    [InlineData("sr-1a", "sr")]
    public void ToOpenGraphLocale_WritesTheLanguageAndTheTerritory(string? locale, string expected)
    {
        SeoText.ToOpenGraphLocale(locale).ShouldBe(expected);
    }

    // Known defect, pinned as current behavior: a locale without a region is written as the bare language, not the language_TERRITORY form Open Graph expects. Update this expectation when it is fixed.
    [Theory]
    [InlineData("de", "de")]
    [InlineData("en", "en")]
    public void ToOpenGraphLocale_LanguageWithoutRegion_WritesTheBareLanguage(string locale, string expected)
    {
        SeoText.ToOpenGraphLocale(locale).ShouldBe(expected);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData(" ", "")]
    [InlineData("my-first_POST", "My First Post")]
    [InlineData("--a--b--", "A B")]
    public void FormatBreadcrumbSegment_TurnsSlugsIntoTitleCase(string segment, string expected)
    {
        SeoText.FormatBreadcrumbSegment(segment).ShouldBe(expected);
    }

    [Theory]
    [InlineData("theme-color", true)]
    [InlineData("dc.creator", true)]
    [InlineData("og:video", true)]
    [InlineData("a", true)]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("1abc", false)]
    [InlineData("has space", false)]
    [InlineData("a\"b", false)]
    [InlineData("a<b", false)]
    [InlineData("a>b", false)]
    [InlineData("naïve", false)]
    public void IsSafeMetaName_AcceptsOnlyPlainNames(string? name, bool expected)
    {
        SeoText.IsSafeMetaName(name).ShouldBe(expected);
    }

    [Fact]
    public void IsSafeMetaName_NameOverSixtyFourCharacters_IsRejected()
    {
        SeoText.IsSafeMetaName(new string('a', 64)).ShouldBeTrue();
        SeoText.IsSafeMetaName(new string('a', 65)).ShouldBeFalse();
    }
}
