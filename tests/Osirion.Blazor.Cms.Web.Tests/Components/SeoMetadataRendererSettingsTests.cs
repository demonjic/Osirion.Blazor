using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Osirion.Blazor.Cms.Domain.ValueObjects;
using Osirion.Blazor.Cms.Web.Components;
using Osirion.Blazor.Cms.Web.Options;
using Osirion.Blazor.Cms.Web.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.Components;

/// <summary>
/// Pins how page parameters, the configured options and the front matter combine, including the page switching
/// off what the options turned on and the AI content declaration that only the site or the page may make.
/// </summary>
public class SeoMetadataRendererSettingsTests : CmsWebComponentTestBase
{
    private static readonly string[] AiContentDeclarationNames =
        ["ai-content-declaration", "ai-content-detection", "ai-content-attribution"];

    // Fails before the fix: every page said "human-created", whatever its content was.
    [Fact]
    public void SeoMetadataRenderer_NoDeclaration_WritesNoAiContentDeclarationTags()
    {
        var cut = RenderPage(_ => { });

        foreach (var name in AiContentDeclarationNames)
        {
            cut.FindAll($"meta[name='{name}']").ShouldBeEmpty(name);
        }
    }

    [Fact]
    public void SeoMetadataRenderer_FrontMatterDeclaration_IsWritten()
    {
        var item = SeoTestContent.Article();
        item.Metadata = new FrontMatter
        {
            SeoProperties = new SeoMetadata
            {
                MetaTags = new Dictionary<string, string> { ["ai-content-declaration"] = "ai-assisted" }
            }
        };

        var cut = RenderPage(_ => { }, item);

        Meta(cut, "ai-content-declaration").ShouldBe("ai-assisted");
    }

    [Fact]
    public void SeoMetadataRenderer_ConfiguredDeclaration_IsWrittenOnEveryPage()
    {
        Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new SeoMetadataOptions
        {
            AdditionalMetaTags = new Dictionary<string, string> { ["ai-content-declaration"] = "human-created" }
        }));

        var cut = RenderPage(_ => { });

        Meta(cut, "ai-content-declaration").ShouldBe("human-created");
    }

    [Fact]
    public void SeoMetadataRenderer_FrontMatterDeclaration_ReplacesTheConfiguredOne()
    {
        Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new SeoMetadataOptions
        {
            AdditionalMetaTags = new Dictionary<string, string> { ["ai-content-declaration"] = "human-created" }
        }));
        var item = SeoTestContent.Article();
        item.Metadata = new FrontMatter
        {
            SeoProperties = new SeoMetadata
            {
                MetaTags = new Dictionary<string, string> { ["AI-Content-Declaration"] = "ai-assisted" }
            }
        };

        var cut = RenderPage(_ => { }, item);

        Meta(cut, "ai-content-declaration").ShouldBe("ai-assisted");
    }

    [Fact]
    public void SeoMetadataRenderer_ConfiguredMetaTagWithUnsafeName_IsSkipped()
    {
        Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new SeoMetadataOptions
        {
            AdditionalMetaTags = new Dictionary<string, string> { ["x\" onload=\"alert(1)"] = "value" }
        }));

        var cut = RenderPage(_ => { });

        cut.Markup.ShouldNotContain("onload");
    }

    // Fails before the fix: an explicit 1200 was read as "not set" and replaced by the configured 800.
    [Fact]
    public void SeoMetadataRenderer_ExplicitDefaultImageSize_WinsOverTheConfiguredSize()
    {
        Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(
            new SeoMetadataOptions { DefaultImageUrl = "/default.png", DefaultImageWidth = 800, DefaultImageHeight = 400 }));

        var cut = RenderPage(p => p
            .Add(c => c.DefaultImageWidth, 1200)
            .Add(c => c.DefaultImageHeight, 630), ItemWithoutImage());

        Property(cut, "og:image:width").ShouldBe("1200");
        Property(cut, "og:image:height").ShouldBe("630");
    }

    [Fact]
    public void SeoMetadataRenderer_NoDefaultImageSize_UsesTheConfiguredSize()
    {
        Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(
            new SeoMetadataOptions { DefaultImageUrl = "/default.png", DefaultImageWidth = 800, DefaultImageHeight = 400 }));

        var cut = RenderPage(_ => { }, ItemWithoutImage());

        Property(cut, "og:image:width").ShouldBe("800");
        Property(cut, "og:image:height").ShouldBe("400");
    }

    // The default image takes the size of the options, 1200 by 630 unless the site sets another.
    [Fact]
    public void SeoMetadataRenderer_DefaultImageWithoutAConfiguredSize_TakesTheOptionsDefaultSize()
    {
        var cut = RenderPage(p => p.Add(c => c.DefaultImageUrl, "/default.png"), ItemWithoutImage());

        Property(cut, "og:image").ShouldBe("http://localhost/default.png");
        Property(cut, "og:image:width").ShouldBe("1200");
        Property(cut, "og:image:height").ShouldBe("630");
    }

    // Fails before the fix: 1200 by 630 was declared for any image, so a 800 by 400 placeholder was said to be
    // 1200 by 630. An item's own image has no known size, so none is declared.
    [Fact]
    public void SeoMetadataRenderer_ItemImage_DeclaresNoSizeEvenWithAConfiguredOne()
    {
        Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(
            new SeoMetadataOptions { DefaultImageUrl = "/default.png", DefaultImageWidth = 1200, DefaultImageHeight = 630 }));

        var cut = RenderPage(_ => { });

        Property(cut, "og:image").ShouldBe("http://localhost/images/cover.png");
        cut.FindAll("meta[property='og:image:width']").ShouldBeEmpty();
        cut.FindAll("meta[property='og:image:height']").ShouldBeEmpty();
    }

    [Fact]
    public void SeoMetadataRenderer_ListWithItsOwnImage_DeclaresTheDefaultSizeAndAnItemImageNone()
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo(SeoTestContent.PageUrl);

        var withPageImage = RenderStatic<SeoMetadataRenderer>(builder => builder
            .Add(c => c.ContentItems, [SeoTestContent.Article()])
            .Add(c => c.DefaultImageUrl, "/list.png")
            .Add(c => c.DefaultImageWidth, 1000)
            .Add(c => c.DefaultImageHeight, 500));
        var withItemImage = RenderStatic<SeoMetadataRenderer>(builder => builder
            .Add(c => c.ContentItems, [SeoTestContent.Article()])
            .Add(c => c.DefaultImageWidth, 1000)
            .Add(c => c.DefaultImageHeight, 500));

        Property(withPageImage, "og:image:width").ShouldBe("1000");
        withItemImage.FindAll("meta[property='og:image:width']").ShouldBeEmpty();
    }

    private static Osirion.Blazor.Cms.Domain.Entities.ContentItem ItemWithoutImage()
    {
        var item = SeoTestContent.Article();
        item.FeaturedImageUrl = null;
        return item;
    }

    // Fails before the fix: the page value was combined with the option using |=, so false could not win.
    [Fact]
    public void SeoMetadataRenderer_PageTurnsMobileMetaTagsOff_WinsOverTheOptions()
    {
        Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new SeoMetadataOptions { EmitMobileMetaTags = true }));

        var cut = RenderPage(p => p.Add(c => c.EmitMobileMetaTags, false));

        cut.FindAll("meta[name='viewport']").ShouldBeEmpty();
    }

    [Fact]
    public void SeoMetadataRenderer_OptionsTurnMobileMetaTagsOn_WhenThePageSetsNothing()
    {
        Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new SeoMetadataOptions { EmitMobileMetaTags = true }));

        var cut = RenderPage(_ => { });

        cut.FindAll("meta[name='viewport']").ShouldHaveSingleItem();
    }

    // Fails before the fix: same |= merge, so the page could not switch the generated answers off.
    [Fact]
    public void SeoMetadataRenderer_PageTurnsGeneratedFaqOff_WinsOverTheOptions()
    {
        Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new SeoMetadataOptions { EnableGeneratedFaq = true }));

        var cut = RenderPage(p => p.Add(c => c.EnableGeneratedFaq, false));

        cut.FindAll("meta[name='answers-to']").ShouldBeEmpty();
    }

    [Fact]
    public void SeoMetadataRenderer_OptionsTurnGeneratedFaqOn_WhenThePageSetsNothing()
    {
        Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new SeoMetadataOptions { EnableGeneratedFaq = true }));

        var cut = RenderPage(_ => { });

        cut.FindAll("meta[name='answers-to']").ShouldHaveSingleItem();
    }

    private IRenderedComponent<SeoMetadataRenderer> RenderPage(
        Action<ComponentParameterCollectionBuilder<SeoMetadataRenderer>> parameters,
        Osirion.Blazor.Cms.Domain.Entities.ContentItem? item = null)
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo(SeoTestContent.PageUrl);
        return RenderStatic<SeoMetadataRenderer>(builder =>
        {
            builder.Add(c => c.Content, item ?? SeoTestContent.Article());
            parameters(builder);
        });
    }

    private static string? Meta(IRenderedComponent<SeoMetadataRenderer> cut, string name) =>
        cut.FindAll($"meta[name='{name}']").ShouldHaveSingleItem().GetAttribute("content");

    private static string? Property(IRenderedComponent<SeoMetadataRenderer> cut, string property) =>
        cut.FindAll($"meta[property='{property}']").ShouldHaveSingleItem().GetAttribute("content");
}
