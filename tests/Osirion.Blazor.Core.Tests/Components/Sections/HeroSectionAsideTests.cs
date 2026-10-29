using Bunit;
using Microsoft.AspNetCore.Components;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Components.Sections;

/// <summary>The aside of <see cref="HeroSection"/>, shown beside the copy instead of the side image.</summary>
public class HeroSectionAsideTests : OsirionComponentTestBase
{
    private static readonly RenderFragment SampleAside = builder =>
    {
        builder.OpenElement(0, "figure");
        builder.AddAttribute(1, "class", "sample-record");
        builder.AddContent(2, "Blocked");
        builder.CloseElement();
    };

    // A guard test: a hero without an aside keeps the markup it rendered before Aside existed (captured from master at
    // dfe2cc8c), with a framed side image, an eyebrow and highlights.
    [Fact]
    public void HeroSection_AsideUnset_RendersTheMarkupItAlwaysDid()
    {
        var cut = RenderStatic<HeroSection>(p => p
            .Add(c => c.Title, "Build faster")
            .Add(c => c.Eyebrow, "New")
            .Add(c => c.Highlights, ["Fast"])
            .Add(c => c.ImageUrl, "/hero.png")
            .Add(c => c.ImageAlt, "Product")
            .Add(c => c.FrameImage, true));

        StableMarkup.Of(cut.Markup).ShouldBe(MarkupWithoutAside);
    }

    [Fact]
    public void HeroSection_Aside_RendersBesideTheCopy()
    {
        var cut = RenderStatic<HeroSection>(p => p
            .Add(c => c.Title, "Build faster")
            .Add(c => c.Aside, SampleAside));

        cut.Find("section").ClassList.ShouldContain("osirion-hero-with-aside");
        var grid = cut.Find(".osirion-hero-grid");
        grid.Children.Length.ShouldBe(2);
        grid.Children[0].ClassName.ShouldBe("osirion-hero-content");
        grid.Children[1].ClassName.ShouldBe("osirion-hero-aside");
        grid.Children[1].QuerySelector("figure.sample-record")!.TextContent.ShouldBe("Blocked");
    }

    [Fact]
    public void HeroSection_AsideAndSideImage_TheAsideWins()
    {
        var cut = RenderStatic<HeroSection>(p => p
            .Add(c => c.Title, "Build faster")
            .Add(c => c.ImageUrl, "/hero.png")
            .Add(c => c.FrameImage, true)
            .Add(c => c.Aside, SampleAside));

        cut.FindAll("img").ShouldBeEmpty();
        cut.FindAll(".osirion-hero-image-frame-bar").ShouldBeEmpty();
        cut.Find("section").ClassList.ShouldNotContain("osirion-hero-with-side-image");
        cut.FindAll(".osirion-hero-aside figure").Count.ShouldBe(1);
    }

    [Fact]
    public void HeroSection_AsideWithBackgroundImage_KeepsTheBackground()
    {
        var cut = RenderStatic<HeroSection>(p => p
            .Add(c => c.Title, "Build faster")
            .Add(c => c.ImageUrl, "/hero.png")
            .Add(c => c.UseBackgroundImage, true)
            .Add(c => c.Aside, SampleAside));

        var section = cut.Find("section");
        section.ClassList.ShouldContain("osirion-hero-with-background");
        section.ClassList.ShouldContain("osirion-hero-with-aside");
        section.GetAttribute("style")!.ShouldContain("/hero.png");
        cut.FindAll(".osirion-hero-aside figure").Count.ShouldBe(1);
    }

    private const string MarkupWithoutAside =
        """<section class="osirion-hero-section osirion-hero-variant-hero osirion-hero-align-left osirion-hero-with-side-image osirion-hero-image-right hero-divider"><div class="osirion-hero-container"><div class="osirion-hero-grid"><div class="osirion-hero-content"><p class="osirion-hero-eyebrow">New</p><h1 class="osirion-hero-title">Build faster</h1><div class="osirion-hero-highlights"><div class="osirion-checklist"><ul class="osirion-checklist-list" role="list"><li class="osirion-checklist-item"><svg class="osirion-checklist-mark" aria-hidden="true" focusable="false" viewBox="0 0 16 16" width="16" height="16"><path d="M3 8.5l3 3 7-7" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"></path></svg><span class="osirion-checklist-text"><span class="osirion-checklist-item-title">Fast</span></span></li></ul></div></div></div><div class="osirion-hero-image-container osirion-hero-image-framed"><div class="osirion-hero-image-frame-bar" aria-hidden="true"><span></span><span></span><span></span></div><img src="/hero.png" alt="Product" class="osirion-hero-featured-image" width="600" height="315" fetchpriority="high" /></div></div></div></section>""";
}
