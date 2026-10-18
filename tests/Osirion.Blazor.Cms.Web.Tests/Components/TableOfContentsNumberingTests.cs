using System.Text.RegularExpressions;
using Bunit;
using Osirion.Blazor.Cms.Web.Components;
using Osirion.Blazor.Cms.Web.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.Components;

/// <summary>Checks that the table of contents numbers its entries only when <c>ShowNumbers</c> asks for it.</summary>
public partial class TableOfContentsNumberingTests : CmsWebComponentTestBase
{
    private const string Content = "<h2 id=\"one\">One</h2><h3 id=\"one-a\">One A</h3><h2 id=\"two\">Two</h2>";

    // Fails before the change: there was no numbered class, the numbers came from the stylesheet on every list.
    [Fact]
    public void TableOfContents_Default_RendersTheNumberedClass()
    {
        // Act
        var cut = RenderStatic<TableOfContents>(p => p.Add(c => c.Content, Content));

        // Assert
        cut.Find("nav.osirion-table-of-contents").ClassList.ShouldContain("osirion-toc-numbered");
    }

    // Fails before the change: the component had no ShowNumbers parameter.
    [Fact]
    public void TableOfContents_ShowNumbersFalse_LeavesTheNumberedClassOut()
    {
        // Act
        var cut = RenderStatic<TableOfContents>(p => p
            .Add(c => c.Content, Content)
            .Add(c => c.ShowNumbers, false)
            .Add(c => c.Class, "toc"));

        // Assert
        var nav = cut.Find("nav.osirion-table-of-contents");
        nav.ClassList.ShouldNotContain("osirion-toc-numbered");
        nav.ClassList.ShouldContain("toc");
    }

    // Fails before the change: the counters and the "1." and "1.2" markers applied to every table of contents.
    [Fact]
    public void Stylesheet_CounterRules_AreScopedToTheNumberedClass()
    {
        // Arrange
        var css = Squash(TableOfContentsCss());

        // Act
        var counterRules = CounterRule().Matches(css).Select(m => m.Groups["selector"].Value).ToList();

        // Assert
        counterRules.Count.ShouldBeGreaterThanOrEqualTo(6);
        counterRules.ShouldAllBe(selector => selector.StartsWith(".osirion-toc-numbered", StringComparison.Ordinal));
    }

    private static string Squash(string css) => WhitespaceRun().Replace(CssComment().Replace(css, string.Empty), " ");

    private static string TableOfContentsCss()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Osirion.Blazor.slnx")))
            {
                return File.ReadAllText(Path.Combine(
                    directory.FullName, "src", "Osirion.Blazor.Cms.Web", "Components", "Navigation", "TableOfContents.razor.css"));
            }
        }

        throw new InvalidOperationException("Osirion.Blazor.slnx not found above " + AppContext.BaseDirectory);
    }

    // A rule whose body resets, increments or prints a counter; the selector is captured without the leading space.
    [GeneratedRegex(@"(?<=^|\})\s*(?<selector>[^{}]+?)\s*\{[^}]*(?:counter-reset|counter-increment|counter\()[^}]*\}")]
    private static partial Regex CounterRule();

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex CssComment();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRun();
}
