using Bunit;
using Microsoft.AspNetCore.Components;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Components.Sections;

public class OsirionSplitSectionTests : OsirionComponentTestBase
{
    private static RenderFragment Markup(string html) => builder => builder.AddMarkupContent(0, html);

    [Fact]
    public void OsirionSplitSection_ContentAndAside_RendersTheCopyFirstThenTheAside()
    {
        var cut = RenderStatic<OsirionSplitSection>(p => p
            .Add(c => c.Content, Markup("<p class=\"copy\">Copy</p>"))
            .Add(c => c.Aside, Markup("<img class=\"shot\" src=\"/a.png\" alt=\"A\">")));

        var root = cut.Find("div.osirion-split-section");
        root.ClassList.ShouldBe(["osirion-split-section"]);
        root.HasAttribute("style").ShouldBeFalse();
        root.Children.Select(child => child.ClassName).ShouldBe(
            ["osirion-split-section-content", "osirion-split-section-aside"]);
        root.QuerySelector(".osirion-split-section-content > p.copy").ShouldNotBeNull();
        root.QuerySelector(".osirion-split-section-aside > img.shot").ShouldNotBeNull();
    }

    [Fact]
    public void OsirionSplitSection_AsideAtStart_AddsTheModifierButKeepsTheCopyFirstInTheMarkup()
    {
        var cut = RenderStatic<OsirionSplitSection>(p => p
            .Add(c => c.Content, Markup("<p>Copy</p>"))
            .Add(c => c.Aside, Markup("<p>Aside</p>"))
            .Add(c => c.AsidePosition, SplitAsidePosition.Start));

        var root = cut.Find("div.osirion-split-section");
        root.ClassList.ShouldContain("osirion-split-section-aside-start");
        root.Children[0].ClassName.ShouldBe("osirion-split-section-content");
    }

    [Fact]
    public void OsirionSplitSection_OnlyContent_RendersNoAsideWrapper()
    {
        var cut = RenderStatic<OsirionSplitSection>(p => p.Add(c => c.Content, Markup("<p>Copy</p>")));

        cut.FindAll(".osirion-split-section-aside").ShouldBeEmpty();
        cut.FindAll(".osirion-split-section-content").Count.ShouldBe(1);
    }

    [Fact]
    public void OsirionSplitSection_ClassStyleAndAttributes_GoOnTheRoot()
    {
        var cut = RenderStatic<OsirionSplitSection>(p => p
            .Add(c => c.Class, "extra")
            .Add(c => c.Style, "gap: 0")
            .AddUnmatched("id", "how"));

        var root = cut.Find("div.osirion-split-section");
        root.ClassList.ShouldContain("extra");
        root.GetAttribute("style").ShouldBe("gap: 0");
        root.Id.ShouldBe("how");
    }
}
