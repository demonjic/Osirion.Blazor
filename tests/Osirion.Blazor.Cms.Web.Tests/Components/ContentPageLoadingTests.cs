using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Enums;
using Osirion.Blazor.Cms.Domain.Models;
using Osirion.Blazor.Cms.Domain.Repositories;
using Osirion.Blazor.Cms.Domain.Services;
using Osirion.Blazor.Cms.Web.Components;
using Osirion.Blazor.Cms.Web.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.Components;

public class ContentPageLoadingTests : CmsWebComponentTestBase
{
    private readonly IContentProvider _provider = Substitute.For<IContentProvider>();
    private readonly IContentProviderManager _manager = Substitute.For<IContentProviderManager>();
    private readonly RecordingLoggerFactory _logs = new();

    public ContentPageLoadingTests()
    {
        Services.AddSingleton(_manager);
        _manager.GetDefaultProvider().Returns(_provider);
        _provider.GetItemsByQueryAsync(Arg.Any<ContentQuery>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>?>([]));
        _provider.GetCategoriesAsync().Returns(Task.FromResult<IReadOnlyList<ContentCategory>>([]));
        _provider.GetTagsAsync().Returns(Task.FromResult<IReadOnlyList<ContentTag>>([]));
        _logs.Register(Services);
    }

    [Fact]
    public void ContentPage_IgnorePathSegmentMissingFromPath_QueriesWithTheSegmentPrepended()
    {
        RenderPage(p => p
            .Add(c => c.Path, "first")
            .Add(c => c.IgnorePathSegment, "blog"));

        _provider.Received(1).GetItemByUrlAsync("blog/first");
    }

    [Fact]
    public void ContentPage_IgnorePathSegmentAlreadyInPath_QueriesThePathUnchanged()
    {
        RenderPage(p => p
            .Add(c => c.Path, "blog/first")
            .Add(c => c.IgnorePathSegment, "blog"));

        _provider.Received(1).GetItemByUrlAsync("blog/first");
    }

    [Fact]
    public void ContentPage_PathReplacePattern_RewritesThePathBeforeQuerying()
    {
        RenderPage(p => p
            .Add(c => c.Path, "posts/first")
            .Add(c => c.PathReplacePattern, "^posts/|articles/"));

        _provider.Received(1).GetItemByUrlAsync("articles/first");
    }

    // The path comes from the URL, so a host pattern with catastrophic backtracking must not tie up the request:
    // the match times out and the path is used unchanged.
    [Fact]
    public void ContentPage_PathReplacePatternThatBacktracks_TimesOutAndKeepsThePath()
    {
        var path = new string('a', 40) + "!";

        RenderPage(p => p
            .Add(c => c.Path, path)
            .Add(c => c.PathReplacePattern, "^(a+)+$|x"));

        _provider.Received(1).GetItemByUrlAsync(path);
    }

    [Fact]
    public void ContentPage_PathReplacePatternWithoutASeparator_IsIgnored()
    {
        RenderPage(p => p
            .Add(c => c.Path, "posts/first")
            .Add(c => c.PathReplacePattern, "posts"));

        _provider.Received(1).GetItemByUrlAsync("posts/first");
    }

    [Fact]
    public void ContentPage_NoDefaultProvider_ShowsNotFoundWithTheBackLink()
    {
        _manager.GetDefaultProvider().Returns((IContentProvider?)null);

        var cut = RenderPage(p => p
            .Add(c => c.Path, "first")
            .Add(c => c.NotFoundTitle, "Gone")
            .Add(c => c.NotFoundText, "Nothing here.")
            .Add(c => c.BackLinkUrl, "/blog")
            .Add(c => c.BackLinkText, "Back to the blog"));

        cut.Find(".osirion-content-not-found h2").TextContent.ShouldBe("Gone");
        cut.Find(".osirion-content-not-found p").TextContent.ShouldBe("Nothing here.");
        var back = cut.Find("a.osirion-content-back-link");
        back.GetAttribute("href").ShouldBe("/blog");
        back.TextContent.ShouldBe("Back to the blog");
    }

    [Fact]
    public void ContentPage_NotFoundWithBackLinkHidden_RendersNoBackLink()
    {
        var cut = RenderPage(p => p
            .Add(c => c.Path, "missing")
            .Add(c => c.ShowBackLink, false));

        cut.FindAll(".osirion-content-not-found").ShouldHaveSingleItem();
        cut.FindAll("a.osirion-content-back-link").ShouldBeEmpty();
    }

    [Fact]
    public void ContentPage_ProviderThrows_LogsTheErrorAndShowsNotFound()
    {
        _provider.GetItemByUrlAsync("first").Returns<Task<ContentItem?>>(_ => throw new InvalidOperationException("down"));

        var cut = RenderPage(p => p.Add(c => c.Path, "first"));

        _logs.Entries.ShouldContain(e => e.Level == LogLevel.Error && e.Exception is InvalidOperationException);
        cut.FindAll(".osirion-content-not-found").ShouldHaveSingleItem();
        cut.Markup.ShouldNotContain("down");
    }

    [Fact]
    public void ContentPage_WhileTheContentLoads_ShowsTheLoadingText()
    {
        var pending = new TaskCompletionSource<ContentItem?>();
        _provider.GetItemByUrlAsync("slow").Returns(pending.Task);

        var cut = RenderPage(p => p
            .Add(c => c.Path, "slow")
            .Add(c => c.LoadingText, "Please wait"));

        cut.Find(".osirion-content-loading p").TextContent.ShouldBe("Please wait");

        pending.SetResult(Item("slow"));

        cut.WaitForAssertion(() => cut.Find("article.osirion-content-page .osirion-content-header h1").TextContent.ShouldBe("slow"));
    }

    [Fact]
    public void ContentPage_ContentLoaded_RaisesOnContentLoadedWithTheItem()
    {
        var item = Item("first");
        _provider.GetItemByUrlAsync("first").Returns(item);
        ContentItem? loaded = null;

        RenderPage(p => p
            .Add(c => c.Path, "first")
            .Add(c => c.OnContentLoaded, (ContentItem i) => loaded = i));

        loaded.ShouldBeSameAs(item);
    }

    [Fact]
    public void ContentPage_ContentMissing_DoesNotRaiseOnContentLoaded()
    {
        var raised = false;

        RenderPage(p => p
            .Add(c => c.Path, "missing")
            .Add(c => c.OnContentLoaded, (ContentItem _) => raised = true));

        raised.ShouldBeFalse();
    }

    [Fact]
    public void ContentPage_NavigationLinksHidden_DoesNotQueryNeighbours()
    {
        _provider.GetItemByUrlAsync("first").Returns(Item("first"));

        RenderPage(p => p
            .Add(c => c.Path, "first")
            .Add(c => c.ShowNavigationLinks, false));

        _provider.DidNotReceive().GetItemsByQueryAsync(Arg.Any<ContentQuery>());
    }

    [Fact]
    public void ContentPage_NeighbourQuery_AsksForTheItemDirectoryAndLocaleNewestFirst()
    {
        var directory = DirectoryItem.Create("d", "blog/2026", "2026", "test");
        var item = Item("first");
        item.SetLocale("de");
        directory.AddItem(item);
        _provider.GetItemByUrlAsync("first").Returns(item);

        // With a neighbor found, the page hands both to its view, which then runs no query of its own.
        _provider.GetItemsByQueryAsync(Arg.Any<ContentQuery>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>?>([item, Item("second")]));

        RenderPage(p => p.Add(c => c.Path, "first"));

        _provider.Received(1).GetItemsByQueryAsync(Arg.Is<ContentQuery>(q =>
            q.Directory == "blog/2026" && q.Locale == "de"
            && q.SortBy == SortField.Date && q.SortDirection == Osirion.Blazor.Cms.Domain.Enums.SortDirection.Descending));
    }

    [Fact]
    public void ContentPage_NeighbourQueryThrows_LogsAndStillShowsTheContent()
    {
        _provider.GetItemByUrlAsync("first").Returns(Item("first"));
        _provider.GetItemsByQueryAsync(Arg.Any<ContentQuery>())
            .Returns<Task<IReadOnlyList<ContentItem>?>>(_ => throw new InvalidOperationException("query failed"));

        var cut = RenderPage(p => p.Add(c => c.Path, "first"));

        _logs.Entries.ShouldContain(e => e.Level == LogLevel.Error && e.Exception is InvalidOperationException);
        cut.Instance.ContentItem.ShouldNotBeNull();
        cut.Instance.PreviousItem.ShouldBeNull();
        cut.Instance.NextItem.ShouldBeNull();
    }

    [Fact]
    public void ContentPage_NeighbourQueryReturnsNull_LeavesNeighboursEmpty()
    {
        _provider.GetItemByUrlAsync("first").Returns(Item("first"));
        _provider.GetItemsByQueryAsync(Arg.Any<ContentQuery>()).Returns(Task.FromResult<IReadOnlyList<ContentItem>?>(null));

        var cut = RenderPage(p => p.Add(c => c.Path, "first"));

        cut.Instance.PreviousItem.ShouldBeNull();
        cut.Instance.NextItem.ShouldBeNull();
    }

    [Fact]
    public void ContentPage_BothNeighboursSuppliedByTheParent_KeepsThem()
    {
        var first = Item("first");
        _provider.GetItemByUrlAsync("first").Returns(first);
        _provider.GetItemsByQueryAsync(Arg.Any<ContentQuery>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>?>([Item("loaded-previous"), first, Item("loaded-next")]));
        var previous = Item("p");
        var next = Item("n");

        var cut = RenderPage(p => p
            .Add(c => c.Path, "first")
            .Add(c => c.PreviousItem, previous)
            .Add(c => c.NextItem, next));

        cut.Instance.PreviousItem.ShouldBeSameAs(previous);
        cut.Instance.NextItem.ShouldBeSameAs(next);
    }

    [Fact]
    public void ContentPage_RelatedContent_ShowsUpToThreeItemsSharingATagOrCategoryBestMatchFirst()
    {
        var current = Item("current", tags: ["a", "b"], categories: ["news"]);
        var both = Item("both", tags: ["A", "b"]);
        var one = Item("one", tags: ["a"]);
        var category = Item("category", categories: ["NEWS"]);
        var extra = Item("extra", tags: ["b"]);
        var unrelated = Item("unrelated", tags: ["z"]);
        _provider.GetItemByUrlAsync("current").Returns(current);
        _provider.GetItemsByQueryAsync(Arg.Any<ContentQuery>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>?>([current, unrelated, one, both, category, extra]));

        var cut = RenderPage(p => p
            .Add(c => c.Path, "current")
            .Add(c => c.ShowNavigationLinks, false)
            .Add(c => c.ShowRelatedContent, true)
            .Add(c => c.RelatedContentTitle, "See also"));

        cut.Find(".osirion-content-related-title").TextContent.ShouldBe("See also");
        var titles = cut.FindAll(".osirion-content-related-item-title a").Select(a => a.TextContent).ToList();
        titles.Count.ShouldBe(3);
        titles[0].ShouldBe("both");
        titles.ShouldNotContain("current");
        titles.ShouldNotContain("unrelated");
    }

    [Fact]
    public void ContentPage_RelatedContentForAnItemWithoutTagsOrCategories_QueriesNothingAndShowsNoSection()
    {
        _provider.GetItemByUrlAsync("plain").Returns(Item("plain"));

        var cut = RenderPage(p => p
            .Add(c => c.Path, "plain")
            .Add(c => c.ShowNavigationLinks, false)
            .Add(c => c.ShowRelatedContent, true));

        _provider.DidNotReceive().GetItemsByQueryAsync(Arg.Any<ContentQuery>());
        cut.FindAll(".osirion-content-related").ShouldBeEmpty();
    }

    [Fact]
    public void ContentPage_RelatedQueryThrows_LogsAndShowsNoRelatedSection()
    {
        _provider.GetItemByUrlAsync("tagged").Returns(Item("tagged", tags: ["a"]));
        _provider.GetItemsByQueryAsync(Arg.Any<ContentQuery>())
            .Returns<Task<IReadOnlyList<ContentItem>?>>(_ => throw new InvalidOperationException("related failed"));

        var cut = RenderPage(p => p
            .Add(c => c.Path, "tagged")
            .Add(c => c.ShowNavigationLinks, false)
            .Add(c => c.ShowRelatedContent, true));

        _logs.Entries.ShouldContain(e => e.Level == LogLevel.Error && e.Exception is InvalidOperationException);
        cut.FindAll(".osirion-content-related").ShouldBeEmpty();
    }

    [Fact]
    public void ContentPage_RelatedQueryReturnsNull_ShowsNoRelatedSection()
    {
        _provider.GetItemByUrlAsync("tagged").Returns(Item("tagged", tags: ["a"]));
        _provider.GetItemsByQueryAsync(Arg.Any<ContentQuery>()).Returns(Task.FromResult<IReadOnlyList<ContentItem>?>(null));

        var cut = RenderPage(p => p
            .Add(c => c.Path, "tagged")
            .Add(c => c.ShowNavigationLinks, false)
            .Add(c => c.ShowRelatedContent, true));

        cut.FindAll(".osirion-content-related").ShouldBeEmpty();
    }

    [Fact]
    public void ContentPage_RelatedItemsSuppliedByTheParent_AreShownWithoutQuerying()
    {
        _provider.GetItemByUrlAsync("tagged").Returns(Item("tagged", tags: ["a"]));
        var supplied = Item("supplied");

        var cut = RenderPage(p => p
            .Add(c => c.Path, "tagged")
            .Add(c => c.ShowNavigationLinks, false)
            .Add(c => c.ShowRelatedContent, true)
            .Add(c => c.RelatedItems, [supplied]));

        _provider.DidNotReceive().GetItemsByQueryAsync(Arg.Any<ContentQuery>());
        cut.Find(".osirion-content-related-item-title a").TextContent.ShouldBe("supplied");
    }

    [Fact]
    public void ContentPage_RelatedItemWithAnImage_RendersTheImageAndTheDate()
    {
        using var culture = new CultureScope("en-US");
        _provider.GetItemByUrlAsync("tagged").Returns(Item("tagged", tags: ["a"]));
        var related = Item("pictured", tags: ["a"]);
        related.SetFeaturedImage("/img/pictured.png");
        related.SetCreatedDate(new DateTime(2026, 3, 4, 0, 0, 0, DateTimeKind.Utc));
        _provider.GetItemsByQueryAsync(Arg.Any<ContentQuery>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>?>([related]));

        var cut = RenderPage(p => p
            .Add(c => c.Path, "tagged")
            .Add(c => c.ShowNavigationLinks, false)
            .Add(c => c.ShowRelatedContent, true));

        var image = cut.Find("img.osirion-content-related-image");
        image.GetAttribute("src").ShouldBe("/img/pictured.png");
        // Decorative: the title link beside it already names the item, so alt text would only repeat it.
        image.GetAttribute("alt").ShouldBe(string.Empty);
        cut.Find("time.osirion-content-related-date").GetAttribute("datetime").ShouldBe("2026-03-04");
    }

    // Fails before the fix: each related item's date followed the server's culture instead of its own language.
    [Fact]
    public void ContentPage_RelatedItems_EachShowTheDateInTheirOwnLanguage()
    {
        using var culture = new CultureScope("de-DE");
        _provider.GetItemByUrlAsync("tagged").Returns(Item("tagged", tags: ["a"]));
        var english = Item("english", tags: ["a"]);
        english.SetLocale("en");
        english.SetCreatedDate(new DateTime(2026, 10, 17, 0, 0, 0, DateTimeKind.Utc));
        var serbian = Item("serbian", tags: ["a"]);
        serbian.SetLocale("sr-Latn-RS");
        serbian.SetCreatedDate(new DateTime(2026, 10, 17, 0, 0, 0, DateTimeKind.Utc));
        _provider.GetItemsByQueryAsync(Arg.Any<ContentQuery>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>?>([english, serbian]));

        var cut = RenderPage(p => p
            .Add(c => c.Path, "tagged")
            .Add(c => c.ShowNavigationLinks, false)
            .Add(c => c.ShowRelatedContent, true));

        cut.FindAll("time.osirion-content-related-date").Select(time => time.TextContent.Trim())
            .ShouldBe(["Oct 17, 2026", "okt 17, 2026"]);
    }

    [Theory]
    [InlineData("blog/post.md", null, null, "/blog/post")]
    [InlineData("blog/post.markdown", null, null, "/blog/post")]
    [InlineData("blog/post.html", null, null, "/blog/post.html")]
    [InlineData("content/blog/post.md", "content", null, "/blog/post")]
    [InlineData("blog/post.md", "content", null, "/blog/post")]
    [InlineData("blog/post.md", null, "^blog/|articles/", "/articles/post")]
    [InlineData("blog/post.md", null, "blog", "/blog/post")]
    public void ContentPage_RelatedItemLink_IsBuiltFromThePath(string path, string? ignoreSegment, string? replacePattern, string expected)
    {
        var related = ContentItem.Create("id-r", "Related", "<p>Body</p>", path, "test");
        related.AddTag("a");
        _provider.GetItemByUrlAsync(Arg.Any<string>()).Returns(Item("tagged", tags: ["a"]));
        _provider.GetItemsByQueryAsync(Arg.Any<ContentQuery>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>?>([related]));

        var cut = RenderPage(p => p
            .Add(c => c.Path, "tagged")
            .Add(c => c.IgnorePathSegment, ignoreSegment)
            .Add(c => c.PathReplacePattern, replacePattern)
            .Add(c => c.ShowNavigationLinks, false)
            .Add(c => c.ShowRelatedContent, true));

        cut.Find(".osirion-content-related-item-title a").GetAttribute("href").ShouldBe(expected);
    }

    [Fact]
    public void ContentPage_ContentUrlFormatter_BuildsTheRelatedItemLink()
    {
        _provider.GetItemByUrlAsync("tagged").Returns(Item("tagged", tags: ["a"]));
        _provider.GetItemsByQueryAsync(Arg.Any<ContentQuery>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>?>([Item("other", tags: ["a"])]));

        var cut = RenderPage(p => p
            .Add(c => c.Path, "tagged")
            .Add(c => c.ShowNavigationLinks, false)
            .Add(c => c.ShowRelatedContent, true)
            .Add(c => c.ContentUrlFormatter, item => $"/read/{item.Title}"));

        cut.Find(".osirion-content-related-item-title a").GetAttribute("href").ShouldBe("/read/other");
    }

    [Fact]
    public void ContentPage_ShareLinksSuppliedByTheParent_AreRenderedInsteadOfTheDefaults()
    {
        _provider.GetItemByUrlAsync("first").Returns(Item("first"));
        var links = new List<ShareLink>
        {
            new() { Name = "Mastodon", Label = "Toot", Url = "https://example.social/share", Icon = "<svg></svg>" }
        };

        var cut = RenderPage(p => p
            .Add(c => c.Path, "first")
            .Add(c => c.ShowShareLinks, true)
            .Add(c => c.ShareLinksTitle, "Spread it")
            .Add(c => c.ShareLinks, links));

        cut.Find(".osirion-content-share .osirion-content-section-title").TextContent.ShouldBe("Spread it");
        var link = cut.Find("a.osirion-content-share-link");
        link.ClassList.ShouldContain("osirion-content-share-mastodon");
        link.GetAttribute("href").ShouldBe("https://example.social/share");
        link.GetAttribute("title").ShouldBe("Toot");
        link.GetAttribute("rel").ShouldBe("noopener noreferrer");
    }

    [Fact]
    public void ContentPage_DefaultShareLinks_EscapeTheTitle()
    {
        _provider.GetItemByUrlAsync("first").Returns(ContentItem.Create("id", "A & B", "<p>x</p>", "blog/a.md", "test"));

        var cut = RenderPage(p => p
            .Add(c => c.Path, "first")
            .Add(c => c.ShowShareLinks, true));

        var names = cut.FindAll("a.osirion-content-share-link").Select(a => a.GetAttribute("title")).ToList();
        names.ShouldBe(["Twitter", "Facebook", "LinkedIn"]);
        cut.Find("a.osirion-content-share-twitter").GetAttribute("href").ShouldEndWith("&text=A%20%26%20B");
    }

    [Fact]
    public void ContentPage_PathChanges_ReplacesTheDefaultShareLinksBuiltForThePreviousPath()
    {
        _provider.GetItemByUrlAsync("first").Returns(Item("first"));
        _provider.GetItemByUrlAsync("second").Returns(Item("second"));

        var cut = RenderPage(p => p
            .Add(c => c.Path, "first")
            .Add(c => c.ShowShareLinks, true));
        cut.Render(p => p.Add(c => c.Path, "second"));

        cut.Find("a.osirion-content-share-twitter").GetAttribute("href").ShouldEndWith("&text=second");
    }

    [Fact]
    public void ContentPage_PathChanges_ReplacesRelatedItemsLoadedForThePreviousPath()
    {
        var first = Item("first", tags: ["a"]);
        var second = Item("second", tags: ["b"]);
        var relatedToFirst = Item("rel-a", tags: ["a"]);
        var relatedToSecond = Item("rel-b", tags: ["b"]);
        _provider.GetItemByUrlAsync("first").Returns(first);
        _provider.GetItemByUrlAsync("second").Returns(second);
        _provider.GetItemsByQueryAsync(Arg.Any<ContentQuery>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>?>([first, second, relatedToFirst, relatedToSecond]));

        var cut = RenderPage(p => p
            .Add(c => c.Path, "first")
            .Add(c => c.ShowNavigationLinks, false)
            .Add(c => c.ShowRelatedContent, true));
        cut.Render(p => p.Add(c => c.Path, "second"));

        cut.FindAll(".osirion-content-related-item-title a").Select(a => a.TextContent).ShouldBe(["rel-b"]);
    }

    [Fact]
    public void ContentPage_SamePathRenderedAgain_DoesNotReloadTheContent()
    {
        _provider.GetItemByUrlAsync("first").Returns(Item("first"));

        var cut = RenderPage(p => p.Add(c => c.Path, "first"));
        cut.Render(p => p.Add(c => c.LoadingText, "again"));

        _provider.Received(1).GetItemByUrlAsync("first");
    }

    [Fact]
    public void ContentPage_NoPath_ShowsTheItemSuppliedByTheParentWithoutLoading()
    {
        var cut = RenderPage(p => p.Add(c => c.ContentItem, Item("supplied")));

        _provider.DidNotReceive().GetItemByUrlAsync(Arg.Any<string>());
        cut.Find("article.osirion-content-page .osirion-content-header h1").TextContent.ShouldBe("supplied");
    }

    [Fact]
    public void ContentPage_Content_RendersSchemaTypeBreadcrumbsFooterAndClass()
    {
        var directory = DirectoryItem.Create("d", "guides", "Guides", "test");
        var item = Item("first");
        directory.AddItem(item);
        _provider.GetItemByUrlAsync("first").Returns(item);

        var cut = RenderPage(p => p
            .Add(c => c.Path, "first")
            .Add(c => c.SchemaType, "BlogPosting")
            .Add(c => c.BreadcrumbHomeText, "Start")
            .Add(c => c.BreadcrumbHomeUrl, "/start")
            .Add(c => c.FooterContent, "<em>Thanks</em>")
            .Add(c => c.Class, "custom"));

        cut.Find("div.osirion-content-page-container").ClassList.ShouldContain("custom");
        cut.Find("article.osirion-content-page").GetAttribute("itemtype").ShouldBe("https://schema.org/BlogPosting");
        var crumbs = cut.FindAll(".osirion-breadcrumbs-link");
        crumbs[0].GetAttribute("href").ShouldBe("/start");
        crumbs[0].TextContent.ShouldBe("Start");
        crumbs[1].GetAttribute("href").ShouldBe("/guides");
        cut.Find(".osirion-content-custom-footer em").TextContent.ShouldBe("Thanks");
    }

    [Fact]
    public void ContentPage_ItemWithCategoriesAndTags_RendersTheSidebarListsWithTheHostFormatters()
    {
        var item = Item("first", tags: ["C#"], categories: ["News"]);
        _provider.GetItemByUrlAsync("first").Returns(item);
        _provider.GetCategoriesAsync().Returns(Task.FromResult<IReadOnlyList<ContentCategory>>([ContentCategory.Create("News", "news", 2)]));
        _provider.GetTagsAsync().Returns(Task.FromResult<IReadOnlyList<ContentTag>>([ContentTag.Create("C#", "c-sharp", 1)]));

        var cut = RenderPage(p => p
            .Add(c => c.Path, "first")
            .Add(c => c.ShowBreadcrumbs, false)
            .Add(c => c.CategoriesTitle, "Filed under")
            .Add(c => c.TagsTitle, "Topics")
            .Add(c => c.CategoryUrlFormatter, name => $"/c/{name}")
            .Add(c => c.TagUrlFormatter, name => $"/t/{name}"));

        cut.Find(".osirion-content-sidebar .osirion-categories-title").TextContent.ShouldBe("Filed under");
        cut.Find(".osirion-content-sidebar a.osirion-category-link").GetAttribute("href").ShouldBe("/c/News");
        cut.Find(".osirion-content-sidebar .osirion-tag-cloud-title").TextContent.ShouldBe("Topics");
        cut.Find(".osirion-content-sidebar a.osirion-tag-link").GetAttribute("href").ShouldBe("/t/C#");
    }

    [Fact]
    public void ContentPage_SidebarListsWithoutFormatters_UseTheDefaultLinks()
    {
        _provider.GetItemByUrlAsync("first").Returns(Item("first", tags: ["C#"], categories: ["News"]));
        _provider.GetCategoriesAsync().Returns(Task.FromResult<IReadOnlyList<ContentCategory>>([ContentCategory.Create("News", "news", 2)]));
        _provider.GetTagsAsync().Returns(Task.FromResult<IReadOnlyList<ContentTag>>([ContentTag.Create("C#", "c-sharp", 1)]));

        var cut = RenderPage(p => p
            .Add(c => c.Path, "first")
            .Add(c => c.ShowBreadcrumbs, false));

        cut.Find(".osirion-content-sidebar a.osirion-category-link").GetAttribute("href").ShouldBe("/category/news");
        cut.Find(".osirion-content-sidebar a.osirion-tag-link").GetAttribute("href").ShouldBe("/tag/c-sharp");
    }

    [Fact]
    public void ContentPage_UrlHelpersWithoutFormatters_UseTheDefaultRoutes()
    {
        var probe = RenderStatic<ContentPageProbe>(_ => { }).Instance;

        probe.DirectoryUrl(DirectoryItem.Create("d", "guides/intro", "Intro", "test")).ShouldBe("/guides/intro");
        probe.CategoryUrl("News").ShouldBe("/category/news");
        probe.TagUrl("C#").ShouldBe("/tag/c%23");
    }

    [Fact]
    public void ContentPage_UrlHelpersWithFormatters_UseTheFormatters()
    {
        var probe = RenderStatic<ContentPageProbe>(p => p
            .Add(c => c.DirectoryUrlFormatter, d => $"/d/{d.Name}")
            .Add(c => c.CategoryUrlFormatter, c => $"/c/{c}")
            .Add(c => c.TagUrlFormatter, t => $"/t/{t}")).Instance;

        probe.DirectoryUrl(DirectoryItem.Create("d", "guides/intro", "Intro", "test")).ShouldBe("/d/Intro");
        probe.CategoryUrl("News").ShouldBe("/c/News");
        probe.TagUrl("C#").ShouldBe("/t/C#");
    }

    private IRenderedComponent<ContentPage> RenderPage(Action<ComponentParameterCollectionBuilder<ContentPage>> parameters) =>
        RenderStatic<ContentPage>(parameters);

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

    private sealed class ContentPageProbe : ContentPage
    {
        public string DirectoryUrl(DirectoryItem directory) => GetDirectoryUrl(directory);

        public string CategoryUrl(string category) => GetCategoryUrl(category);

        public string TagUrl(string tag) => GetTagUrl(tag);
    }
}
