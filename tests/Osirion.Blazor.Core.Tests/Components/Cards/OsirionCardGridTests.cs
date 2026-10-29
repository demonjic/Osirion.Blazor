using Bunit;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Components.Cards;

public class OsirionCardGridTests : OsirionComponentTestBase
{
    [Fact]
    public void OsirionCardGrid_Defaults_RendersThreeColumnsAroundTheChildContent()
    {
        var cut = RenderStatic<OsirionCardGrid>(p => p
            .AddChildContent("<article class=\"card\">A</article><article class=\"card\">B</article>"));

        var grid = cut.Find("div.osirion-card-grid");
        grid.ClassList.ShouldBe(["osirion-card-grid", "osirion-card-grid-columns-3"]);
        grid.HasAttribute("style").ShouldBeFalse();
        grid.QuerySelectorAll(":scope > article.card").Length.ShouldBe(2);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(4, 4)]
    [InlineData(0, 1)]
    [InlineData(-3, 1)]
    [InlineData(12, 4)]
    public void OsirionCardGrid_Columns_SetsTheColumnClassWithinOneToFour(int columns, int expected)
    {
        var cut = RenderStatic<OsirionCardGrid>(p => p.Add(c => c.Columns, columns));

        cut.Find("div").ClassList.ShouldContain($"osirion-card-grid-columns-{expected}");
    }

    [Fact]
    public void OsirionCardGrid_ClassStyleAndAttributes_GoOnTheGrid()
    {
        var cut = RenderStatic<OsirionCardGrid>(p => p
            .Add(c => c.Class, "extra")
            .Add(c => c.Style, "gap: 0")
            .AddUnmatched("aria-label", "Features"));

        var grid = cut.Find("div.osirion-card-grid");
        grid.ClassList.ShouldContain("extra");
        grid.GetAttribute("style").ShouldBe("gap: 0");
        grid.GetAttribute("aria-label").ShouldBe("Features");
    }
}
