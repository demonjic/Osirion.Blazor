using Bunit;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Tests.Base;
using Shouldly;
using static Osirion.Blazor.Components.OsirionContextCardGrid;

namespace Osirion.Blazor.Core.Tests.Components.Sections;

public class OsirionContextCardGridTests : OsirionComponentTestBase
{
    private IRenderedComponent<OsirionContextCardGrid> RenderGrid(
        Action<ComponentParameterCollectionBuilder<OsirionContextCardGrid>>? parameterBuilder = null) =>
        RenderStatic<OsirionContextCardGrid>(parameters => parameterBuilder?.Invoke(parameters));

    [Fact]
    public void ContextCardGrid_Defaults_RenderALabelledSectionWithTheDefaultHeading()
    {
        var cut = RenderGrid();

        var section = cut.Find("section.osirion-context-card-grid");
        section.HasAttribute("id").ShouldBeFalse();
        cut.Find(".osirion-context-card-grid-container").ClassList.ShouldContain("container");
        var heading = cut.Find("h2");
        section.GetAttribute("aria-labelledby").ShouldBe(heading.Id);
        heading.TextContent.ShouldBe("Context by category");
    }

    // Fails before the fix: the default heading id was a fixed string, so two grids on a page repeated the id.
    [Fact]
    public void ContextCardGrid_TwoGridsWithDefaults_GetDifferentHeadingIds()
    {
        var first = RenderGrid().Find("h2").Id;
        var second = RenderGrid().Find("h2").Id;

        first.ShouldNotBeNullOrWhiteSpace();
        second.ShouldNotBe(first);
    }

    // Fails before the fix: an empty title rendered an empty h2 and the section still pointed aria-labelledby at it.
    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void ContextCardGrid_EmptyTitle_RendersNoHeadingAndNoLabelReference(string title)
    {
        var cut = RenderGrid(p => p
            .Add(c => c.Title, title)
            .Add(c => c.Description, "Where it applies")
            .Add(c => c.Items, [new ContextCardItem("A", "B")]));

        cut.FindAll("h2").ShouldBeEmpty();
        cut.Find("section").HasAttribute("aria-labelledby").ShouldBeFalse();
        cut.Find(".osirion-context-card-grid-heading p").TextContent.ShouldBe("Where it applies");
    }

    [Fact]
    public void ContextCardGrid_NoItemsAndNoDescription_RenderOnlyTheHeading()
    {
        var cut = RenderGrid(p => p.Add(c => c.Description, " "));

        cut.FindAll(".osirion-context-card-grid-heading p").ShouldBeEmpty();
        cut.FindAll(".osirion-context-card-grid-items").ShouldBeEmpty();
    }

    [Fact]
    public void ContextCardGrid_CustomParameters_AreApplied()
    {
        var cut = RenderGrid(p => p
            .Add(c => c.Title, "By region")
            .Add(c => c.Description, "Where it applies")
            .Add(c => c.HeadingId, "regions-heading")
            .Add(c => c.Id, "regions")
            .Add(c => c.SectionClass, "py-5")
            .Add(c => c.ContainerClass, "container-fluid")
            .AddUnmatched("data-test", "grid"));

        var section = cut.Find("section");
        section.Id.ShouldBe("regions");
        section.ClassList.ShouldContain("py-5");
        section.GetAttribute("aria-labelledby").ShouldBe("regions-heading");
        section.GetAttribute("data-test").ShouldBe("grid");
        cut.Find(".osirion-context-card-grid-container").ClassList.ShouldContain("container-fluid");
        cut.Find("h2#regions-heading").TextContent.ShouldBe("By region");
        cut.Find(".osirion-context-card-grid-heading p").TextContent.ShouldBe("Where it applies");
    }

    [Fact]
    public void ContextCardGrid_Items_RenderOneAccentedArticleEach()
    {
        var cut = RenderGrid(p => p.Add(c => c.Items,
        [
            new ContextCardItem("Europe", "EU rules"),
            new ContextCardItem("Asia", "APAC rules", "blue"),
        ]));

        var articles = cut.FindAll(".osirion-context-card-grid-items > article.osirion-context-card-grid-item");
        articles.Count.ShouldBe(2);
        articles[0].ClassList.ShouldContain("osirion-context-card-grid-item-green");
        articles[0].QuerySelector("h3")!.TextContent.ShouldBe("Europe");
        articles[0].QuerySelector("p")!.TextContent.ShouldBe("EU rules");
        articles[1].ClassList.ShouldContain("osirion-context-card-grid-item-blue");
        articles[1].QuerySelector("h3")!.TextContent.ShouldBe("Asia");
    }

    // Fails before the fix: the heading was always an h2 and the card labels always h3, so a grid under a page's h2
    // broke the heading outline.
    [Theory]
    [InlineData(1, "h1", "h2")]
    [InlineData(2, "h2", "h3")]
    [InlineData(3, "h3", "h4")]
    [InlineData(4, "h4", "h5")]
    [InlineData(5, "h5", "h6")]
    [InlineData(6, "h6", "h6")]
    public void ContextCardGrid_HeadingLevel_SetsTheHeadingAndPutsTheCardLabelsOneLevelBelow(int level, string heading, string label)
    {
        var cut = RenderGrid(p => p
            .Add(c => c.HeadingLevel, level)
            .Add(c => c.Items, [new ContextCardItem("Europe", "EU rules")]));

        var title = cut.Find($".osirion-context-card-grid-heading > {heading}");
        title.TextContent.ShouldBe("Context by category");
        cut.Find("section").GetAttribute("aria-labelledby").ShouldBe(title.Id);
        cut.Find($"article.osirion-context-card-grid-item > {label}").TextContent.ShouldBe("Europe");
        cut.FindAll("h1, h2, h3, h4, h5, h6").Count.ShouldBe(2);
    }

    [Theory]
    [InlineData(0, "h1", "h2")]
    [InlineData(-3, "h1", "h2")]
    [InlineData(7, "h6", "h6")]
    [InlineData(int.MaxValue, "h6", "h6")]
    public void ContextCardGrid_HeadingLevelOutsideTheRange_IsMovedToTheNearestLevel(int level, string heading, string label)
    {
        var cut = RenderGrid(p => p
            .Add(c => c.HeadingLevel, level)
            .Add(c => c.Items, [new ContextCardItem("Europe", "EU rules")]));

        cut.Find($".osirion-context-card-grid-heading > {heading}").TextContent.ShouldBe("Context by category");
        cut.Find($"article.osirion-context-card-grid-item > {label}").TextContent.ShouldBe("Europe");
    }

    // Guard: without HeadingLevel the grid keeps the markup it had before the parameter existed.
    [Fact]
    public void ContextCardGrid_DefaultHeadingLevel_KeepsTheMarkup()
    {
        var cut = RenderGrid(p => p
            .Add(c => c.HeadingId, "roles-heading")
            .Add(c => c.Description, "Where it applies")
            .Add(c => c.Items, [new ContextCardItem("Finance", "Cost variance", "blue")]));

        cut.MarkupMatches(
            """
            <section class="osirion-context-card-grid " aria-labelledby="roles-heading">
              <div class="osirion-context-card-grid-container container">
                <div class="osirion-context-card-grid-heading">
                  <h2 id="roles-heading">Context by category</h2>
                  <p>Where it applies</p>
                </div>
                <div class="osirion-context-card-grid-items">
                  <article class="osirion-context-card-grid-item osirion-context-card-grid-item-blue">
                    <h3>Finance</h3>
                    <p>Cost variance</p>
                  </article>
                </div>
              </div>
            </section>
            """);
    }
}
