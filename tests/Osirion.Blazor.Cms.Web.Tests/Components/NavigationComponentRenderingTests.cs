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

public class NavigationComponentRenderingTests : CmsWebComponentTestBase
{
    private readonly IContentProvider _provider = Substitute.For<IContentProvider>();
    private readonly IContentProviderManager _manager = Substitute.For<IContentProviderManager>();
    private readonly RecordingLoggerFactory _logs = new();

    public NavigationComponentRenderingTests()
    {
        Services.AddSingleton(_manager);
        _logs.Register(Services);
        _manager.GetDefaultProvider().Returns(_provider);
        _provider.GetCategoriesAsync().Returns(Task.FromResult<IReadOnlyList<ContentCategory>>(
        [
            ContentCategory.Create("Beta", "beta", 1),
            ContentCategory.Create("Alpha", "alpha", 5),
            ContentCategory.Create("Gamma", "gamma", 3)
        ]));
        _provider.GetTagsAsync().Returns(Task.FromResult<IReadOnlyList<ContentTag>>(
        [
            ContentTag.Create("beta", "beta", 1),
            ContentTag.Create("alpha", "alpha", 5),
            ContentTag.Create("gamma", "gamma", 3)
        ]));
    }

    // Known defect, pinned as current behavior: the internal category links carry rel="nofollow", which tells crawlers not to follow the site's own pages. Update this expectation when it is fixed.
    [Fact]
    public void CategoriesList_Default_ListsCategoriesByCountWithCounts()
    {
        var cut = RenderStatic<CategoriesList>(p => p
            .Add(c => c.Title, "Categories")
            .Add(c => c.Class, "side"));

        cut.Find("div.osirion-categories-list-container").ClassList.ShouldContain("side");
        cut.Find(".osirion-categories-title").TextContent.ShouldBe("Categories");
        var links = cut.FindAll("a.osirion-category-link");
        links.Select(a => a.GetAttribute("href")).ShouldBe(["/category/alpha", "/category/gamma", "/category/beta"]);
        cut.FindAll(".osirion-category-count").Select(c => c.TextContent).ShouldBe(["5", "3", "1"]);
        links.ShouldAllBe(a => a.GetAttribute("rel") == "nofollow");
    }

    [Fact]
    public void CategoriesList_SortedByNameAndLimited_ShowsTheFirstNamesWithoutCounts()
    {
        var cut = RenderStatic<CategoriesList>(p => p
            .Add(c => c.SortByCount, false)
            .Add(c => c.MaxCategories, 2)
            .Add(c => c.ShowCount, false));

        cut.FindAll("a.osirion-category-link").Select(a => a.TextContent.Trim()).ShouldBe(["Alpha", "Beta"]);
        cut.FindAll(".osirion-category-count").ShouldBeEmpty();
    }

    [Theory]
    [InlineData("gamma")]
    [InlineData("GAMMA")]
    [InlineData("Gamma")]
    public void CategoriesList_ActiveCategory_MarksTheMatchingLinkBySlugOrName(string active)
    {
        var cut = RenderStatic<CategoriesList>(p => p.Add(c => c.ActiveCategory, active));

        var activeLinks = cut.FindAll("a.osirion-category-link.osirion-active");
        activeLinks.ShouldHaveSingleItem().TextContent.ShouldContain("Gamma");
    }

    [Fact]
    public void CategoriesList_UrlFormatter_BuildsTheLinks()
    {
        var cut = RenderStatic<CategoriesList>(p => p.Add(c => c.CategoryUrlFormatter, c => $"/c/{c.Name}"));

        cut.Find("a.osirion-category-link").GetAttribute("href").ShouldBe("/c/Alpha");
    }

    [Fact]
    public void CategoriesList_WhileLoading_ShowsTheLoadingText()
    {
        var pending = new TaskCompletionSource<IReadOnlyList<ContentCategory>>();
        _provider.GetCategoriesAsync().Returns(pending.Task);

        var cut = RenderStatic<CategoriesList>(p => p.Add(c => c.LoadingText, "Loading list"));

        cut.Find(".osirion-loading").TextContent.ShouldBe("Loading list");

        pending.SetResult([]);

        cut.WaitForAssertion(() => cut.FindAll(".osirion-no-categories").ShouldHaveSingleItem());
    }

    [Fact]
    public void CategoriesList_ProviderThrows_LogsAndShowsTheEmptyText()
    {
        _provider.GetCategoriesAsync().Returns<Task<IReadOnlyList<ContentCategory>>>(_ => throw new InvalidOperationException("down"));

        var cut = RenderStatic<CategoriesList>(p => p.Add(c => c.NoContentText, "None"));

        _logs.Entries.ShouldContain(e => e.Level == LogLevel.Error && e.Exception is InvalidOperationException);
        cut.Find(".osirion-no-categories").TextContent.ShouldBe("None");
    }

    [Fact]
    public void CategoriesList_NoDefaultProvider_ShowsTheEmptyText()
    {
        _manager.GetDefaultProvider().Returns((IContentProvider?)null);

        var cut = RenderStatic<CategoriesList>(_ => { });

        cut.FindAll(".osirion-no-categories").ShouldHaveSingleItem();
    }

    [Fact]
    public void TagCloud_SortedByNameLimitedWithoutCounts_ShowsTheFirstNames()
    {
        var cut = RenderStatic<TagCloud>(p => p
            .Add(c => c.Title, "Tags")
            .Add(c => c.SortByCount, false)
            .Add(c => c.MaxTags, 2)
            .Add(c => c.ShowCount, false));

        cut.Find(".osirion-tag-cloud-title").TextContent.ShouldBe("Tags");
        cut.FindAll("a.osirion-tag-link").Select(a => a.TextContent.Trim()).ShouldBe(["alpha", "beta"]);
        cut.FindAll(".osirion-tag-count").ShouldBeEmpty();
    }

    [Fact]
    public void TagCloud_ByCount_ShowsCountsAndMarksTheActiveTag()
    {
        var cut = RenderStatic<TagCloud>(p => p.Add(c => c.ActiveTag, "Gamma"));

        cut.FindAll(".osirion-tag-count").Select(c => c.TextContent).ShouldBe(["5", "3", "1"]);
        cut.Find("a.osirion-tag-active").GetAttribute("href").ShouldBe("/tag/gamma");
    }

    [Fact]
    public void TagCloud_TagsSuppliedByTheParent_AreShownWithoutLoading()
    {
        var cut = RenderStatic<TagCloud>(p => p
            .Add(c => c.Tags, [ContentTag.Create("own", "own", 2)])
            .Add(c => c.TagUrlFormatter, t => $"/t/{t.Name}"));

        _provider.DidNotReceive().GetTagsAsync();
        cut.Find("a.osirion-tag-link").GetAttribute("href").ShouldBe("/t/own");
    }

    [Fact]
    public void TagCloud_NoDefaultProvider_ShowsTheEmptyText()
    {
        _manager.GetDefaultProvider().Returns((IContentProvider?)null);

        var cut = RenderStatic<TagCloud>(p => p.Add(c => c.NoContentText, "No tags"));

        cut.Find(".osirion-no-tags").TextContent.ShouldBe("No tags");
    }

    [Fact]
    public void TagCloud_InteractiveWhileLoading_ShowsTheLoadingText()
    {
        var pending = new TaskCompletionSource<IReadOnlyList<ContentTag>>();
        _provider.GetTagsAsync().Returns(pending.Task);
        SetRendererInfo(new RendererInfo("Server", true));

        var cut = Render<TagCloud>(p => p.Add(c => c.LoadingText, "Loading tags"));

        cut.Find(".osirion-loading").TextContent.ShouldBe("Loading tags");

        pending.SetResult([ContentTag.Create("x", "x", 1)]);

        cut.WaitForAssertion(() => cut.FindAll("a.osirion-tag-link").ShouldHaveSingleItem());
    }

    [Fact]
    public void SearchBox_Default_RendersAGetFormWithTheQueryInput()
    {
        var cut = RenderStatic<SearchBox>(_ => { });

        var form = cut.Find("form.osirion-search-box");
        form.GetAttribute("method").ShouldBe("get");
        form.GetAttribute("action").ShouldBe("/search");
        var input = cut.Find("input.osirion-search-input");
        input.GetAttribute("name").ShouldBe("q");
        input.GetAttribute("placeholder").ShouldBe("Search content...");
        cut.Find("label").GetAttribute("for").ShouldBe(input.GetAttribute("id"));
        cut.Find("button[type=submit] span").TextContent.ShouldBe("Search");
        cut.FindAll(".osirion-search-title").ShouldBeEmpty();
    }

    [Fact]
    public void SearchBox_Parameters_ShapeTheFormAndTitle()
    {
        var cut = RenderStatic<SearchBox>(p => p
            .Add(c => c.Title, "Find")
            .Add(c => c.ActionUrl, "/find")
            .Add(c => c.QueryParameterName, "term")
            .Add(c => c.SearchQuery, "blazor")
            .Add(c => c.Placeholder, "Type here")
            .Add(c => c.SearchButtonText, "Go")
            .Add(c => c.Class, "wide"));

        cut.Find("div.osirion-search-container").ClassList.ShouldContain("wide");
        cut.Find(".osirion-search-title").TextContent.ShouldBe("Find");
        cut.Find("form").GetAttribute("action").ShouldBe("/find");
        var input = cut.Find("input");
        input.GetAttribute("name").ShouldBe("term");
        input.GetAttribute("value").ShouldBe("blazor");
        input.GetAttribute("placeholder").ShouldBe("Type here");
        cut.Find("button span").TextContent.ShouldBe("Go");
    }

    [Fact]
    public void TableOfContents_Headings_BuildsANestedListWithLinksToTheCurrentPage()
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo("http://localhost/docs/page?x=1");
        const string html = "<h1>Title</h1><h2 id=\"intro\">Intro <em>text</em></h2><h3>Sub Part!</h3><h2>Next</h2><h3>???</h3>";

        var cut = RenderStatic<TableOfContents>(p => p
            .Add(c => c.Content, html)
            .Add(c => c.Class, "toc"));

        cut.Find("nav.osirion-table-of-contents").ClassList.ShouldContain("toc");
        var topLinks = cut.FindAll("ol.osirion-toc-list > li > a");
        topLinks.Select(a => a.GetAttribute("href")).ShouldBe(["/docs/page#intro", "/docs/page#next"]);
        topLinks[0].TextContent.ShouldBe("Intro text");
        var subLinks = cut.FindAll("ol.osirion-toc-sublist a");
        subLinks.Select(a => a.GetAttribute("href")).ShouldBe(["/docs/page#sub-part", "/docs/page#heading"]);
        cut.FindAll("li.osirion-toc-level-3").Count.ShouldBe(2);
    }

    [Fact]
    public void TableOfContents_Headings_LoadTheScriptThatMarksTheSectionBeingRead()
    {
        var cut = RenderStatic<TableOfContents>(p => p.Add(c => c.Content, "<h2 id=\"one\">One</h2>"));

        cut.Find("bl-script").GetAttribute("src").ShouldBe("./_content/Osirion.Blazor.Cms.Web/Components/Navigation/TableOfContents.razor.js");
        cut.Find("nav.osirion-table-of-contents").HasAttribute("data-ref").ShouldBeTrue();
    }

    [Fact]
    public void TableOfContents_TrackingOff_LoadsNoScript()
    {
        var cut = RenderStatic<TableOfContents>(p => p
            .Add(c => c.Content, "<h2 id=\"one\">One</h2>")
            .Add(c => c.TrackActiveHeading, false));

        cut.FindAll("bl-script").ShouldBeEmpty();
    }

    [Fact]
    public void TableOfContents_NoHeadings_LoadsNoScript()
    {
        var cut = RenderStatic<TableOfContents>(p => p.Add(c => c.Content, "<p>None</p>"));

        cut.FindAll("bl-script").ShouldBeEmpty();
    }

    [Fact]
    public void TableOfContents_LevelRange_KeepsOnlyHeadingsInsideIt()
    {
        var cut = RenderStatic<TableOfContents>(p => p
            .Add(c => c.Content, "<h1>One</h1><h2>Two</h2><h3>Three</h3>")
            .Add(c => c.MinLevel, 1)
            .Add(c => c.MaxLevel, 2));

        cut.FindAll("a.osirion-toc-link").Select(a => a.TextContent).ShouldBe(["One", "Two"]);
    }

    [Fact]
    public void TableOfContents_MaxDepthOne_DropsNestedHeadings()
    {
        var cut = RenderStatic<TableOfContents>(p => p
            .Add(c => c.Content, "<h2>Top</h2><h3>Nested</h3><h2>Second</h2>")
            .Add(c => c.MaxDepth, 1));

        cut.FindAll("a.osirion-toc-link").Select(a => a.TextContent).ShouldBe(["Top", "Second"]);
    }

    // Fails before the fix: a page without headings showed "No headings found." in its sidebar.
    [Fact]
    public void TableOfContents_NoHeadings_RendersNothing()
    {
        var cut = RenderStatic<TableOfContents>(p => p.Add(c => c.Content, "<p>No headings</p>"));

        cut.Markup.Trim().ShouldBeEmpty();
    }

    [Fact]
    public void TableOfContents_NoHeadingsWithEmptyText_ShowsTheEmptyText()
    {
        var cut = RenderStatic<TableOfContents>(p => p
            .Add(c => c.Content, "<p>No headings</p>")
            .Add(c => c.EmptyText, "Nothing to list"));

        cut.Find("nav.osirion-table-of-contents .osirion-toc-empty").TextContent.ShouldBe("Nothing to list");
    }

    [Fact]
    public void TableOfContents_ContentCleared_RendersNothing()
    {
        var cut = RenderStatic<TableOfContents>(p => p.Add(c => c.Content, "<h2>One</h2>"));

        cut.Render(p => p.Add(c => c.Content, null));

        cut.Markup.Trim().ShouldBeEmpty();
    }

    [Fact]
    public void TableOfContents_ContentClearedWithEmptyText_ShowsTheEmptyText()
    {
        var cut = RenderStatic<TableOfContents>(p => p
            .Add(c => c.Content, "<h2>One</h2>")
            .Add(c => c.EmptyText, "Nothing to list"));

        cut.Render(p => p.Add(c => c.Content, null));

        cut.FindAll(".osirion-toc-empty").ShouldHaveSingleItem();
    }

    [Fact]
    public void ContentBreadcrumbs_ContentInANestedDirectory_ListsEveryDirectoryFromTheRoot()
    {
        var root = DirectoryItem.Create("r", "docs", "Docs", "test");
        var child = DirectoryItem.Create("c", "docs/guides", "Guides", "test");
        root.AddChild(child);
        var item = ContentItem.Create("i", "Install", "<p>x</p>", "docs/guides/install.md", "test");
        child.AddItem(item);

        var cut = RenderStatic<ContentBreadcrumbs>(p => p
            .Add(c => c.Content, item)
            .Add(c => c.Class, "crumbs"));

        cut.Find("nav.osirion-breadcrumbs").ClassList.ShouldContain("crumbs");
        cut.FindAll("a.osirion-breadcrumbs-link").Select(a => a.GetAttribute("href")).ShouldBe(["/", "/docs", "/docs/guides"]);
        cut.Find(".osirion-breadcrumbs-current .osirion-breadcrumbs-text").TextContent.ShouldBe("Install");
    }

    // Fails before the fix: the directory links were built in code, which leaves out the scoped-style attribute the
    // markup carries, so the component's link colours never reached them and the dark theme showed the browser's blue.
    [Fact]
    public void ContentBreadcrumbs_DirectoryLinks_CarryTheSameScopedStyleAttributeAsTheHomeLink()
    {
        var directory = DirectoryItem.Create("d", "docs", "Docs", "test");

        var cut = RenderStatic<ContentBreadcrumbs>(p => p.Add(c => c.Directory, directory));

        var links = cut.FindAll("a.osirion-breadcrumbs-link");
        links.Count.ShouldBe(2);
        var scope = links[0].Attributes.Select(a => a.Name).Single(name => name.StartsWith("b-", StringComparison.Ordinal));
        links[1].HasAttribute(scope).ShouldBeTrue();
    }

    [Fact]
    public void ContentBreadcrumbs_HomeAndCurrentHidden_WithFormatter_ListsOnlyTheFormattedDirectory()
    {
        var directory = DirectoryItem.Create("d", "blog", "Blog", "test");

        var cut = RenderStatic<ContentBreadcrumbs>(p => p
            .Add(c => c.Directory, directory)
            .Add(c => c.Content, ContentItem.Create("i", "Post", "<p>x</p>", "blog/post.md", "test"))
            .Add(c => c.ShowHome, false)
            .Add(c => c.HideCurrentItem, true)
            .Add(c => c.DirectoryUrlFormatter, d => $"/section/{d.Name}"));

        var link = cut.FindAll("a.osirion-breadcrumbs-link").ShouldHaveSingleItem();
        link.GetAttribute("href").ShouldBe("/section/Blog");
        cut.FindAll(".osirion-breadcrumbs-current").ShouldBeEmpty();
    }

    // Fails before the fix: with localized content the locale folder was a crumb ("Home / en / Components") and every
    // folder linked to its path with the locale in it (/en/components), an address no page answers.
    [Fact]
    public void ContentBreadcrumbs_LocalizedContent_LeavesOutTheLocaleFolderAndLinksEachFolderAtItsAddress()
    {
        var locale = DirectoryItem.Create("l", "en", "en", "test");
        locale.SetLocale("en");
        var docs = DirectoryItem.Create("d", "en/docs", "Documentation", "test");
        docs.SetUrl("/docs");
        var packages = DirectoryItem.Create("p", "en/docs/packages", "Packages", "test");
        locale.AddChild(docs);
        docs.AddChild(packages);
        var item = ContentItem.Create("i", "Core", "<p>x</p>", "en/docs/packages/core.md", "test");
        packages.AddItem(item);

        var cut = RenderStatic<ContentBreadcrumbs>(p => p.Add(c => c.Content, item));

        var links = cut.FindAll("a.osirion-breadcrumbs-link");
        links.Select(a => a.TextContent).ShouldBe(["Home", "Documentation", "Packages"]);
        links.Select(a => a.GetAttribute("href")).ShouldBe(["/", "/docs", "/docs/packages"]);
    }

    // Fails before the fix: the GitHub provider keeps the repository's content folder in every directory path
    // (content/en/docs), so the locale folder was not recognized. The crumbs read "Home / en / Documentation" and the
    // "en" crumb linked to /content/en, as on getosirion.com.
    [Fact]
    public void ContentBreadcrumbs_GitHubPaths_LeaveOutTheLocaleFolderAndTheContentFolder()
    {
        var locale = DirectoryItem.Create("l", "content/en", "en", "test");
        locale.SetLocale("en");
        var docs = DirectoryItem.Create("d", "content/en/docs", "Documentation", "test");
        docs.SetUrl("/docs");
        var guides = DirectoryItem.Create("g", "content/en/docs/guides", "Guides", "test");
        locale.AddChild(docs);
        docs.AddChild(guides);
        var item = ContentItem.Create("i", "Install", "<p>x</p>", "content/en/docs/guides/install.md", "test");
        guides.AddItem(item);

        var cut = RenderStatic<ContentBreadcrumbs>(p => p.Add(c => c.Content, item));

        var links = cut.FindAll("a.osirion-breadcrumbs-link");
        links.Select(a => a.TextContent).ShouldBe(["Home", "Documentation", "Guides"]);
        links.Select(a => a.GetAttribute("href")).ShouldBe(["/", "/docs", "/docs/guides"]);
    }

    // Fails before the fix: without localization the folder links kept the content folder (/content/blog/2024).
    [Fact]
    public void ContentBreadcrumbs_GitHubPathsWithoutLocalization_LinkFoldersFromTheContentRoot()
    {
        var blog = DirectoryItem.Create("b", "content/blog", "Blog", "test");
        blog.SetLocale("en");
        var year = DirectoryItem.Create("y", "content/blog/2024", "2024", "test");
        blog.AddChild(year);

        var cut = RenderStatic<ContentBreadcrumbs>(p => p.Add(c => c.Directory, year));

        cut.FindAll("a.osirion-breadcrumbs-link").Select(a => a.GetAttribute("href")).ShouldBe(["/", "/blog", "/blog/2024"]);
    }

    // A tree a provider built inconsistently (a child whose path is not under its root) keeps the child's own path.
    [Fact]
    public void ContentBreadcrumbs_ChildPathOutsideItsRoot_LinksTheChildByItsOwnPath()
    {
        var root = DirectoryItem.Create("r", "content/en", "en", "test");
        root.SetLocale("en");
        var stray = DirectoryItem.Create("s", "elsewhere/notes", "Notes", "test");
        root.AddChild(stray);

        var cut = RenderStatic<ContentBreadcrumbs>(p => p.Add(c => c.Directory, stray));

        cut.FindAll("a.osirion-breadcrumbs-link").Select(a => a.GetAttribute("href")).ShouldBe(["/", "/elsewhere/notes"]);
    }

    [Fact]
    public void ContentBreadcrumbs_TopLevelFolderNamedLikeAnotherLocale_IsKept()
    {
        var folder = DirectoryItem.Create("f", "en", "English", "test");
        folder.SetLocale("de");

        var cut = RenderStatic<ContentBreadcrumbs>(p => p.Add(c => c.Directory, folder));

        cut.FindAll("a.osirion-breadcrumbs-link").Select(a => a.GetAttribute("href")).ShouldBe(["/", "/en"]);
    }

    [Fact]
    public void ContentBreadcrumbs_CustomHome_RendersTheHomeLink()
    {
        var cut = RenderStatic<ContentBreadcrumbs>(p => p
            .Add(c => c.HomeText, "Start")
            .Add(c => c.HomeUrl, "/start"));

        var home = cut.Find("a.osirion-breadcrumbs-link");
        home.GetAttribute("href").ShouldBe("/start");
        home.TextContent.ShouldBe("Start");
    }

    [Fact]
    public void DirectoryNavigation_Directories_ListsThemWithCountsAndTheActiveOne()
    {
        var blog = Directory("blog", "Blog", url: "blog", children: ["blog/2025", "blog/2026"]);
        var docs = Directory("docs", "Docs", url: "docs");

        var cut = RenderStatic<DirectoryNavigation>(p => p
            .Add(c => c.Title, "Sections")
            .Add(c => c.Directories, [blog, docs])
            .Add(c => c.CurrentDirectory, "docs"));

        cut.Find(".osirion-directory-title").TextContent.ShouldBe("Sections");
        var links = cut.FindAll("ul.osirion-directory-list > li > a");
        links.Select(a => a.GetAttribute("href")).ShouldBe(["/blog", "/docs"]);
        links[1].ClassList.ShouldContain("osirion-active");
        links[0].ClassList.ShouldNotContain("osirion-active");
        cut.FindAll("ul.osirion-directory-list > li > a > .osirion-directory-count").Select(c => c.TextContent).ShouldBe(["2", "0"]);
        cut.FindAll(".osirion-subdirectory-list").ShouldBeEmpty();
    }

    // Fails before the fix: an _index.md permalink starts with a slash, so the link came out as //docs, which a browser
    // reads as the host "docs"; a folder without one linked to the site root.
    [Fact]
    public void DirectoryNavigation_FolderAddresses_LinkFromTheSiteRoot()
    {
        var docs = Directory("docs", "Docs", url: "/docs");
        var guides = Directory("guides", "Guides");

        var cut = RenderStatic<DirectoryNavigation>(p => p.Add(c => c.Directories, [docs, guides]));

        cut.FindAll("ul.osirion-directory-list > li > a").Select(a => a.GetAttribute("href")).ShouldBe(["/docs", "/guides"]);
    }

    [Fact]
    public void DirectoryNavigation_ExpandedDirectory_ShowsItsSubdirectoriesWithTheFormatter()
    {
        var blog = Directory("blog", "Blog", children: ["blog/2025"]);

        var cut = RenderStatic<DirectoryNavigation>(p => p
            .Add(c => c.Directories, [blog])
            .Add(c => c.ExpandedDirectory, "blog")
            .Add(c => c.ShowItemCount, false)
            .Add(c => c.DirectoryUrlFormatter, d => $"/d/{d.Path}"));

        var sub = cut.Find(".osirion-subdirectory-list a");
        sub.GetAttribute("href").ShouldBe("/d/blog/2025");
        sub.TextContent.Trim().ShouldBe("blog/2025");
        cut.FindAll(".osirion-directory-count").ShouldBeEmpty();
    }

    [Fact]
    public void DirectoryNavigation_ExpandAllWithCounts_ShowsSubdirectoryCounts()
    {
        var cut = RenderStatic<DirectoryNavigation>(p => p
            .Add(c => c.Directories, [Directory("blog", "Blog", children: ["blog/2025"])])
            .Add(c => c.ExpandAllSubdirectories, true));

        cut.Find(".osirion-subdirectory-list .osirion-directory-count").TextContent.ShouldBe("0");
    }

    [Fact]
    public void DirectoryNavigation_SubdirectoriesHidden_RendersNoSublist()
    {
        var cut = RenderStatic<DirectoryNavigation>(p => p
            .Add(c => c.Directories, [Directory("blog", "Blog", children: ["blog/2025"])])
            .Add(c => c.ExpandAllSubdirectories, true)
            .Add(c => c.ShowSubdirectories, false));

        cut.FindAll(".osirion-subdirectory-list").ShouldBeEmpty();
    }

    [Fact]
    public void DirectoryNavigation_Click_RaisesDirectoryClickedForTopAndSubdirectories()
    {
        var blog = Directory("blog", "Blog", children: ["blog/2025"]);
        var clicked = new List<string>();

        var cut = RenderStatic<DirectoryNavigation>(p => p
            .Add(c => c.Directories, [blog])
            .Add(c => c.ExpandAllSubdirectories, true)
            .Add(c => c.DirectoryClicked, (DirectoryItem d) => clicked.Add(d.Path)));

        cut.Find("ul.osirion-directory-list > li > a").Click();
        cut.Find(".osirion-subdirectory-list a").Click();

        clicked.ShouldBe(["blog", "blog/2025"]);
    }

    [Fact]
    public void DirectoryNavigation_ClickWithoutHandler_DoesNotThrow()
    {
        var cut = RenderStatic<DirectoryNavigation>(p => p.Add(c => c.Directories, [Directory("blog", "Blog")]));

        Should.NotThrow(() => cut.Find("a.osirion-directory-link").Click());
    }

    [Theory]
    [InlineData(true, false, ".osirion-loading", "Loading navigation...")]
    [InlineData(false, false, ".osirion-no-directories", "No directories available.")]
    [InlineData(false, true, ".osirion-no-directories", "No directories available.")]
    public void DirectoryNavigation_LoadingOrEmpty_ShowsTheMatchingText(bool loading, bool emptyList, string selector, string text)
    {
        var cut = RenderStatic<DirectoryNavigation>(p => p
            .Add(c => c.IsLoading, loading)
            .Add(c => c.Directories, emptyList ? [] : null));

        cut.Find(selector).TextContent.ShouldBe(text);
        cut.Find("nav").ClassList.ShouldContain("osirion-directory-navigation");
    }

    private static DirectoryItem Directory(string path, string name, string? url = null, string[]? children = null)
    {
        var directory = DirectoryItem.Create($"id-{path}", path, name, "test");
        if (url is not null)
        {
            directory.SetUrl(url);
        }

        foreach (var childPath in children ?? [])
        {
            directory.AddChild(DirectoryItem.Create($"id-{childPath}", childPath, childPath, "test"));
        }

        return directory;
    }
}
