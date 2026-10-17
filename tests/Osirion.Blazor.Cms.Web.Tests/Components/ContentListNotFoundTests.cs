using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Repositories;
using Osirion.Blazor.Cms.Domain.Services;
using Osirion.Blazor.Cms.Web.Components;
using Osirion.Blazor.Cms.Web.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.Components;

public class ContentListNotFoundTests : CmsWebComponentTestBase
{
    private readonly IContentProvider _provider = Substitute.For<IContentProvider>();
    private readonly RecordingNavigationManager _navigation = new();

    public ContentListNotFoundTests()
    {
        Services.AddSingleton<NavigationManager>(_navigation);
        Services.GetRequiredService<IContentProviderManager>().GetDefaultProvider().Returns(_provider);
        ReturnItems();
    }

    // Fails before the fix: /tag/<a tag no post uses> answered 200 with an empty list.
    [Fact]
    public void ContentListPage_TagNoItemUses_SignalsNotFoundAndKeepsTheNoContentText()
    {
        var cut = RenderStatic<ContentListPage>(p => p.Add(c => c.Tag, "no-such-tag"));

        _navigation.NotFoundCalls.ShouldBe(1);
        cut.Find(".osirion-no-content").TextContent.ShouldContain("Nothing has been published here yet.");
    }

    [Fact]
    public void ContentListPage_EmptyDirectoryWithoutFilter_IsAnOrdinaryPage()
    {
        RenderStatic<ContentListPage>(p => p.Add(c => c.Directory, "blog"));

        _navigation.NotFoundCalls.ShouldBe(0);
    }

    [Theory]
    [InlineData("category")]
    [InlineData("tag")]
    [InlineData("author")]
    public void ContentList_FilterMatchesNothing_SignalsNotFound(string filter)
    {
        RenderStatic<ContentList>(p => p
            .Add(c => c.NotFoundWhenFilterMatchesNothing, true)
            .Add(c => c.Category, filter == "category" ? "missing" : null)
            .Add(c => c.Tag, filter == "tag" ? "missing" : null)
            .Add(c => c.Author, filter == "author" ? "missing" : null));

        _navigation.NotFoundCalls.ShouldBe(1);
    }

    [Fact]
    public void ContentList_PagedFilterMatchesNothing_SignalsNotFound()
    {
        RenderStatic<ContentList>(p => p
            .Add(c => c.NotFoundWhenFilterMatchesNothing, true)
            .Add(c => c.ShowPagination, true)
            .Add(c => c.Category, "missing"));

        _navigation.NotFoundCalls.ShouldBe(1);
    }

    [Fact]
    public void ContentList_FilterMatchesItems_DoesNotSignalNotFound()
    {
        ReturnItems(ContentItem.Create("id-1", "First post", "<p>Body</p>", "blog/first-post.md", "test"));

        RenderStatic<ContentList>(p => p
            .Add(c => c.NotFoundWhenFilterMatchesNothing, true)
            .Add(c => c.Tag, "blazor"));

        _navigation.NotFoundCalls.ShouldBe(0);
    }

    [Fact]
    public void ContentList_OptionOff_AnEmptyFilteredListIsAnOrdinaryPage()
    {
        RenderStatic<ContentList>(p => p.Add(c => c.Tag, "missing"));

        _navigation.NotFoundCalls.ShouldBe(0);
    }

    [Fact]
    public void ContentList_ProviderThrows_DoesNotSignalNotFound()
    {
        _provider.GetItemsByQueryAsync(Arg.Any<ContentQuery>())
            .Returns<Task<IReadOnlyList<ContentItem>?>>(_ => throw new InvalidOperationException("down"));

        RenderStatic<ContentList>(p => p
            .Add(c => c.NotFoundWhenFilterMatchesNothing, true)
            .Add(c => c.Tag, "blazor"));

        _navigation.NotFoundCalls.ShouldBe(0);
    }

    [Fact]
    public void ContentList_RenderedAgainWithTheSameFilter_SignalsNotFoundOnce()
    {
        var cut = RenderStatic<ContentList>(p => p
            .Add(c => c.NotFoundWhenFilterMatchesNothing, true)
            .Add(c => c.Tag, "missing"));
        cut.Render(p => p.Add(c => c.Tag, "missing"));

        _navigation.NotFoundCalls.ShouldBe(1);
    }

    private void ReturnItems(params ContentItem[] items) =>
        _provider.GetItemsByQueryAsync(Arg.Any<ContentQuery>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>?>(items));
}
