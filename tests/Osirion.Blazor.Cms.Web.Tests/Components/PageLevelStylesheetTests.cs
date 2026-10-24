using System.Text.RegularExpressions;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.Components;

/// <summary>Checks rules in the page components' own stylesheets.</summary>
public partial class PageLevelStylesheetTests
{
    [Fact]
    public void AboutPage_BodyWidth_ComesFromItsOwnPropertyWithTheOldWidthAsDefault()
    {
        // Act
        var body = AboutBodyRule().Match(Collapse(PageCss("AboutPage.razor.css"))).Value;

        // Assert
        body.ShouldContain("max-width: var(--osirion-about-page-width, var(--osirion-container-lg, 960px));");
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

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex CssComment();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRun();
}
