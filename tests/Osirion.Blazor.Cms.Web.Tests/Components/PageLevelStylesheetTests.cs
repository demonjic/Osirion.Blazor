using System.Text.RegularExpressions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Services;
using Osirion.Blazor.Cms.Web.Components;
using Osirion.Blazor.Cms.Web.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.Components;

/// <summary>
/// Checks rules in the page components' own stylesheets. The rules that hide parts of <c>ArticlePage</c> are read
/// from the stylesheet and run as selectors against the rendered page, so the test fails when either the rule or
/// the markup it relies on changes.
/// </summary>
public partial class PageLevelStylesheetTests : CmsWebComponentTestBase
{
    private const string SmallScreenMedia = "@media (max-width: 640px) {";

    public PageLevelStylesheetTests()
    {
        Services.AddSingleton(Substitute.For<IContentProviderManager>());
        Services.AddSingleton<NavigationManager>(new RecordingNavigationManager());
        Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new Osirion.Blazor.Cms.Web.Options.SeoMetadataOptions()));
    }

    [Fact]
    public void AboutPage_BodyWidth_ComesFromItsOwnPropertyWithTheOldWidthAsDefault()
    {
        // Act
        var body = AboutBodyRule().Match(Collapse(PageCss("AboutPage.razor.css"))).Value;

        // Assert
        body.ShouldContain("max-width: var(--osirion-about-page-width, var(--osirion-container-lg, 960px));");
    }

    // Fails before the fix: below 640 pixels the table of contents was hidden, but its empty sidebar kept its padding
    // and the layout's gap under the article.
    [Fact]
    public void ArticlePage_SmallScreenWithOnlyTheTableOfContents_HidesTheSidebar()
    {
        // Arrange
        var hide = HidingSelectors(SmallScreenCss()).ShouldHaveSingleItem();

        // Act
        var cut = RenderArticle(Article("<h2 id=\"one\">One</h2><p>Body</p>"));

        // Assert
        cut.FindAll(".osirion-article-layout " + hide).ShouldHaveSingleItem().TagName.ShouldBe("ASIDE");
    }

    [Fact]
    public void ArticlePage_SmallScreenWithTags_KeepsTheSidebar()
    {
        // Arrange
        var hide = HidingSelectors(SmallScreenCss()).ShouldHaveSingleItem();
        var article = Article("<h2 id=\"one\">One</h2><p>Body</p>");
        article.AddTag("blazor");

        // Act
        var cut = RenderArticle(article);

        // Assert
        cut.FindAll(".osirion-article-layout " + hide).ShouldBeEmpty();
        cut.FindAll(".osirion-sticky-sidebar").ShouldHaveSingleItem();
    }

    [Fact]
    public void ArticlePage_ArticleWithoutHeadings_HidesTheTableOfContentsSection()
    {
        // Arrange
        var hide = HidingSelectors(OutsideSmallScreenCss()).ShouldHaveSingleItem();

        // Act
        var cut = RenderArticle(Article("<p>Body</p>"));

        // Assert
        cut.FindAll(".osirion-article-layout " + hide).ShouldHaveSingleItem().ClassList.ShouldContain("osirion-toc");
    }

    [Fact]
    public void ArticlePage_ArticleWithHeadings_KeepsTheTableOfContentsSection()
    {
        // Arrange
        var hide = HidingSelectors(OutsideSmallScreenCss()).ShouldHaveSingleItem();

        // Act
        var cut = RenderArticle(Article("<h2 id=\"one\">One</h2><p>Body</p>"));

        // Assert
        cut.FindAll(".osirion-article-layout " + hide).ShouldBeEmpty();
        cut.FindAll(".osirion-toc .osirion-table-of-contents").ShouldHaveSingleItem();
    }

    private IRenderedComponent<ArticlePage> RenderArticle(ContentItem article) =>
        RenderStatic<ArticlePage>(p => p
            .Add(c => c.Item, article)
            .Add(c => c.ShowRelatedContent, false));

    private static ContentItem Article(string content)
    {
        var item = ContentItem.Create("id-article", "Article", content, "blog/article.md", "test");
        item.SetUrl("/blog/article");
        return item;
    }

    /// <summary>The selectors after <c>::deep</c> of the rules that hide part of the article layout.</summary>
    private static List<string> HidingSelectors(string css) =>
        HidingRule().Matches(css).Select(match => match.Groups["selector"].Value).ToList();

    private static string SmallScreenCss()
    {
        var css = Collapse(PageCss("ArticlePage.razor.css"));
        var start = css.IndexOf(SmallScreenMedia, StringComparison.Ordinal);
        start.ShouldBeGreaterThanOrEqualTo(0);
        return css[start..MediaBlockEnd(css, start)];
    }

    private static string OutsideSmallScreenCss()
    {
        var css = Collapse(PageCss("ArticlePage.razor.css"));
        var start = css.IndexOf(SmallScreenMedia, StringComparison.Ordinal);
        start.ShouldBeGreaterThanOrEqualTo(0);
        return css[..start] + css[MediaBlockEnd(css, start)..];
    }

    private static int MediaBlockEnd(string css, int start)
    {
        var depth = 0;
        for (var index = css.IndexOf('{', start); index < css.Length; index++)
        {
            depth += css[index] switch { '{' => 1, '}' => -1, _ => 0 };
            if (depth == 0)
            {
                return index + 1;
            }
        }

        throw new InvalidOperationException("The media block is not closed.");
    }

    private static string Collapse(string css) => WhitespaceRun().Replace(CssComment().Replace(css, string.Empty), " ");

    private static string PageCss(string file)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Osirion.Blazor.slnx")))
            {
                return File.ReadAllText(Path.Combine(
                    directory.FullName, "src", "Osirion.Blazor.Cms.Web", "Components", "PageLevel", file));
            }
        }

        throw new InvalidOperationException("Osirion.Blazor.slnx not found above " + AppContext.BaseDirectory);
    }

    [GeneratedRegex(@"(?:^|\} )[^{}]*\.osirion-about-page-body[^{}]*\{[^}]*\}")]
    private static partial Regex AboutBodyRule();

    [GeneratedRegex(@"\.osirion-article-layout ::deep (?<selector>[^{]+?) \{ display: none; \}")]
    private static partial Regex HidingRule();

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex CssComment();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRun();
}
