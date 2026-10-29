using Bunit;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Models;
using Osirion.Blazor.Core.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Components.Content;

public class OsirionChecklistTests : OsirionComponentTestBase
{
    private static readonly ChecklistItem[] TwoItems =
    [
        new("Works without script", "Every item renders under static SSR."),
        new("Themed by tokens"),
    ];

    [Fact]
    public void OsirionChecklist_TitleAndItems_RendersAListNamedByItsHeading()
    {
        var cut = RenderStatic<OsirionChecklist>(p => p
            .Add(c => c.Title, "Included")
            .Add(c => c.Items, TwoItems));

        var heading = cut.Find("div.osirion-checklist > h3.osirion-checklist-title");
        heading.TextContent.ShouldBe("Included");
        heading.Id.ShouldNotBeNullOrWhiteSpace();
        var list = cut.Find("ul.osirion-checklist-list");
        list.GetAttribute("role").ShouldBe("list");
        list.GetAttribute("aria-labelledby").ShouldBe(heading.Id);

        var items = cut.FindAll("li.osirion-checklist-item");
        items.Count.ShouldBe(2);
        items[0].QuerySelector(".osirion-checklist-item-title")!.TextContent.ShouldBe("Works without script");
        items[0].QuerySelector(".osirion-checklist-item-description")!.TextContent.ShouldBe("Every item renders under static SSR.");
        items[1].QuerySelector(".osirion-checklist-item-title")!.TextContent.ShouldBe("Themed by tokens");
        items[1].QuerySelector(".osirion-checklist-item-description").ShouldBeNull();
    }

    [Fact]
    public void OsirionChecklist_CheckMark_IsHiddenFromAssistiveTechnology()
    {
        var cut = RenderStatic<OsirionChecklist>(p => p.Add(c => c.Items, TwoItems));

        foreach (var mark in cut.FindAll("svg.osirion-checklist-mark"))
        {
            mark.GetAttribute("aria-hidden").ShouldBe("true");
            mark.GetAttribute("focusable").ShouldBe("false");
        }
    }

    [Fact]
    public void OsirionChecklist_ItemsWithoutTitle_RenderNoHeadingAndNoLabel()
    {
        var cut = RenderStatic<OsirionChecklist>(p => p.Add(c => c.Items, TwoItems));

        cut.FindAll(".osirion-checklist-title").ShouldBeEmpty();
        cut.Find("ul").HasAttribute("aria-labelledby").ShouldBeFalse();
    }

    [Fact]
    public void OsirionChecklist_TitleWithoutItems_RendersOnlyTheHeading()
    {
        var cut = RenderStatic<OsirionChecklist>(p => p.Add(c => c.Title, "Included"));

        cut.Find("h3").TextContent.ShouldBe("Included");
        cut.FindAll("ul").ShouldBeEmpty();
    }

    [Fact]
    public void OsirionChecklist_NothingToShow_RendersNothing()
    {
        var cut = RenderStatic<OsirionChecklist>(p => p.Add(c => c.Title, "  "));

        cut.Markup.Trim().ShouldBeEmpty();
    }

    [Theory]
    [InlineData(1, "h1")]
    [InlineData(2, "h2")]
    [InlineData(4, "h4")]
    [InlineData(5, "h5")]
    [InlineData(6, "h6")]
    [InlineData(0, "h1")]
    [InlineData(9, "h6")]
    public void OsirionChecklist_HeadingLevel_SetsTheHeadingElementWithinOneToSix(int level, string element)
    {
        var cut = RenderStatic<OsirionChecklist>(p => p
            .Add(c => c.Title, "Included")
            .Add(c => c.HeadingLevel, level));

        cut.Find($"{element}.osirion-checklist-title").TextContent.ShouldBe("Included");
    }

    [Fact]
    public void OsirionChecklist_TwoInstances_GenerateDifferentHeadingIds()
    {
        var first = RenderStatic<OsirionChecklist>(p => p.Add(c => c.Title, "A"));
        var second = RenderStatic<OsirionChecklist>(p => p.Add(c => c.Title, "B"));

        first.Find("h3").Id.ShouldNotBe(second.Find("h3").Id);
    }

    [Fact]
    public void OsirionChecklist_TextIsEncoded()
    {
        var cut = RenderStatic<OsirionChecklist>(p => p
            .Add(c => c.Items, [new ChecklistItem("<b>bold</b>", "<i>x</i>")]));

        cut.FindAll("b").ShouldBeEmpty();
        cut.FindAll("i").ShouldBeEmpty();
        cut.Find(".osirion-checklist-item-title").TextContent.ShouldBe("<b>bold</b>");
    }

    [Fact]
    public void OsirionChecklist_ClassStyleAndAttributes_GoOnTheRoot()
    {
        var cut = RenderStatic<OsirionChecklist>(p => p
            .Add(c => c.Items, TwoItems)
            .Add(c => c.Class, "extra")
            .Add(c => c.Style, "margin: 0")
            .AddUnmatched("data-test", "list"));

        var root = cut.Find("div.osirion-checklist");
        root.ClassList.ShouldBe(["osirion-checklist", "extra"]);
        root.GetAttribute("style").ShouldBe("margin: 0");
        root.GetAttribute("data-test").ShouldBe("list");
    }
}
