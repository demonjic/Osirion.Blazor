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

public class ContentNavigationRenderingTests : CmsWebComponentTestBase
{
    private readonly IContentProvider _provider = Substitute.For<IContentProvider>();
    private readonly IContentProviderManager _manager = Substitute.For<IContentProviderManager>();
    private readonly RecordingLoggerFactory _logs = new();

    public ContentNavigationRenderingTests()
    {
        Services.AddSingleton(_manager);
        _logs.Register(Services);
        _manager.GetDefaultProvider().Returns(_provider);
        _manager.GetContentByQueryAsync(Arg.Any<ContentQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>>([]));
    }

    [Fact]
    public void OsirionContentNavigation_NeighboursFromTheManager_RendersBothLinksWithDefaultIcons()
    {
        var directory = DirectoryItem.Create("d", "docs", "docs", "test");
        var first = Item("first");
        var current = Item("current");
        var last = Item("last");
        directory.AddItem(current);
        current.SetLocale("de");
        _manager.GetContentByQueryAsync(Arg.Any<ContentQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>>([first, current, last]));

        var cut = RenderStatic<OsirionContentNavigation>(p => p
            .Add(c => c.Item, current)
            .Add(c => c.Class, "mine")
            .AddUnmatched("data-test", "nav"));

        _manager.Received(1).GetContentByQueryAsync(Arg.Is<ContentQuery>(q =>
            q.Locale == "de" && q.Directory == "docs" && q.SortBy == SortField.Order
            && q.SortDirection == Osirion.Blazor.Cms.Domain.Enums.SortDirection.Ascending), Arg.Any<CancellationToken>());
        var nav = cut.Find("nav");
        nav.ClassList.ShouldBe(["osirion-content-navigation", "osirion-content-navigation-default", "mine"], ignoreOrder: true);
        nav.GetAttribute("aria-label").ShouldBe("Content navigation");
        nav.GetAttribute("data-test").ShouldBe("nav");
        cut.Find("a.osirion-content-previous").GetAttribute("href").ShouldBe("/docs/first");
        cut.Find("a.osirion-content-next .osirion-content-navigation-title").TextContent.ShouldBe("last");
        cut.Find("a.osirion-content-previous .osirion-content-navigation-icon polyline").GetAttribute("points").ShouldBe("15 18 9 12 15 6");
        cut.Find("a.osirion-content-next .osirion-content-navigation-icon polyline").GetAttribute("points").ShouldBe("9 18 15 12 9 6");
    }

    [Fact]
    public void OsirionContentNavigation_OnlyANextItem_MarksSingleAndShowsAPlaceholder()
    {
        var current = Item("current");
        _manager.GetContentByQueryAsync(Arg.Any<ContentQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>>([current, Item("next")]));

        var cut = RenderStatic<OsirionContentNavigation>(p => p
            .Add(c => c.Item, current)
            .Add(c => c.ShowPlaceholder, true)
            .Add(c => c.Variant, ContentNavigationVariant.Card));

        var nav = cut.Find("nav");
        nav.ClassList.ShouldContain("osirion-content-navigation-card");
        nav.ClassList.ShouldContain("osirion-content-navigation-single");
        cut.FindAll(".osirion-content-navigation-placeholder").ShouldHaveSingleItem();
        cut.FindAll("a.osirion-content-previous").ShouldBeEmpty();
    }

    [Fact]
    public void OsirionContentNavigation_OnlyAPreviousItem_ShowsAPlaceholderForNext()
    {
        var cut = RenderStatic<OsirionContentNavigation>(p => p
            .Add(c => c.Item, Item("current"))
            .Add(c => c.PreviousItem, Item("older"))
            .Add(c => c.ShowPlaceholder, true));

        cut.FindAll("a.osirion-content-previous").ShouldHaveSingleItem();
        cut.FindAll(".osirion-content-navigation-placeholder").ShouldHaveSingleItem();
    }

    [Fact]
    public void OsirionContentNavigation_SuppliedNeighbours_AreNotQueried()
    {
        var cut = RenderStatic<OsirionContentNavigation>(p => p
            .Add(c => c.Item, Item("current"))
            .Add(c => c.PreviousItem, Item("older"))
            .Add(c => c.NextItem, Item("newer")));

        _manager.DidNotReceive().GetContentByQueryAsync(Arg.Any<ContentQuery>(), Arg.Any<CancellationToken>());
        cut.Find("nav").ClassList.ShouldNotContain("osirion-content-navigation-single");
    }

    [Fact]
    public void OsirionContentNavigation_Extractors_ShapeTheLinksAndDescriptions()
    {
        var older = Item("older");
        older.SetDescription("Older description");

        var cut = RenderStatic<OsirionContentNavigation>(p => p
            .Add(c => c.Item, Item("current"))
            .Add(c => c.PreviousItem, older)
            .Add(c => c.NextItem, Item("newer"))
            .Add(c => c.ShowDescription, true)
            .Add(c => c.PreviousLabel, "Back")
            .Add(c => c.NextLabel, "Forward")
            .Add(c => c.AriaLabel, "Pager")
            .Add(c => c.UrlGenerator, i => $"/read/{i.Title}")
            .Add(c => c.TitleExtractor, i => i.Title.ToUpperInvariant())
            .Add(c => c.DescriptionExtractor, i => i.Title == "newer" ? "Newer description" : i.Description));

        cut.Find("nav").GetAttribute("aria-label").ShouldBe("Pager");
        var previous = cut.Find("a.osirion-content-previous");
        previous.GetAttribute("href").ShouldBe("/read/older");
        previous.QuerySelector(".osirion-content-navigation-label")!.TextContent.ShouldContain("Back");
        previous.QuerySelector(".osirion-content-navigation-title")!.TextContent.ShouldBe("OLDER");
        previous.QuerySelector(".osirion-content-navigation-description")!.TextContent.ShouldBe("Older description");
        var next = cut.Find("a.osirion-content-next");
        next.QuerySelector(".osirion-content-navigation-label")!.TextContent.ShouldContain("Forward");
        next.QuerySelector(".osirion-content-navigation-description")!.TextContent.ShouldBe("Newer description");
    }

    [Fact]
    public void OsirionContentNavigation_DescriptionsHiddenByDefault()
    {
        var older = Item("older");
        older.SetDescription("Hidden");

        var cut = RenderStatic<OsirionContentNavigation>(p => p
            .Add(c => c.Item, Item("current"))
            .Add(c => c.PreviousItem, older)
            .Add(c => c.NextItem, Item("newer")));

        cut.FindAll(".osirion-content-navigation-description").ShouldBeEmpty();
    }

    [Fact]
    public void OsirionContentNavigation_IconsHidden_RendersNoIcons()
    {
        var cut = RenderStatic<OsirionContentNavigation>(p => p
            .Add(c => c.Item, Item("current"))
            .Add(c => c.PreviousItem, Item("older"))
            .Add(c => c.NextItem, Item("newer"))
            .Add(c => c.ShowIcons, false));

        cut.FindAll(".osirion-content-navigation-icon").ShouldBeEmpty();
    }

    [Fact]
    public void OsirionContentNavigation_CustomIcons_ReplaceTheDefaults()
    {
        var cut = RenderStatic<OsirionContentNavigation>(p => p
            .Add(c => c.Item, Item("current"))
            .Add(c => c.PreviousItem, Item("older"))
            .Add(c => c.NextItem, Item("newer"))
            .Add(c => c.PreviousIcon, b => b.AddMarkupContent(0, "<i class=\"back\"></i>"))
            .Add(c => c.NextIcon, b => b.AddMarkupContent(0, "<i class=\"forward\"></i>")));

        cut.FindAll(".osirion-content-navigation-icon i.back").ShouldHaveSingleItem();
        cut.FindAll(".osirion-content-navigation-icon i.forward").ShouldHaveSingleItem();
        cut.FindAll(".osirion-content-navigation-icon svg").ShouldBeEmpty();
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void OsirionContentNavigation_HiddenOrWithoutItem_RendersNothingAndQueriesNothing(bool showNavigation, bool withItem)
    {
        var cut = RenderStatic<OsirionContentNavigation>(p => p
            .Add(c => c.Item, withItem ? Item("current") : null)
            .Add(c => c.NextItem, Item("newer"))
            .Add(c => c.ShowNavigation, showNavigation));

        cut.Markup.Trim().ShouldBeEmpty();
        _manager.DidNotReceive().GetContentByQueryAsync(Arg.Any<ContentQuery>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void OsirionContentNavigation_ItemNotInTheList_RendersNothing()
    {
        _manager.GetContentByQueryAsync(Arg.Any<ContentQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>>([Item("a"), Item("b")]));

        var cut = RenderStatic<OsirionContentNavigation>(p => p.Add(c => c.Item, Item("current")));

        cut.FindAll("nav").ShouldBeEmpty();
    }

    [Fact]
    public void OsirionContentNavigation_QueryThrows_LogsAndRendersNothing()
    {
        _manager.GetContentByQueryAsync(Arg.Any<ContentQuery>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<ContentItem>>>(_ => throw new InvalidOperationException("down"));

        var cut = RenderStatic<OsirionContentNavigation>(p => p.Add(c => c.Item, Item("current")));

        _logs.Entries.ShouldContain(e => e.Level == LogLevel.Error && e.Exception is InvalidOperationException);
        cut.FindAll("nav").ShouldBeEmpty();
    }

    [Fact]
    public void LocalizedNavigation_Directories_RendersThemInOrderWithDefaultLinks()
    {
        var second = Directory("docs", "Docs", order: 2);
        var first = Directory("blog", "Blog", order: 1);
        _provider.GetDirectoriesAsync("de", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<DirectoryItem>>([second, first]));

        var cut = RenderStatic<LocalizedNavigation>(p => p
            .Add(c => c.CurrentLocale, "de")
            .Add(c => c.Title, "Browse")
            .Add(c => c.Class, "side"));

        cut.Find("nav.osirion-localized-navigation").ClassList.ShouldContain("side");
        cut.Find(".osirion-nav-title").TextContent.ShouldBe("Browse");
        cut.FindAll("a.osirion-nav-link").Select(a => a.GetAttribute("href")).ShouldBe(["/blog", "/docs"]);
        cut.FindAll(".osirion-nav-sublist").ShouldBeEmpty();
        cut.FindAll(".osirion-locale-selector").ShouldBeEmpty();
    }

    [Fact]
    public void LocalizedNavigation_LocalizationDisabled_LoadsEveryDirectory()
    {
        _provider.GetDirectoriesAsync(null, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<DirectoryItem>>([Directory("blog", "Blog")]));

        var cut = RenderStatic<LocalizedNavigation>(p => p
            .Add(c => c.CurrentLocale, "de")
            .Add(c => c.EnableLocalization, false));

        cut.FindAll("a.osirion-nav-link").ShouldHaveSingleItem();
        _provider.DidNotReceive().GetDirectoriesAsync("de", Arg.Any<CancellationToken>());
    }

    [Fact]
    public void LocalizedNavigation_ExpandAll_ShowsContentItemsByTitleAndSubdirectories()
    {
        var blog = Directory("blog", "Blog");
        var zeta = ContentItem.Create("z", "Zeta", "<p>x</p>", "blog/zeta.md", "test");
        zeta.SetUrl("/blog/zeta");
        var alpha = ContentItem.Create("a", "Alpha", "<p>x</p>", "blog/alpha.md", "test");
        alpha.SetUrl("/blog/alpha");
        blog.AddItem(zeta);
        blog.AddItem(alpha);
        var archive = Directory("blog/archive", "Archive");
        blog.AddChild(archive);
        ReturnDirectories(blog);

        var cut = RenderStatic<LocalizedNavigation>(p => p.Add(c => c.ExpandAllDirectories, true));

        cut.FindAll("a.osirion-nav-content-link").Select(a => a.GetAttribute("href")).ShouldBe(["/blog/alpha", "/blog/zeta"]);
        cut.FindAll(".osirion-nav-sublist a.osirion-nav-link").Select(a => a.GetAttribute("href")).ShouldBe(["/blog/archive"]);
    }

    // A raw file (a notes.txt loaded because the provider reads .txt) is not a page, so the menu does not link it.
    [Fact]
    public void LocalizedNavigation_RawFileInADirectory_IsNotListed()
    {
        var blog = Directory("blog", "Blog");
        var post = ContentItem.Create("a", "Alpha", "<p>x</p>", "blog/alpha.md", "test");
        post.SetUrl("/blog/alpha");
        blog.AddItem(post);
        blog.AddItem(ContentItem.Create("n", "Notes", "notes", "blog/notes.txt", "test"));
        ReturnDirectories(blog);

        var cut = RenderStatic<LocalizedNavigation>(p => p.Add(c => c.ExpandAllDirectories, true));

        cut.FindAll("a.osirion-nav-content-link").Select(a => a.GetAttribute("href")).ShouldBe(["/blog/alpha"]);
    }

    [Fact]
    public void LocalizedNavigation_ExpandedDirectoryWithItemsHidden_ShowsOnlySubdirectories()
    {
        var blog = Directory("blog", "Blog");
        blog.AddItem(ContentItem.Create("a", "Alpha", "<p>x</p>", "blog/alpha.md", "test"));
        blog.AddChild(Directory("blog/archive", "Archive"));
        ReturnDirectories(blog, Directory("docs", "Docs"));

        var cut = RenderStatic<LocalizedNavigation>(p => p
            .Add(c => c.ExpandedDirectoryId, "id-blog")
            .Add(c => c.ShowContentItems, false));

        cut.FindAll("a.osirion-nav-content-link").ShouldBeEmpty();
        cut.FindAll(".osirion-nav-sublist").ShouldHaveSingleItem();
    }

    [Fact]
    public void LocalizedNavigation_ExpandedDirectoryWithOnlyHiddenItems_RendersNoSublist()
    {
        var blog = Directory("blog", "Blog");
        blog.AddItem(ContentItem.Create("a", "Alpha", "<p>x</p>", "blog/alpha.md", "test"));
        ReturnDirectories(blog);

        var cut = RenderStatic<LocalizedNavigation>(p => p
            .Add(c => c.ExpandAllDirectories, true)
            .Add(c => c.ShowContentItems, false));

        cut.FindAll(".osirion-nav-sublist").ShouldBeEmpty();
    }

    [Fact]
    public void LocalizedNavigation_Formatters_BuildTheLinks()
    {
        var blog = Directory("blog", "Blog");
        blog.AddItem(ContentItem.Create("a", "Alpha", "<p>x</p>", "blog/alpha.md", "test"));
        ReturnDirectories(blog);

        var cut = RenderStatic<LocalizedNavigation>(p => p
            .Add(c => c.ExpandAllDirectories, true)
            .Add(c => c.DirectoryUrlFormatter, d => $"/d/{d.Name}")
            .Add(c => c.ContentUrlFormatter, i => $"/c/{i.Title}"));

        cut.Find("a.osirion-nav-link").GetAttribute("href").ShouldBe("/d/Blog");
        cut.Find("a.osirion-nav-content-link").GetAttribute("href").ShouldBe("/c/Alpha");
    }

    [Fact]
    public void LocalizedNavigation_Clicks_RaiseTheSelectionCallbacks()
    {
        var blog = Directory("blog", "Blog");
        var item = ContentItem.Create("a", "Alpha", "<p>x</p>", "blog/alpha.md", "test");
        blog.AddItem(item);
        ReturnDirectories(blog);
        DirectoryItem? selectedDirectory = null;
        ContentItem? selectedContent = null;

        var cut = RenderStatic<LocalizedNavigation>(p => p
            .Add(c => c.ExpandAllDirectories, true)
            .Add(c => c.OnDirectorySelected, (DirectoryItem d) => selectedDirectory = d)
            .Add(c => c.OnContentSelected, (ContentItem i) => selectedContent = i));

        cut.Find("a.osirion-nav-link").Click();
        cut.Find("a.osirion-nav-content-link").Click();

        selectedDirectory.ShouldBeSameAs(blog);
        selectedContent.ShouldBeSameAs(item);
    }

    [Fact]
    public void LocalizedNavigation_NoDirectories_ShowsTheEmptyText()
    {
        ReturnDirectories();

        var cut = RenderStatic<LocalizedNavigation>(p => p.Add(c => c.NoContentText, "Nothing"));

        cut.Find(".osirion-no-directories").TextContent.ShouldBe("Nothing");
    }

    [Fact]
    public void LocalizedNavigation_NoDefaultProvider_ShowsTheEmptyText()
    {
        _manager.GetDefaultProvider().Returns((IContentProvider?)null);

        var cut = RenderStatic<LocalizedNavigation>(_ => { });

        cut.FindAll(".osirion-no-directories").ShouldHaveSingleItem();
    }

    [Fact]
    public void LocalizedNavigation_ProviderThrows_LogsAndShowsTheEmptyText()
    {
        _provider.GetDirectoriesAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<DirectoryItem>>>(_ => throw new InvalidOperationException("down"));

        var cut = RenderStatic<LocalizedNavigation>(_ => { });

        _logs.Entries.ShouldContain(e => e.Level == LogLevel.Error && e.Exception is InvalidOperationException);
        cut.FindAll(".osirion-no-directories").ShouldHaveSingleItem();
    }

    [Fact]
    public void LocalizedNavigation_WhileLoading_ShowsTheLoadingText()
    {
        var pending = new TaskCompletionSource<IReadOnlyList<DirectoryItem>>();
        _provider.GetDirectoriesAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(pending.Task);

        var cut = RenderStatic<LocalizedNavigation>(p => p.Add(c => c.LoadingText, "Loading menu"));

        cut.Find(".osirion-loading").TextContent.ShouldBe("Loading menu");

        pending.SetResult([Directory("blog", "Blog")]);

        cut.WaitForAssertion(() => cut.FindAll("a.osirion-nav-link").ShouldHaveSingleItem());
    }

    private void ReturnDirectories(params DirectoryItem[] directories) =>
        _provider.GetDirectoriesAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<DirectoryItem>>(directories));

    private static DirectoryItem Directory(string path, string name, int order = 0)
    {
        var directory = DirectoryItem.Create($"id-{path}", path, name, "test");
        directory.SetOrder(order);
        return directory;
    }

    private static ContentItem Item(string name)
    {
        var item = ContentItem.Create($"id-{name}", name, "<p>Body</p>", $"docs/{name}.md", "test");
        item.SetUrl($"/docs/{name}");
        return item;
    }
}
