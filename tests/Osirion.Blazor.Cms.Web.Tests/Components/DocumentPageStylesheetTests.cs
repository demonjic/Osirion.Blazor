using System.Text.RegularExpressions;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.Components;

/// <summary>Checks rules in the document page's own stylesheet, which no component test renders.</summary>
public partial class DocumentPageStylesheetTests
{
    // Fails before the fix: a title that is one long word, such as OsirionResponsiveShowcaseSection, did not wrap, so
    // on a 375 pixel screen it pushed the page 41 pixels wider than the screen.
    [Fact]
    public void Title_WrapsAWordLongerThanTheLine()
    {
        // Act
        var title = TitleRule().Match(Squash(DocumentPageCss())).Value;

        // Assert
        title.ShouldNotBeEmpty();
        title.ShouldContain("overflow-wrap:break-word;");
    }

    // Fails before the fix: once TableOfContents rendered nothing for a document without headings, its box kept
    // only the title.
    [Fact]
    public void TableOfContents_WithoutAList_IsHidden()
    {
        // Act
        var css = Squash(DocumentPageCss());

        // Assert
        css.ShouldContain(".osirion-document-content::deep.osirion-document-toc:not(:has(.osirion-table-of-contents)){display:none;}");
    }

    private static string Squash(string css) => WhitespaceRun().Replace(CssComment().Replace(css, string.Empty), string.Empty);

    private static string DocumentPageCss()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Osirion.Blazor.slnx")))
            {
                return File.ReadAllText(Path.Combine(
                    directory.FullName, "src", "Osirion.Blazor.Cms.Web", "Components", "PageLevel", "DocumentPage.razor.css"));
            }
        }

        throw new InvalidOperationException("Osirion.Blazor.slnx not found above " + AppContext.BaseDirectory);
    }

    [GeneratedRegex(@"(?:^|\})\.osirion-document-title\{[^}]*\}")]
    private static partial Regex TitleRule();

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex CssComment();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRun();
}
