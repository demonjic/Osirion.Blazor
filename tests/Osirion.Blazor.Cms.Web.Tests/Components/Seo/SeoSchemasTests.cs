using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Web.Components;
using Osirion.Blazor.Cms.Web.Components.Seo;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.Components.Seo;

public class SeoSchemasTests
{
    [Fact]
    public void Create_Faq_ReturnsNoSchema()
    {
        SeoSchemas.Create(SchemaType.FAQ, CreateContext()).ShouldBeNull();
    }

    [Fact]
    public void Breadcrumbs_WithoutContent_ListsOnlyHome()
    {
        var schema = SeoSchemas.Serialize(NavigationSchemas.Breadcrumbs(CreateContext()));

        schema.ShouldContain("\"name\": \"Start\"");
        schema.ShouldNotContain("\"position\": 2");
    }

    [Fact]
    public void CollectionPage_WithoutItems_HasAnEmptyItemList()
    {
        var schema = SeoSchemas.Serialize(NavigationSchemas.CollectionPage(CreateContext()));

        schema.ShouldContain("\"numberOfItems\": 0");
        schema.ShouldContain("\"itemListElement\": []");
        schema.ShouldContain("\"name\": \"Meta title\"");
        schema.ShouldContain("\"url\": \"https://site.test/logo.png\"");
    }

    [Fact]
    public void WebSite_ConfiguredLogo_IsMadeAbsolute()
    {
        var context = CreateContext() with { SiteLogoUrl = "/brand.svg" };

        var schema = SeoSchemas.Serialize(SeoSchemas.Create(SchemaType.WebSite, context)!);

        schema.ShouldContain("\"url\": \"https://site.test/brand.svg\"");
        schema.ShouldContain("\"urlTemplate\": \"https://site.test/search?q={search_term_string}\"");
    }

    [Fact]
    public void FaqPage_WritesOneQuestionPerAnswer()
    {
        var schema = SeoSchemas.Serialize(NavigationSchemas.FaqPage(new Dictionary<string, string>
        {
            ["How?"] = "Like this.",
            ["Why?"] = "Because.",
        }));

        schema.ShouldContain("\"name\": \"How?\"");
        schema.ShouldContain("\"text\": \"Because.\"");
    }

    // Fails before the fix: an item URL without a leading slash was appended to the host as it was, so the
    // item list held "https://site.testdocs/install".
    [Theory]
    [InlineData("docs/install")]
    [InlineData("/docs/install")]
    public void CollectionPage_ItemUrl_IsJoinedToTheHostWithOneSlash(string itemUrl)
    {
        var context = CreateContext() with { ContentItems = [CreateItem(itemUrl)] };

        var schema = SeoSchemas.Serialize(NavigationSchemas.CollectionPage(context));

        schema.ShouldContain("\"url\": \"https://site.test/docs/install\"");
    }

    // Fails before the fix: same missing slash in the article written for each collection item.
    [Theory]
    [InlineData("docs/install")]
    [InlineData("/docs/install")]
    public void CollectionItemPosting_ItemUrl_IsJoinedToTheHostWithOneSlash(string itemUrl)
    {
        var schema = SeoSchemas.Serialize(NavigationSchemas.CollectionItemPosting(CreateItem(itemUrl), CreateContext(), "Article"));

        schema.ShouldContain("\"url\": \"https://site.test/docs/install\"");
    }

    private static ContentItem CreateItem(string url) => new()
    {
        Title = "Install",
        Path = "docs/install.md",
        Url = url,
    };

    private static SeoSchemaContext CreateContext() => new()
    {
        SiteName = "Site",
        BaseUrl = "https://site.test",
        CurrentUrl = "https://site.test/page",
        BreadcrumbHomeText = "Start",
        MetaTitle = "Meta title",
    };
}
