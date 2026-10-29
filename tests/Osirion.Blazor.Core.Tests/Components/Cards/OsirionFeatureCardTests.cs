using Bunit;
using Microsoft.AspNetCore.Components;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Components.Cards;

public class OsirionFeatureCardTests : OsirionComponentTestBase
{
    private IRenderedComponent<OsirionFeatureCard> RenderCard(
        Action<ComponentParameterCollectionBuilder<OsirionFeatureCard>>? parameterBuilder = null) =>
        RenderStatic<OsirionFeatureCard>(parameters => parameterBuilder?.Invoke(parameters));

    private static string[] CardClasses(IRenderedComponent<OsirionFeatureCard> cut) =>
        cut.Find("div.osirion-feature-card").ClassList.ToArray();

    [Fact]
    public void FeatureCard_Defaults_RenderTheDefaultClassSet()
    {
        var cut = RenderCard();

        CardClasses(cut).ShouldBe(
        [
            "osirion-feature-card",
            "osirion-feature-card-without-image",
            "osirion-feature-card-md",
            "osirion-feature-card-content-center-left",
            "osirion-feature-card-font-md",
            "osirion-feature-card-border-always",
            "osirion-feature-card-shadow-hover",
            "osirion-feature-card-transform-hover",
        ]);
    }

    [Fact]
    public void FeatureCard_ClassAndStyle_AreAppliedToTheRoot()
    {
        var cut = RenderCard(p => p
            .Add(c => c.Class, "extra")
            .Add(c => c.Style, "color: red;")
            .AddUnmatched("data-test", "card"));

        var root = cut.Find("div.osirion-feature-card");
        root.ClassList.ShouldContain("extra");
        root.GetAttribute("style").ShouldBe("color: red;");
        root.GetAttribute("data-test").ShouldBe("card");
    }

    [Theory]
    [InlineData(ImagePosition.Top, "osirion-feature-card-image-top")]
    [InlineData(ImagePosition.Bottom, "osirion-feature-card-image-bottom")]
    [InlineData(ImagePosition.Right, "osirion-feature-card-image-right")]
    [InlineData(ImagePosition.Left, "osirion-feature-card-image-left")]
    [InlineData((ImagePosition)99, "osirion-feature-card-image-left")]
    public void FeatureCard_WithImage_MapsImagePositionToClass(ImagePosition position, string expected)
    {
        var cut = RenderCard(p => p
            .Add(c => c.ImageUrl, "/img.png")
            .Add(c => c.ImagePosition, position));

        CardClasses(cut).ShouldContain(expected);
    }

    // Without an image the card has one column of text, whatever ImagePosition says.
    [Fact]
    public void FeatureCard_WithoutImage_UsesTheTextOnlyLayout()
    {
        var cut = RenderCard(p => p.Add(c => c.ImagePosition, ImagePosition.Top));

        CardClasses(cut).ShouldContain("osirion-feature-card-without-image");
        CardClasses(cut).ShouldNotContain("osirion-feature-card-image-top");
        CardClasses(cut).ShouldNotContain("osirion-feature-card-image-left");
    }

    [Theory]
    [InlineData(ImagePosition.Top, true)]
    [InlineData(ImagePosition.Left, true)]
    [InlineData(ImagePosition.Bottom, false)]
    [InlineData(ImagePosition.Right, false)]
    [InlineData((ImagePosition)99, true)]
    public void FeatureCard_ImagePosition_DecidesWhetherTheImageComesFirst(ImagePosition position, bool imageFirst)
    {
        var cut = RenderCard(p => p
            .Add(c => c.ImageUrl, "/img.png")
            .Add(c => c.Title, "Title")
            .Add(c => c.ImagePosition, position));

        var children = cut.Find("div.osirion-feature-card").Children;
        children.Length.ShouldBe(2);
        children[imageFirst ? 0 : 1].ClassList.ShouldContain("osirion-feature-card-image-container");
        children[imageFirst ? 1 : 0].ClassList.ShouldContain("osirion-feature-card-content");
    }

    // Known defect, pinned as current behavior: the image falls back through an inline onerror handler that loads https://placehold.co, which a strict Content Security Policy blocks. Update this expectation when it is fixed.
    [Theory]
    [InlineData(CardSize.ExtraExtraLarge, "osirion-feature-card-xxl", "1200x1200")]
    [InlineData(CardSize.ExtraLarge, "osirion-feature-card-xl", "900x900")]
    [InlineData(CardSize.Large, "osirion-feature-card-lg", "600x600")]
    [InlineData(CardSize.Normal, "osirion-feature-card-md", "450x450")]
    [InlineData(CardSize.Small, "osirion-feature-card-sm", "300x300")]
    [InlineData(CardSize.ExtraSmall, "osirion-feature-card-xs", "150x150")]
    [InlineData(CardSize.ExtraExtraSmall, "osirion-feature-card-xxs", "75x75")]
    [InlineData((CardSize)99, "osirion-feature-card-md", "450x450")]
    public void FeatureCard_CardSize_MapsToClassAndInlineOnErrorPlaceholder(CardSize size, string expectedClass, string expectedDimensions)
    {
        var cut = RenderCard(p => p
            .Add(c => c.ImageUrl, "/img.png")
            .Add(c => c.CardSize, size));

        CardClasses(cut).ShouldContain(expectedClass);
        cut.Find("img").GetAttribute("onerror").ShouldBe(
            $"this.onerror=null;this.src='https://placehold.co/{expectedDimensions}/1a1a1a/ffffff?text=No+Image';");
    }

    [Theory]
    [InlineData(ContentAlignment.TopLeft, "osirion-feature-card-content-top-left")]
    [InlineData(ContentAlignment.TopCenter, "osirion-feature-card-content-top-center")]
    [InlineData(ContentAlignment.TopRight, "osirion-feature-card-content-top-right")]
    [InlineData(ContentAlignment.CenterLeft, "osirion-feature-card-content-center-left")]
    [InlineData(ContentAlignment.Center, "osirion-feature-card-content-center")]
    [InlineData(ContentAlignment.CenterRight, "osirion-feature-card-content-center-right")]
    [InlineData(ContentAlignment.BottomLeft, "osirion-feature-card-content-bottom-left")]
    [InlineData(ContentAlignment.BottomCenter, "osirion-feature-card-content-bottom-center")]
    [InlineData(ContentAlignment.BottomRight, "osirion-feature-card-content-bottom-right")]
    [InlineData((ContentAlignment)99, "osirion-feature-card-content-center-left")]
    public void FeatureCard_ContentAlignment_MapsToClass(ContentAlignment alignment, string expected)
    {
        var cut = RenderCard(p => p.Add(c => c.ContentAlignment, alignment));

        CardClasses(cut).ShouldContain(expected);
    }

    [Theory]
    [InlineData(FontSizeVariant.ExtraExtraSmall, "osirion-feature-card-font-xxs")]
    [InlineData(FontSizeVariant.ExtraSmall, "osirion-feature-card-font-xs")]
    [InlineData(FontSizeVariant.Small, "osirion-feature-card-font-sm")]
    [InlineData(FontSizeVariant.Normal, "osirion-feature-card-font-md")]
    [InlineData(FontSizeVariant.Large, "osirion-feature-card-font-lg")]
    [InlineData(FontSizeVariant.ExtraLarge, "osirion-feature-card-font-xl")]
    [InlineData(FontSizeVariant.ExtraExtraLarge, "osirion-feature-card-font-xxl")]
    [InlineData((FontSizeVariant)99, "osirion-feature-card-font-md")]
    public void FeatureCard_ContentFontSize_MapsToClass(FontSizeVariant fontSize, string expected)
    {
        var cut = RenderCard(p => p.Add(c => c.ContentFontSize, fontSize));

        CardClasses(cut).ShouldContain(expected);
    }

    [Fact]
    public void FeatureCard_AllEffectsNever_RendersNoEffectClasses()
    {
        var cut = RenderCard(p => p
            .Add(c => c.BorderTiming, EffectTiming.Never)
            .Add(c => c.BorderColor, ThemeColor.Primary)
            .Add(c => c.ShadowTiming, EffectTiming.Never)
            .Add(c => c.ShadowColor, ThemeColor.Primary)
            .Add(c => c.TransformTiming, EffectTiming.Never));

        CardClasses(cut).ShouldNotContain(c => c.Contains("-border-") || c.Contains("-shadow-") || c.Contains("-transform-"));
    }

    [Theory]
    [InlineData(EffectTiming.Always, "always")]
    [InlineData(EffectTiming.OnHover, "hover")]
    public void FeatureCard_EffectTiming_MapsToBorderShadowAndTransformClasses(EffectTiming timing, string suffix)
    {
        var cut = RenderCard(p => p
            .Add(c => c.BorderTiming, timing)
            .Add(c => c.ShadowTiming, timing)
            .Add(c => c.TransformTiming, timing));

        var classes = CardClasses(cut);
        classes.ShouldContain($"osirion-feature-card-border-{suffix}");
        classes.ShouldContain($"osirion-feature-card-shadow-{suffix}");
        classes.ShouldContain($"osirion-feature-card-transform-{suffix}");
    }

    [Fact]
    public void FeatureCard_UndefinedEffectTiming_RendersNoEffectClasses()
    {
        var cut = RenderCard(p => p
            .Add(c => c.BorderTiming, (EffectTiming)99)
            .Add(c => c.ShadowTiming, (EffectTiming)99)
            .Add(c => c.TransformTiming, (EffectTiming)99));

        CardClasses(cut).ShouldNotContain(c => c.Contains("-border-") || c.Contains("-shadow-") || c.Contains("-transform-"));
    }

    [Theory]
    [InlineData(ThemeColor.Primary, "primary")]
    [InlineData(ThemeColor.Secondary, "secondary")]
    [InlineData(ThemeColor.Tertiary, "tertiary")]
    public void FeatureCard_EffectColor_MapsToBorderAndShadowColorClasses(ThemeColor color, string suffix)
    {
        var cut = RenderCard(p => p
            .Add(c => c.BorderColor, color)
            .Add(c => c.ShadowColor, color));

        var classes = CardClasses(cut);
        classes.ShouldContain($"osirion-feature-card-border-{suffix}");
        classes.ShouldContain($"osirion-feature-card-shadow-{suffix}");
    }

    [Fact]
    public void FeatureCard_UndefinedEffectColor_AddsNoColorClass()
    {
        var cut = RenderCard(p => p
            .Add(c => c.BorderColor, (ThemeColor)99)
            .Add(c => c.ShadowColor, (ThemeColor)99));

        CardClasses(cut).ShouldNotContain(c =>
            c.EndsWith("-primary", StringComparison.Ordinal)
            || c.EndsWith("-secondary", StringComparison.Ordinal)
            || c.EndsWith("-tertiary", StringComparison.Ordinal));
    }

    [Fact]
    public void FeatureCard_ImageTransformOnHover_AddsTheImageTransformClass()
    {
        var cut = RenderCard(p => p.Add(c => c.ImageTransformOnHover, true));

        CardClasses(cut).ShouldContain("osirion-feature-card-image-transform");
    }

    // Fails before the fix: a card without an image drew a placeholder, inside a link when it had a Url.
    [Fact]
    public void FeatureCard_WithoutImage_RendersNoImageArea()
    {
        var cut = RenderCard(p => p.Add(c => c.Url, "/features/fast"));

        cut.FindAll(".osirion-feature-card-image-container, .osirion-feature-card-image-link, img, svg").ShouldBeEmpty();
    }

    [Fact]
    public void FeatureCard_WithImage_RendersTheImageWithAltFromTitle()
    {
        var cut = RenderCard(p => p
            .Add(c => c.ImageUrl, "/img.png")
            .Add(c => c.Title, "Fast"));

        var image = cut.Find(".osirion-feature-card-image-container img.osirion-feature-card-image");
        image.GetAttribute("src").ShouldBe("/img.png");
        image.GetAttribute("alt").ShouldBe("Fast");
        image.GetAttribute("loading").ShouldBe("lazy");
    }

    [Theory]
    [InlineData("Explicit", "Title", "Explicit")]
    [InlineData(null, "Title", "Title")]
    [InlineData(null, null, "Feature image")]
    public void FeatureCard_ImageAlt_FallsBackToTitleThenToADefault(string? imageAlt, string? title, string expected)
    {
        var cut = RenderCard(p => p
            .Add(c => c.ImageUrl, "/img.png")
            .Add(c => c.ImageAlt, imageAlt)
            .Add(c => c.Title, title));

        cut.Find("img").GetAttribute("alt").ShouldBe(expected);
    }

    [Fact]
    public void FeatureCard_ImageLazyLoadingOff_LoadsEagerly()
    {
        var cut = RenderCard(p => p
            .Add(c => c.ImageUrl, "/img.png")
            .Add(c => c.ImageLazyLoading, false));

        cut.Find("img").GetAttribute("loading").ShouldBe("eager");
    }

    [Fact]
    public void FeatureCard_WithUrl_WrapsTheImageInALink()
    {
        var cut = RenderCard(p => p
            .Add(c => c.ImageUrl, "/img.png")
            .Add(c => c.Url, "/features/fast"));

        var link = cut.Find("a.osirion-feature-card-image-link");
        link.GetAttribute("href").ShouldBe("/features/fast");
        link.QuerySelector("img").ShouldNotBeNull();
    }

    [Fact]
    public void FeatureCard_WithoutUrl_DoesNotWrapTheImage()
    {
        var cut = RenderCard(p => p.Add(c => c.ImageUrl, "/img.png"));

        cut.FindAll("a.osirion-feature-card-image-link").ShouldBeEmpty();
    }

    [Fact]
    public void FeatureCard_TitleDescriptionAndChildContent_AreRenderedInTheContentBlock()
    {
        var cut = RenderCard(p => p
            .Add(c => c.Title, "Fast")
            .Add(c => c.Description, "Very fast")
            .Add(c => c.ChildContent, (RenderFragment)(b => b.AddMarkupContent(0, "<span class=\"child\">Child</span>"))));

        cut.Find(".osirion-feature-card-content .osirion-feature-card-title").TextContent.Trim().ShouldBe("Fast");
        cut.Find(".osirion-feature-card-content .osirion-feature-card-description").TextContent.ShouldBe("Very fast");
        cut.Find(".osirion-feature-card-content span.child").TextContent.ShouldBe("Child");
    }

    [Fact]
    public void FeatureCard_EmptyTitleAndDescription_RenderNoTextElements()
    {
        var cut = RenderCard(p => p
            .Add(c => c.Title, " ")
            .Add(c => c.Description, " "));

        cut.FindAll(".osirion-feature-card-title").ShouldBeEmpty();
        cut.FindAll(".osirion-feature-card-description").ShouldBeEmpty();
    }

    [Fact]
    public void FeatureCard_ShowDescriptionOff_HidesTheDescription()
    {
        var cut = RenderCard(p => p
            .Add(c => c.Description, "Very fast")
            .Add(c => c.ShowDescription, false));

        cut.FindAll(".osirion-feature-card-description").ShouldBeEmpty();
    }

    [Fact]
    public void FeatureCard_ReadMoreHidden_RendersNoReadMoreLink()
    {
        var cut = RenderCard(p => p.Add(c => c.ReadMoreHref, "/more"));

        cut.FindAll("a.osirion-read-more-link, .osirion-read-more-text").ShouldBeEmpty();
    }

    [Fact]
    public void FeatureCard_ReadMoreShownWithoutAnyHref_RendersNoReadMoreLink()
    {
        var cut = RenderCard(p => p.Add(c => c.ShowReadMoreLink, true));

        cut.FindAll(".osirion-read-more-text").ShouldBeEmpty();
    }

    [Fact]
    public void FeatureCard_ReadMoreShown_PrefersReadMoreHrefOverUrl()
    {
        var cut = RenderCard(p => p
            .Add(c => c.ShowReadMoreLink, true)
            .Add(c => c.ReadMoreHref, "/more")
            .Add(c => c.Url, "/card")
            .Add(c => c.ReadMoreText, "Learn more"));

        var link = cut.Find(".osirion-feature-card-content a");
        link.GetAttribute("href").ShouldBe("/more");
        link.TextContent.ShouldContain("Learn more");
    }

    [Fact]
    public void FeatureCard_ReadMoreShownWithOnlyUrl_LinksToTheUrl()
    {
        var cut = RenderCard(p => p
            .Add(c => c.ShowReadMoreLink, true)
            .Add(c => c.Url, "/card"));

        cut.Find(".osirion-feature-card-content a").GetAttribute("href").ShouldBe("/card");
    }

    [Theory]
    [InlineData(ContentAlignment.TopLeft, "justify-content: flex-start;")]
    [InlineData(ContentAlignment.CenterLeft, "justify-content: flex-start;")]
    [InlineData(ContentAlignment.BottomLeft, "justify-content: flex-start;")]
    [InlineData(ContentAlignment.TopCenter, "justify-content: center;")]
    [InlineData(ContentAlignment.Center, "justify-content: center;")]
    [InlineData(ContentAlignment.BottomCenter, "justify-content: center;")]
    [InlineData(ContentAlignment.TopRight, "justify-content: flex-end;")]
    [InlineData(ContentAlignment.CenterRight, "justify-content: flex-end;")]
    [InlineData(ContentAlignment.BottomRight, "justify-content: flex-end;")]
    public void FeatureCard_ReadMoreLink_JustifiesByContentAlignment(ContentAlignment alignment, string expected)
    {
        var cut = RenderCard(p => p
            .Add(c => c.ShowReadMoreLink, true)
            .Add(c => c.ReadMoreHref, "/more")
            .Add(c => c.ContentAlignment, alignment));

        var style = cut.Find(".osirion-feature-card-content a").GetAttribute("style");
        style.ShouldNotBeNull();
        style.ShouldContain(expected);
        style.ShouldNotContain("bottom:50px;");
    }

    [Fact]
    public void FeatureCard_ReadMoreLinkWithUndefinedAlignment_HasNoJustifyRule()
    {
        var cut = RenderCard(p => p
            .Add(c => c.ShowReadMoreLink, true)
            .Add(c => c.ReadMoreHref, "/more")
            .Add(c => c.ContentAlignment, (ContentAlignment)99));

        var style = cut.Find(".osirion-feature-card-content a").GetAttribute("style") ?? string.Empty;
        style.ShouldNotContain("justify-content");
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void FeatureCard_StretchedReadMoreWithPillows_IsLiftedAbovePillows(bool stretched, bool expectLifted)
    {
        var cut = RenderCard(p => p
            .Add(c => c.ShowReadMoreLink, true)
            .Add(c => c.ReadMoreHref, "/more")
            .Add(c => c.ReadMoreStretched, stretched)
            .Add(c => c.ShowPillowButtons, true)
            .Add(c => c.PillowButtons, [new PillowButton("Tag", "/tag")]));

        var style = cut.Find(".osirion-feature-card-content > a:not(.osirion-feature-card-pillow)").GetAttribute("style") ?? string.Empty;
        style.Contains("bottom:50px;", StringComparison.Ordinal).ShouldBe(expectLifted);
    }

    [Fact]
    public void FeatureCard_PillowButtons_RenderWithColorTargetAndLabel()
    {
        var cut = RenderCard(p => p
            .Add(c => c.ShowPillowButtons, true)
            .Add(c => c.PillowButtons,
            [
                new PillowButton("One", "/one"),
                new PillowButton("Two", "/two", ThemeColor.Secondary) { Target = "_blank", AriaLabel = "Second" },
                new PillowButton("Three", "/three", ThemeColor.Tertiary),
                new PillowButton("Four", "/four", ThemeColor.Neutral),
            ]));

        var pillows = cut.FindAll(".osirion-feature-card-pillows a.osirion-feature-card-pillow");
        pillows.Count.ShouldBe(4);
        pillows[0].ClassList.ShouldContain("osirion-feature-card-pillow-primary");
        pillows[0].GetAttribute("href").ShouldBe("/one");
        pillows[0].TextContent.Trim().ShouldBe("One");
        pillows[1].ClassList.ShouldContain("osirion-feature-card-pillow-secondary");
        pillows[1].GetAttribute("target").ShouldBe("_blank");
        pillows[1].GetAttribute("aria-label").ShouldBe("Second");
        pillows[2].ClassList.ShouldContain("osirion-feature-card-pillow-tertiary");
        pillows[3].ClassList.ShouldContain("osirion-feature-card-pillow-primary");
    }

    [Fact]
    public void FeatureCard_PillowButtonsNotShown_RendersNoPillows()
    {
        var cut = RenderCard(p => p.Add(c => c.PillowButtons, [new PillowButton("One", "/one")]));

        cut.FindAll(".osirion-feature-card-pillows").ShouldBeEmpty();
    }

    [Fact]
    public void FeatureCard_ShowPillowButtonsWithEmptyList_RendersNoPillows()
    {
        var cut = RenderCard(p => p
            .Add(c => c.ShowPillowButtons, true)
            .Add(c => c.PillowButtons, []));

        cut.FindAll(".osirion-feature-card-pillows").ShouldBeEmpty();
    }

    [Fact]
    public void PillowButton_DefaultConstructor_UsesSafeDefaults()
    {
        var button = new PillowButton();

        button.Text.ShouldBe(string.Empty);
        button.Href.ShouldBe("#");
        button.Color.ShouldBe(ThemeColor.Primary);
        button.Target.ShouldBeNull();
        button.AriaLabel.ShouldBeNull();
    }
}
