using Bunit;
using Osirion.Blazor.Cms.Web.Components;
using Osirion.Blazor.Cms.Web.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.Components;

/// <summary>Checks the accessible name of the table of contents and the attributes it passes to its <c>nav</c>.</summary>
public class TableOfContentsLabelTests : CmsWebComponentTestBase
{
    private const string Content = "<h2 id=\"one\">One</h2><h2 id=\"two\">Two</h2>";

    [Fact]
    public void TableOfContents_WithoutALabel_IsNamedInEnglish()
    {
        var cut = RenderStatic<TableOfContents>(p => p.Add(c => c.Content, Content));

        cut.Find("nav.osirion-table-of-contents").GetAttribute("aria-label").ShouldBe("Table of Contents");
    }

    // Fails before the change: the name was fixed in English, so a page in another language could not name its contents.
    [Fact]
    public void TableOfContents_AriaLabel_NamesTheNav()
    {
        var cut = RenderStatic<TableOfContents>(p => p
            .Add(c => c.Content, Content)
            .Add(c => c.AriaLabel, "Садржај"));

        cut.Find("nav.osirion-table-of-contents").GetAttribute("aria-label").ShouldBe("Садржај");
    }

    // Fails before the change: the attributes the component captured never reached its root element.
    [Fact]
    public void TableOfContents_AdditionalAttributes_ReachTheNav()
    {
        var cut = RenderStatic<TableOfContents>(p => p
            .Add(c => c.Content, Content)
            .AddUnmatched("data-section", "text"));

        cut.Find("nav.osirion-table-of-contents").GetAttribute("data-section").ShouldBe("text");
    }
}
