using System.Text.Json.Nodes;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Repositories;
using Osirion.Blazor.Cms.Domain.Services;
using Osirion.Blazor.Cms.Web.Components;
using Osirion.Blazor.Cms.Web.Tests.Base;
using Shouldly;
using DocumentPage = Osirion.Blazor.Cms.Components.DocumentPage;
using DocumentNavigationItem = Osirion.Blazor.Cms.Components.DocumentNavigationItem;

namespace Osirion.Blazor.Cms.Web.Tests.Components;

public class DocumentPageTests : CmsWebComponentTestBase
{
    private readonly IContentProviderManager _manager = Substitute.For<IContentProviderManager>();
    private readonly IContentProvider _provider = Substitute.For<IContentProvider>();
    private readonly ContentItem _install = Doc("install", "Install", 1);
    private readonly ContentItem _configure = Doc("configure", "Configure", 2);
    private readonly ContentItem _deploy = Doc("deploy", "Deploy", 3);

    public DocumentPageTests()
    {
        Services.AddSingleton(_manager);
        Services.AddSingleton<NavigationManager>(new RecordingNavigationManager());
        Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new Osirion.Blazor.Cms.Web.Options.SeoMetadataOptions()));
        _manager.GetDefaultProvider().Returns(_provider);
        _provider.GetItemByPathAsync(_configure.Path).Returns(_configure);
        _provider.GetItemsByQueryAsync(Arg.Any<ContentQuery>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>?>([_install, _configure, _deploy]));
    }

    // Fails before the fix: the page wrapped its body in a <main> inside the layout's <main>.
    [Fact]
    public void DocumentPage_Body_IsNotASecondMainLandmark()
    {
        var cut = RenderDocument();

        cut.FindAll("main").ShouldBeEmpty();
        cut.Find("div.osirion-document-main article").ShouldNotBeNull();
    }

    // Fails before the fix: the page had no title tag, meta description or structured data at all.
    [Fact]
    public void DocumentPage_Defaults_WriteTechArticleStructuredData()
    {
        var head = Render<HeadOutlet>();

        RenderDocument();

        head.FindAll("script[type='application/ld+json']")
            .Select(script => JsonNode.Parse(script.TextContent)?["@type"]?.GetValue<string>())
            .ShouldContain("TechArticle");
    }

    // Fails before the fix: "Last updated" showed the creation date.
    [Fact]
    public void DocumentPage_LastUpdated_IsTheLastModifiedDate()
    {
        _configure.LastModified = new DateTime(2026, 9, 30, 0, 0, 0, DateTimeKind.Utc);

        var cut = RenderDocument();

        cut.Find(".osirion-document-date").TextContent.ShouldContain("September 30, 2026");
    }

    [Fact]
    public void DocumentPage_EditUrlFormatter_LinksToTheSource()
    {
        var cut = RenderDocument(p => p.Add(c => c.EditUrlFormatter, item => $"https://github.com/o/r/edit/main/{item.Path}"));

        var link = cut.Find(".osirion-document-edit a");
        link.GetAttribute("href").ShouldBe("https://github.com/o/r/edit/main/docs/configure.md");
        link.TextContent.ShouldBe("Edit this page");
    }

    [Fact]
    public void DocumentPage_Defaults_NumberTheTableOfContents()
    {
        var cut = RenderDocument();

        cut.Find(".osirion-document-toc nav.osirion-table-of-contents").ClassList.ShouldContain("osirion-toc-numbered");
    }

    // Fails before the change: the page had no TocShowNumbers parameter to pass on to its table of contents.
    [Fact]
    public void DocumentPage_TocShowNumbersFalse_ReachesTheTableOfContents()
    {
        var cut = RenderDocument(p => p.Add(c => c.TocShowNumbers, false));

        cut.Find(".osirion-document-toc nav.osirion-table-of-contents").ClassList.ShouldNotContain("osirion-toc-numbered");
    }

    [Fact]
    public void DocumentPage_NoEditUrlFormatter_HasNoEditLink()
    {
        var cut = RenderDocument();

        cut.FindAll(".osirion-document-edit").ShouldBeEmpty();
    }

    [Fact]
    public void DocumentPage_NoNavigationItems_ListsTheFolderWithTheCurrentPageMarked()
    {
        var cut = RenderDocument();

        var links = cut.FindAll(".osirion-sidenav-link");
        links.Select(a => a.TextContent.Trim()).ShouldBe(["Install", "Configure", "Deploy"]);
        links.Select(a => a.GetAttribute("href")).ShouldBe(["/docs/install", "/docs/configure", "/docs/deploy"]);
        cut.Find(".osirion-sidenav-link[aria-current='page']").TextContent.Trim().ShouldBe("Configure");
    }

    [Fact]
    public void DocumentPage_NavigationItems_WinOverTheFolder()
    {
        var cut = RenderDocument(p => p.Add(c => c.NavigationItems, [new DocumentNavigationItem { Title = "Overview", Url = "/docs" }]));

        cut.FindAll(".osirion-sidenav-link").Select(a => a.TextContent.Trim()).ShouldBe(["Overview"]);
    }

    // Fails before the fix: previous and next linked to the source file path ("/docs/install.md").
    [Fact]
    public void DocumentPage_PreviousAndNext_LinkToThePagesUrls()
    {
        var cut = RenderDocument();

        cut.Find(".osirion-doc-nav-previous").GetAttribute("href").ShouldBe("/docs/install");
        cut.Find(".osirion-doc-nav-next").GetAttribute("href").ShouldBe("/docs/deploy");
    }

    // Fails before the fix: on a phone the whole side navigation came before the page, so a reader scrolled past every
    // link of the documentation to reach the content.
    [Fact]
    public void DocumentPage_SideNavigation_FoldsBehindAToggleThatControlsItsPanel()
    {
        var cut = RenderDocument();

        var toggle = cut.Find("nav.osirion-document-sidenav > input.osirion-sidenav-toggler");
        toggle.GetAttribute("type").ShouldBe("checkbox");
        var label = cut.Find("nav.osirion-document-sidenav > label.osirion-sidenav-toggle");
        label.GetAttribute("for").ShouldBe(toggle.Id);
        label.TextContent.ShouldBe("Documentation menu");
        var panel = cut.Find("nav.osirion-document-sidenav > .osirion-sidenav-panel");
        toggle.GetAttribute("aria-controls").ShouldBe(panel.Id);
        panel.QuerySelectorAll("a.osirion-sidenav-link").Length.ShouldBe(3);
    }

    [Fact]
    public void DocumentPage_SideNavigationToggleText_LabelsTheToggleAndIdsDifferPerPage()
    {
        var first = RenderDocument(p => p.Add(c => c.SideNavigationToggleText, "Pages"));
        var second = RenderDocument();

        first.Find("label.osirion-sidenav-toggle").TextContent.ShouldBe("Pages");
        first.Find("input.osirion-sidenav-toggler").Id.ShouldNotBe(second.Find("input.osirion-sidenav-toggler").Id);
    }

    [Fact]
    public void DocumentPage_WithoutSideNavigation_HasNoToggle()
    {
        var cut = RenderDocument(p => p.Add(c => c.ShowSideNavigation, false));

        cut.FindAll(".osirion-sidenav-toggler").ShouldBeEmpty();
    }

    private IRenderedComponent<DocumentPage> RenderDocument(
        Action<ComponentParameterCollectionBuilder<DocumentPage>>? configure = null) =>
        RenderStatic<DocumentPage>(p =>
        {
            p.Add(c => c.ContentPath, _configure.Path);
            configure?.Invoke(p);
        });

    private static ContentItem Doc(string slug, string title, int order)
    {
        var item = ContentItem.Create($"id-{slug}", title, $"<p>{title} body</p>", $"docs/{slug}.md", "test");
        item.SetUrl($"docs/{slug}");
        item.SetDescription($"{title} the library.");
        item.OrderIndex = order;
        return item;
    }
}
