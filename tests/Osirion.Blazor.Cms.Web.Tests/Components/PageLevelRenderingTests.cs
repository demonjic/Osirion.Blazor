using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Osirion.Blazor.Cms.Components;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Enums;
using Osirion.Blazor.Cms.Domain.Repositories;
using Osirion.Blazor.Cms.Domain.Services;
using Osirion.Blazor.Cms.Web.Components;
using Osirion.Blazor.Cms.Web.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.Components;

public class PageLevelRenderingTests : CmsWebComponentTestBase
{
    private readonly IContentProvider _provider = Substitute.For<IContentProvider>();
    private readonly IContentProviderManager _manager = Substitute.For<IContentProviderManager>();
    private readonly RecordingNavigationManager _navigation = new();
    private readonly RecordingLoggerFactory _logs = new();

    public PageLevelRenderingTests()
    {
        Services.AddSingleton(_manager);
        Services.AddSingleton<NavigationManager>(_navigation);
        Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new Osirion.Blazor.Cms.Web.Options.SeoMetadataOptions()));
        _logs.Register(Services);
        _manager.GetDefaultProvider().Returns(_provider);
        _manager.GetContentByQueryAsync(Arg.Any<ContentQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>>([]));
        _provider.GetItemsByQueryAsync(Arg.Any<ContentQuery>()).Returns(Task.FromResult<IReadOnlyList<ContentItem>?>([]));
        _provider.GetCategoriesAsync().Returns(Task.FromResult<IReadOnlyList<ContentCategory>>([ContentCategory.Create("News", "news", 1)]));
        _provider.GetTagsAsync().Returns(Task.FromResult<IReadOnlyList<ContentTag>>([ContentTag.Create("dotnet", "dotnet", 1)]));
    }

    // Known defect, pinned as current behavior: the default content links keep the ".md" extension of the source file, while ContentPage strips it, so the two disagree on the item URL. Update this expectation when it is fixed.
    [Fact]
    public void DocumentPage_ContentPath_LoadsTheDocumentAndItsNeighbours()
    {
        var directory = DirectoryItem.Create("d", "docs", "Docs", "test");
        var intro = Doc("intro");
        var install = Doc("install");
        var usage = Doc("usage");
        directory.AddItem(install);
        _provider.GetItemByPathAsync("docs/install.md").Returns(install);
        _provider.GetItemsByQueryAsync(Arg.Any<ContentQuery>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>?>([intro, install, usage]));

        var cut = RenderStatic<DocumentPage>(p => p.Add(c => c.ContentPath, "docs/install.md"));

        _provider.Received(1).GetItemsByQueryAsync(Arg.Is<ContentQuery>(q =>
            q.Directory == "docs" && q.SortBy == SortField.Order
            && q.SortDirection == Osirion.Blazor.Cms.Domain.Enums.SortDirection.Ascending));
        cut.Find(".osirion-document-title").TextContent.ShouldBe("install");
        var previous = cut.Find("a.osirion-doc-nav-previous");
        previous.GetAttribute("href").ShouldBe("/docs/intro");
        previous.QuerySelector(".osirion-doc-nav-title")!.TextContent.ShouldBe("intro");
        cut.Find("a.osirion-doc-nav-next").GetAttribute("href").ShouldBe("/docs/usage");
        _navigation.NotFoundCalls.ShouldBe(0);
    }

    [Fact]
    public void DocumentPage_ContentPathNotFound_SignalsNotFoundOnce()
    {
        var cut = RenderStatic<DocumentPage>(p => p.Add(c => c.ContentPath, "docs/missing.md"));
        cut.Render(p => p.Add(c => c.ContentPath, "docs/missing.md"));

        _navigation.NotFoundCalls.ShouldBe(1);
        cut.FindAll(".osirion-content-not-found").ShouldHaveSingleItem();
        cut.FindAll("article").ShouldBeEmpty();
    }

    [Fact]
    public void DocumentPage_ProviderThrows_LogsAndShowsNotFound()
    {
        _provider.GetItemByPathAsync("docs/a.md").Returns<Task<ContentItem?>>(_ => throw new InvalidOperationException("down"));

        var cut = RenderStatic<DocumentPage>(p => p.Add(c => c.ContentPath, "docs/a.md"));

        _logs.Entries.ShouldContain(e => e.Level == LogLevel.Error && e.Exception is InvalidOperationException);
        cut.FindAll(".osirion-content-not-found").ShouldHaveSingleItem();
        cut.Markup.ShouldNotContain("down");
    }

    [Fact]
    public void DocumentPage_NoDefaultProvider_ShowsNotFound()
    {
        _manager.GetDefaultProvider().Returns((IContentProvider?)null);

        var cut = RenderStatic<DocumentPage>(p => p.Add(c => c.ContentPath, "docs/a.md"));

        cut.FindAll(".osirion-content-not-found").ShouldHaveSingleItem();
    }

    [Fact]
    public void DocumentPage_WhileLoading_ShowsThePageLoading()
    {
        var pending = new TaskCompletionSource<ContentItem?>();
        _provider.GetItemByPathAsync("docs/slow.md").Returns(pending.Task);

        var cut = RenderStatic<DocumentPage>(p => p
            .Add(c => c.ContentPath, "docs/slow.md")
            .Add(c => c.LoadingText, "Opening"));

        cut.Find(".osirion-loading p").TextContent.ShouldBe("Opening");

        pending.SetResult(Doc("slow"));

        cut.WaitForAssertion(() => cut.Find(".osirion-document-title").TextContent.ShouldBe("slow"));
    }

    [Fact]
    public void DocumentPage_NoContentAndNoPath_RendersTheLayoutWithoutAnArticle()
    {
        var cut = RenderStatic<DocumentPage>(p => p.Add(c => c.Class, "docs"));

        cut.Find("div.osirion-document-page").ClassList.ShouldContain("docs");
        cut.FindAll(".osirion-document-layout").ShouldHaveSingleItem();
        cut.FindAll("article").ShouldBeEmpty();
        _navigation.NotFoundCalls.ShouldBe(0);
    }

    [Fact]
    public void DocumentPage_SuppliedContent_RendersMetadataTocFooterAndFormattedNeighbourLinks()
    {
        using var culture = new CultureScope("en-US");
        var content = Doc("guide", "<h2>Setup</h2><p>Text</p>");
        content.SetAuthor("Ada");
        content.SetCreatedDate(new DateTime(2026, 2, 3, 0, 0, 0, DateTimeKind.Utc));
        content.LastModified = new DateTime(2026, 2, 3, 0, 0, 0, DateTimeKind.Utc);
        _provider.GetItemsByQueryAsync(Arg.Any<ContentQuery>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>?>([Doc("before"), content, Doc("after")]));

        var cut = RenderStatic<DocumentPage>(p => p
            .Add(c => c.Content, content)
            .Add(c => c.TableOfContentsTitle, "Contents")
            .Add(c => c.CustomFooterContent, "<p>Footer note</p>")
            .Add(c => c.ContentUrlFormatter, i => $"/read/{i.Title}"));

        cut.Find(".osirion-document-author").TextContent.ShouldContain("Ada");
        cut.Find(".osirion-document-date").TextContent.ShouldContain("February 3, 2026");
        cut.Find(".osirion-toc-title").TextContent.ShouldBe("Contents");
        cut.Find(".osirion-document-toc a.osirion-toc-link").TextContent.ShouldBe("Setup");
        cut.Find("a.osirion-doc-nav-previous").GetAttribute("href").ShouldBe("/read/before");
        cut.Find("a.osirion-doc-nav-next").GetAttribute("href").ShouldBe("/read/after");
        cut.Find(".osirion-section-variant-highlight").TextContent.ShouldContain("Footer note");
        cut.Find("article").GetAttribute("itemtype").ShouldBe("https://schema.org/TechArticle");
    }

    [Fact]
    public void DocumentPage_OptionalPartsHidden_RendersOnlyTheTitleAndBody()
    {
        var cut = RenderStatic<DocumentPage>(p => p
            .Add(c => c.Content, Doc("plain"))
            .Add(c => c.ShowMetadata, false)
            .Add(c => c.ShowTableOfContents, false)
            .Add(c => c.ShowNavigationLinks, false)
            .Add(c => c.ShowBreadcrumbs, false)
            .Add(c => c.ShowSideNavigation, false));

        _provider.DidNotReceive().GetItemsByQueryAsync(Arg.Any<ContentQuery>());
        cut.Find("div.osirion-document-page").ClassList.ShouldNotContain("osirion-document-with-sidenav");
        cut.FindAll(".osirion-document-meta").ShouldBeEmpty();
        cut.FindAll(".osirion-document-toc").ShouldBeEmpty();
        cut.FindAll(".osirion-document-pagination").ShouldBeEmpty();
        cut.FindAll(".osirion-breadcrumbs").ShouldBeEmpty();
        cut.FindAll(".osirion-document-sidenav").ShouldBeEmpty();
        cut.Find(".osirion-document-body").TextContent.ShouldContain("Body");
    }

    // Known defect, pinned as current behavior: the default content links keep the ".md" extension of the source file, while ContentPage strips it, so the two disagree on the item URL. Update this expectation when it is fixed.
    [Fact]
    public void DocumentPage_SuppliedNeighbours_AreShownWithoutQuerying()
    {
        var cut = RenderStatic<DocumentPage>(p => p
            .Add(c => c.Content, Doc("middle"))
            .Add(c => c.PreviousItem, Doc("first"))
            .Add(c => c.NextItem, Doc("last"))
            .Add(c => c.ShowSideNavigation, false));

        _provider.DidNotReceive().GetItemsByQueryAsync(Arg.Any<ContentQuery>());
        cut.Find("a.osirion-doc-nav-previous").GetAttribute("href").ShouldBe("/docs/first");
    }

    [Fact]
    public void DocumentPage_NeighbourQueryThrows_LogsAndShowsTheDocument()
    {
        _provider.GetItemsByQueryAsync(Arg.Any<ContentQuery>())
            .Returns<Task<IReadOnlyList<ContentItem>?>>(_ => throw new InvalidOperationException("down"));

        var cut = RenderStatic<DocumentPage>(p => p.Add(c => c.Content, Doc("guide")));

        _logs.Entries.ShouldContain(e => e.Level == LogLevel.Error && e.Exception is InvalidOperationException);
        cut.Find(".osirion-document-title").TextContent.ShouldBe("guide");
        cut.FindAll(".osirion-document-pagination").ShouldBeEmpty();
    }

    [Fact]
    public void DocumentPage_NeighbourQueryReturnsNullOrNoProvider_ShowsNoPagination()
    {
        _provider.GetItemsByQueryAsync(Arg.Any<ContentQuery>()).Returns(Task.FromResult<IReadOnlyList<ContentItem>?>(null));

        var withNull = RenderStatic<DocumentPage>(p => p.Add(c => c.Content, Doc("guide")));
        _manager.GetDefaultProvider().Returns((IContentProvider?)null);
        var withoutProvider = RenderStatic<DocumentPage>(p => p.Add(c => c.Content, Doc("guide")));

        withNull.FindAll(".osirion-document-pagination").ShouldBeEmpty();
        withoutProvider.FindAll(".osirion-document-pagination").ShouldBeEmpty();
    }

    [Fact]
    public void DocumentPage_SideNavigation_MarksTheCurrentPageAndExpandsItsParent()
    {
        var items = new List<DocumentNavigationItem>
        {
            new()
            {
                Title = "Guides",
                Url = "/docs/guides",
                Children =
                [
                    new() { Title = "Install", Url = "/docs/install", Path = "docs/install.md" },
                    new() { Title = "Usage", Url = "/docs/usage" }
                ]
            },
            new()
            {
                Title = "Reference",
                Url = "/docs/reference",
                Children = [new() { Title = "Api", Url = "/docs/api" }]
            },
            new() { Title = "Faq", Url = "/docs/faq", IsExpanded = true, Children = [new() { Title = "General", Url = "/docs/general" }] },
            new() { Title = "Empty", Url = "/docs/empty", Children = [] }
        };

        var cut = RenderStatic<DocumentPage>(p => p
            .Add(c => c.Content, Doc("install"))
            .Add(c => c.NavigationTitle, "Documentation")
            .Add(c => c.NavigationItems, items));

        cut.Find(".osirion-sidenav-title").TextContent.ShouldBe("Documentation");
        var active = cut.Find("a.osirion-sidenav-active");
        active.TextContent.Trim().ShouldBe("Install");
        active.GetAttribute("aria-current").ShouldBe("page");
        cut.FindAll(".osirion-sidenav-sublist").Count.ShouldBe(2);
        cut.FindAll(".osirion-sidenav-sublist a").Select(a => a.TextContent.Trim()).ShouldBe(["Install", "Usage", "General"]);
        cut.FindAll("a[aria-current]").ShouldHaveSingleItem();
    }

    [Fact]
    public void DocumentPage_CurrentPathMatchedByUrl_MarksTheLinkWithTheFormattedUrl()
    {
        var items = new List<DocumentNavigationItem>
        {
            new() { Title = "Install", Url = "docs/install" },
            new() { Title = "Usage", Url = "/docs/usage" }
        };

        var cut = RenderStatic<DocumentPage>(p => p
            .Add(c => c.CurrentPath, "/docs/install")
            .Add(c => c.NavigationItems, items)
            .Add(c => c.NavigationUrlFormatter, item => $"/v2/{item.Url.TrimStart('/')}"));

        var links = cut.FindAll(".osirion-sidenav-link");
        links.Select(a => a.GetAttribute("href")).ShouldBe(["/v2/docs/install", "/v2/docs/usage"]);
        links[0].ClassList.ShouldContain("osirion-sidenav-active");
        links[1].ClassList.ShouldNotContain("osirion-sidenav-active");
    }

    [Fact]
    public void DocumentPage_NoCurrentPath_MarksNothingActive()
    {
        var cut = RenderStatic<DocumentPage>(p => p
            .Add(c => c.NavigationItems, [new DocumentNavigationItem { Title = "Install", Url = "/docs/install", Children = [new() { Title = "Sub", Url = "/s" }] }]));

        cut.FindAll(".osirion-sidenav-active").ShouldBeEmpty();
        cut.FindAll(".osirion-sidenav-sublist").ShouldBeEmpty();
    }

    [Theory]
    [InlineData(SchemaType.FAQ, "https://schema.org/FAQPage")]
    [InlineData(SchemaType.BlogPosting, "https://schema.org/BlogPosting")]
    public void DocumentPage_FirstSchemaType_SetsTheItemType(SchemaType schemaType, string expected)
    {
        var cut = RenderStatic<DocumentPage>(p => p
            .Add(c => c.Content, Doc("guide"))
            .Add(c => c.SchemaTypes, [schemaType]));

        cut.Find("article").GetAttribute("itemtype").ShouldBe(expected);
    }

    [Fact]
    public void ArticlePage_WhileLoading_ShowsThePageLoading()
    {
        var pending = new TaskCompletionSource<IReadOnlyList<ContentItem>>();
        _manager.GetContentByQueryAsync(Arg.Any<ContentQuery>(), Arg.Any<CancellationToken>()).Returns(pending.Task);

        var cut = RenderStatic<ArticlePage>(p => p
            .Add(c => c.ItemSlug, "slow")
            .Add(c => c.LoadingText, "Opening article"));

        cut.Find(".osirion-loading p").TextContent.ShouldBe("Opening article");

        pending.SetResult([Doc("slow")]);

        cut.WaitForAssertion(() => cut.Find(".osirion-hero-title").TextContent.ShouldBe("slow"));
        cut.FindAll(".osirion-loading").ShouldBeEmpty();
    }

    [Fact]
    public void ArticlePage_ItemSuppliedByTheParent_IsShownWithoutLoadingAndWithTheCategoryFormatter()
    {
        var item = Doc("supplied");
        item.AddCategory("News");

        var cut = RenderStatic<ArticlePage>(p => p
            .Add(c => c.Item, item)
            .Add(c => c.ShowRelatedContent, false)
            .Add(c => c.ShowNavigation, false)
            .Add(c => c.Class, "wide")
            .Add(c => c.CategoryUrlFormatter, c => $"/c/{c}"));

        _manager.DidNotReceive().GetContentByQueryAsync(Arg.Any<ContentQuery>(), Arg.Any<CancellationToken>());
        cut.Find("div.osirion-article-page").ClassList.ShouldContain("wide");
        cut.Find("a.osirion-sidebar-category").GetAttribute("href").ShouldBe("/c/News");
    }

    // Fails before the fix: the hero's date followed the server's culture instead of the article's language.
    [Fact]
    public void ArticlePage_EnglishArticleOnASerbianServer_ShowsTheHeroDateInEnglish()
    {
        using var culture = new CultureScope("sr-Latn-RS");
        var item = Doc("article");
        item.SetLocale("en");
        item.SetCreatedDate(new DateTime(2026, 10, 17, 0, 0, 0, DateTimeKind.Utc));

        var cut = RenderStatic<ArticlePage>(p => p
            .Add(c => c.Item, item)
            .Add(c => c.ShowRelatedContent, false)
            .Add(c => c.ShowNavigation, false));

        cut.Find(".osirion-article-metadata-date time").TextContent.ShouldBe("10/17/2026");
    }

    [Fact]
    public void ArticlePage_SidebarHidden_LeavesOutTheSidebarClass()
    {
        var cut = RenderStatic<ArticlePage>(p => p
            .Add(c => c.Item, Doc("supplied"))
            .Add(c => c.ShowSidebar, false));

        cut.Find("div.osirion-article-page").ClassList.ShouldNotContain("osirion-article-with-sidebar");
        cut.FindAll(".osirion-sidebar-section").ShouldBeEmpty();
    }

    [Fact]
    public void ArticlePage_RelatedQueryThrows_LogsAndStillShowsTheArticle()
    {
        var item = Doc("tagged");
        item.AddTag("a");
        _manager.GetContentByQueryAsync(Arg.Is<ContentQuery>(q => q.Slug == "tagged"), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>>([item]));
        _manager.GetContentByQueryAsync(Arg.Is<ContentQuery>(q => q.Slug == null), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<ContentItem>>>(_ => throw new InvalidOperationException("related"));

        var cut = RenderStatic<ArticlePage>(p => p
            .Add(c => c.ItemSlug, "tagged")
            .Add(c => c.ShowNavigation, false));

        _logs.Entries.ShouldContain(e => e.Level == LogLevel.Error && e.Exception is InvalidOperationException);
        cut.Instance.Item.ShouldBeSameAs(item);
        cut.Instance.RelatedItems.ShouldBeNull();
    }

    [Fact]
    public void HomePage_Content_RendersTheHeroBodyFeaturedSectionAndHead()
    {
        var head = Render<HeadOutlet>();
        var content = Doc("Welcome", "<p>Home body</p>");
        content.SetDescription("Start here");
        // The section renders for the featured items of the locale; without any it is left out.
        _manager.GetContentByQueryAsync(Arg.Is<ContentQuery>(q => q.IsFeatured == true), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>>([Doc("Pick")]));

        var cut = RenderStatic<HomePage>(p => p
            .Add(c => c.Content, content)
            .Add(c => c.FeaturedSectionTitle, "Picks")
            .Add(c => c.Class, "home"));

        cut.Find("div.osirion-homepage").ClassList.ShouldContain("home");
        cut.Find(".osirion-hero-title").TextContent.ShouldBe("Welcome");
        cut.Find(".osirion-hero-subtitle").TextContent.ShouldBe("Start here");
        cut.Find(".osirion-homepage-content").TextContent.ShouldContain("Home body");
        cut.Find(".osirion-section-title").TextContent.ShouldBe("Picks");
        head.WaitForAssertion(() => head.Markup.ShouldContain("Welcome"));
        _navigation.NotFoundCalls.ShouldBe(0);
    }

    [Fact]
    public void HomePage_NoContentAndNoChildContent_SignalsNotFound()
    {
        var cut = RenderStatic<HomePage>(_ => { });

        _navigation.NotFoundCalls.ShouldBe(1);
        cut.FindAll(".osirion-content-not-found").ShouldHaveSingleItem();
    }

    [Fact]
    public void HomePage_ChildContentOnly_RendersItWithoutHeroOrFeatured()
    {
        var cut = RenderStatic<HomePage>(p => p
            .Add(c => c.ShowHero, false)
            .Add(c => c.ShowFeaturedContent, false)
            .AddChildContent("<p class=\"own\">Own content</p>"));

        _navigation.NotFoundCalls.ShouldBe(0);
        cut.Find("p.own").TextContent.ShouldBe("Own content");
        cut.FindAll(".osirion-hero-title").ShouldBeEmpty();
        cut.FindAll(".osirion-featured-posts-section").ShouldBeEmpty();
    }

    [Fact]
    public void HomePage_BrowseAndCustomContent_RenderTheCategoriesTagsAndCustomBlock()
    {
        var cut = RenderStatic<HomePage>(p => p
            .Add(c => c.Content, Doc("Welcome"))
            .Add(c => c.ShowFeaturedContent, false)
            .Add(c => c.ShowCategories, true)
            .Add(c => c.ShowTags, true)
            .Add(c => c.BrowseSectionTitle, "Explore")
            .Add(c => c.CustomContent, "<p>Custom block</p>"));

        cut.Find(".osirion-homepage-browse .osirion-content-section-title").TextContent.ShouldBe("Explore");
        cut.Find(".osirion-homepage-browse a.osirion-category-link").GetAttribute("href").ShouldBe("/category/news");
        cut.Find(".osirion-homepage-browse a.osirion-tag-link").GetAttribute("href").ShouldBe("/tag/dotnet");
        cut.Find(".osirion-homepage-custom").TextContent.ShouldContain("Custom block");
    }

    // Threw InvalidCastException before the fix: the page passed its Func<string, string> formatters to
    // components that take a category or tag.
    [Fact]
    public void HomePage_UrlFormatters_ApplyToTheCategoryAndTagLinks()
    {
        var cut = RenderStatic<HomePage>(p => p
            .Add(c => c.Content, Doc("Welcome"))
            .Add(c => c.ShowFeaturedContent, false)
            .Add(c => c.ShowCategories, true)
            .Add(c => c.ShowTags, true)
            .Add(c => c.CategoryUrlFormatter, name => $"/topics/{name}")
            .Add(c => c.TagUrlFormatter, name => $"/labels/{name}"));

        cut.Find(".osirion-homepage-browse a.osirion-category-link").GetAttribute("href").ShouldBe("/topics/News");
        cut.Find(".osirion-homepage-browse a.osirion-tag-link").GetAttribute("href").ShouldBe("/labels/dotnet");
    }

    [Fact]
    public void HomePage_OnlyTags_RendersTheTagCloudWithoutCategories()
    {
        var cut = RenderStatic<HomePage>(p => p
            .Add(c => c.Content, Doc("Welcome"))
            .Add(c => c.ShowFeaturedContent, false)
            .Add(c => c.ShowTags, true));

        cut.FindAll(".osirion-homepage-browse a.osirion-tag-link").ShouldHaveSingleItem();
        cut.FindAll(".osirion-homepage-browse .osirion-categories-list-container").ShouldBeEmpty();
    }

    [Fact]
    public void HomePage_WhileLoading_ShowsThePageLoading()
    {
        var pending = new TaskCompletionSource<IReadOnlyList<ContentItem>>();
        _manager.GetContentByQueryAsync(Arg.Any<ContentQuery>(), Arg.Any<CancellationToken>()).Returns(pending.Task);

        var cut = RenderStatic<HomePage>(p => p.Add(c => c.LoadingText, "Opening home"));

        cut.Find(".osirion-loading p").TextContent.ShouldBe("Opening home");
        _navigation.NotFoundCalls.ShouldBe(0);

        pending.SetResult([]);

        cut.WaitForAssertion(() => cut.FindAll(".osirion-content-not-found").ShouldHaveSingleItem());
        _navigation.NotFoundCalls.ShouldBe(1);
    }

    [Fact]
    public void LandingPage_ContentPath_LoadsTheContentIntoTheHeroAndSection()
    {
        var head = Render<HeadOutlet>();
        var content = Doc("Launch", "<p>Landing body</p>");
        content.SetDescription("Big news");
        content.SetFeaturedImage("/img/launch.png");
        _provider.GetItemByPathAsync("pages/launch.md").Returns(content);

        var cut = RenderStatic<LandingPage>(p => p
            .Add(c => c.ContentPath, "pages/launch.md")
            .Add(c => c.HeroTitle, "Ignored")
            .Add(c => c.ContentWidth, "wide")
            .Add(c => c.ContentAlign, "center")
            .Add(c => c.ContentVariant, "highlight")
            .Add(c => c.ContentPadding, "large")
            .Add(c => c.Class, "landing"));

        cut.Find("div.osirion-landing-page").ClassList.ShouldContain("landing");
        cut.Find(".osirion-hero-title").TextContent.ShouldBe("Launch");
        cut.Find(".osirion-hero-subtitle").TextContent.ShouldBe("Big news");
        var section = cut.Find("section.osirion-content-section");
        section.ClassList.ShouldContain("osirion-section-width-wide");
        section.ClassList.ShouldContain("osirion-section-align-center");
        section.ClassList.ShouldContain("osirion-section-variant-highlight");
        section.ClassList.ShouldContain("osirion-section-padding-large");
        section.TextContent.ShouldContain("Landing body");
        head.WaitForAssertion(() => head.Markup.ShouldContain("Launch"));
    }

    [Fact]
    public void LandingPage_NoContent_UsesTheHeroParametersCustomContentAndChildContent()
    {
        var cut = RenderStatic<LandingPage>(p => p
            .Add(c => c.HeroTitle, "Hello")
            .Add(c => c.HeroSubtitle, "World")
            .Add(c => c.CustomContent, "<p>Custom</p>")
            .AddChildContent("<p class=\"own\">Own</p>"));

        _provider.DidNotReceive().GetItemByPathAsync(Arg.Any<string>());
        cut.Find(".osirion-hero-title").TextContent.ShouldBe("Hello");
        cut.Find(".osirion-hero-subtitle").TextContent.ShouldBe("World");
        cut.Find(".osirion-section-variant-highlight").TextContent.ShouldContain("Custom");
        cut.Find("p.own").TextContent.ShouldBe("Own");
    }

    [Fact]
    public void LandingPage_HeroHidden_RendersNoHero()
    {
        var cut = RenderStatic<LandingPage>(p => p
            .Add(c => c.Content, Doc("Launch"))
            .Add(c => c.ShowHero, false));

        cut.FindAll(".osirion-hero-title").ShouldBeEmpty();
        cut.FindAll("section.osirion-content-section").ShouldHaveSingleItem();
    }

    [Fact]
    public void LandingPage_ContentPathNotFound_ShowsTheNotFoundText()
    {
        var cut = RenderStatic<LandingPage>(p => p
            .Add(c => c.ContentPath, "pages/missing.md")
            .Add(c => c.NotFoundText, "No such page"));

        cut.Find(".osirion-not-found p").TextContent.ShouldBe("No such page");
    }

    [Fact]
    public void LandingPage_NoDefaultProvider_ShowsTheNotFoundText()
    {
        _manager.GetDefaultProvider().Returns((IContentProvider?)null);

        var cut = RenderStatic<LandingPage>(p => p.Add(c => c.ContentPath, "pages/a.md"));

        cut.FindAll(".osirion-not-found").ShouldHaveSingleItem();
    }

    [Fact]
    public void LandingPage_ProviderThrows_LogsAndShowsTheNotFoundText()
    {
        _provider.GetItemByPathAsync("pages/a.md").Returns<Task<ContentItem?>>(_ => throw new InvalidOperationException("down"));

        var cut = RenderStatic<LandingPage>(p => p.Add(c => c.ContentPath, "pages/a.md"));

        _logs.Entries.ShouldContain(e => e.Level == LogLevel.Error && e.Exception is InvalidOperationException);
        cut.FindAll(".osirion-not-found").ShouldHaveSingleItem();
    }

    [Fact]
    public void LandingPage_InteractiveWhileLoading_ShowsTheLoadingText()
    {
        var pending = new TaskCompletionSource<ContentItem?>();
        _provider.GetItemByPathAsync("pages/slow.md").Returns(pending.Task);
        SetRendererInfo(new RendererInfo("Server", true));

        var cut = Render<LandingPage>(p => p
            .Add(c => c.ContentPath, "pages/slow.md")
            .Add(c => c.LoadingText, "Preparing"));

        cut.Find(".osirion-loading p").TextContent.ShouldBe("Preparing");

        pending.SetResult(Doc("slow"));

        cut.WaitForAssertion(() => cut.Find(".osirion-hero-title").TextContent.ShouldBe("slow"));
    }

    [Fact]
    public void ContentListPage_NoItems_LoadsThemForTheDirectoryAndLocale()
    {
        _manager.GetContentByQueryAsync(Arg.Any<ContentQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>>([Doc("a"), Doc("b")]));

        var cut = RenderStatic<ListPage>(p => p
            .Add(c => c.DirectoryName, "blog")
            .Add(c => c.Locale, "de"));

        _manager.Received(1).GetContentByQueryAsync(Arg.Is<ContentQuery>(q => q.Directory == "blog" && q.Locale == "de"), Arg.Any<CancellationToken>());
        cut.Markup.ShouldBe("items:2");
    }

    [Fact]
    public void ContentListPage_Query_IsSentAsGiven()
    {
        var query = new ContentQuery { Tag = "given" };

        RenderStatic<ListPage>(p => p.Add(c => c.Query, query));

        _manager.Received(1).GetContentByQueryAsync(query, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ContentListPage_ItemsSupplied_AreKeptWithoutQuerying()
    {
        var cut = RenderStatic<ListPage>(p => p.Add(c => c.Items, [Doc("own")]));

        _manager.DidNotReceive().GetContentByQueryAsync(Arg.Any<ContentQuery>(), Arg.Any<CancellationToken>());
        cut.Markup.ShouldBe("items:1");
    }

    [Fact]
    public void ContentListPage_QueryThrows_LogsAndLeavesItemsEmpty()
    {
        _manager.GetContentByQueryAsync(Arg.Any<ContentQuery>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<ContentItem>>>(_ => throw new InvalidOperationException("down"));

        var cut = RenderStatic<ListPage>(_ => { });

        _logs.Entries.ShouldContain(e => e.Level == LogLevel.Error && e.Exception is InvalidOperationException);
        cut.Markup.ShouldBe("items:none");
    }

    [Fact]
    public void ContentListPage_WhileLoading_ReportsLoading()
    {
        var pending = new TaskCompletionSource<IReadOnlyList<ContentItem>>();
        _manager.GetContentByQueryAsync(Arg.Any<ContentQuery>(), Arg.Any<CancellationToken>()).Returns(pending.Task);

        var cut = RenderStatic<ListPage>(_ => { });

        cut.Markup.ShouldBe("loading");

        pending.SetResult([Doc("late")]);

        cut.WaitForAssertion(() => cut.Markup.ShouldBe("items:1"));
    }

    private static ContentItem Doc(string name, string content = "<p>Body</p>")
    {
        var item = ContentItem.Create($"id-{name}", name, content, $"docs/{name}.md", "test");
        item.SetUrl($"docs/{name}");
        return item;
    }

    private sealed class ListPage : OsirionContentListPageBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder) =>
            builder.AddContent(0, IsLoading ? "loading" : $"items:{Items?.Count.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "none"}");
    }

    private sealed class RecordingNavigationManager : NavigationManager
    {
        public RecordingNavigationManager()
        {
            Initialize("http://localhost/", "http://localhost/docs/page");
            OnNotFound += (_, _) => NotFoundCalls++;
        }

        public int NotFoundCalls { get; private set; }

        protected override void NavigateToCore(string uri, NavigationOptions options) => Uri = ToAbsoluteUri(uri).ToString();
    }
}
