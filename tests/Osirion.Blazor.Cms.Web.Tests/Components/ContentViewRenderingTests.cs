using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Enums;
using Osirion.Blazor.Cms.Domain.Repositories;
using Osirion.Blazor.Cms.Domain.Services;
using Osirion.Blazor.Cms.Web.Components;
using Osirion.Blazor.Cms.Web.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.Components;

public class ContentViewRenderingTests : CmsWebComponentTestBase
{
    private readonly IContentProvider _provider = Substitute.For<IContentProvider>();
    private readonly IContentProviderManager _manager = Substitute.For<IContentProviderManager>();
    private readonly RecordingLoggerFactory _logs = new();

    public ContentViewRenderingTests()
    {
        Services.AddSingleton(_manager);
        _logs.Register(Services);
        _manager.GetDefaultProvider().Returns(_provider);
    }

    [Fact]
    public void ContentView_NoPathAndNoItem_ShowsNotFoundWithoutLoading()
    {
        var cut = RenderStatic<ContentView>(p => p
            .Add(c => c.NotFoundText, "Missing")
            .Add(c => c.Class, "extra"));

        cut.Find("div.osirion-content-view").ClassList.ShouldContain("extra");
        cut.Find(".osirion-content-not-found-message").TextContent.ShouldBe("Missing");
        _provider.DidNotReceive().GetItemByPathAsync(Arg.Any<string>());
    }

    [Fact]
    public void ContentView_NoDefaultProvider_ShowsNotFound()
    {
        _manager.GetDefaultProvider().Returns((IContentProvider?)null);

        var cut = RenderStatic<ContentView>(p => p.Add(c => c.Path, "blog/post.md"));

        cut.FindAll(".osirion-content-not-found").ShouldHaveSingleItem();
    }

    [Fact]
    public void ContentView_ProviderThrows_LogsAndShowsNotFound()
    {
        _provider.GetItemByPathAsync("blog/post.md").Returns<Task<ContentItem?>>(_ => throw new InvalidOperationException("down"));

        var cut = RenderStatic<ContentView>(p => p.Add(c => c.Path, "blog/post.md"));

        _logs.Entries.ShouldContain(e => e.Level == LogLevel.Error && e.Exception is InvalidOperationException);
        cut.FindAll(".osirion-content-not-found").ShouldHaveSingleItem();
        cut.Markup.ShouldNotContain("down");
    }

    [Fact]
    public void ContentView_InteractiveWhileLoading_ShowsTheLoadingText()
    {
        var pending = new TaskCompletionSource<ContentItem?>();
        _provider.GetItemByPathAsync("blog/post.md").Returns(pending.Task);
        SetRendererInfo(new RendererInfo("Server", true));

        var cut = Render<ContentView>(p => p
            .Add(c => c.Path, "blog/post.md")
            .Add(c => c.LoadingText, "Hold on"));

        cut.Find(".osirion-content-loading p").TextContent.ShouldBe("Hold on");

        pending.SetResult(Item("post"));

        cut.WaitForAssertion(() => cut.Find(".osirion-content-title").TextContent.ShouldBe("post"));
    }

    [Fact]
    public void ContentView_FullItem_RendersAuthorImageTagsAndCategoriesWithDefaultLinks()
    {
        using var culture = new CultureScope("en-US");
        var item = Item("post", tags: ["C#"], categories: ["News"]);
        item.SetAuthor("Ada");
        item.SetFeaturedImage("/img/post.png");
        item.SetCreatedDate(new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc));

        var cut = RenderStatic<ContentView>(p => p.Add(c => c.Item, item));

        cut.Find(".osirion-content-author").TextContent.ShouldContain("Ada");
        cut.Find(".osirion-content-date").TextContent.ShouldContain("Jan 2, 2026");
        cut.Find("img.osirion-content-featured-image").GetAttribute("src").ShouldBe("/img/post.png");
        cut.FindAll("a.osirion-content-tag").Select(a => a.GetAttribute("href")).ShouldBe(["/tag/c%23", "/tag/c%23"]);
        cut.Find("a.osirion-content-category").GetAttribute("href").ShouldBe("/category/news");
    }

    // Fails before the fix: the date followed the server's culture instead of the content's language.
    [Theory]
    [InlineData("sr-Latn-RS", "en", "Oct 17, 2026")]
    [InlineData("en-US", "sr-Latn-RS", "okt 17, 2026")]
    [InlineData("sr-Latn-RS", "not-a-culture", "okt 17, 2026")]
    public void ContentView_Date_FollowsTheContentLanguage(string serverCulture, string locale, string expected)
    {
        using var culture = new CultureScope(serverCulture);
        var item = Item("post");
        item.SetLocale(locale);
        item.SetCreatedDate(new DateTime(2026, 10, 17, 0, 0, 0, DateTimeKind.Utc));

        var cut = RenderStatic<ContentView>(p => p.Add(c => c.Item, item));

        var time = cut.Find(".osirion-content-date time");
        time.GetAttribute("datetime").ShouldBe("2026-10-17");
        time.TextContent.ShouldBe(expected);
    }

    [Fact]
    public void ContentView_PlainItem_RendersNoOptionalSections()
    {
        var cut = RenderStatic<ContentView>(p => p.Add(c => c.Item, Item("post")));

        cut.FindAll(".osirion-content-author").ShouldBeEmpty();
        cut.FindAll("img.osirion-content-featured-image").ShouldBeEmpty();
        cut.FindAll(".osirion-content-tags").ShouldBeEmpty();
        cut.FindAll(".osirion-content-categories").ShouldBeEmpty();
        cut.FindAll(".osirion-content-navigation").ShouldBeEmpty();
    }

    [Fact]
    public void ContentView_Formatters_BuildTheTagCategoryAndNeighbourLinks()
    {
        var item = Item("post", tags: ["C#"], categories: ["News"]);

        var cut = RenderStatic<ContentView>(p => p
            .Add(c => c.Item, item)
            .Add(c => c.PreviousItem, Item("older"))
            .Add(c => c.NextItem, Item("newer"))
            .Add(c => c.ShowNavigationLinks, true)
            .Add(c => c.TagUrlFormatter, t => $"/t/{t}")
            .Add(c => c.CategoryUrlFormatter, c => $"/c/{c}")
            .Add(c => c.ContentUrlFormatter, i => $"/read/{i.Title}"));

        cut.Find("a.osirion-content-tag").GetAttribute("href").ShouldBe("/t/C#");
        cut.Find("a.osirion-content-category").GetAttribute("href").ShouldBe("/c/News");
        cut.Find("a.osirion-content-previous").GetAttribute("href").ShouldBe("/read/older");
        cut.Find("a.osirion-content-next").GetAttribute("href").ShouldBe("/read/newer");
    }

    // Known defect, pinned as current behavior: the default content links keep the ".md" extension of the source file, while ContentPage strips it, so the two disagree on the item URL. Update this expectation when it is fixed.
    [Fact]
    public void ContentView_NavigationLinks_LoadsNeighboursFromTheSameDirectoryNewestFirst()
    {
        var older = Item("older");
        var current = Item("current");
        var newer = Item("newer");
        _provider.GetItemByPathAsync("blog/current.md").Returns(current);
        _provider.GetItemsByQueryAsync(Arg.Any<ContentQuery>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>?>([newer, current, older]));

        var cut = RenderStatic<ContentView>(p => p
            .Add(c => c.Path, "blog/current.md")
            .Add(c => c.ShowNavigationLinks, true));

        _provider.Received(1).GetItemsByQueryAsync(Arg.Is<ContentQuery>(q =>
            q.Directory == "blog" && q.SortBy == SortField.Date
            && q.SortDirection == Osirion.Blazor.Cms.Domain.Enums.SortDirection.Descending));
        var previous = cut.Find("a.osirion-content-previous");
        previous.GetAttribute("href").ShouldBe("/blog/newer.md");
        previous.QuerySelector(".osirion-content-navigation-title")!.TextContent.ShouldBe("newer");
        cut.Find("a.osirion-content-next").GetAttribute("href").ShouldBe("/blog/older.md");
    }

    [Fact]
    public void ContentView_FirstItemInTheDirectory_HasOnlyANextLink()
    {
        var current = Item("current");
        _provider.GetItemByPathAsync("blog/current.md").Returns(current);
        _provider.GetItemsByQueryAsync(Arg.Any<ContentQuery>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>?>([current, Item("older")]));

        var cut = RenderStatic<ContentView>(p => p
            .Add(c => c.Path, "blog/current.md")
            .Add(c => c.ShowNavigationLinks, true));

        cut.FindAll("a.osirion-content-previous").ShouldBeEmpty();
        cut.FindAll("a.osirion-content-next").ShouldHaveSingleItem();
    }

    [Fact]
    public void ContentView_ItemMissingFromTheNeighbourList_HasNoNavigation()
    {
        _provider.GetItemByPathAsync("blog/current.md").Returns(Item("current"));
        _provider.GetItemsByQueryAsync(Arg.Any<ContentQuery>()).Returns(Task.FromResult<IReadOnlyList<ContentItem>?>(null));

        var cut = RenderStatic<ContentView>(p => p
            .Add(c => c.Path, "blog/current.md")
            .Add(c => c.ShowNavigationLinks, true));

        cut.FindAll(".osirion-content-navigation").ShouldBeEmpty();
    }

    // Known defect, pinned as current behavior: the default content links keep the ".md" extension of the source file, while ContentPage strips it, so the two disagree on the item URL. Update this expectation when it is fixed.
    [Fact]
    public void ContentView_PathChanges_ReplacesTheNeighboursItLoaded()
    {
        var first = Item("first");
        var second = Item("second");
        var third = Item("third");
        _provider.GetItemByPathAsync("blog/first.md").Returns(first);
        _provider.GetItemByPathAsync("blog/third.md").Returns(third);
        _provider.GetItemsByQueryAsync(Arg.Any<ContentQuery>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>?>([first, second, third]));
        var suppliedPrevious = Item("supplied");

        var cut = RenderStatic<ContentView>(p => p
            .Add(c => c.Path, "blog/first.md")
            .Add(c => c.PreviousItem, suppliedPrevious)
            .Add(c => c.ShowNavigationLinks, true));
        cut.Find("a.osirion-content-next").GetAttribute("href").ShouldBe("/blog/second.md");

        cut.Render(p => p.Add(c => c.Path, "blog/third.md"));

        cut.Find("a.osirion-content-previous").GetAttribute("href").ShouldBe("/blog/second.md");
        cut.FindAll("a.osirion-content-next").ShouldBeEmpty();
    }

    private static ContentItem Item(string name, string[]? tags = null, string[]? categories = null)
    {
        var item = ContentItem.Create($"id-{name}", name, "<p>Body</p>", $"blog/{name}.md", "test");
        foreach (var tag in tags ?? [])
        {
            item.AddTag(tag);
        }

        foreach (var category in categories ?? [])
        {
            item.AddCategory(category);
        }

        return item;
    }
}
