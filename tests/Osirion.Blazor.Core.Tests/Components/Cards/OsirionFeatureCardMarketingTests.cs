using Bunit;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Components.Cards;

/// <summary>
/// The icon slot and the item list of <see cref="OsirionFeatureCard"/>.
/// </summary>
public class OsirionFeatureCardMarketingTests : OsirionComponentTestBase
{
    // A guard test: a card that sets neither Icon nor Items must keep the markup it rendered before they existed.
    [Fact]
    public void FeatureCard_IconAndItemsUnused_RendersTheMarkupItAlwaysDid()
    {
        var cut = RenderStatic<OsirionFeatureCard>(p => p
            .Add(c => c.Title, "Fast")
            .Add(c => c.Description, "Renders on the server"));

        StableMarkup.Of(cut.Markup).ShouldBe(UnchangedMarkup);
    }

    [Fact]
    public void FeatureCard_IconWithoutImage_ReplacesTheImageColumnWithAHiddenIcon()
    {
        var cut = RenderStatic<OsirionFeatureCard>(p => p
            .Add(c => c.Title, "Fast")
            .Add(c => c.Url, "/features/fast")
            .Add(c => c.Icon, builder => builder.AddMarkupContent(0, "<svg class=\"bolt\"></svg>")));

        var card = cut.Find("div.osirion-feature-card");
        card.ClassList.ShouldContain("osirion-feature-card-with-icon");
        card.ClassList.ShouldNotContain("osirion-feature-card-image-left");
        cut.FindAll(".osirion-feature-card-image-placeholder").ShouldBeEmpty();
        cut.FindAll(".osirion-feature-card-image-link").ShouldBeEmpty();

        var content = cut.Find(".osirion-feature-card-content");
        var icon = content.Children[0];
        icon.ClassName.ShouldBe("osirion-feature-card-icon");
        icon.GetAttribute("aria-hidden").ShouldBe("true");
        icon.QuerySelector("svg.bolt").ShouldNotBeNull();
        content.Children[1].ClassName.ShouldBe("osirion-feature-card-title");
    }

    [Fact]
    public void FeatureCard_IconWithImage_KeepsTheImageAndShowsTheIcon()
    {
        var cut = RenderStatic<OsirionFeatureCard>(p => p
            .Add(c => c.Title, "Fast")
            .Add(c => c.ImageUrl, "/fast.png")
            .Add(c => c.Icon, builder => builder.AddMarkupContent(0, "<svg></svg>")));

        cut.Find("div.osirion-feature-card").ClassList.ShouldContain("osirion-feature-card-image-left");
        cut.Find("img.osirion-feature-card-image").GetAttribute("src").ShouldBe("/fast.png");
        cut.FindAll(".osirion-feature-card-icon").Count.ShouldBe(1);
    }

    [Fact]
    public void FeatureCard_Items_RenderAChecklistAfterTheDescriptionWithoutEmptyPhrases()
    {
        var cut = RenderStatic<OsirionFeatureCard>(p => p
            .Add(c => c.Title, "Fast")
            .Add(c => c.Description, "Renders on the server")
            .Add(c => c.Items, ["No script", "", "Cached"])
            .AddChildContent("<p class=\"extra\">More</p>"));

        var content = cut.Find(".osirion-feature-card-content");
        content.Children.Select(child => child.ClassName).ShouldBe(
            ["osirion-feature-card-title", "osirion-feature-card-description", "osirion-feature-card-items", "extra"]);
        cut.FindAll(".osirion-feature-card-items .osirion-checklist-item-title").Select(item => item.TextContent)
            .ShouldBe(["No script", "Cached"]);
    }

    [Fact]
    public void FeatureCard_OnlyEmptyItems_RenderNoItems()
    {
        var cut = RenderStatic<OsirionFeatureCard>(p => p
            .Add(c => c.Title, "Fast")
            .Add(c => c.Items, [" "]));

        cut.FindAll(".osirion-feature-card-items").ShouldBeEmpty();
    }

    private const string UnchangedMarkup =
        """<div class="osirion-feature-card osirion-feature-card-image-left osirion-feature-card-md osirion-feature-card-content-center-left osirion-feature-card-font-md osirion-feature-card-border-always osirion-feature-card-shadow-hover osirion-feature-card-transform-hover"><div class="osirion-feature-card-image-placeholder"><svg xmlns="http://www.w3.org/2000/svg" width="36" height="36" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="18" height="18" rx="2" ry="2"></rect><circle cx="8.5" cy="8.5" r="1.5"></circle><polyline points="21 15 16 10 5 21"></polyline></svg></div><div class="osirion-feature-card-content"><p class="osirion-feature-card-title">Fast</p><p class="osirion-feature-card-description">Renders on the server</p></div></div>""";
}
