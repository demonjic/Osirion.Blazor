using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Tests.Base;
using Osirion.Blazor.Core.Utils;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Components.Sections;

public class HeroSectionRenderingTests : OsirionComponentTestBase
{
    [Fact]
    public void HeroSection_Defaults_RendersDefaultClassesAndMinHeight()
    {
        var cut = RenderStatic<HeroSection>(_ => { });

        var section = cut.Find("section");
        section.ClassList.ShouldBe(
            ["osirion-hero-section", "osirion-hero-variant-hero", "osirion-hero-align-left", "hero-divider"]);
        section.GetAttribute("style").ShouldBe("min-height: 60vh;");
    }

    [Fact]
    public void HeroSection_CustomClass_IsAddedOnceAsItsOwnClass()
    {
        var cut = RenderStatic<HeroSection>(parameters => parameters
            .Add(p => p.Class, "landing-hero"));

        cut.Find("section").GetAttribute("class")
            .ShouldBe("osirion-hero-section osirion-hero-variant-hero osirion-hero-align-left landing-hero hero-divider");
    }

    [Fact]
    public void HeroSection_TextParameters_RendersTitleSubtitleAndSummary()
    {
        var cut = RenderStatic<HeroSection>(parameters => parameters
            .Add(p => p.Title, "Welcome")
            .Add(p => p.Subtitle, "To the site")
            .Add(p => p.Summary, "A short summary"));

        cut.Find("h1.osirion-hero-title").TextContent.ShouldBe("Welcome");
        cut.Find("p.osirion-hero-subtitle").TextContent.ShouldBe("To the site");
        cut.Find("p.osirion-hero-summary").TextContent.ShouldBe("A short summary");
    }

    [Fact]
    public void HeroSection_ContentFragments_ReplaceTheTextParameters()
    {
        var cut = RenderStatic<HeroSection>(parameters => parameters
            .Add(p => p.Title, "Ignored")
            .Add(p => p.Subtitle, "Ignored")
            .Add(p => p.Summary, "Ignored")
            .Add(p => p.TitleContent, Markup("<h2 class=\"t\">Title</h2>"))
            .Add(p => p.SubtitleContent, Markup("<p class=\"s\">Subtitle</p>"))
            .Add(p => p.SummaryContent, Markup("<p class=\"m\">Summary</p>")));

        cut.Find(".osirion-hero-content .t").TextContent.ShouldBe("Title");
        cut.Find(".osirion-hero-content .s").TextContent.ShouldBe("Subtitle");
        cut.Find(".osirion-hero-content .m").TextContent.ShouldBe("Summary");
        cut.FindAll(".osirion-hero-title, .osirion-hero-subtitle, .osirion-hero-summary").ShouldBeEmpty();
    }

    [Fact]
    public void HeroSection_NoText_RendersNoTextElements()
    {
        var cut = RenderStatic<HeroSection>(_ => { });

        cut.FindAll(".osirion-hero-title, .osirion-hero-subtitle, .osirion-hero-summary").ShouldBeEmpty();
        cut.FindAll(".osirion-hero-buttons").ShouldBeEmpty();
        cut.FindAll(".osirion-hero-additional").ShouldBeEmpty();
        cut.FindAll(".osirion-article-metadata").ShouldBeEmpty();
    }

    [Fact]
    public void HeroSection_BothButtons_RendersPrimaryThenSecondary()
    {
        var cut = RenderStatic<HeroSection>(parameters => parameters
            .Add(p => p.PrimaryButtonText, "Start")
            .Add(p => p.PrimaryButtonUrl, "/start")
            .Add(p => p.SecondaryButtonText, "Docs")
            .Add(p => p.SecondaryButtonUrl, "/docs"));

        var buttons = cut.FindAll(".osirion-hero-buttons a");
        buttons.Select(a => (a.TextContent.Trim(), a.GetAttribute("href"))).ShouldBe([("Start", "/start"), ("Docs", "/docs")]);
        buttons[0].ClassList.ShouldBe(
            ["osirion-button", "osirion-button-primary", "osirion-button-lg", "osirion-hero-button", "osirion-hero-button-primary"],
            ignoreOrder: true);
        buttons[1].ClassList.ShouldContain("osirion-button-secondary");
        buttons[1].ClassList.ShouldNotContain("osirion-button-primary");
    }

    // Fails before the fix: the secondary button carried osirion-hero-button-primary, so hero styles meant for the
    // primary button also applied to it, and it was the outlined primary button rather than the secondary one.
    [Fact]
    public void HeroSection_SecondaryButton_CarriesTheSecondaryHeroClass()
    {
        var cut = RenderStatic<HeroSection>(parameters => parameters
            .Add(p => p.ShowPrimaryButton, false)
            .Add(p => p.SecondaryButtonText, "Docs")
            .Add(p => p.SecondaryButtonUrl, "/docs"));

        cut.Find(".osirion-hero-buttons a").ClassList.ShouldBe(
            ["osirion-button", "osirion-button-secondary", "osirion-button-lg", "osirion-hero-button", "osirion-hero-button-secondary"],
            ignoreOrder: true);
    }

    // Fails before the fix: under every framework the hero's secondary button had the outlined primary classes while
    // OsirionCallToActionSection's had the secondary ones, so the two sections' buttons looked different.
    [Theory]
    [InlineData(CssFramework.None)]
    [InlineData(CssFramework.Bootstrap)]
    [InlineData(CssFramework.FluentUI)]
    [InlineData(CssFramework.MudBlazor)]
    [InlineData(CssFramework.Radzen)]
    public void HeroSection_SecondaryButton_TakesTheSharedSecondaryClassesUnderEachFramework(CssFramework framework)
    {
        Services.AddSingleton(Options.Create(new ThemingOptions { Framework = framework }));

        var cut = RenderStatic<HeroSection>(parameters => parameters
            .Add(p => p.ShowPrimaryButton, false)
            .Add(p => p.SecondaryButtonText, "Docs")
            .Add(p => p.SecondaryButtonUrl, "/docs"));

        var classes = cut.Find(".osirion-hero-buttons a").ClassList;
        foreach (var expected in ButtonClasses.Secondary(framework).Split(' '))
        {
            classes.ShouldContain(expected);
        }
    }

    [Fact]
    public void HeroSection_PrimaryButtonHidden_RendersOnlySecondaryButton()
    {
        var cut = RenderStatic<HeroSection>(parameters => parameters
            .Add(p => p.ShowPrimaryButton, false)
            .Add(p => p.PrimaryButtonText, "Start")
            .Add(p => p.PrimaryButtonUrl, "/start")
            .Add(p => p.SecondaryButtonText, "Docs")
            .Add(p => p.SecondaryButtonUrl, "/docs"));

        cut.FindAll(".osirion-hero-buttons a").Select(a => a.TextContent.Trim()).ShouldBe(["Docs"]);
    }

    [Theory]
    [InlineData("Start", null)]
    [InlineData(null, "/start")]
    public void HeroSection_PrimaryButtonMissingTextOrUrl_RendersNoButtons(string? text, string? url)
    {
        var cut = RenderStatic<HeroSection>(parameters => parameters
            .Add(p => p.PrimaryButtonText, text)
            .Add(p => p.PrimaryButtonUrl, url)
            .Add(p => p.ShowSecondaryButton, false)
            .Add(p => p.SecondaryButtonText, "Docs")
            .Add(p => p.SecondaryButtonUrl, "/docs"));

        cut.FindAll(".osirion-hero-buttons").ShouldBeEmpty();
    }

    [Fact]
    public void HeroSection_ShowMetadata_RendersArticleMetadata()
    {
        var cut = RenderStatic<HeroSection>(parameters => parameters
            .Add(p => p.ShowMetadata, true)
            .Add(p => p.Author, "Ada")
            .Add(p => p.ReadTime, "4"));

        cut.Find(".osirion-article-metadata-author").TextContent.Trim().ShouldBe("Ada");
        cut.Find(".osirion-article-metadata-read-time").TextContent.Trim().ShouldBe("4 min");
    }

    // Fails before the fix: the hero had no way to pass the page's culture to the metadata date.
    [Fact]
    public void HeroSection_Culture_ReachesTheMetadataDate()
    {
        var cut = RenderStatic<HeroSection>(parameters => parameters
            .Add(p => p.ShowMetadata, true)
            .Add(p => p.PublishDate, new DateTime(2026, 10, 17, 0, 0, 0, DateTimeKind.Utc))
            .Add(p => p.Culture, System.Globalization.CultureInfo.GetCultureInfo("sr-Latn-RS")));

        // The short date pattern comes from the platform's ICU data (macOS writes Serbian dates with spaces).
        var serbian = new DateTime(2026, 10, 17).ToString("d", System.Globalization.CultureInfo.GetCultureInfo("sr-Latn-RS"));
        cut.Find(".osirion-article-metadata-date time").TextContent.ShouldBe(serbian);
        serbian.ShouldNotBe(new DateTime(2026, 10, 17).ToString("d", System.Globalization.CultureInfo.InvariantCulture));
    }

    [Fact]
    public void HeroSection_ChildContent_RendersAdditionalBlock()
    {
        var cut = RenderStatic<HeroSection>(parameters => parameters
            .AddChildContent("<span class=\"extra\">Extra</span>"));

        cut.Find(".osirion-hero-additional .extra").TextContent.ShouldBe("Extra");
    }

    [Fact]
    public void HeroSection_SideImage_RendersImageWithTitleAsAltAndHeroSize()
    {
        var cut = RenderStatic<HeroSection>(parameters => parameters
            .Add(p => p.Title, "Welcome")
            .Add(p => p.ImageUrl, "/img/hero.webp"));

        var image = cut.Find(".osirion-hero-image-container img");
        image.GetAttribute("src").ShouldBe("/img/hero.webp");
        image.GetAttribute("alt").ShouldBe("Welcome");
        (image.GetAttribute("width"), image.GetAttribute("height")).ShouldBe(("600", "315"));
        cut.Find("section").ClassList.ShouldContain("osirion-hero-with-side-image");
        cut.Find("section").ClassList.ShouldContain("osirion-hero-image-right");
        cut.FindAll(".osirion-hero-overlay").ShouldBeEmpty();
    }

    [Fact]
    public void HeroSection_SideImageWithAlt_UsesTheAlt()
    {
        var cut = RenderStatic<HeroSection>(parameters => parameters
            .Add(p => p.Title, "Welcome")
            .Add(p => p.ImageAlt, "A mountain")
            .Add(p => p.ImageUrl, "/img/hero.webp"));

        cut.Find("img").GetAttribute("alt").ShouldBe("A mountain");
    }

    [Fact]
    public void HeroSection_SideImageWithoutTitleOrAlt_UsesFallbackAlt()
    {
        var cut = RenderStatic<HeroSection>(parameters => parameters
            .Add(p => p.ImageUrl, "/img/hero.webp"));

        cut.Find("img").GetAttribute("alt").ShouldBe("Hero image");
    }

    [Theory]
    [InlineData(HeroVariant.Jumbotron, "600", "600")]
    [InlineData(HeroVariant.Minimal, "315", "315")]
    [InlineData((HeroVariant)99, "600", "315")]
    public void HeroSection_Variant_SetsVariantClassAndImageSize(HeroVariant variant, string width, string height)
    {
        var cut = RenderStatic<HeroSection>(parameters => parameters
            .Add(p => p.Variant, variant)
            .Add(p => p.ImageUrl, "/img/hero.webp"));

        var image = cut.Find("img");
        (image.GetAttribute("width"), image.GetAttribute("height")).ShouldBe((width, height));
        cut.Find("section").ClassList.ShouldContain($"osirion-hero-variant-{variant.ToString().ToLowerInvariant()}");
    }

    [Theory]
    [InlineData(Alignment.Left, "osirion-hero-image-right")]
    [InlineData(Alignment.Justify, "osirion-hero-image-right")]
    [InlineData(Alignment.Center, "osirion-hero-image-center")]
    [InlineData(Alignment.Right, "osirion-hero-image-right")]
    public void HeroSection_ImagePosition_MapsLeftAndJustifyToRight(Alignment position, string expected)
    {
        var cut = RenderStatic<HeroSection>(parameters => parameters
            .Add(p => p.ImageUrl, "/img/hero.webp")
            .Add(p => p.ImagePosition, position));

        cut.Find("section").ClassList.ShouldContain(expected);
    }

    [Theory]
    [InlineData(Alignment.Justify, "osirion-hero-align-left")]
    [InlineData(Alignment.Center, "osirion-hero-align-center")]
    public void HeroSection_Alignment_MapsJustifyToLeft(Alignment alignment, string expected)
    {
        var cut = RenderStatic<HeroSection>(parameters => parameters
            .Add(p => p.Alignment, alignment));

        cut.Find("section").ClassList.ShouldContain(expected);
    }

    [Fact]
    public void HeroSection_BackgroundImageWithOverlay_RendersOverlayAndNoSideImageOrPattern()
    {
        var cut = RenderStatic<HeroSection>(parameters => parameters
            .Add(p => p.UseBackgroundImage, true)
            .Add(p => p.HasOverlay, true)
            .Add(p => p.BackgroundPattern, BackgroundPatternType.Dots)
            .Add(p => p.ImageUrl, "/img/hero.webp"));

        var section = cut.Find("section");
        section.ClassList.ShouldContain("osirion-hero-with-background");
        section.ClassList.ShouldContain("osirion-hero-with-overlay");
        cut.FindAll(".osirion-hero-overlay").Count.ShouldBe(1);
        cut.FindAll("img").ShouldBeEmpty();
        cut.FindAll(".osirion-bg-wrapper").ShouldBeEmpty();
    }

    [Fact]
    public void HeroSection_BackgroundImageWithoutOverlay_RendersNoOverlay()
    {
        var cut = RenderStatic<HeroSection>(parameters => parameters
            .Add(p => p.UseBackgroundImage, true)
            .Add(p => p.ImageUrl, "/img/hero.webp"));

        cut.Find("section").ClassList.ShouldNotContain("osirion-hero-with-overlay");
        cut.FindAll(".osirion-hero-overlay").ShouldBeEmpty();
    }

    [Fact]
    public void HeroSection_PatternWithoutBackgroundImage_RendersPattern()
    {
        var cut = RenderStatic<HeroSection>(parameters => parameters
            .Add(p => p.BackgroundPattern, BackgroundPatternType.Diagonal));

        cut.Find(".osirion-bg-wrapper").ClassList.ShouldContain("osirion-bg-diagonal");
    }

    [Fact]
    public void HeroSection_ColorsHeightAndStyle_AreWrittenToTheStyleAttribute()
    {
        var cut = RenderStatic<HeroSection>(parameters => parameters
            .Add(p => p.BackgroundColor, "#111")
            .Add(p => p.TextColor, "#eee")
            .Add(p => p.MinHeight, "40vh")
            .Add(p => p.Style, "padding: 0"));

        cut.Find("section").GetAttribute("style").ShouldBe("background-color: #111; color: #eee; min-height: 40vh;padding: 0");
    }

    [Fact]
    public void HeroSection_DividerDisabled_LeavesOutDividerClass()
    {
        var cut = RenderStatic<HeroSection>(parameters => parameters
            .Add(p => p.HasDivider, false));

        cut.Find("section").ClassList.ShouldNotContain("hero-divider");
    }

    [Fact]
    public void HeroSection_AdditionalAttributes_AreWrittenOnTheSection()
    {
        var cut = RenderStatic<HeroSection>(parameters => parameters
            .AddUnmatched("data-testid", "hero"));

        cut.Find("section").GetAttribute("data-testid").ShouldBe("hero");
    }

    private static RenderFragment Markup(string markup) => builder => builder.AddMarkupContent(0, markup);
}
