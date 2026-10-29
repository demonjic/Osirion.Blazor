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

public class PageLevelTests : CmsWebComponentTestBase
{
    private readonly IContentProviderManager _manager;
    private readonly RecordingNavigationManager _navigation = new();
    private readonly RecordingLogger _articleLogger = new();

    public PageLevelTests()
    {
        _manager = Substitute.For<IContentProviderManager>();
        Services.AddSingleton(_manager);
        Services.AddSingleton<NavigationManager>(_navigation);
        Services.AddSingleton<ILoggerFactory>(new RecordingLoggerFactory(_articleLogger));
        Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new Osirion.Blazor.Cms.Web.Options.SeoMetadataOptions()));
    }

    // Fails before the fix: tag badges were images from img.shields.io and used string.GetHashCode for the colour.
    [Fact]
    public void ArticlePage_Tags_RenderAsLocalBadgesWithEncodedLinks()
    {
        var item = Article("Tagged");
        item.AddTag("C#");
        item.AddTag("Machine Learning");
        ReturnFromQuery(item);

        var cut = RenderStatic<ArticlePage>(p => p.Add(c => c.ItemSlug, "tagged"));

        cut.Markup.ShouldNotContain("shields.io");
        cut.FindAll("img.osirion-tag-shield").ShouldBeEmpty();
        var badges = cut.FindAll("a.osirion-sidebar-tag");
        badges.Select(b => b.GetAttribute("href")).ShouldBe(["/tag/c%23", "/tag/machine-learning"]);
        badges.Select(b => b.TextContent).ShouldBe(["C#", "Machine Learning"]);
        badges.ShouldAllBe(b => b.ClassList.Any(c => c.StartsWith("osirion-tag-badge-")));
    }

    // Fails before the fix: the last breadcrumb was the slug turned into words ("Pre Mvp Make It Right").
    [Fact]
    public void ArticlePage_Breadcrumbs_NameTheCurrentPageWithTheArticleTitle()
    {
        var item = ContentItem.Create("id-pre-mvp", "Pre-MVP: make it right", "<p>Body</p>", "blog/pre-mvp-make-it-right.md", "test");
        item.SetUrl("/blog/pre-mvp-make-it-right");
        ReturnFromQuery(item);

        var cut = RenderStatic<ArticlePage>(p => p.Add(c => c.ItemSlug, "pre-mvp-make-it-right"));

        cut.Find(".osirion-breadcrumbs [aria-current]").TextContent.Trim().ShouldBe("Pre-MVP: make it right");
    }

    [Fact]
    public void ArticlePage_SameTag_AlwaysGetsTheSameBadgeColour()
    {
        var item = Article("Tagged");
        item.AddTag("alpha");
        ReturnFromQuery(item);

        string BadgeClass() => RenderStatic<ArticlePage>(p => p.Add(c => c.ItemSlug, "tagged"))
            .Find("a.osirion-sidebar-tag").ClassList.Single(c => c.StartsWith("osirion-tag-badge-"));

        // FNV-1a("alpha") % 8, computed independently of the production helper.
        var hash = 2166136261u;
        foreach (var octet in "alpha"u8)
        {
            hash = (hash ^ octet) * 16777619u;
        }

        BadgeClass().ShouldBe($"osirion-tag-badge-{hash % 8}");
    }

    // Fails before the fix: missing content rendered with HTTP 200 because nothing told the framework it was a 404.
    [Fact]
    public void ArticlePage_ContentNotFound_SignalsNotFoundAndRendersTheNotFoundContent()
    {
        ReturnFromQuery();

        var cut = RenderStatic<ArticlePage>(p => p.Add(c => c.ItemSlug, "nothing"));

        _navigation.NotFoundCalls.ShouldBe(1);
        cut.FindAll(".osirion-content-not-found, .osirion-not-found, [class*='not-found']").ShouldNotBeEmpty();
    }

    [Fact]
    public void ArticlePage_ContentFound_DoesNotSignalNotFound()
    {
        ReturnFromQuery(Article("Found"));

        RenderStatic<ArticlePage>(p => p.Add(c => c.ItemSlug, "found"));

        _navigation.NotFoundCalls.ShouldBe(0);
    }

    // Fails before the fix: Item was only loaded while null, so a new ItemSlug kept showing the first article.
    [Fact]
    public void ArticlePage_ItemSlugChanges_LoadsTheNewArticle()
    {
        _manager.GetContentByQueryAsync(Arg.Is<ContentQuery>(q => q.Slug == "one"), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>>([Article("One")]));
        _manager.GetContentByQueryAsync(Arg.Is<ContentQuery>(q => q.Slug == "two"), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>>([Article("Two")]));

        var cut = RenderStatic<ArticlePage>(p => p.Add(c => c.ItemSlug, "one"));
        cut.Instance.Item!.Title.ShouldBe("One");

        cut.Render(p => p.Add(c => c.ItemSlug, "two"));

        cut.Instance.Item!.Title.ShouldBe("Two");
    }

    // Fails before the fix: the provider exception was swallowed by a bare catch, with nothing logged.
    [Fact]
    public void ArticlePage_ProviderThrows_LogsTheFailureAndShowsNotFound()
    {
        _manager.GetContentByQueryAsync(Arg.Any<ContentQuery>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<ContentItem>>>(_ => throw new InvalidOperationException("provider down"));

        var cut = RenderStatic<ArticlePage>(p => p.Add(c => c.ItemSlug, "x"));

        _articleLogger.Entries.ShouldContain(e => e.Level == LogLevel.Error && e.Exception is InvalidOperationException);
        cut.Instance.Item.ShouldBeNull();
        cut.Markup.ShouldNotContain("provider down");
    }

    [Fact]
    public void DocumentPage_ItemType_IsASchemaOrgTypeName()
    {
        var provider = Substitute.For<IContentProvider>();
        _manager.GetDefaultProvider().Returns(provider);
        provider.GetItemByPathAsync("docs/a.md").Returns(Article("Doc"));

        var cut = RenderStatic<Osirion.Blazor.Cms.Components.DocumentPage>(p => p
            .Add(c => c.ContentPath, "docs/a.md")
            .Add(c => c.SchemaTypes, [SchemaType.Article, SchemaType.BreadcrumbList]));

        var article = cut.Find("article.osirion-document-content");
        article.GetAttribute("itemtype").ShouldBe("https://schema.org/Article");
    }

    // Fails before the fix: an article alone in its folder showed the list's "404 Page Not Found" block, with an h1,
    // as its related items.
    [Fact]
    public void ArticlePage_NothingRelated_LeavesTheRelatedSectionOut()
    {
        ReturnFromQuery(Article("Alone"));

        var cut = RenderStatic<ArticlePage>(p => p
            .Add(c => c.ItemSlug, "alone")
            .Add(c => c.ShowRelatedContent, true));

        cut.FindAll(".osirion-content-not-found").ShouldBeEmpty();
        cut.Markup.ShouldNotContain("Related items");
        cut.FindAll("h1").Count.ShouldBe(1);
    }

    // Fails before the fix: the hero showed the title as an h1 and the body's "# Title" showed it again as a second h1.
    [Fact]
    public void ArticlePage_WithItsHero_ShowsTheTitleOnce()
    {
        ReturnFromQuery(TitledArticle());

        var cut = RenderStatic<ArticlePage>(p => p
            .Add(c => c.ItemSlug, "titled")
            .Add(c => c.ShowRelatedContent, false));

        cut.FindAll("h1").Select(h => h.TextContent.Trim()).ShouldBe(["Titled"]);
        cut.Find(".osirion-article-main").TextContent.ShouldContain("Body");
    }

    [Fact]
    public void ArticlePage_HeaderContent_ReplacesTheHeroAndKeepsOneH1()
    {
        ReturnFromQuery(TitledArticle());

        var cut = RenderStatic<ArticlePage>(p => p
            .Add(c => c.ItemSlug, "titled")
            .Add(c => c.ShowRelatedContent, false)
            .Add(c => c.HeaderContent, item => builder => builder.AddMarkupContent(0, $"<h1 class=\"custom-title\">{item.Title}</h1>")));

        cut.FindAll(".osirion-hero-section, .osirion-hero").ShouldBeEmpty();
        cut.Find(".osirion-article-header h1.custom-title").TextContent.ShouldBe("Titled");
        cut.FindAll("h1").Select(h => h.TextContent.Trim()).ShouldBe(["Titled"]);
        cut.Find(".osirion-article-main").TextContent.ShouldContain("Body");
    }

    [Fact]
    public void ArticlePage_WithoutItsHero_KeepsTheContentsTitle()
    {
        ReturnFromQuery(TitledArticle());

        var cut = RenderStatic<ArticlePage>(p => p
            .Add(c => c.ItemSlug, "titled")
            .Add(c => c.ShowHero, false)
            .Add(c => c.ShowRelatedContent, false));

        cut.FindAll("h1").Select(h => h.TextContent.Trim()).ShouldBe(["Titled"]);
        cut.Find(".osirion-article-main h1").ShouldNotBeNull();
    }

    [Fact]
    public void HomePage_WithItsHero_ShowsTheTitleOnce()
    {
        var cut = RenderStatic<Osirion.Blazor.Cms.Components.HomePage>(p => p
            .Add(c => c.Content, TitledArticle())
            .Add(c => c.ShowFeaturedContent, false));

        cut.FindAll("h1").Select(h => h.TextContent.Trim()).ShouldBe(["Titled"]);
        cut.Find(".osirion-homepage-content").TextContent.ShouldContain("Body");
    }

    private static ContentItem TitledArticle()
    {
        var item = ContentItem.Create("id-titled", "Titled", "<h1 id=\"titled\">Titled</h1><p>Body</p>", "blog/titled.md", "test");
        item.SetUrl("/blog/titled");
        return item;
    }

    [Fact]
    public void ArticlePage_SeriesPart_ShowsTheSeriesNavigationAboveTheBody()
    {
        var first = SeriesPart("set-up", "Set up", 1);
        var second = SeriesPart("deploy", "Deploy", 2);
        ReturnFromQuery(first, second);

        var cut = RenderStatic<ArticlePage>(p => p.Add(c => c.Item, first).Add(c => c.ShowRelatedContent, false));

        var main = cut.Find(".osirion-article-main");
        main.FirstElementChild!.ClassList.ShouldContain("osirion-series-nav");
        cut.Find(".osirion-series-nav-position").TextContent.ShouldBe("Part 1 of 2");
    }

    [Fact]
    public void ArticlePage_ShowSeriesNavigationOff_LeavesItOut()
    {
        var first = SeriesPart("set-up", "Set up", 1);
        ReturnFromQuery(first, SeriesPart("deploy", "Deploy", 2));

        var cut = RenderStatic<ArticlePage>(p => p
            .Add(c => c.Item, first)
            .Add(c => c.ShowRelatedContent, false)
            .Add(c => c.ShowSeriesNavigation, false));

        cut.FindAll(".osirion-series-nav").ShouldBeEmpty();
    }

    [Fact]
    public void ArticlePage_FooterContent_FollowsTheLayoutOutsideTheArticleColumn()
    {
        ReturnFromQuery(Article("Closing"));

        var cut = RenderStatic<ArticlePage>(p => p
            .Add(c => c.ItemSlug, "closing")
            .Add(c => c.ShowRelatedContent, false)
            .Add(c => c.FooterContent, article => $"<p class=\"cta\">Talk about {article.Title}</p>"));

        var footer = cut.Find(".osirion-article-page-content > footer.osirion-article-page-footer");
        footer.QuerySelector(".cta")!.TextContent.ShouldBe("Talk about Closing");
        cut.FindAll(".osirion-article-layout ~ footer.osirion-article-page-footer").ShouldHaveSingleItem();
        footer.NextElementSibling.ShouldBeNull();
        cut.FindAll(".osirion-article-layout .cta").ShouldBeEmpty();
    }

    [Fact]
    public void ArticlePage_AuthorContent_FollowsTheBodyWithTheArticle()
    {
        var item = Article("Signed");
        item.SetAuthor("Ana Author");
        ReturnFromQuery(item);

        var cut = RenderStatic<ArticlePage>(p => p
            .Add(c => c.ItemSlug, "signed")
            .Add(c => c.ShowRelatedContent, false)
            .Add(c => c.AuthorContent, article => $"<p class=\"byline\">{article.Author}</p>"));

        cut.Find(".osirion-article-main .osirion-article-author .byline").TextContent.ShouldBe("Ana Author");
    }

    // Fails before the fix: the related section was always titled "Related items" and the parameter was ignored.
    [Fact]
    public void ArticlePage_RelatedContentTitle_NamesTheRelatedSection()
    {
        ReturnFromQuery(Article("Main"));

        var cut = RenderStatic<ArticlePage>(p => p
            .Add(c => c.ItemSlug, "main")
            .Add(c => c.RelatedItems, [Article("Other")])
            .Add(c => c.RelatedContentTitle, "Keep reading"));

        cut.Markup.ShouldContain("Keep reading");
        cut.Markup.ShouldNotContain("Related items");
    }

    // Fails before the fix: CustomFooterContent was accepted and never rendered.
    [Fact]
    public void ArticlePage_CustomFooterContent_IsRenderedSanitized()
    {
        ReturnFromQuery(Article("Main"));

        var cut = RenderStatic<ArticlePage>(p => p
            .Add(c => c.ItemSlug, "main")
            .Add(c => c.ShowRelatedContent, false)
            .Add(c => c.CustomFooterContent, "<p class=\"cta\">Subscribe</p><script>alert(1)</script>"));

        cut.Find(".osirion-article-footer .cta").TextContent.ShouldBe("Subscribe");
        cut.FindAll(".osirion-article-footer script").ShouldBeEmpty();
    }

    private static ContentItem SeriesPart(string slug, string title, int order)
    {
        var item = Article(title);
        item.Url = $"blog/{slug}";
        item.Metadata = new Osirion.Blazor.Cms.Domain.ValueObjects.FrontMatter { Title = title, Series = "Build a blog", SeriesOrder = order };
        return item;
    }

    private void ReturnFromQuery(params ContentItem[] items) =>
        _manager.GetContentByQueryAsync(Arg.Any<ContentQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>>(items));

    private static ContentItem Article(string title)
    {
        var item = ContentItem.Create($"id-{title}", title, "<p>Body</p>", $"blog/{title}.md", "test");
        item.SetUrl($"/blog/{title}");
        return item;
    }

    private sealed class RecordingLoggerFactory(RecordingLogger logger) : ILoggerFactory
    {
        public void AddProvider(ILoggerProvider provider)
        {
        }

        public ILogger CreateLogger(string categoryName) => logger;

        public void Dispose()
        {
        }
    }

    private sealed class RecordingLogger : ILogger
    {
        public List<(LogLevel Level, Exception? Exception)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
            Entries.Add((logLevel, exception));
    }
}
