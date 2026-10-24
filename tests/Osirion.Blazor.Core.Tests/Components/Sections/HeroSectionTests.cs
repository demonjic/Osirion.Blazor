using AngleSharp.Css.Parser;
using Bunit;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Components.Sections;

// ArticlePage, HomePage and LandingPage pass a content item's front matter featured_image to ImageUrl. Each hostile
// case below changed the style declarations before the fix, when the URL was written into url('...') unescaped.
public class HeroSectionTests : OsirionComponentTestBase
{
    [Fact]
    public void HeroSection_BackgroundImageUrlThatClosesTheCssString_AddsNoDeclaration()
    {
        var declarations = RenderBackgroundStyle(
            "x'); position:fixed; inset:0; z-index:2147483647; background:url('https://attacker.example/fake-notice.png");

        declarations.GetPropertyValue("position").ShouldBeEmpty();
        declarations.GetPropertyValue("z-index").ShouldBeEmpty();
    }

    [Fact]
    public void HeroSection_BackgroundImageUrlWithLineBreak_WritesNoRawLineBreak()
    {
        // A raw line break ends a CSS string early, and the browser then reads what follows as new declarations
        var cut = RenderStatic<HeroSection>(parameters => parameters
            .Add(p => p.UseBackgroundImage, true)
            .Add(p => p.ImageUrl, "x\n); position:fixed; z-index:9;"));

        cut.Find("section").GetAttribute("style")!.ShouldNotContain("\n");
    }

    [Fact]
    public void HeroSection_BackgroundImageUrlEndingInBackslash_KeepsFollowingDeclarations()
    {
        var cut = RenderStatic<HeroSection>(parameters => parameters
            .Add(p => p.UseBackgroundImage, true)
            .Add(p => p.ImageUrl, "x\\")
            .Add(p => p.MinHeight, "40vh"));
        var declarations = new CssParser().ParseDeclaration(cut.Find("section").GetAttribute("style")!)!;

        declarations.GetPropertyValue("min-height").ShouldBe("40vh");
    }

    [Theory]
    [InlineData("assets/images/hexabg.webp")]
    [InlineData("https://cdn.example.com/images/hero.png?w=1200&h=630")]
    public void HeroSection_PlainBackgroundImageUrl_IsWrittenUnchanged(string imageUrl)
    {
        var cut = RenderStatic<HeroSection>(parameters => parameters
            .Add(p => p.UseBackgroundImage, true)
            .Add(p => p.ImageUrl, imageUrl));

        cut.Find("section").GetAttribute("style").ShouldStartWith($"background-image: url('{imageUrl}');");
    }

    private AngleSharp.Css.Dom.ICssStyleDeclaration RenderBackgroundStyle(string imageUrl)
    {
        var cut = RenderStatic<HeroSection>(parameters => parameters
            .Add(p => p.UseBackgroundImage, true)
            .Add(p => p.ImageUrl, imageUrl));

        return new CssParser().ParseDeclaration(cut.Find("section").GetAttribute("style")!)
            ?? throw new InvalidOperationException("The style attribute could not be parsed.");
    }
}
