using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Osirion.Blazor.Cms.Web.Components;
using Osirion.Blazor.Cms.Web.Options;
using Osirion.Blazor.Cms.Web.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.Components;

/// <summary>
/// Pins which image a list page shares (issue 291): the image the page sets, then the first item's featured image,
/// then the site-wide default image.
/// </summary>
public class SeoMetadataRendererCollectionImageTests : CmsWebComponentTestBase
{
    // Fails before the fix: the first item's image came before the page's own image.
    [Fact]
    public void SeoMetadataRenderer_CollectionWithItsOwnImage_SharesItBeforeTheFirstItemsImage()
    {
        var cut = RenderPage(p => p
            .Add(c => c.ContentItems, [SeoTestContent.Article(), SeoTestContent.BarePost()])
            .Add(c => c.DefaultImageUrl, "/images/blog-card.png"));

        Property(cut, "og:image").ShouldBe("http://localhost/images/blog-card.png");
        Meta(cut, "twitter:image").ShouldBe("http://localhost/images/blog-card.png");
    }

    // Fails before the fix: a page's image lost to the first item even when a site-wide default was configured.
    [Fact]
    public void SeoMetadataRenderer_CollectionWithItsOwnImage_SharesItBeforeTheSiteWideDefault()
    {
        UseSiteWideDefaultImage("/images/site-card.png");

        var cut = RenderPage(p => p
            .Add(c => c.ContentItems, [SeoTestContent.Article()])
            .Add(c => c.DefaultImageUrl, "/images/blog-card.png"));

        Property(cut, "og:image").ShouldBe("http://localhost/images/blog-card.png");
    }

    [Fact]
    public void SeoMetadataRenderer_CollectionWithoutItsOwnImage_SharesTheFirstItemsImageBeforeTheSiteWideDefault()
    {
        UseSiteWideDefaultImage("/images/site-card.png");

        var cut = RenderPage(p => p.Add(c => c.ContentItems, [SeoTestContent.Article(), SeoTestContent.BarePost()]));

        Property(cut, "og:image").ShouldBe("http://localhost/images/cover.png");
    }

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    public void SeoMetadataRenderer_CollectionWithABlankImage_SharesTheFirstItemsImage(string? pageImage)
    {
        UseSiteWideDefaultImage("/images/site-card.png");

        var cut = RenderPage(p => p
            .Add(c => c.ContentItems, [SeoTestContent.Article()])
            .Add(c => c.DefaultImageUrl, pageImage));

        Property(cut, "og:image").ShouldBe("http://localhost/images/cover.png");
    }

    [Fact]
    public void SeoMetadataRenderer_CollectionWithoutAnyImage_SharesTheSiteWideDefaultLast()
    {
        UseSiteWideDefaultImage("/images/site-card.png");

        var cut = RenderPage(p => p.Add(c => c.ContentItems, [SeoTestContent.BarePost()]));

        Property(cut, "og:image").ShouldBe("http://localhost/images/site-card.png");
    }

    [Fact]
    public void SeoMetadataRenderer_ContentPage_KeepsItsFeaturedImageBeforeTheDefaultImage()
    {
        var cut = RenderPage(p => p
            .Add(c => c.Content, SeoTestContent.Article())
            .Add(c => c.DefaultImageUrl, "/images/blog-card.png"));

        Property(cut, "og:image").ShouldBe("http://localhost/images/cover.png");
    }

    private void UseSiteWideDefaultImage(string imageUrl) =>
        Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new SeoMetadataOptions { DefaultImageUrl = imageUrl }));

    private IRenderedComponent<SeoMetadataRenderer> RenderPage(Action<ComponentParameterCollectionBuilder<SeoMetadataRenderer>> parameters)
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo(SeoTestContent.PageUrl);
        return RenderStatic(parameters);
    }

    private static string? Meta(IRenderedComponent<SeoMetadataRenderer> cut, string name) =>
        cut.FindAll($"meta[name='{name}']").ShouldHaveSingleItem().GetAttribute("content");

    private static string? Property(IRenderedComponent<SeoMetadataRenderer> cut, string property) =>
        cut.FindAll($"meta[property='{property}']").ShouldHaveSingleItem().GetAttribute("content");
}
