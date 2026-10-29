using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.Logging;
using Osirion.Blazor.Cms.Domain.ContentBlocks;
using Osirion.Blazor.Cms.Web.ContentBlocks;
using Osirion.Blazor.Cms.Web.DependencyInjection;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.ContentBlocks;

/// <summary>
/// Site renderers, components and fallbacks, and what happens to a block nothing can render.
/// </summary>
public class ContentBlockRegistryTests : ContentBlockTestBase
{
    [Fact]
    public void AddRenderer_ReplacesTheLibraryRendererOfAType()
    {
        Services.AddOsirionContentBlocks(blocks => blocks.AddRenderer<SiteHeroRenderer>("hero"));

        var cut = RenderBlocks("blocks:\n  - { type: hero, title: Welcome }\n");

        cut.Find("p.site-hero").TextContent.ShouldBe("Welcome");
        cut.FindAll("h1").ShouldBeEmpty();
    }

    [Fact]
    public void AddRenderer_RendersANewTypeWithItsValues()
    {
        Services.AddOsirionContentBlocks(blocks => blocks.AddRenderer<SitePricingRenderer>("pricing-table"));

        var cut = RenderBlocks("blocks:\n  - { type: pricing-table, title: Prices, currency: EUR }\n");

        cut.Find("p.site-pricing").TextContent.ShouldBe("Prices in EUR");
    }

    [Fact]
    public void FallbackRenderer_RendersATypeWithoutRenderer()
    {
        Services.AddOsirionContentBlocks(blocks => blocks.UseFallbackRenderer<SiteFallbackRenderer>());

        var cut = RenderBlocks("blocks:\n  - { type: testimonial, title: Ada }\n");

        cut.Find("p.site-fallback").TextContent.ShouldBe("testimonial");
    }

    [Fact]
    public void UnknownTypeWithoutFallback_RendersNothingAndLogsAnErrorInProduction()
    {
        var cut = RenderBlocks("blocks:\n  - { type: markdown, markdown: before }\n  - { type: testimonial, title: Ada }\n");

        cut.FindAll(".osirion-content-block-problem").ShouldBeEmpty();
        cut.Find(".osirion-content-blocks").TextContent.ShouldNotContain("Ada");
        var error = Logs.Problems.ShouldHaveSingleItem();
        error.Message.ShouldContain(SourcePath);
        error.Message.ShouldContain("blocks[1]");
        error.Message.ShouldContain("testimonial");
    }

    [Fact]
    public void UnknownTypeWithoutFallback_RendersANoticeInDevelopment()
    {
        UseDevelopment();

        var cut = RenderBlocks("blocks:\n  - { type: testimonial, title: Ada }\n");

        var notice = cut.Find(".osirion-content-block-problem");
        notice.GetAttribute("role").ShouldBe("alert");
        notice.TextContent.ShouldContain("en/home.md: blocks[0] (testimonial): no renderer is registered for type 'testimonial'");
    }

    [Fact]
    public void Component_RendersTheRegisteredComponentWithItsParameters()
    {
        Services.AddOsirionContentBlocks(blocks => blocks.AddComponent<ProductTour>("product-tour"));

        var cut = RenderBlocks("blocks:\n  - { type: component, name: product-tour, parameters: { start: 2, mode: compact } }\n");

        cut.Find("div.tour").TextContent.ShouldBe("mode=compact;start=2");
    }

    [Fact]
    public void Component_WithoutParametersParameter_RendersWhenTheBlockHasNone()
    {
        Services.AddOsirionContentBlocks(blocks => blocks.AddComponent<Badge>("badge"));

        var cut = RenderBlocks("blocks:\n  - { type: component, name: badge }\n");

        cut.Find("span.badge").TextContent.ShouldBe("New");
    }

    [Fact]
    public void Component_ParametersForAComponentWithoutThem_IsAProblem()
    {
        Services.AddOsirionContentBlocks(blocks => blocks.AddComponent<Badge>("badge"));

        var cut = RenderBlocks("blocks:\n  - { type: component, name: badge, parameters: { a: b } }\n");

        cut.FindAll("span.badge").ShouldBeEmpty();
        Logs.Problems.ShouldHaveSingleItem().Message.ShouldContain("has no Parameters parameter");
    }

    [Fact]
    public void Component_UnknownName_IsAProblem()
    {
        UseDevelopment();

        var cut = RenderBlocks("blocks:\n  - { type: component, name: product-tour }\n");

        cut.Find(".osirion-content-block-problem").TextContent.ShouldContain("no component is registered as 'product-tour'");
        Logs.Problems.ShouldHaveSingleItem().Level.ShouldBe(LogLevel.Error);
    }

    [Fact]
    public void AddIcon_GivesACardItsIcon_AndAnUnknownNameNone()
    {
        Services.AddOsirionContentBlocks(blocks => blocks.AddIcon("shield", icon =>
        {
            icon.OpenElement(0, "svg");
            icon.AddAttribute(1, "class", "site-shield");
            icon.CloseElement();
        }));

        var cut = RenderBlocks("blocks:\n  - { type: cards, items: [{ title: A, icon: shield }, { title: B, icon: rocket }, { title: C }] }\n");

        var cards = cut.FindAll(".osirion-feature-card");
        cards[0].QuerySelector(".osirion-feature-card-icon svg.site-shield").ShouldNotBeNull();
        cards[1].QuerySelectorAll(".osirion-feature-card-icon").ShouldBeEmpty();
        cards[2].QuerySelectorAll(".osirion-feature-card-icon").ShouldBeEmpty();
        Logs.Problems.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("Shield")]
    [InlineData("")]
    public void AddIcon_InvalidName_Throws(string name)
    {
        Should.Throw<ArgumentException>(() => new ContentBlockOptions().AddIcon(name, _ => { }));
    }

    [Theory]
    [InlineData("Hero")]
    [InlineData("pricing_table")]
    [InlineData("")]
    public void AddRenderer_InvalidTypeName_Throws(string type)
    {
        Should.Throw<ArgumentException>(() => new ContentBlockOptions().AddRenderer<SiteHeroRenderer>(type));
    }

    [Fact]
    public void RendererForTheWrongBlockClass_ThrowsAClearMessage()
    {
        Services.AddOsirionContentBlocks(blocks => blocks.AddRenderer<SiteHeroRenderer>("cta"));

        var exception = Should.Throw<InvalidOperationException>(() => RenderBlocks("blocks:\n  - { type: cta, title: T }\n"));

        exception.Message.ShouldContain("SiteHeroRenderer renders HeroBlock blocks and was given a CtaBlock");
    }

    private sealed class SiteHeroRenderer : ContentBlockRenderer<HeroBlock>
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "p");
            builder.AddAttribute(1, "class", "site-hero");
            builder.AddContent(2, Model.Title);
            builder.CloseElement();
        }
    }

    private sealed class SitePricingRenderer : ContentBlockRenderer<CustomContentBlock>
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "p");
            builder.AddAttribute(1, "class", "site-pricing");
            builder.AddContent(2, $"{Model.Title} in {Model.Values["currency"]}");
            builder.CloseElement();
        }
    }

    private sealed class SiteFallbackRenderer : ContentBlockRenderer
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "p");
            builder.AddAttribute(1, "class", "site-fallback");
            builder.AddContent(2, Block.Type);
            builder.CloseElement();
        }
    }

    private sealed class ProductTour : ComponentBase
    {
        [Parameter]
        public IReadOnlyDictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "tour");
            builder.AddContent(2, string.Join(";", Parameters.OrderBy(pair => pair.Key, StringComparer.Ordinal).Select(pair => $"{pair.Key}={pair.Value}")));
            builder.CloseElement();
        }
    }

    private sealed class Badge : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "span");
            builder.AddAttribute(1, "class", "badge");
            builder.AddContent(2, "New");
            builder.CloseElement();
        }
    }
}
