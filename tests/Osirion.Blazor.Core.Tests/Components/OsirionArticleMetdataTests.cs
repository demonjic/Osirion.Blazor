using System.Globalization;
using Bunit;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Components;

public class OsirionArticleMetdataTests : OsirionComponentTestBase
{
    private static readonly DateTime PublishDate = new(2026, 3, 14, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void ArticleMetadata_NoMetadata_RendersNothing()
    {
        var cut = RenderStatic<OsirionArticleMetdata>(parameters => parameters
            .Add(p => p.Author, "  ")
            .Add(p => p.ReadTime, ""));

        cut.Markup.Trim().ShouldBeEmpty();
    }

    [Fact]
    public void ArticleMetadata_AllValues_RendersAuthorDateAndReadTime()
    {
        var cut = RenderStatic<OsirionArticleMetdata>(parameters => parameters
            .Add(p => p.Author, "Ada")
            .Add(p => p.PublishDate, PublishDate)
            .Add(p => p.DateFormat, "yyyy-MM-dd")
            .Add(p => p.ReadTime, "5"));

        cut.Find(".osirion-article-metadata-author").TextContent.Trim().ShouldBe("Ada");
        cut.Find(".osirion-article-metadata-date").TextContent.Trim().ShouldBe("2026-03-14");
        cut.Find(".osirion-article-metadata-read-time").TextContent.Trim().ShouldBe("5 min");
    }

    [Fact]
    public void ArticleMetadata_DateOnly_UsesShortDatePatternOfCurrentCulture()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
        try
        {
            var cut = RenderStatic<OsirionArticleMetdata>(parameters => parameters
                .Add(p => p.PublishDate, PublishDate));

            cut.Find(".osirion-article-metadata-date").TextContent.Trim().ShouldBe("14.03.2026");
            cut.FindAll(".osirion-article-metadata-author").ShouldBeEmpty();
            cut.FindAll(".osirion-article-metadata-read-time").ShouldBeEmpty();
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    // Fails before the fix: the date always followed the current culture, so a host could not write an English
    // article's date in English on a server running under another culture.
    [Fact]
    public void ArticleMetadata_Culture_WritesTheDateInThatCulture()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
        try
        {
            var cut = RenderStatic<OsirionArticleMetdata>(parameters => parameters
                .Add(p => p.PublishDate, PublishDate)
                .Add(p => p.DateFormat, "MMMM d, yyyy")
                .Add(p => p.Culture, CultureInfo.GetCultureInfo("en-US")));

            cut.Find(".osirion-article-metadata-date").TextContent.Trim().ShouldBe("March 14, 2026");
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    // Fails before the fix: the date was bare text, with no machine-readable form.
    [Fact]
    public void ArticleMetadata_Date_IsATimeElementWithTheIsoDate()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("th-TH");
        try
        {
            var cut = RenderStatic<OsirionArticleMetdata>(parameters => parameters
                .Add(p => p.PublishDate, PublishDate));

            cut.Find(".osirion-article-metadata-date time").GetAttribute("datetime").ShouldBe("2026-03-14");
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Theory]
    [InlineData("Ada", null, ".osirion-article-metadata-author")]
    [InlineData(null, "3", ".osirion-article-metadata-read-time")]
    public void ArticleMetadata_SingleValue_RendersOnlyThatItem(string? author, string? readTime, string expectedSelector)
    {
        var cut = RenderStatic<OsirionArticleMetdata>(parameters => parameters
            .Add(p => p.Author, author)
            .Add(p => p.ReadTime, readTime));

        cut.Find(".osirion-article-metadata").Children.Length.ShouldBe(1);
        cut.Find(expectedSelector).ShouldNotBeNull();
    }

    // Fails before the fix: the author icon carried only osirion-article-meta-icon, which no stylesheet defines, so it
    // lacked the shrink and opacity rules the other two icons get.
    [Fact]
    public void ArticleMetadata_AuthorIcon_CarriesTheStylesheetClassAndTheOldOne()
    {
        var cut = RenderStatic<OsirionArticleMetdata>(parameters => parameters.Add(p => p.Author, "Ada"));

        var icon = cut.Find(".osirion-article-metadata-author svg");
        icon.ClassList.ShouldContain("osirion-article-metadata-icon");
        icon.ClassList.ShouldContain("osirion-article-meta-icon");
    }

    // Fails before the fix: the root rendered as <div class="osirion-article-metadata"> whatever Class, Style, Theme and
    // unmatched attributes were passed.
    [Fact]
    public void ArticleMetadata_ClassStyleThemeAndAttributes_ReachTheRoot()
    {
        var cut = RenderStatic<OsirionArticleMetdata>(parameters => parameters
            .Add(p => p.Author, "Ada")
            .Add(p => p.Class, "extra")
            .Add(p => p.Style, "margin: 0")
            .Add(p => p.Theme, ThemeMode.Dark)
            .AddUnmatched("data-test", "meta"));

        var root = cut.Find(".osirion-article-metadata");
        root.ClassList.ShouldContain("extra");
        root.GetAttribute("style").ShouldBe("margin: 0");
        root.GetAttribute("data-theme").ShouldBe("dark");
        root.GetAttribute("data-test").ShouldBe("meta");
    }
}
