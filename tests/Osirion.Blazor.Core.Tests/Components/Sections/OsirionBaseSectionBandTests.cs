using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Components.Sections;

/// <summary>
/// The eyebrow and the band variants of <see cref="OsirionBaseSection"/>.
/// </summary>
public class OsirionBaseSectionBandTests : OsirionComponentTestBase
{
    // A guard test: a section that sets neither Eyebrow nor Variant must keep the markup it rendered before they existed.
    [Fact]
    public void BaseSection_EyebrowAndVariantUnused_RendersTheMarkupItAlwaysDid()
    {
        var cut = RenderStatic<OsirionBaseSection>(p => p
            .Add(c => c.Id, "features")
            .Add(c => c.Title, "Features")
            .Add(c => c.Description, "Everything you need")
            .AddChildContent("<p>Body</p>"));

        StableMarkup.Of(cut.Markup).ShouldBe(UnchangedMarkup);
    }

    [Fact]
    public void BaseSection_Eyebrow_RendersAParagraphBeforeTheTitle()
    {
        var cut = RenderStatic<OsirionBaseSection>(p => p
            .Add(c => c.Title, "Everything you need")
            .Add(c => c.Eyebrow, "Features"));

        var header = cut.Find(".osirion-section-header");
        header.Children.Select(child => child.ClassName).ShouldBe(["osirion-section-eyebrow", "osirion-section-title"]);
        header.Children[0].LocalName.ShouldBe("p");
        header.Children[0].TextContent.ShouldBe("Features");
    }

    [Fact]
    public void BaseSection_EyebrowWithTitleContent_RendersBeforeTheCustomHeading()
    {
        var cut = RenderStatic<OsirionBaseSection>(p => p
            .Add(c => c.Eyebrow, "Features")
            .Add(c => c.TitleContent, builder => builder.AddMarkupContent(0, "<h3 class=\"custom\">Custom</h3>")));

        cut.Find(".osirion-section-header").Children.Select(child => child.ClassName)
            .ShouldBe(["osirion-section-eyebrow", "custom"]);
    }

    [Fact]
    public void BaseSection_EyebrowWithoutTitle_RendersNoHeader()
    {
        var cut = RenderStatic<OsirionBaseSection>(p => p.Add(c => c.Eyebrow, "Features"));

        cut.FindAll(".osirion-section-eyebrow").ShouldBeEmpty();
        cut.FindAll(".osirion-section-header").ShouldBeEmpty();
    }

    [Fact]
    public void BaseSection_Eyebrow_IsEncoded()
    {
        var cut = RenderStatic<OsirionBaseSection>(p => p
            .Add(c => c.Title, "Title")
            .Add(c => c.Eyebrow, "<b>New</b>"));

        cut.FindAll(".osirion-section-eyebrow b").ShouldBeEmpty();
        cut.Find(".osirion-section-eyebrow").TextContent.ShouldBe("<b>New</b>");
    }

    [Fact]
    public void BaseSection_TintedVariant_AddsTheTintedClassAndNoThemeAttribute()
    {
        var cut = RenderStatic<OsirionBaseSection>(p => p.Add(c => c.Variant, SectionVariant.Tinted));

        var section = cut.Find("section");
        section.ClassList.ShouldContain("osirion-section-variant-tinted");
        section.HasAttribute("data-theme").ShouldBeFalse();
        section.HasAttribute("data-bs-theme").ShouldBeFalse();
    }

    [Fact]
    public void BaseSection_DarkVariant_AddsTheDarkClassAndTheDarkThemeScope()
    {
        var cut = RenderStatic<OsirionBaseSection>(p => p.Add(c => c.Variant, SectionVariant.Dark));

        var section = cut.Find("section");
        section.ClassList.ShouldContain("osirion-section-variant-dark");
        section.GetAttribute("data-theme").ShouldBe("dark");
        section.HasAttribute("data-bs-theme").ShouldBeFalse();
    }

    [Fact]
    public void BaseSection_DarkVariantUnderBootstrap_WritesBootstrapsThemeAttribute()
    {
        Services.AddSingleton(Options.Create(new ThemingOptions { Framework = CssFramework.Bootstrap }));

        var cut = RenderStatic<OsirionBaseSection>(p => p.Add(c => c.Variant, SectionVariant.Dark));

        var section = cut.Find("section");
        section.GetAttribute("data-bs-theme").ShouldBe("dark");
        section.HasAttribute("data-theme").ShouldBeFalse();
    }

    [Fact]
    public void BaseSection_DarkVariantWithAnExplicitTheme_KeepsTheExplicitTheme()
    {
        var cut = RenderStatic<OsirionBaseSection>(p => p
            .Add(c => c.Variant, SectionVariant.Dark)
            .Add(c => c.Theme, ThemeMode.Light));

        cut.Find("section").GetAttribute("data-theme").ShouldBe("light");
    }

    [Fact]
    public void BaseSection_DefaultVariant_AddsNoVariantClass()
    {
        var cut = RenderStatic<OsirionBaseSection>(p => p.Add(c => c.Variant, SectionVariant.Default));

        cut.Find("section").ClassList.ShouldNotContain(name => name.StartsWith("osirion-section-variant-", StringComparison.Ordinal));
    }

    private const string UnchangedMarkup =
        """<section id="features" class="osirion-section osirion-section-align-center osirion-section-padding-medium" style=""><div class="osirion-container"><div class="osirion-section-header osirion-section-header-center"><h2 class="osirion-section-title">Features</h2><p class="osirion-section-description">Everything you need</p></div><div class="osirion-section-content"><p>Body</p></div></div></section>""";
}
