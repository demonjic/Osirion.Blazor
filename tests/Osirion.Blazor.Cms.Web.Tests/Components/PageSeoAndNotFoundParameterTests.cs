using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Osirion.Blazor.Cms.Components;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Repositories;
using Osirion.Blazor.Cms.Domain.ValueObjects;
using Osirion.Blazor.Cms.Web.Components;
using Osirion.Blazor.Cms.Web.Options;
using Osirion.Blazor.Cms.Web.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.Components;

/// <summary>
/// The SEO parameters every page component declares reach <c>SeoMetadataRenderer</c>, and the not-found parameters
/// shape the not-found state.
/// </summary>
public class PageSeoAndNotFoundParameterTests : CmsPageComponentTestBase
{
    private void ReturnItem(ContentItem item) =>
        Manager.GetContentByQueryAsync(Arg.Any<ContentQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>>([item]));

    // Fails before the fix: no page passed TwitterSite, TwitterCreator, FacebookAppId or DefaultImageUrl on.
    [Fact]
    public void ArticlePage_SeoParameters_ReachTheMetaTags()
    {
        var head = Render<HeadOutlet>();
        ReturnItem(Doc("one"));

        RenderStatic<ArticlePage>(p => p
            .Add(c => c.ItemSlug, "one")
            .Add(c => c.TwitterSite, "@acme")
            .Add(c => c.TwitterCreator, "@ada")
            .Add(c => c.FacebookAppId, "1234")
            .Add(c => c.DefaultImageUrl, "https://example.test/cover.png"));

        head.WaitForAssertion(() =>
        {
            head.Markup.ShouldContain("name=\"twitter:site\" content=\"@acme\"");
            head.Markup.ShouldContain("name=\"twitter:creator\" content=\"@ada\"");
            head.Markup.ShouldContain("property=\"fb:app_id\" content=\"1234\"");
            head.Markup.ShouldContain("https://example.test/cover.png");
        });
    }

    // Fails before the fix: AllowAiTraining and the other switches were never passed on.
    [Fact]
    public void ArticlePage_AllowAiTrainingFalse_OptsOutOfTraining()
    {
        var head = Render<HeadOutlet>();
        ReturnItem(Doc("one"));

        RenderStatic<ArticlePage>(p => p
            .Add(c => c.ItemSlug, "one")
            .Add(c => c.AllowAiTraining, false));

        head.WaitForAssertion(() => head.Markup.ShouldContain("name=\"ai-training\" content=\"disallow\""));
    }

    // Fails before the fix: the page did not pass AllowSearchIndexing on.
    [Fact]
    public void ArticlePage_AllowSearchIndexingFalse_WritesNoindex()
    {
        var head = Render<HeadOutlet>();
        ReturnItem(Doc("one"));

        RenderStatic<ArticlePage>(p => p
            .Add(c => c.ItemSlug, "one")
            .Add(c => c.AllowSearchIndexing, false));

        head.WaitForAssertion(() => head.Markup.ShouldContain("noindex"));
    }

    // The page's own defaults must not beat the site-wide configuration: only a switch the page was given is passed on.
    [Fact]
    public void ArticlePage_SwitchesNotSet_LeaveTheConfiguredValuesInForce()
    {
        Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new SeoMetadataOptions { AllowAiTraining = false }));
        var head = Render<HeadOutlet>();
        ReturnItem(Doc("one"));

        RenderStatic<ArticlePage>(p => p.Add(c => c.ItemSlug, "one"));

        head.WaitForAssertion(() => head.Markup.ShouldContain("name=\"ai-training\" content=\"disallow\""));
    }

    // The same switch, set on the page, wins over the configuration in both directions.
    [Fact]
    public void ArticlePage_SwitchSetToTrue_WinsOverTheConfiguredFalse()
    {
        Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new SeoMetadataOptions { AllowAiTraining = false }));
        var head = Render<HeadOutlet>();
        ReturnItem(Doc("one"));

        RenderStatic<ArticlePage>(p => p
            .Add(c => c.ItemSlug, "one")
            .Add(c => c.AllowAiTraining, true));

        head.WaitForAssertion(() => head.Markup.ShouldContain("name=\"ai-training\" content=\"allow\""));
    }

    // Fails before the fix for an item without front matter (a WebPage). With front matter it guards that an article
    // stays an Article once SeoMetadata.Type no longer defaults to "Article".
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ArticlePage_ItemWithoutAType_IsPublishedAsAnArticle(bool withFrontMatter)
    {
        var head = Render<HeadOutlet>();
        var item = Doc("one");
        item.Metadata = withFrontMatter ? new FrontMatter() : null;
        ReturnItem(item);

        RenderStatic<ArticlePage>(p => p.Add(c => c.ItemSlug, "one"));

        head.WaitForAssertion(() =>
        {
            head.Markup.ShouldContain("property=\"og:type\" content=\"article\"");
            head.Markup.ShouldContain("\"@type\": \"Article\"");
        });
    }

    // A front matter type still decides on an article page.
    [Fact]
    public void ArticlePage_ItemWithAFrontMatterType_KeepsIt()
    {
        var head = Render<HeadOutlet>();
        var item = Doc("one");
        item.Metadata = new FrontMatter
        {
            SeoProperties = new SeoMetadata { Type = "BlogPosting" }
        };
        ReturnItem(item);

        RenderStatic<ArticlePage>(p => p.Add(c => c.ItemSlug, "one"));

        head.WaitForAssertion(() => head.Markup.ShouldContain("\"@type\": \"BlogPosting\""));
    }

    // Fails before the fix: the home page did not pass TwitterSite on.
    [Fact]
    public void HomePage_SeoParameters_ReachTheMetaTags()
    {
        var head = Render<HeadOutlet>();

        RenderStatic<HomePage>(p => p
            .Add(c => c.Content, Doc("home"))
            .Add(c => c.ShowFeaturedContent, false)
            .Add(c => c.ShowRecentContent, false)
            .Add(c => c.TwitterSite, "@acme")
            .Add(c => c.AllowAiTraining, false));

        head.WaitForAssertion(() =>
        {
            head.Markup.ShouldContain("name=\"twitter:site\" content=\"@acme\"");
            head.Markup.ShouldContain("name=\"ai-training\" content=\"disallow\"");
        });
    }

    // Fails before the fix: the document page did not pass TwitterSite on.
    [Fact]
    public void DocumentPage_SeoParameters_ReachTheMetaTags()
    {
        var head = Render<HeadOutlet>();

        RenderStatic<DocumentPage>(p => p
            .Add(c => c.Content, Doc("guide"))
            .Add(c => c.TwitterSite, "@acme")
            .Add(c => c.AllowAiTraining, false));

        head.WaitForAssertion(() =>
        {
            head.Markup.ShouldContain("name=\"twitter:site\" content=\"@acme\"");
            head.Markup.ShouldContain("name=\"ai-training\" content=\"disallow\"");
        });
    }

    // Fails before the fix: the about page did not pass TwitterSite on.
    [Fact]
    public void AboutPage_SeoParameters_ReachTheMetaTags()
    {
        var head = Render<HeadOutlet>();
        ReturnItem(Doc("about"));

        RenderStatic<AboutPage>(p => p
            .Add(c => c.ItemSlug, "about")
            .Add(c => c.TwitterSite, "@acme")
            .Add(c => c.AllowAiTraining, false));

        head.WaitForAssertion(() =>
        {
            head.Markup.ShouldContain("name=\"twitter:site\" content=\"@acme\"");
            head.Markup.ShouldContain("name=\"ai-training\" content=\"disallow\"");
        });
    }

    // Fails before the fix: the contact page did not pass TwitterSite on.
    [Fact]
    public void ContactPage_SeoParameters_ReachTheMetaTags()
    {
        var head = Render<HeadOutlet>();
        ReturnItem(Doc("contact"));

        RenderStatic<ContactPage>(p => p
            .Add(c => c.ItemSlug, "contact")
            .Add(c => c.TwitterSite, "@acme")
            .Add(c => c.AllowAiTraining, false));

        head.WaitForAssertion(() =>
        {
            head.Markup.ShouldContain("name=\"twitter:site\" content=\"@acme\"");
            head.Markup.ShouldContain("name=\"ai-training\" content=\"disallow\"");
        });
    }

    // Fails before the fix: ArticlePage and HomePage did not pass the organization name on, which the renderer uses as
    // the site name when WebsiteName is not set.
    [Fact]
    public void ArticlePage_OrganizationName_NamesTheSiteInTheTitle()
    {
        var head = Render<HeadOutlet>();
        ReturnItem(Doc("one"));

        RenderStatic<ArticlePage>(p => p
            .Add(c => c.ItemSlug, "one")
            .Add(c => c.OrganizationName, "Acme Labs"));

        head.WaitForAssertion(() => head.Markup.ShouldContain("<title>one | Acme Labs</title>"));
    }

    // Fails before the fix: the same for the home page.
    [Fact]
    public void HomePage_OrganizationName_NamesTheSiteInTheTitle()
    {
        var head = Render<HeadOutlet>();

        RenderStatic<HomePage>(p => p
            .Add(c => c.Content, Doc("home"))
            .Add(c => c.ShowFeaturedContent, false)
            .Add(c => c.ShowRecentContent, false)
            .Add(c => c.OrganizationName, "Acme Labs"));

        head.WaitForAssertion(() => head.Markup.ShouldContain("<title>home | Acme Labs</title>"));
    }

    // Fails before the fix: the not-found state ignored NotFoundTitle, NotFoundText and the back link parameters.
    [Fact]
    public void ArticlePage_NotFoundParameters_ShapeTheNotFoundState()
    {
        var cut = RenderStatic<ArticlePage>(p => p
            .Add(c => c.ItemSlug, "missing")
            .Add(c => c.NotFoundTitle, "Gone")
            .Add(c => c.NotFoundText, "Nothing lives here.")
            .Add(c => c.BackLinkUrl, "/blog")
            .Add(c => c.BackLinkText, "Back to the blog"));

        AssertNotFound(cut.Find(".osirion-content-not-found"), "Gone", "Nothing lives here.", "/blog", "Back to the blog");
    }

    // Fails before the fix: the same ignored parameters on the home page.
    [Fact]
    public void HomePage_NotFoundParameters_ShapeTheNotFoundState()
    {
        var cut = RenderStatic<HomePage>(p => p
            .Add(c => c.NotFoundTitle, "Gone")
            .Add(c => c.NotFoundText, "Nothing lives here.")
            .Add(c => c.BackLinkUrl, "/start")
            .Add(c => c.BackLinkText, "Start again"));

        AssertNotFound(cut.Find(".osirion-content-not-found"), "Gone", "Nothing lives here.", "/start", "Start again");
    }

    // Fails before the fix: the same ignored parameters on the document page.
    [Fact]
    public void DocumentPage_NotFoundParameters_ShapeTheNotFoundState()
    {
        var cut = RenderStatic<DocumentPage>(p => p
            .Add(c => c.ContentPath, "docs/missing.md")
            .Add(c => c.NotFoundTitle, "Gone")
            .Add(c => c.NotFoundText, "Nothing lives here.")
            .Add(c => c.BackLinkUrl, "/docs")
            .Add(c => c.BackLinkText, "All documents"));

        AssertNotFound(cut.Find(".osirion-content-not-found"), "Gone", "Nothing lives here.", "/docs", "All documents");
    }

    // Fails before the fix: the same ignored parameters on the about page.
    [Fact]
    public void AboutPage_NotFoundParameters_ShapeTheNotFoundState()
    {
        var cut = RenderStatic<AboutPage>(p => p
            .Add(c => c.ItemSlug, "missing")
            .Add(c => c.NotFoundTitle, "Gone")
            .Add(c => c.NotFoundText, "Nothing lives here.")
            .Add(c => c.BackLinkUrl, "/")
            .Add(c => c.BackLinkText, "Home"));

        AssertNotFound(cut.Find(".osirion-content-not-found"), "Gone", "Nothing lives here.", "/", "Home");
    }

    // Fails before the fix: ShowBackLink did nothing.
    [Fact]
    public void ArticlePage_ShowBackLinkFalse_RendersNoBackLink()
    {
        var cut = RenderStatic<ArticlePage>(p => p
            .Add(c => c.ItemSlug, "missing")
            .Add(c => c.ShowBackLink, false));

        cut.FindAll(".osirion-content-not-found-actions a").ShouldBeEmpty();
    }

    [Fact]
    public void ArticlePage_EmptyBackLinkUrl_RendersNoBackLink()
    {
        var cut = RenderStatic<ArticlePage>(p => p
            .Add(c => c.ItemSlug, "missing")
            .Add(c => c.BackLinkUrl, " "));

        cut.FindAll(".osirion-content-not-found-actions a").ShouldBeEmpty();
    }

    [Fact]
    public void ArticlePage_NotFoundWithDefaults_UsesTheDeclaredDefaults()
    {
        var cut = RenderStatic<ArticlePage>(p => p.Add(c => c.ItemSlug, "missing"));

        AssertNotFound(cut.Find(".osirion-content-not-found"), "Page Not Found", "The page you're looking for doesn't exist or has been moved.", "/", "Go to Homepage");
        Navigation.NotFoundCalls.ShouldBe(1);
    }

    private static void AssertNotFound(AngleSharp.Dom.IElement notFound, string title, string text, string url, string linkText)
    {
        notFound.QuerySelector(".osirion-content-not-found-title")!.TextContent.ShouldBe(title);
        notFound.QuerySelector(".osirion-content-not-found-message")!.TextContent.ShouldBe(text);
        var link = notFound.QuerySelector(".osirion-content-not-found-actions a")!;
        link.GetAttribute("href").ShouldBe(url);
        link.TextContent.Trim().ShouldBe(linkText);
    }
}
