using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Repositories;
using Osirion.Blazor.Cms.Domain.Services;
using Osirion.Blazor.Cms.Domain.ValueObjects;
using Osirion.Blazor.Cms.Web.Components;
using Osirion.Blazor.Cms.Web.Options;
using Osirion.Blazor.Cms.Web.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.Components;

/// <summary>
/// The image a list page shares (issue 291): the image the page sets, then the image of the section's own index page
/// (<c>_index.md</c>), then the first item's featured image, then the site-wide default image.
/// </summary>
public class ContentListShareImageTests : CmsWebComponentTestBase
{
    private const string Origin = "http://localhost";

    private readonly IContentProvider _provider = Substitute.For<IContentProvider>();

    // Registered before the first service is read; a test sets the site-wide default image on it.
    private readonly SeoMetadataOptions _seoOptions = new();

    public ContentListShareImageTests()
    {
        Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(_seoOptions));
        Services.GetRequiredService<IContentProviderManager>().GetDefaultProvider().Returns(_provider);
    }

    private NavigationManager Navigation => Services.GetRequiredService<NavigationManager>();

    // Fails before the fix: the list shared its newest post's image and had no image of its own to set.
    [Fact]
    public void ContentListPage_ImageUrl_IsSharedBeforeTheIndexPageAndTheFirstItem()
    {
        ReturnItems(Index("/images/index.png"), Post("first-post", "/images/first-post.png"));

        var head = RenderListPage("/blog", "/images/blog-card.png");

        SharedImage(head).ShouldBe("http://localhost/images/blog-card.png");
    }

    // Fails before the fix: the index page was left out of the list and its image with it.
    [Fact]
    public void ContentListPage_OnTheSectionPage_SharesTheIndexPagesImageBeforeTheFirstItems()
    {
        ReturnItems(Index("/images/index.png"), Post("first-post", "/images/first-post.png"));

        var head = RenderListPage("/blog", imageUrl: null);

        SharedImage(head).ShouldBe("http://localhost/images/index.png");
    }

    // Fails before the fix: the index page's og_image_url was never read for the list.
    [Fact]
    public void ContentListPage_OnTheSectionPage_PrefersTheIndexPagesOpenGraphImage()
    {
        var index = Index("/images/index.png");
        index.SetSeoMetadata(new SeoMetadata { OgImageUrl = "https://cdn.example.com/blog-og.png" });
        ReturnItems(index, Post("first-post", "/images/first-post.png"));

        var head = RenderListPage("/blog", imageUrl: null);

        SharedImage(head).ShouldBe("https://cdn.example.com/blog-og.png");
    }

    [Fact]
    public void ContentListPage_IndexPageWithoutAnImage_SharesTheFirstItemsImage()
    {
        ReturnItems(Index(imageUrl: null), Post("first-post", "/images/first-post.png"));

        var head = RenderListPage("/blog", imageUrl: null);

        SharedImage(head).ShouldBe("http://localhost/images/first-post.png");
    }

    [Fact]
    public void ContentListPage_NoImageAnywhere_SharesTheSiteWideDefaultLast()
    {
        _seoOptions.DefaultImageUrl = "/images/site-card.png";
        ReturnItems(Index(imageUrl: null), Post("first-post", imageUrl: null));

        var head = RenderListPage("/blog", imageUrl: null);

        SharedImage(head).ShouldBe("http://localhost/images/site-card.png");
    }

    [Fact]
    public void ContentListPage_SiteWideDefault_DoesNotReplaceTheFirstItemsImage()
    {
        _seoOptions.DefaultImageUrl = "/images/site-card.png";
        ReturnItems(Post("first-post", "/images/first-post.png"));

        var head = RenderListPage("/blog/tag/blazor", imageUrl: null);

        SharedImage(head).ShouldBe("http://localhost/images/first-post.png");
    }

    [Fact]
    public void ContentList_PageImageUrl_IsShared()
    {
        ReturnItems(Post("first-post", "/images/first-post.png"));
        var head = Render<HeadOutlet>();
        Navigation.NavigateTo(Origin + "/news");

        RenderStatic<ContentList>(p => p.Add(c => c.PageImageUrl, "/images/news-card.png"));

        SharedImage(head).ShouldBe("http://localhost/images/news-card.png");
    }

    private IRenderedComponent<HeadOutlet> RenderListPage(string relativeUrl, string? imageUrl)
    {
        var head = Render<HeadOutlet>();
        Navigation.NavigateTo(Origin + relativeUrl);
        RenderStatic<ContentListPage>(p => p
            .Add(c => c.Directory, "blog")
            .Add(c => c.ImageUrl, imageUrl));
        return head;
    }

    private static string? SharedImage(IRenderedComponent<HeadOutlet> head) =>
        head.FindAll("meta[property='og:image']").ShouldHaveSingleItem().GetAttribute("content");

    private static ContentItem Index(string? imageUrl) => Item("blog-index", "Blog", "blog/_index.md", "blog", imageUrl);

    private static ContentItem Post(string slug, string? imageUrl) =>
        Item($"id-{slug}", slug, $"blog/{slug}.md", $"blog/{slug}", imageUrl);

    private static ContentItem Item(string id, string title, string path, string url, string? imageUrl)
    {
        var item = ContentItem.Create(id, title, "<p>Body</p>", path, "test");
        item.SetUrl(url);
        item.FeaturedImageUrl = imageUrl;
        return item;
    }

    private void ReturnItems(params ContentItem[] items) =>
        _provider.GetItemsByQueryAsync(Arg.Any<ContentQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>?>(items));
}
