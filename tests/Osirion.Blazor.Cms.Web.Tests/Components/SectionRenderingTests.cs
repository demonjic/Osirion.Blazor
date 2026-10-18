using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Osirion.Blazor.Cms.Components;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Enums;
using Osirion.Blazor.Cms.Domain.Repositories;
using Osirion.Blazor.Cms.Domain.Services;
using Osirion.Blazor.Cms.Web.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.Components;

public class SectionRenderingTests : CmsWebComponentTestBase
{
    private readonly IContentProvider _provider = Substitute.For<IContentProvider>();
    private readonly IContentProviderManager _manager = Substitute.For<IContentProviderManager>();
    private readonly RecordingLoggerFactory _logs = new();

    public SectionRenderingTests()
    {
        Services.AddSingleton(_manager);
        _logs.Register(Services);
        _manager.GetDefaultProvider().Returns(_provider);
        _provider.GetItemsByQueryAsync(Arg.Any<ContentQuery>()).Returns(Task.FromResult<IReadOnlyList<ContentItem>?>([]));
        _manager.GetContentByQueryAsync(Arg.Any<ContentQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>>([]));
    }

    [Fact]
    public void FeaturedPostsSection_NoPostsSupplied_QueriesFeaturedPostsWithTheFilters()
    {
        RenderStatic<FeaturedPostsSection>(p => p
            .Add(c => c.Directory, "blog")
            .Add(c => c.Category, "news")
            .Add(c => c.Tag, "dotnet")
            .Add(c => c.Locale, "de")
            .Add(c => c.PostCount, 4));

        _provider.Received(1).GetItemsByQueryAsync(Arg.Is<ContentQuery>(q =>
            q.IsFeatured == true && q.Directory == "blog" && q.Category == "news" && q.Tag == "dotnet"
            && q.Locale == "de" && q.Take == 4 && q.SortBy == SortField.Date
            && q.SortDirection == Osirion.Blazor.Cms.Domain.Enums.SortDirection.Descending));
    }

    [Fact]
    public void FeaturedPostsSection_NoPosts_ShowsTheNoContentText()
    {
        var cut = RenderStatic<FeaturedPostsSection>(p => p
            .Add(c => c.NoContentText, "Nothing featured")
            .Add(c => c.Class, "home"));

        cut.Find("section.osirion-featured-posts-section").ClassList.ShouldContain("home");
        cut.Find(".osirion-no-content p").TextContent.ShouldBe("Nothing featured");
    }

    [Fact]
    public void FeaturedPostsSection_NoDefaultProvider_ShowsTheNoContentText()
    {
        _manager.GetDefaultProvider().Returns((IContentProvider?)null);

        var cut = RenderStatic<FeaturedPostsSection>(_ => { });

        cut.FindAll(".osirion-no-content").ShouldHaveSingleItem();
    }

    [Fact]
    public void FeaturedPostsSection_ProviderThrows_LogsAndShowsTheNoContentText()
    {
        _provider.GetItemsByQueryAsync(Arg.Any<ContentQuery>())
            .Returns<Task<IReadOnlyList<ContentItem>?>>(_ => throw new InvalidOperationException("down"));

        var cut = RenderStatic<FeaturedPostsSection>(_ => { });

        _logs.Entries.ShouldContain(e => e.Level == LogLevel.Error && e.Exception is InvalidOperationException);
        cut.FindAll(".osirion-no-content").ShouldHaveSingleItem();
    }

    [Fact]
    public void FeaturedPostsSection_InteractiveWhileLoading_ShowsTheLoadingText()
    {
        var pending = new TaskCompletionSource<IReadOnlyList<ContentItem>?>();
        _provider.GetItemsByQueryAsync(Arg.Any<ContentQuery>()).Returns(pending.Task);
        SetRendererInfo(new RendererInfo("Server", true));

        var cut = Render<FeaturedPostsSection>(p => p.Add(c => c.LoadingText, "Fetching posts"));

        cut.Find(".osirion-loading p").TextContent.ShouldBe("Fetching posts");

        pending.SetResult([Post("Loaded")]);

        cut.WaitForAssertion(() => cut.Find(".osirion-featured-post-title-link").TextContent.ShouldBe("Loaded"));
    }

    // Known defect, pinned as current behavior: the default content links keep the ".md" extension of the source file, while ContentPage strips it, so the two disagree on the item URL. Update this expectation when it is fixed.
    [Fact]
    public void FeaturedPostsSection_SuppliedPosts_RenderEveryPart()
    {
        using var culture = new CultureScope("en-US");
        var post = Post("With image", categories: ["News", "Other"]);
        post.SetFeaturedImage("/img/a.png");
        post.SetAuthor("Ada");
        post.SetDescription("Short description");
        post.SetCreatedDate(new DateTime(2026, 7, 8, 0, 0, 0, DateTimeKind.Utc));

        var cut = RenderStatic<FeaturedPostsSection>(p => p
            .Add(c => c.Posts, [post])
            .Add(c => c.Title, "Featured")
            .Add(c => c.Description, "Hand picked")
            .Add(c => c.ViewAllUrl, "/blog")
            .Add(c => c.ViewAllText, "All posts")
            .Add(c => c.ReadMoreText, "Continue")
            .Add(c => c.DateFormat, "yyyy-MM-dd"));

        _provider.DidNotReceive().GetItemsByQueryAsync(Arg.Any<ContentQuery>());
        cut.Find(".osirion-section-title").TextContent.ShouldBe("Featured");
        cut.Find(".osirion-section-description").TextContent.ShouldBe("Hand picked");
        cut.Find(".osirion-featured-posts").ClassList.ShouldBe(["osirion-featured-posts", "osirion-layout-grid", "osirion-cols-3"], ignoreOrder: true);
        cut.Find("img.osirion-featured-post-image").GetAttribute("src").ShouldBe("/img/a.png");
        cut.Find("a.osirion-featured-post-image-link").GetAttribute("href").ShouldBe("/blog/with-image.md");
        var category = cut.Find("a.osirion-featured-post-category");
        category.GetAttribute("href").ShouldBe("/category/news");
        category.TextContent.Trim().ShouldBe("News");
        cut.Find(".osirion-featured-post-date").TextContent.Trim().ShouldBe("2026-07-08");
        cut.Find(".osirion-featured-post-excerpt").TextContent.ShouldBe("Short description");
        cut.Find(".osirion-author-name").TextContent.ShouldBe("Ada");
        cut.Find("a.osirion-featured-post-read-more").TextContent.ShouldContain("Continue");
        var viewAll = cut.Find("a.osirion-view-all-link");
        viewAll.GetAttribute("href").ShouldBe("/blog");
        viewAll.TextContent.ShouldContain("All posts");
    }

    [Fact]
    public void FeaturedPostsSection_EverythingOptionalHidden_RendersOnlyTheTitles()
    {
        var cut = RenderStatic<FeaturedPostsSection>(p => p
            .Add(c => c.Posts, [Post("Plain", categories: ["News"])])
            .Add(c => c.ShowMetadata, false)
            .Add(c => c.ShowExcerpt, false)
            .Add(c => c.ShowAuthor, false)
            .Add(c => c.ShowReadMoreLink, false)
            .Add(c => c.ShowViewAllLink, false)
            .Add(c => c.ViewAllUrl, "/blog"));

        cut.FindAll(".osirion-featured-post-image-placeholder").ShouldHaveSingleItem();
        cut.FindAll(".osirion-featured-post-meta").ShouldBeEmpty();
        cut.FindAll(".osirion-featured-post-excerpt").ShouldBeEmpty();
        cut.FindAll(".osirion-featured-post-author").ShouldBeEmpty();
        cut.FindAll(".osirion-featured-post-read-more").ShouldBeEmpty();
        cut.FindAll(".osirion-view-all-container").ShouldBeEmpty();
        cut.FindAll(".osirion-section-header").ShouldBeEmpty();
    }

    [Fact]
    public void FeaturedPostsSection_CategoriesHidden_KeepsTheDate()
    {
        var cut = RenderStatic<FeaturedPostsSection>(p => p
            .Add(c => c.Posts, [Post("Plain", categories: ["News"])])
            .Add(c => c.ShowCategories, false));

        cut.FindAll("a.osirion-featured-post-category").ShouldBeEmpty();
        cut.FindAll(".osirion-featured-post-date").ShouldHaveSingleItem();
    }

    [Fact]
    public void FeaturedPostsSection_Formatters_BuildThePostAndCategoryLinks()
    {
        var cut = RenderStatic<FeaturedPostsSection>(p => p
            .Add(c => c.Posts, [Post("Formatted", categories: ["News"])])
            .Add(c => c.PostUrlFormatter, post => $"/read/{post.Title}")
            .Add(c => c.CategoryUrlFormatter, category => $"/c/{category}"));

        cut.Find("a.osirion-featured-post-title-link").GetAttribute("href").ShouldBe("/read/Formatted");
        cut.Find("a.osirion-featured-post-category").GetAttribute("href").ShouldBe("/c/News");
    }

    [Theory]
    [InlineData("list", null, "osirion-layout-list")]
    [InlineData("carousel", null, "osirion-layout-carousel")]
    [InlineData("featured-first", null, "osirion-layout-featured-first")]
    [InlineData("grid", 2, "osirion-cols-2")]
    public void FeaturedPostsSection_Layout_SetsTheLayoutClass(string layout, int? columns, string expected)
    {
        var cut = RenderStatic<FeaturedPostsSection>(p =>
        {
            p.Add(c => c.Posts, [Post("Any")]).Add(c => c.Layout, layout);
            if (columns is { } count)
            {
                p.Add(c => c.Columns, count);
            }
        });

        cut.Find(".osirion-featured-posts").ClassList.ShouldContain(expected);
    }

    [Theory]
    [InlineData("<p>Short <b>text</b></p>", 150, "Short text")]
    [InlineData("<p>one two three four</p>", 10, "one two...")]
    [InlineData("abcdefghijklmnop", 5, "abcde...")]
    [InlineData("", 5, "")]
    public void FeaturedPostsSection_ExcerptFromContent_StripsTagsAndCutsAtAWord(string content, int length, string expected)
    {
        var post = ContentItem.Create("id", "Excerpt", content, "blog/excerpt.md", "test");

        var cut = RenderStatic<FeaturedPostsSection>(p => p
            .Add(c => c.Posts, [post])
            .Add(c => c.ExcerptLength, length));

        cut.Find(".osirion-featured-post-excerpt").TextContent.ShouldBe(expected);
    }

    // Fails before the fix: every post's date followed the server's culture instead of the post's own language.
    [Fact]
    public void FeaturedPostsSection_Posts_EachShowTheDateInTheirOwnLanguage()
    {
        using var culture = new CultureScope("de-DE");
        var english = Post("English");
        english.SetLocale("en");
        english.SetCreatedDate(new DateTime(2026, 10, 17, 0, 0, 0, DateTimeKind.Utc));
        var serbian = Post("Serbian");
        serbian.SetLocale("sr-Latn-RS");
        serbian.SetCreatedDate(new DateTime(2026, 10, 17, 0, 0, 0, DateTimeKind.Utc));

        var cut = RenderStatic<FeaturedPostsSection>(p => p.Add(c => c.Posts, [english, serbian]));

        var times = cut.FindAll(".osirion-featured-post-date time");
        times.Select(time => time.TextContent).ShouldBe(["Oct 17, 2026", "okt 17, 2026"]);
        times.Select(time => time.GetAttribute("datetime")).ShouldBe(["2026-10-17", "2026-10-17"]);
    }

    // Fails before the fix: the metadata date followed the server's culture instead of the item's own language.
    [Fact]
    public void OsirionContentListSection_Items_EachShowTheDateInTheirOwnLanguage()
    {
        using var culture = new CultureScope("de-DE");
        var english = Post("English");
        english.SetLocale("en");
        english.SetCreatedDate(new DateTime(2026, 10, 17, 0, 0, 0, DateTimeKind.Utc));
        var serbian = Post("Serbian");
        serbian.SetLocale("sr-Latn-RS");
        serbian.SetCreatedDate(new DateTime(2026, 10, 17, 0, 0, 0, DateTimeKind.Utc));

        var cut = RenderStatic<OsirionContentListSection>(p => p.Add(c => c.Items, new List<ContentItem> { english, serbian }));

        var times = cut.FindAll(".osirion-article-metadata-date time");
        times.Select(time => time.TextContent).ShouldBe(["10/17/2026", "17.10.2026."]);
        times.Select(time => time.GetAttribute("datetime")).ShouldBe(["2026-10-17", "2026-10-17"]);
    }

    [Fact]
    public void OsirionContentListSection_NoItems_QueriesTheManagerWithTheFilters()
    {
        RenderStatic<OsirionContentListSection>(p => p
            .Add(c => c.FeaturedOnly, true)
            .Add(c => c.Directory, "blog")
            .Add(c => c.Category, "news")
            .Add(c => c.Tag, "dotnet")
            .Add(c => c.Locale, "de")
            .Add(c => c.PostCount, 5)
            .Add(c => c.SortField, SortField.Title)
            .Add(c => c.SortDirection, Osirion.Blazor.Cms.Domain.Enums.SortDirection.Ascending));

        _manager.Received(1).GetContentByQueryAsync(Arg.Is<ContentQuery>(q =>
            q.IsFeatured == true && q.Directory == "blog" && q.Category == "news" && q.Tag == "dotnet"
            && q.Locale == "de" && q.Take == 5 && q.SortBy == SortField.Title
            && q.SortDirection == Osirion.Blazor.Cms.Domain.Enums.SortDirection.Ascending), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void OsirionContentListSection_NotFeaturedOnly_LeavesTheFeaturedFilterOpen()
    {
        RenderStatic<OsirionContentListSection>(_ => { });

        _manager.Received(1).GetContentByQueryAsync(Arg.Is<ContentQuery>(q => q.IsFeatured == null), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void OsirionContentListSection_Query_IsSentAsGiven()
    {
        var query = new ContentQuery { Tag = "given" };

        RenderStatic<OsirionContentListSection>(p => p.Add(c => c.Query, query));

        _manager.Received(1).GetContentByQueryAsync(query, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void OsirionContentListSection_ManagerThrows_LogsAndShowsNotFound()
    {
        _manager.GetContentByQueryAsync(Arg.Any<ContentQuery>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<ContentItem>>>(_ => throw new InvalidOperationException("down"));

        var cut = RenderStatic<OsirionContentListSection>(_ => { });

        _logs.Entries.ShouldContain(e => e.Level == LogLevel.Error && e.Exception is InvalidOperationException);
        cut.FindAll(".osirion-content-not-found").ShouldHaveSingleItem();
        cut.FindAll(".osirion-featured-post").ShouldBeEmpty();
        cut.Markup.ShouldNotContain("down");
    }

    [Fact]
    public void OsirionContentListSection_WhileLoading_ShowsThePageLoading()
    {
        var pending = new TaskCompletionSource<IReadOnlyList<ContentItem>>();
        _manager.GetContentByQueryAsync(Arg.Any<ContentQuery>(), Arg.Any<CancellationToken>()).Returns(pending.Task);

        var cut = RenderStatic<OsirionContentListSection>(p => p.Add(c => c.LoadingText, "Loading section"));

        cut.Find(".osirion-loading p").TextContent.ShouldBe("Loading section");

        pending.SetResult([Post("Loaded")]);

        cut.WaitForAssertion(() => cut.Find(".osirion-featured-post-title").TextContent.Trim().ShouldBe("Loaded"));
    }

    [Fact]
    public void OsirionContentListSection_SuppliedItems_RenderEveryPart()
    {
        var pictured = Post("Pictured", categories: ["News", "Tips"]);
        pictured.SetFeaturedImage("/img/p.png");
        pictured.SetUrl("/blog/pictured");
        pictured.SetDescription("Described");

        var cut = RenderStatic<OsirionContentListSection>(p => p
            .Add(c => c.Items, [pictured, Post("Plain")])
            .Add(c => c.Title, "Latest")
            .Add(c => c.Description, "From the blog")
            .Add(c => c.Layout, "list")
            .Add(c => c.ShowReadMoreLink, true)
            .Add(c => c.ReadMoreText, "More")
            .Add(c => c.ViewAllUrl, "/blog")
            .Add(c => c.Class, "extra"));

        _manager.DidNotReceive().GetContentByQueryAsync(Arg.Any<ContentQuery>(), Arg.Any<CancellationToken>());
        cut.Find("section").ClassList.ShouldContain("extra");
        cut.Find(".osirion-section-title").TextContent.ShouldBe("Latest");
        cut.Find(".osirion-section-description").TextContent.ShouldBe("From the blog");
        cut.Find(".osirion-featured-posts").ClassList.ShouldContain("osirion-layout-list");
        cut.Find("img.osirion-featured-post-image").GetAttribute("src").ShouldBe("/img/p.png");
        cut.FindAll(".osirion-featured-post-image-placeholder").ShouldHaveSingleItem();
        cut.Find("a.osirion-featured-post-title-link").GetAttribute("href").ShouldBe("/blog/pictured");
        cut.Find(".osirion-featured-post-excerpt").TextContent.ShouldBe("Described");
        cut.FindAll("a.osirion-featured-post-category").Select(a => a.GetAttribute("href")).ShouldBe(["/category/news", "/category/tips"]);
        cut.FindAll("a.osirion-featured-post-read-more").Count.ShouldBe(2);
        cut.Find("a.osirion-view-all-link").GetAttribute("href").ShouldBe("/blog");
    }

    [Fact]
    public void OsirionContentListSection_OptionalPartsHidden_RendersOnlyTitles()
    {
        var cut = RenderStatic<OsirionContentListSection>(p => p
            .Add(c => c.Items, [Post("Plain", categories: ["News"])])
            .Add(c => c.ShowExcerpt, false)
            .Add(c => c.ShowMetadata, false)
            .Add(c => c.ShowViewAllLink, false)
            .Add(c => c.ViewAllUrl, "/blog"));

        cut.FindAll(".osirion-featured-post-excerpt").ShouldBeEmpty();
        cut.FindAll(".osirion-featured-post-categories").ShouldBeEmpty();
        cut.FindAll(".osirion-view-all-container").ShouldBeEmpty();
        cut.FindAll(".osirion-featured-post-read-more").ShouldBeEmpty();
    }

    [Fact]
    public void OsirionContentListSection_CategoriesHiddenWithFormatter_ShowsNoCategoryLinks()
    {
        var cut = RenderStatic<OsirionContentListSection>(p => p
            .Add(c => c.Items, [Post("Plain", categories: ["News"])])
            .Add(c => c.ShowCategories, false));

        cut.FindAll("a.osirion-featured-post-category").ShouldBeEmpty();
    }

    [Fact]
    public void OsirionContentListSection_CategoryFormatter_BuildsTheCategoryLinks()
    {
        var cut = RenderStatic<OsirionContentListSection>(p => p
            .Add(c => c.Items, [Post("Plain", categories: ["News"])])
            .Add(c => c.CategoryUrlFormatter, category => $"/c/{category}"));

        cut.Find("a.osirion-featured-post-category").GetAttribute("href").ShouldBe("/c/News");
    }

    [Theory]
    [InlineData("carousel", "osirion-layout-carousel")]
    [InlineData("featured-first", "osirion-layout-featured-first")]
    [InlineData("grid", "osirion-cols-3")]
    public void OsirionContentListSection_Layout_SetsTheLayoutClass(string layout, string expected)
    {
        var cut = RenderStatic<OsirionContentListSection>(p => p
            .Add(c => c.Items, [Post("Any")])
            .Add(c => c.Layout, layout));

        cut.Find(".osirion-featured-posts").ClassList.ShouldContain(expected);
    }

    [Theory]
    [InlineData("<p>Short <b>text</b></p>", 160, "Short text")]
    [InlineData("<p>one two three four</p>", 10, "one two...")]
    [InlineData("abcdefghijklmnop", 5, "abcde...")]
    [InlineData("", 5, "")]
    public void OsirionContentListSection_ExcerptFromContent_StripsTagsAndCutsAtAWord(string content, int length, string expected)
    {
        var post = ContentItem.Create("id", "Excerpt", content, "blog/excerpt.md", "test");

        var cut = RenderStatic<OsirionContentListSection>(p => p
            .Add(c => c.Items, [post])
            .Add(c => c.ExcerptLength, length));

        cut.Find(".osirion-featured-post-excerpt").TextContent.ShouldBe(expected);
    }

    [Fact]
    public void ContentSection_AllParts_RenderWithTheirClasses()
    {
        var cut = RenderStatic<ContentSection>(p => p
            .Add(c => c.Title, "About")
            .Add(c => c.Subtitle, "Who we are")
            .Add(c => c.Content, "<p>Body text</p>")
            .Add(c => c.Variant, "highlight")
            .Add(c => c.Width, "wide")
            .Add(c => c.TextAlign, "center")
            .Add(c => c.Padding, "large")
            .Add(c => c.ActionText, "Contact")
            .Add(c => c.ActionUrl, "/contact")
            .Add(c => c.Class, "mine")
            .AddChildContent("<span class=\"child\">Child</span>"));

        cut.Find("section").ClassList.ShouldBe(
            ["osirion-content-section", "osirion-section-variant-highlight", "osirion-section-width-wide",
             "osirion-section-align-center", "osirion-section-padding-large", "mine"], ignoreOrder: true);
        cut.Find(".osirion-content-section-title").TextContent.ShouldBe("About");
        cut.Find(".osirion-content-section-subtitle").TextContent.ShouldBe("Who we are");
        cut.Find(".osirion-content-section-body").TextContent.ShouldContain("Body text");
        cut.Find(".osirion-content-section-children span.child").TextContent.ShouldBe("Child");
        var action = cut.Find("a.osirion-content-section-action");
        action.GetAttribute("href").ShouldBe("/contact");
        action.TextContent.ShouldContain("Contact");
    }

    [Fact]
    public void ContentSection_Defaults_RenderOnlyTheEmptyBody()
    {
        var cut = RenderStatic<ContentSection>(_ => { });

        cut.Find("section").ClassList.ShouldBe(
            ["osirion-content-section", "osirion-section-variant-default", "osirion-section-width-normal",
             "osirion-section-align-left", "osirion-section-padding-normal"], ignoreOrder: true);
        cut.FindAll(".osirion-content-section-header").ShouldBeEmpty();
        cut.FindAll(".osirion-content-section-children").ShouldBeEmpty();
        cut.FindAll(".osirion-content-section-footer").ShouldBeEmpty();
        cut.Find(".osirion-content-section-body").TextContent.Trim().ShouldBeEmpty();
    }

    [Fact]
    public void ContentSection_TitleWithoutSubtitleAndActionWithoutUrl_RendersTheTitleOnly()
    {
        var cut = RenderStatic<ContentSection>(p => p
            .Add(c => c.Title, "About")
            .Add(c => c.ActionText, "Contact"));

        cut.FindAll(".osirion-content-section-subtitle").ShouldBeEmpty();
        cut.FindAll(".osirion-content-section-footer").ShouldBeEmpty();
    }

    private static ContentItem Post(string title, string[]? categories = null)
    {
        var post = ContentItem.Create($"id-{title}", title, "<p>Body</p>", $"blog/{title.ToLowerInvariant().Replace(' ', '-')}.md", "test");
        foreach (var category in categories ?? [])
        {
            post.AddCategory(category);
        }

        return post;
    }
}
