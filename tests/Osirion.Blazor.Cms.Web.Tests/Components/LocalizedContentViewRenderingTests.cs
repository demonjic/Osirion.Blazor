using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Repositories;
using Osirion.Blazor.Cms.Domain.Services;
using Osirion.Blazor.Cms.Web.Components;
using Osirion.Blazor.Cms.Web.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.Components;

public class LocalizedContentViewRenderingTests : CmsWebComponentTestBase
{
    private readonly IContentProvider _provider = Substitute.For<IContentProvider>();
    private readonly IContentProviderManager _manager = Substitute.For<IContentProviderManager>();
    private readonly RecordingLoggerFactory _logs = new();

    public LocalizedContentViewRenderingTests()
    {
        Services.AddSingleton(_manager);
        _logs.Register(Services);
        _manager.GetDefaultProvider().Returns(_provider);
        _provider.GetContentTranslationsAsync(Arg.Any<string>()).Returns(new Dictionary<string, ContentItem>());
        _provider.GetItemsByQueryAsync(Arg.Any<ContentQuery>()).Returns(Task.FromResult<IReadOnlyList<ContentItem>?>([]));
    }

    [Fact]
    public void LocalizedContentView_Path_LoadsTheItemAndRendersTheHeader()
    {
        using var culture = new CultureScope("en-US");
        var item = Item("post");
        item.SetAuthor("Ada");
        item.SetFeaturedImage("/img/post.png");
        item.SetCreatedDate(new DateTime(2026, 5, 6, 0, 0, 0, DateTimeKind.Utc));
        _provider.GetItemByPathAsync("blog/post.md").Returns(item);

        var cut = RenderStatic<LocalizedContentView>(p => p
            .Add(c => c.Path, "blog/post.md")
            .Add(c => c.Class, "extra"));

        cut.Find("div.osirion-localized-content-view").ClassList.ShouldContain("extra");
        cut.Find(".osirion-content-header h1").TextContent.ShouldBe("post");
        cut.Find(".osirion-content-author").TextContent.ShouldBe("By Ada");
        cut.Find(".osirion-content-date").TextContent.ShouldBe("May 6, 2026");
        cut.Find("img.osirion-content-featured-image").GetAttribute("src").ShouldBe("/img/post.png");
    }

    // Fails before the fix: the date followed the server's culture instead of the content's language.
    [Theory]
    [InlineData("sr-Latn-RS", "en", "October 17, 2026")]
    [InlineData("en-US", "sr-Latn-RS", "oktobar 17, 2026")]
    [InlineData("sr-Latn-RS", "not-a-culture", "oktobar 17, 2026")]
    public void LocalizedContentView_Date_FollowsTheContentLanguage(string serverCulture, string locale, string expected)
    {
        using var culture = new CultureScope(serverCulture);
        var item = Item("post");
        item.SetLocale(locale);
        item.SetCreatedDate(new DateTime(2026, 10, 17, 0, 0, 0, DateTimeKind.Utc));

        var cut = RenderStatic<LocalizedContentView>(p => p.Add(c => c.Item, item));

        var time = cut.Find(".osirion-content-date time");
        time.GetAttribute("datetime").ShouldBe("2026-10-17");
        time.TextContent.ShouldBe(expected);
    }

    [Fact]
    public void LocalizedContentView_JumbotronHidden_RendersNoHeader()
    {
        var cut = RenderStatic<LocalizedContentView>(p => p
            .Add(c => c.Item, Item("post"))
            .Add(c => c.ShowJumbotron, false));

        cut.FindAll(".osirion-content-header").ShouldBeEmpty();
        cut.FindAll(".osirion-content-body").ShouldHaveSingleItem();
    }

    [Fact]
    public void LocalizedContentView_ItemWithoutAuthorOrImage_RendersTheHeaderWithoutThem()
    {
        var cut = RenderStatic<LocalizedContentView>(p => p.Add(c => c.Item, Item("post")));

        cut.FindAll(".osirion-content-header").ShouldHaveSingleItem();
        cut.FindAll(".osirion-content-author").ShouldBeEmpty();
        cut.FindAll("img.osirion-content-featured-image").ShouldBeEmpty();
    }

    [Fact]
    public void LocalizedContentView_PathNotFound_ShowsTheNotFoundText()
    {
        var cut = RenderStatic<LocalizedContentView>(p => p
            .Add(c => c.Path, "blog/missing.md")
            .Add(c => c.NotFoundText, "Not here"));

        cut.Find(".osirion-not-found").TextContent.ShouldBe("Not here");
    }

    [Fact]
    public void LocalizedContentView_NoDefaultProvider_ShowsNotFound()
    {
        _manager.GetDefaultProvider().Returns((IContentProvider?)null);

        var cut = RenderStatic<LocalizedContentView>(p => p.Add(c => c.Path, "blog/post.md"));

        cut.FindAll(".osirion-not-found").ShouldHaveSingleItem();
    }

    [Fact]
    public void LocalizedContentView_ProviderThrows_LogsAndShowsNotFound()
    {
        _provider.GetItemByPathAsync("blog/post.md").Returns<Task<ContentItem?>>(_ => throw new InvalidOperationException("down"));

        var cut = RenderStatic<LocalizedContentView>(p => p.Add(c => c.Path, "blog/post.md"));

        _logs.Entries.ShouldContain(e => e.Level == LogLevel.Error && e.Exception is InvalidOperationException);
        cut.FindAll(".osirion-not-found").ShouldHaveSingleItem();
    }

    [Fact]
    public void LocalizedContentView_TranslationsThrow_LogsAndStillShowsTheContent()
    {
        _provider.GetContentTranslationsAsync(Arg.Any<string>())
            .Returns<Task<Dictionary<string, ContentItem>>>(_ => throw new InvalidOperationException("translations"));

        var cut = RenderStatic<LocalizedContentView>(p => p.Add(c => c.Item, Item("post")));

        _logs.Entries.ShouldContain(e => e.Level == LogLevel.Error && e.Exception is InvalidOperationException);
        cut.FindAll(".osirion-content-body").ShouldHaveSingleItem();
        cut.FindAll(".osirion-translations").ShouldBeEmpty();
    }

    [Fact]
    public void LocalizedContentView_LocalizationDisabled_NeitherLoadsNorShowsTranslations()
    {
        var cut = RenderStatic<LocalizedContentView>(p => p
            .Add(c => c.Item, Item("post"))
            .Add(c => c.EnableLocalization, false));

        _provider.DidNotReceive().GetContentTranslationsAsync(Arg.Any<string>());
        cut.FindAll(".osirion-translations").ShouldBeEmpty();
    }

    [Fact]
    public void LocalizedContentView_LocaleNameFormatter_NamesTheTranslationLinks()
    {
        var item = Item("post");
        item.SetContentId("loc-1");
        var german = Item("post-de");
        german.SetUrl("de/blog/post");
        _provider.GetContentTranslationsAsync("loc-1").Returns(new Dictionary<string, ContentItem>
        {
            ["en"] = item,
            ["de"] = german
        });

        var cut = RenderStatic<LocalizedContentView>(p => p
            .Add(c => c.Item, item)
            .Add(c => c.LocaleNameFormatter, locale => locale == "de" ? "Deutsch" : locale));

        var link = cut.Find(".osirion-translations a");
        link.TextContent.Trim().ShouldBe("Deutsch");
        link.GetAttribute("href").ShouldBe("/de/blog/post");
    }

    // Fails before the fix: the component's CSS styles .osirion-translation-link, but the links had no class.
    [Fact]
    public void LocalizedContentView_TranslationLinks_CarryTheTranslationLinkClass()
    {
        var item = Item("post");
        var german = Item("post-de");
        german.SetUrl("de/blog/post");
        _provider.GetContentTranslationsAsync(item.ContentId).Returns(new Dictionary<string, ContentItem>
        {
            ["en"] = item,
            ["de"] = german
        });

        var cut = RenderStatic<LocalizedContentView>(p => p.Add(c => c.Item, item));

        var link = cut.Find(".osirion-translations a");
        link.ClassList.ShouldContain("osirion-translation-link");
        link.ClassList.ShouldNotContain("osirion-active");
    }

    [Fact]
    public void LocalizedContentView_CategoriesAndTags_LinkBelowTheCurrentLocale()
    {
        var cut = RenderStatic<LocalizedContentView>(p => p
            .Add(c => c.Item, Item("post", tags: ["C#"], categories: ["News"]))
            .Add(c => c.CurrentLocale, "sr"));

        cut.Find("a.osirion-content-category").GetAttribute("href").ShouldBe("/sr/category/news");
        cut.Find("a.osirion-content-tag").GetAttribute("href").ShouldBe("/sr/tag/c%23");
    }

    [Fact]
    public void LocalizedContentView_CategoryFormatter_BuildsTheCategoryLinks()
    {
        var cut = RenderStatic<LocalizedContentView>(p => p
            .Add(c => c.Item, Item("post", categories: ["News"]))
            .Add(c => c.CategoryUrlFormatter, c => $"/c/{c}"));

        cut.Find("a.osirion-content-category").GetAttribute("href").ShouldBe("/c/News");
        cut.FindAll(".osirion-content-tags").ShouldBeEmpty();
    }

    // Known defect, pinned as current behavior: the default content links keep the ".md" extension of the source file, while ContentPage strips it, so the two disagree on the item URL. Update this expectation when it is fixed.
    [Fact]
    public void LocalizedContentView_NavigationLinks_LoadsNeighboursInTheItemDirectoryAndLocale()
    {
        var directory = DirectoryItem.Create("d", "blog", "blog", "test");
        var newer = Item("newer");
        var current = Item("current");
        var older = Item("older");
        directory.AddItem(current);
        _provider.GetItemsByQueryAsync(Arg.Any<ContentQuery>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>?>([newer, current, older]));

        var cut = RenderStatic<LocalizedContentView>(p => p
            .Add(c => c.Item, current)
            .Add(c => c.CurrentLocale, "de")
            .Add(c => c.ShowNavigationLinks, true));

        _provider.Received(1).GetItemsByQueryAsync(Arg.Is<ContentQuery>(q => q.Directory == "blog" && q.Locale == "de"));
        cut.Find("a.osirion-content-previous").GetAttribute("href").ShouldBe("/blog/newer.md");
        cut.Find("a.osirion-content-next .osirion-content-navigation-title").TextContent.ShouldBe("older");
    }

    [Fact]
    public void LocalizedContentView_NeighbourQueryReturnsNull_RendersNoNavigation()
    {
        _provider.GetItemsByQueryAsync(Arg.Any<ContentQuery>()).Returns(Task.FromResult<IReadOnlyList<ContentItem>?>(null));

        var cut = RenderStatic<LocalizedContentView>(p => p
            .Add(c => c.Item, Item("current"))
            .Add(c => c.ShowNavigationLinks, true));

        cut.FindAll(".osirion-content-navigation").ShouldBeEmpty();
    }

    [Fact]
    public void LocalizedContentView_ContentUrlFormatter_BuildsTheNeighbourLinks()
    {
        var cut = RenderStatic<LocalizedContentView>(p => p
            .Add(c => c.Item, Item("current"))
            .Add(c => c.PreviousItem, Item("older"))
            .Add(c => c.ShowNavigationLinks, true)
            .Add(c => c.ContentUrlFormatter, i => $"/read/{i.Title}"));

        cut.Find("a.osirion-content-previous").GetAttribute("href").ShouldBe("/read/older");
        cut.FindAll("a.osirion-content-next").ShouldBeEmpty();
    }

    [Fact]
    public void LocalizedContentView_NewItem_LoadsTheTranslationsOfThatItem()
    {
        var first = Item("first");
        first.SetContentId("loc-first");
        var second = Item("second");
        second.SetContentId("loc-second");

        var cut = RenderStatic<LocalizedContentView>(p => p.Add(c => c.Item, first));
        cut.Render(p => p.Add(c => c.Item, second));

        _provider.Received(1).GetContentTranslationsAsync("loc-first");
        _provider.Received(1).GetContentTranslationsAsync("loc-second");
        cut.Find(".osirion-content-header h1").TextContent.ShouldBe("second");
    }

    [Fact]
    public void LocalizedContentView_InteractivePathLoad_ShowsTheItemOnceLoaded()
    {
        var pending = new TaskCompletionSource<ContentItem?>();
        _provider.GetItemByPathAsync("blog/post.md").Returns(pending.Task);
        SetRendererInfo(new RendererInfo("Server", true));

        var cut = Render<LocalizedContentView>(p => p
            .Add(c => c.Path, "blog/post.md")
            .Add(c => c.LoadingText, "Wait"));

        pending.SetResult(Item("post"));

        cut.WaitForAssertion(() => cut.Find(".osirion-content-header h1").TextContent.ShouldBe("post"));
    }

    private static ContentItem Item(string name, string[]? tags = null, string[]? categories = null)
    {
        var item = ContentItem.Create($"id-{name}", name, "<p>Body</p>", $"blog/{name}.md", "test");
        item.SetContentId($"loc-{name}");
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
