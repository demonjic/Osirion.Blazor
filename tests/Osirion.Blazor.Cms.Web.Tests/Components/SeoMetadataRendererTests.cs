using System.Text.Json.Nodes;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.ValueObjects;
using Osirion.Blazor.Cms.Web.Components;
using Osirion.Blazor.Cms.Web.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.Components;

public class SeoMetadataRendererTests : CmsWebComponentTestBase
{
    private const string Origin = "http://localhost";

    private NavigationManager Navigation => Services.GetRequiredService<NavigationManager>();

    // Fails before the fix: the canonical URL, og:url and the schema url all repeated NavigationManager.Uri
    // including "?utm_source=x#top".
    [Fact]
    public void SeoMetadataRenderer_UrlWithQueryAndFragment_CanonicalOgUrlAndSchemaUrlHaveNeither()
    {
        Navigation.NavigateTo($"{Origin}/blog/post?utm_source=x#top");

        var cut = RenderStatic<SeoMetadataRenderer>(p => p
            .Add(c => c.Content, CreateArticle("Post", "/blog/post"))
            .Add(c => c.SchemaTypes, [SchemaType.Article]));

        cut.Find("link[rel='canonical']").GetAttribute("href").ShouldBe($"{Origin}/blog/post");
        cut.Find("meta[property='og:url']").GetAttribute("content").ShouldBe($"{Origin}/blog/post");
        var schema = JsonNode.Parse(cut.Find("script[type='application/ld+json']").TextContent)!;
        schema["url"]!.GetValue<string>().ShouldBe($"{Origin}/blog/post");
        schema["mainEntityOfPage"]!["@id"]!.GetValue<string>().ShouldBe($"{Origin}/blog/post");
    }

    [Fact]
    public void SeoMetadataRenderer_FrontMatterCanonical_WinsOverTheCurrentUrl()
    {
        var item = CreateArticle("Post", "/blog/post");
        item.Metadata!.SeoProperties!.Canonical = "https://example.com/original";

        var cut = RenderStatic<SeoMetadataRenderer>(p => p.Add(c => c.Content, item));

        cut.Find("link[rel='canonical']").GetAttribute("href").ShouldBe("https://example.com/original");
        cut.Find("meta[property='og:url']").GetAttribute("content").ShouldBe("https://example.com/original");
    }

    // Fails before the fix: collection pages had no canonical link at all.
    [Fact]
    public void SeoMetadataRenderer_CollectionPage_HasCanonicalWithoutQuery()
    {
        Navigation.NavigateTo($"{Origin}/insights?page=2&utm_medium=mail");

        var cut = RenderStatic<SeoMetadataRenderer>(p => p
            .Add(c => c.ContentItems, [CreateArticle("Post", "/blog/post")])
            .Add(c => c.CollectionTitle, "Insights"));

        cut.Find("link[rel='canonical']").GetAttribute("href").ShouldBe($"{Origin}/insights");
    }

    [Fact]
    public void SeoMetadataRenderer_CanonicalQueryParameters_KeepsOnlyTheListedOnes()
    {
        Navigation.NavigateTo($"{Origin}/insights?page=2&utm_medium=mail");

        var cut = RenderStatic<SeoMetadataRenderer>(p => p
            .Add(c => c.ContentItems, [CreateArticle("Post", "/blog/post")])
            .Add(c => c.CollectionTitle, "Insights")
            .Add(c => c.CanonicalQueryParameters, ["page"]));

        cut.Find("link[rel='canonical']").GetAttribute("href").ShouldBe($"{Origin}/insights?page=2");
    }

    // Fails before the fix: everything was computed once in OnInitialized, so a reused instance kept page one's data.
    [Fact]
    public void SeoMetadataRenderer_ContentChanges_RecomputesCanonicalAndStructuredData()
    {
        Navigation.NavigateTo($"{Origin}/blog/first");
        var cut = RenderStatic<SeoMetadataRenderer>(p => p
            .Add(c => c.Content, CreateArticle("First", "/blog/first"))
            .Add(c => c.SchemaTypes, [SchemaType.Article]));

        Navigation.NavigateTo($"{Origin}/blog/second");
        cut.Render(p => p
            .Add(c => c.Content, CreateArticle("Second", "/blog/second"))
            .Add(c => c.SchemaTypes, [SchemaType.Article]));

        cut.Find("link[rel='canonical']").GetAttribute("href").ShouldBe($"{Origin}/blog/second");
        JsonNode.Parse(cut.Find("script[type='application/ld+json']").TextContent)!["headline"]!
            .GetValue<string>().ShouldBe("Second");
    }

    // Fails before the fix: the FAQ block serialized "context"/"type" and questions were invented from titles.
    [Fact]
    public void SeoMetadataRenderer_Defaults_EmitNoGeneratedFaqOrAnswerMetaTags()
    {
        var cut = RenderStatic<SeoMetadataRenderer>(p => p
            .Add(c => c.Content, CreateArticle("How does testing work?", "/blog/guide", "By running it.")));

        cut.FindAll("meta[name='answers-to']").ShouldBeEmpty();
        cut.Markup.ShouldNotContain("FAQPage");
        cut.Markup.ShouldNotContain("&quot;context&quot;");
    }

    [Fact]
    public void SeoMetadataRenderer_GeneratedFaqEnabled_EmitsValidFaqPageJsonLd()
    {
        var cut = RenderStatic<SeoMetadataRenderer>(p => p
            .Add(c => c.Content, CreateArticle("How does testing work?", "/blog/guide", "By running it."))
            .Add(c => c.EnableGeneratedFaq, true));

        var faq = cut.FindAll("script[type='application/ld+json']")
            .Select(script => JsonNode.Parse(script.TextContent)!)
            .Single(node => node["@type"]!.GetValue<string>() == "FAQPage");
        faq["@context"]!.GetValue<string>().ShouldBe("https://schema.org");
        var question = faq["mainEntity"]![0]!;
        question["@type"]!.GetValue<string>().ShouldBe("Question");
        question["acceptedAnswer"]!["@type"]!.GetValue<string>().ShouldBe("Answer");
        cut.FindAll("meta[name='answers-to']").ShouldHaveSingleItem();
    }

    [Fact]
    public void SeoMetadataRenderer_FaqSchemaTypeWithoutGeneratedFaq_EmitsNoEmptyFaqPage()
    {
        var cut = RenderStatic<SeoMetadataRenderer>(p => p
            .Add(c => c.Content, CreateArticle("Post", "/blog/post"))
            .Add(c => c.SchemaTypes, [SchemaType.FAQ]));

        cut.Markup.ShouldNotContain("FAQPage");
    }

    // Fails before the fix: endDate was invented as the last modified date or PublishDate + 2 hours.
    [Fact]
    public void SeoMetadataRenderer_EventSchema_DoesNotInventAnEndDate()
    {
        var cut = RenderStatic<SeoMetadataRenderer>(p => p
            .Add(c => c.Content, CreateArticle("Launch", "/events/launch"))
            .Add(c => c.SchemaTypes, [SchemaType.Event]));

        var schema = JsonNode.Parse(cut.Find("script[type='application/ld+json']").TextContent)!.AsObject();
        schema.ContainsKey("endDate").ShouldBeFalse();
        schema.ContainsKey("startDate").ShouldBeTrue();
    }

    // Fails before the fix: keywords was written for _metaKeywords and again for the tags.
    [Fact]
    public void SeoMetadataRenderer_ContentWithTags_EmitsKeywordsOnce()
    {
        var item = CreateArticle("Post", "/blog/post");
        item.AddTag("alpha");
        item.AddTag("beta");

        var cut = RenderStatic<SeoMetadataRenderer>(p => p.Add(c => c.Content, item));

        cut.FindAll("meta[name='keywords']").ShouldHaveSingleItem().GetAttribute("content").ShouldBe("alpha, beta");
    }

    // Fails before the fix: article:section was also written as a name meta.
    [Fact]
    public void SeoMetadataRenderer_ArticleWithCategory_EmitsSectionOnlyAsProperty()
    {
        var item = CreateArticle("Post", "/blog/post");
        item.AddCategory("News");

        var cut = RenderStatic<SeoMetadataRenderer>(p => p.Add(c => c.Content, item));

        cut.FindAll("meta[property='article:section']").ShouldHaveSingleItem();
        cut.FindAll("meta[name='article:section']").ShouldBeEmpty();
    }

    // Fails before the fix: the viewport and theme-color tags were always emitted, duplicating the host's.
    [Fact]
    public void SeoMetadataRenderer_Defaults_DoNotEmitViewportOrThemeColor()
    {
        var cut = RenderStatic<SeoMetadataRenderer>(p => p.Add(c => c.Content, CreateArticle("Post", "/blog/post")));

        cut.FindAll("meta[name='viewport']").ShouldBeEmpty();
        cut.FindAll("meta[name='theme-color']").ShouldBeEmpty();
    }

    [Fact]
    public void SeoMetadataRenderer_EmitMobileMetaTags_EmitsViewportAndThemeColor()
    {
        var cut = RenderStatic<SeoMetadataRenderer>(p => p
            .Add(c => c.Content, CreateArticle("Post", "/blog/post"))
            .Add(c => c.EmitMobileMetaTags, true));

        cut.FindAll("meta[name='viewport']").ShouldHaveSingleItem();
        cut.FindAll("meta[name='theme-color']").ShouldHaveSingleItem();
    }

    // Fails before the fix: Split('|')[1] threw IndexOutOfRangeException and the page failed to render.
    [Fact]
    public void SeoMetadataRenderer_MalformedAlternateLanguageEntry_IsSkipped()
    {
        var cut = RenderStatic<SeoMetadataRenderer>(p => p
            .Add(c => c.Content, CreateArticle("Post", "/blog/post"))
            .Add(c => c.AlternateLanguageUrls, ["sr", "|/x", "de|", "sr|https://example.com/sr?a=1|2"]));

        var link = cut.FindAll("link[rel='alternate']").ShouldHaveSingleItem();
        link.GetAttribute("hreflang").ShouldBe("sr");
        link.GetAttribute("href").ShouldBe("https://example.com/sr?a=1|2");
    }

    // Fails before the fix: only the lower-case "article" front matter type produced the article:* tags,
    // although og:type was derived case-insensitively.
    [Fact]
    public void SeoMetadataRenderer_OgTypeArticleInAnyCase_EmitsArticleTags()
    {
        var item = CreateArticle("Post", "/blog/post");
        item.Metadata!.SeoProperties!.OgType = "Article";

        var cut = RenderStatic<SeoMetadataRenderer>(p => p.Add(c => c.Content, item));

        cut.FindAll("meta[property='article:published_time']").ShouldHaveSingleItem();
    }

    [Fact]
    public void SeoMetadataRenderer_PublishedTime_IsFormattedAsUtc()
    {
        var item = CreateArticle("Post", "/blog/post");
        item.SetCreatedDate(new DateTime(2026, 3, 4, 5, 6, 7, DateTimeKind.Utc));

        var cut = RenderStatic<SeoMetadataRenderer>(p => p.Add(c => c.Content, item));

        cut.Find("meta[property='article:published_time']").GetAttribute("content").ShouldBe("2026-03-04T05:06:07Z");
    }

    [Fact]
    public void ContentUrls_FormatUtc_HandlesEveryDateTimeKind()
    {
        var local = new DateTime(2026, 3, 4, 5, 6, 7, DateTimeKind.Local);

        ContentUrls.FormatUtc(new DateTime(2026, 3, 4, 5, 6, 7, DateTimeKind.Unspecified)).ShouldBe("2026-03-04T05:06:07Z");
        ContentUrls.FormatUtc(new DateTime(2026, 3, 4, 5, 6, 7, DateTimeKind.Utc)).ShouldBe("2026-03-04T05:06:07Z");
        ContentUrls.FormatUtc(local).ShouldBe(local.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss'Z'"));
    }

    private static ContentItem CreateArticle(string title, string url, string description = "Summary")
    {
        var item = ContentItem.Create($"id-{title}", title, "<p>Body</p>", $"blog/{title}.md", "test");
        item.SetUrl(url);
        item.SetDescription(description);
        item.Metadata = new FrontMatter
        {
            Title = title,
            SeoProperties = new SeoMetadata { Type = "Article" }
        };

        return item;
    }
}
