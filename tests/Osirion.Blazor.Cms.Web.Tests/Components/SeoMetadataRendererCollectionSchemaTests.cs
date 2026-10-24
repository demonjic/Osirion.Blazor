using System.Text.Json.Nodes;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.ValueObjects;
using Osirion.Blazor.Cms.Web.Components;
using Osirion.Blazor.Cms.Web.Options;
using Osirion.Blazor.Cms.Web.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.Components;

/// <summary>
/// Pins the structured data of a list page: the type each listed item gets, the breadcrumb list the page asks for,
/// no property without a value, and addresses on the configured site address.
/// </summary>
public class SeoMetadataRendererCollectionSchemaTests : CmsWebComponentTestBase
{
    // Fails before the fix: every listed item was a BlogPosting, a case study or a documentation page included.
    [Fact]
    public void SeoMetadataRenderer_ListedItemWithoutType_IsAnArticle()
    {
        var schemas = RenderSchemas(p => p.Add(c => c.ContentItems, [SeoTestContent.Article()]));

        schemas.ShouldHaveSingleItem()["@type"]!.GetValue<string>().ShouldBe("Article");
    }

    [Theory]
    [InlineData("BlogPosting", "BlogPosting")]
    [InlineData("techarticle", "TechArticle")]
    [InlineData("Product", "Article")]
    public void SeoMetadataRenderer_ListedItemWithAFrontMatterType_KeepsAnArticleType(string declaredType, string expectedType)
    {
        var item = SeoTestContent.Article();
        item.Metadata = new FrontMatter { SeoProperties = new SeoMetadata { Type = declaredType } };

        var schemas = RenderSchemas(p => p.Add(c => c.ContentItems, [item]));

        schemas.ShouldHaveSingleItem()["@type"]!.GetValue<string>().ShouldBe(expectedType);
    }

    [Fact]
    public void SeoMetadataRenderer_PageNamingBlogPosting_MakesTheListedItemsBlogPostings()
    {
        var schemas = RenderSchemas(p => p
            .Add(c => c.ContentItems, [SeoTestContent.Article(), SeoTestContent.BarePost()])
            .Add(c => c.SchemaTypes, [SchemaType.CollectionPage, SchemaType.BlogPosting]));

        schemas.Select(TypeOf).ShouldBe(["CollectionPage", "BlogPosting", "BlogPosting"]);
    }

    [Fact]
    public void SeoMetadataRenderer_ItemTypeWinsOverThePageType()
    {
        var item = SeoTestContent.Article();
        item.Metadata = new FrontMatter { SeoProperties = new SeoMetadata { Type = "TechArticle" } };

        var schemas = RenderSchemas(p => p
            .Add(c => c.ContentItems, [item])
            .Add(c => c.SchemaTypes, [SchemaType.BlogPosting]));

        schemas.Select(TypeOf).ShouldBe(["TechArticle"]);
    }

    // Fails before the fix: a BreadcrumbList in SchemaTypes was ignored on a list page.
    [Fact]
    public void SeoMetadataRenderer_CollectionWithBreadcrumbList_EmitsItAfterTheCollectionPage()
    {
        var schemas = RenderSchemas(p => p
            .Add(c => c.ContentItems, [SeoTestContent.Article()])
            .Add(c => c.CollectionTitle, "All posts")
            .Add(c => c.SchemaTypes, [SchemaType.CollectionPage, SchemaType.BreadcrumbList]));

        schemas.Select(TypeOf).ShouldBe(["CollectionPage", "BreadcrumbList", "Article"]);
        var crumbs = schemas[1]["itemListElement"]!.AsArray();
        crumbs.Select(crumb => crumb!["name"]!.GetValue<string>()).ShouldBe(["Home", "All posts"]);
        crumbs.Select(crumb => crumb!["item"]!.GetValue<string>()).ShouldBe([SeoTestContent.Origin, SeoTestContent.PageUrl]);
        crumbs.ToJsonString().ShouldBe(schemas[0]["breadcrumb"]!["itemListElement"]!.ToJsonString());
    }

    [Fact]
    public void SeoMetadataRenderer_CollectionWithBreadcrumbListAlone_EmitsItFirst()
    {
        var schemas = RenderSchemas(p => p
            .Add(c => c.ContentItems, [SeoTestContent.Article()])
            .Add(c => c.SchemaTypes, [SchemaType.BreadcrumbList]));

        schemas.Select(TypeOf).ShouldBe(["BreadcrumbList", "Article"]);
    }

    [Fact]
    public void SeoMetadataRenderer_CollectionWithCustomBreadcrumbs_UsesThem()
    {
        var schemas = RenderSchemas(p => p
            .Add(c => c.ContentItems, [SeoTestContent.Article()])
            .Add(c => c.CollectionTitle, "Guides")
            .Add(c => c.SchemaTypes, [SchemaType.BreadcrumbList])
            .Add(c => c.CustomBreadcrumbs, [("Blog", "http://localhost/blog"), ("Guides", "http://localhost/blog/guides")]));

        schemas[0]["itemListElement"]!.AsArray()
            .Select(crumb => crumb!["name"]!.GetValue<string>())
            .ShouldBe(["Home", "Blog", "Guides"]);
    }

    // Fails before the fix: an item without tags, image or author wrote "keywords": null, "image": null and an
    // author named "".
    [Fact]
    public void SeoMetadataRenderer_ListedItemWithoutOptionalValues_LeavesThemOut()
    {
        var item = SeoTestContent.Article(withTaxonomy: false);
        item.FeaturedImageUrl = null;
        item.Author = string.Empty;
        item.Description = string.Empty;

        var schemas = RenderSchemas(p => p
            .Add(c => c.ContentItems, [item])
            .Add(c => c.SchemaTypes, [SchemaType.CollectionPage]));

        var posting = schemas[1].AsObject();
        foreach (var name in new[] { "keywords", "image", "author", "description", "articleSection" })
        {
            posting.ContainsKey(name).ShouldBeFalse(name);
        }

        var listItem = schemas[0]["mainEntity"]!["itemListElement"]![0]!.AsObject();
        listItem.ContainsKey("image").ShouldBeFalse();
        listItem.ContainsKey("description").ShouldBeFalse();
        schemas.ShouldAllBe(schema => !schema.ToJsonString().Contains("null", StringComparison.Ordinal));
    }

    [Fact]
    public void SeoMetadataRenderer_ListedItemWithoutImage_UsesThePageImage()
    {
        var schemas = RenderSchemas(p => p.Add(c => c.ContentItems, [SeoTestContent.Article(), SeoTestContent.BarePost()]));

        schemas[1]["image"]!.GetValue<string>().ShouldBe("http://localhost/images/cover.png");
    }

    // Guard: a static export renders from a local host, so every address of a list page must use the configured site
    // address, never the host that served the request.
    [Fact]
    public void SeoMetadataRenderer_ConfiguredSiteUrl_EveryCollectionAddressUsesIt()
    {
        Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new SeoMetadataOptions { SiteUrl = "https://example.com" }));

        var cut = RenderPage(p => p
            .Add(c => c.ContentItems, [SeoTestContent.Article()])
            .Add(c => c.CollectionTitle, "All posts")
            .Add(c => c.SchemaTypes, [SchemaType.CollectionPage, SchemaType.BreadcrumbList]));

        var schemas = Schemas(cut);
        schemas[0]["url"]!.GetValue<string>().ShouldBe("https://example.com/blog/guides/my-post");
        schemas[0]["url"]!.GetValue<string>().ShouldBe(cut.Find("link[rel='canonical']").GetAttribute("href"));
        cut.Markup.ShouldNotContain("localhost");
    }

    private static string TypeOf(JsonNode schema) => schema["@type"]!.GetValue<string>();

    private List<JsonNode> RenderSchemas(Action<ComponentParameterCollectionBuilder<SeoMetadataRenderer>> parameters) =>
        Schemas(RenderPage(parameters));

    private static List<JsonNode> Schemas(IRenderedComponent<SeoMetadataRenderer> cut) =>
        cut.FindAll("script[type='application/ld+json']")
            .Select(script => JsonNode.Parse(script.TextContent)!)
            .ToList();

    private IRenderedComponent<SeoMetadataRenderer> RenderPage(Action<ComponentParameterCollectionBuilder<SeoMetadataRenderer>> parameters)
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo(SeoTestContent.PageUrl);
        return RenderStatic(parameters);
    }
}
