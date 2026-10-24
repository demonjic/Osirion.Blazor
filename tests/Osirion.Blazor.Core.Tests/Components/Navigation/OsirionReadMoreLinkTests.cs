using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Components.Navigation;

public class OsirionReadMoreLinkTests : OsirionComponentTestBase
{
    [Fact]
    public void ReadMoreLink_Defaults_RendersTextAndRightChevronWithTextAsAriaLabel()
    {
        var cut = RenderStatic<OsirionReadMoreLink>(parameters => parameters
            .Add(p => p.Href, "/blog/post"));

        var link = cut.Find("a");
        link.GetAttribute("href").ShouldBe("/blog/post");
        link.GetAttribute("aria-label").ShouldBe("Read more");
        link.QuerySelector(".osirion-read-more-text")!.TextContent.ShouldBe("Read more");
        cut.FindAll(".osirion-read-more-icon-left").ShouldBeEmpty();
        cut.Find(".osirion-read-more-icon-right svg path").GetAttribute("d")!.ShouldStartWith("M4.646 1.646");
    }

    [Fact]
    public void ReadMoreLink_Defaults_RendersDefaultClasses()
    {
        var cut = RenderStatic<OsirionReadMoreLink>(parameters => parameters
            .Add(p => p.Href, "/post"));

        cut.Find("a").ClassList.ShouldBe(
        [
            "osirion-read-more",
            "osirion-link",
            "osirion-read-more-default",
            "osirion-read-more-normal",
            "osirion-read-more-animated",
            "osirion-read-more-icon-right"
        ]);
    }

    [Fact]
    public void ReadMoreLink_ExplicitAriaLabel_IsKept()
    {
        var cut = RenderStatic<OsirionReadMoreLink>(parameters => parameters
            .Add(p => p.Href, "/post")
            .Add(p => p.Text, "Continue")
            .Add(p => p.AriaLabel, "Continue reading the post"));

        cut.Find("a").GetAttribute("aria-label").ShouldBe("Continue reading the post");
    }

    [Fact]
    public void ReadMoreLink_NoAriaLabel_UsesTheText()
    {
        var cut = RenderStatic<OsirionReadMoreLink>(parameters => parameters
            .Add(p => p.Href, "/post")
            .Add(p => p.Text, "Continue"));

        cut.Find("a").GetAttribute("aria-label").ShouldBe("Continue");
    }

    // Fails before the fix: OnInitialized copied Text into the AriaLabel parameter once, so after Text changed the
    // link still announced the first text.
    [Fact]
    public void ReadMoreLink_NoAriaLabel_FollowsTextWhenItChanges()
    {
        var cut = RenderStatic<OsirionReadMoreLink>(parameters => parameters
            .Add(p => p.Href, "/post")
            .Add(p => p.Text, "Continue"));

        cut.Render(parameters => parameters
            .Add(p => p.Href, "/post")
            .Add(p => p.Text, "Keep reading"));

        cut.Find("a").GetAttribute("aria-label").ShouldBe("Keep reading");
        cut.Instance.AriaLabel.ShouldBeNull();
    }

    [Fact]
    public void ReadMoreLink_ExplicitAriaLabel_IsKeptWhenTextChanges()
    {
        var cut = RenderStatic<OsirionReadMoreLink>(parameters => parameters
            .Add(p => p.Href, "/post")
            .Add(p => p.Text, "Continue")
            .Add(p => p.AriaLabel, "Continue reading the post"));

        cut.Render(parameters => parameters
            .Add(p => p.Href, "/post")
            .Add(p => p.Text, "Keep reading")
            .Add(p => p.AriaLabel, "Continue reading the post"));

        cut.Find("a").GetAttribute("aria-label").ShouldBe("Continue reading the post");
    }

    [Fact]
    public void ReadMoreLink_IconOnTheLeft_RendersLeftIconOnly()
    {
        var cut = RenderStatic<OsirionReadMoreLink>(parameters => parameters
            .Add(p => p.Href, "/post")
            .Add(p => p.IconPosition, IconPosition.Left));

        cut.FindAll(".osirion-read-more-icon-left svg").Count.ShouldBe(1);
        cut.FindAll(".osirion-read-more-icon-right").ShouldBeEmpty();
        cut.Find("a").ClassList.ShouldContain("osirion-read-more-icon-left");
    }

    [Theory]
    [InlineData(IconPosition.Left, ".osirion-read-more-icon-left")]
    [InlineData(IconPosition.Right, ".osirion-read-more-icon-right")]
    public void ReadMoreLink_CustomIcon_ReplacesTheDefaultIcon(IconPosition position, string iconSelector)
    {
        var cut = RenderStatic<OsirionReadMoreLink>(parameters => parameters
            .Add(p => p.Href, "/post")
            .Add(p => p.IconPosition, position)
            .Add(p => p.IconContent, builder => builder.AddMarkupContent(0, "<i class=\"custom-icon\"></i>")));

        var icon = cut.Find(iconSelector);
        icon.QuerySelector("i.custom-icon").ShouldNotBeNull();
        icon.QuerySelector("svg").ShouldBeNull();
    }

    [Fact]
    public void ReadMoreLink_TextAndIconHidden_RendersOnlyChildContent()
    {
        var cut = RenderStatic<OsirionReadMoreLink>(parameters => parameters
            .Add(p => p.Href, "/post")
            .Add(p => p.ShowText, false)
            .Add(p => p.ShowIcon, false)
            .AddChildContent("<span class=\"child\">More</span>"));

        var link = cut.Find("a");
        link.QuerySelector(".osirion-read-more-text").ShouldBeNull();
        link.QuerySelector(".osirion-read-more-icon").ShouldBeNull();
        link.QuerySelector(".child")!.TextContent.ShouldBe("More");
        link.ClassList.ShouldNotContain("osirion-read-more-icon-right");
    }

    [Theory]
    [InlineData(ReadMoreVariant.Arrow, 1, "M1 8a.5.5")]
    [InlineData(ReadMoreVariant.External, 2, "M8.636 3.5")]
    [InlineData(ReadMoreVariant.Download, 2, "M.5 9.9")]
    [InlineData(ReadMoreVariant.Plain, 1, "M4.646 1.646")]
    public void ReadMoreLink_Variant_RendersItsDefaultIcon(ReadMoreVariant variant, int pathCount, string firstPathStart)
    {
        var cut = RenderStatic<OsirionReadMoreLink>(parameters => parameters
            .Add(p => p.Href, "/post")
            .Add(p => p.Variant, variant));

        var paths = cut.FindAll(".osirion-read-more-icon svg path");
        paths.Count.ShouldBe(pathCount);
        paths[0].GetAttribute("d")!.ShouldStartWith(firstPathStart);
        cut.Find("a").ClassList.ShouldContain($"osirion-read-more-{variant.ToString().ToLowerInvariant()}");
    }

    [Fact]
    public void ReadMoreLink_LayoutOptions_RendersMatchingClassesStyleAndTarget()
    {
        var cut = RenderStatic<OsirionReadMoreLink>(parameters => parameters
            .Add(p => p.Href, "https://example.com")
            .Add(p => p.Size, LinkSize.Large)
            .Add(p => p.Stretched, true)
            .Add(p => p.Block, true)
            .Add(p => p.Animated, false)
            .Add(p => p.Target, "_blank")
            .Add(p => p.Style, "color: red")
            .Add(p => p.Class, "card-link"));

        var link = cut.Find("a");
        link.ClassList.ShouldBe(
        [
            "osirion-read-more",
            "osirion-link",
            "osirion-read-more-default",
            "osirion-read-more-large",
            "osirion-read-more-stretched",
            "osirion-stretched",
            "osirion-read-more-block",
            "osirion-read-more-icon-right",
            "card-link"
        ]);
        link.GetAttribute("target").ShouldBe("_blank");
        link.GetAttribute("style").ShouldBe("color: red;");
    }

    [Fact]
    public void ReadMoreLink_NoStyle_RendersNoStyleAttribute()
    {
        var cut = RenderStatic<OsirionReadMoreLink>(parameters => parameters
            .Add(p => p.Href, "/post"));

        // An empty style="" is still an inline style attribute to a Content Security Policy
        cut.Find("a").HasAttribute("style").ShouldBeFalse();
    }

    // The filter keeps the framework classes; the component's own osirion-read-more classes are covered elsewhere.
    [Theory]
    [InlineData(CssFramework.Bootstrap, false, new[] { "icon-link", "icon-link-hover" })]
    [InlineData(CssFramework.Bootstrap, true, new[] { "stretched-link" })]
    [InlineData(CssFramework.FluentUI, true, new[] { "osirion-fluent-link", "osirion-fluent-stretched" })]
    [InlineData(CssFramework.MudBlazor, true, new[] { "mud-link", "mud-stretched" })]
    [InlineData(CssFramework.Radzen, true, new[] { "rz-link", "rz-stretched" })]
    public void ReadMoreLink_Framework_AddsExactlyTheFrameworkLinkAndStretchedClasses(
        CssFramework framework, bool stretched, string[] expectedFrameworkClasses)
    {
        Services.AddSingleton(Options.Create(new ThemingOptions { Framework = framework }));

        var cut = RenderStatic<OsirionReadMoreLink>(parameters => parameters
            .Add(p => p.Href, "/post")
            .Add(p => p.Stretched, stretched));

        cut.Find("a").ClassList.Where(c => !c.StartsWith("osirion-read-more", StringComparison.Ordinal))
            .ShouldBe(expectedFrameworkClasses, ignoreOrder: true);
    }
}
