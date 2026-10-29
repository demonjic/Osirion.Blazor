using Bunit;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Components.Sections;

/// <summary>
/// The eyebrow, the highlights and the framed image of <see cref="HeroSection"/>.
/// </summary>
public class HeroSectionMarketingTests : OsirionComponentTestBase
{
    // A guard test: a hero that sets none of Eyebrow, Highlights and FrameImage must keep the markup it rendered before
    // they existed.
    [Fact]
    public void HeroSection_MarketingParametersUnused_RendersTheMarkupItAlwaysDid()
    {
        var cut = RenderStatic<HeroSection>(p => p
            .Add(c => c.Title, "Build faster")
            .Add(c => c.Subtitle, "A subtitle")
            .Add(c => c.Summary, "A summary")
            .Add(c => c.ImageUrl, "/hero.png")
            .Add(c => c.ImageAlt, "Product")
            .Add(c => c.PrimaryButtonText, "Start")
            .Add(c => c.PrimaryButtonUrl, "/start"));

        StableMarkup.Of(cut.Markup).ShouldBe(UnchangedMarkup);
    }

    [Fact]
    public void HeroSection_Eyebrow_RendersAParagraphBeforeTheTitle()
    {
        var cut = RenderStatic<HeroSection>(p => p
            .Add(c => c.Title, "Build faster")
            .Add(c => c.Eyebrow, "New"));

        var content = cut.Find(".osirion-hero-content");
        content.Children[0].LocalName.ShouldBe("p");
        content.Children[0].ClassName.ShouldBe("osirion-hero-eyebrow");
        content.Children[0].TextContent.ShouldBe("New");
        content.Children[1].ClassName.ShouldBe("osirion-hero-title");
    }

    [Fact]
    public void HeroSection_Eyebrow_IsEncoded()
    {
        var cut = RenderStatic<HeroSection>(p => p.Add(c => c.Eyebrow, "<b>New</b>"));

        cut.FindAll(".osirion-hero-eyebrow b").ShouldBeEmpty();
        cut.Find(".osirion-hero-eyebrow").TextContent.ShouldBe("<b>New</b>");
    }

    [Fact]
    public void HeroSection_Highlights_RenderAChecklistAfterTheSummaryWithoutEmptyPhrases()
    {
        var cut = RenderStatic<HeroSection>(p => p
            .Add(c => c.Title, "Build faster")
            .Add(c => c.Summary, "A summary")
            .Add(c => c.PrimaryButtonText, "Start")
            .Add(c => c.PrimaryButtonUrl, "/start")
            .Add(c => c.Highlights, ["Static SSR", " ", "Themed by tokens"]));

        var content = cut.Find(".osirion-hero-content");
        content.Children.Select(child => child.ClassName!.Split(' ')[0]).ShouldBe(
            ["osirion-hero-title", "osirion-hero-summary", "osirion-hero-highlights", "osirion-hero-buttons"]);
        var items = cut.FindAll(".osirion-hero-highlights ul.osirion-checklist-list > li .osirion-checklist-item-title");
        items.Select(item => item.TextContent).ShouldBe(["Static SSR", "Themed by tokens"]);
        cut.FindAll(".osirion-hero-highlights svg").ShouldAllBe(mark => mark.GetAttribute("aria-hidden") == "true");
    }

    [Fact]
    public void HeroSection_OnlyEmptyHighlights_RenderNoHighlights()
    {
        var cut = RenderStatic<HeroSection>(p => p
            .Add(c => c.Title, "Build faster")
            .Add(c => c.Highlights, ["", "  "]));

        cut.FindAll(".osirion-hero-highlights").ShouldBeEmpty();
    }

    [Fact]
    public void HeroSection_FrameImage_WrapsTheSideImageInADecorativeFrame()
    {
        var cut = RenderStatic<HeroSection>(p => p
            .Add(c => c.Title, "Build faster")
            .Add(c => c.ImageUrl, "/screen.png")
            .Add(c => c.ImageAlt, "The dashboard")
            .Add(c => c.FrameImage, true));

        var container = cut.Find(".osirion-hero-image-container");
        container.ClassList.ShouldBe(["osirion-hero-image-container", "osirion-hero-image-framed"]);
        var bar = container.Children[0];
        bar.ClassName.ShouldBe("osirion-hero-image-frame-bar");
        bar.GetAttribute("aria-hidden").ShouldBe("true");
        bar.Children.Length.ShouldBe(3);
        var image = container.Children[1];
        image.LocalName.ShouldBe("img");
        image.GetAttribute("alt").ShouldBe("The dashboard");
    }

    [Fact]
    public void HeroSection_FrameImageWithABackgroundImage_RendersNoFrame()
    {
        var cut = RenderStatic<HeroSection>(p => p
            .Add(c => c.Title, "Build faster")
            .Add(c => c.ImageUrl, "/screen.png")
            .Add(c => c.UseBackgroundImage, true)
            .Add(c => c.FrameImage, true));

        cut.FindAll(".osirion-hero-image-frame-bar").ShouldBeEmpty();
        cut.FindAll(".osirion-hero-image-framed").ShouldBeEmpty();
    }

    private const string UnchangedMarkup =
        """<section class="osirion-hero-section osirion-hero-variant-hero osirion-hero-align-left osirion-hero-with-side-image osirion-hero-image-right hero-divider"><div class="osirion-hero-container"><div class="osirion-hero-grid"><div class="osirion-hero-content"><h1 class="osirion-hero-title">Build faster</h1><p class="osirion-hero-subtitle">A subtitle</p><p class="osirion-hero-summary">A summary</p><div class="osirion-hero-buttons"><a href="/start" class="osirion-button osirion-button-primary osirion-button-lg osirion-hero-button osirion-hero-button-primary">Start</a></div></div><div class="osirion-hero-image-container"><img src="/hero.png" alt="Product" class="osirion-hero-featured-image" width="600" height="315" fetchpriority="high" /></div></div></div></section>""";
}
