using Osirion.Blazor.Cms.Web.Components.Seo;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.Components;

public class SiteSchemasTests
{
    private static readonly SeoSchemaContext Context = new()
    {
        SiteName = "Acme Docs",
        BaseUrl = "https://example.com",
        CurrentUrl = "https://example.com/",
        BreadcrumbHomeText = "Home",
        MetaTitle = "Acme Docs",
        LinkToSite = true,
    };

    [Fact]
    public void WebSite_WithoutDescriptionOrSearch_LeavesThemOut()
    {
        var website = SiteSchemas.WebSite(Context, description: " ", searchUrlTemplate: null);

        website["description"].ShouldBeNull();
        website.ContainsKey("potentialAction").ShouldBeFalse();
        website["@id"].ShouldBe("https://example.com/#website");
    }

    [Fact]
    public void WebSite_AbsoluteSearchTemplate_IsKeptAsItIs()
    {
        var website = SiteSchemas.WebSite(Context, "Guides", "https://search.example/?q={search_term_string}");

        var target = (Dictionary<string, object?>)((Dictionary<string, object?>)website["potentialAction"]!)["target"]!;
        target["urlTemplate"].ShouldBe("https://search.example/?q={search_term_string}");
    }

    [Fact]
    public void Link_SchemaThatIsNotAPropertyMap_IsReturnedAsItIs()
    {
        var schema = new object();

        SiteSchemas.Link(schema, Context).ShouldBeSameAs(schema);
    }

    [Fact]
    public void Link_SchemaWithoutPublisherOrType_GetsNoReferences()
    {
        var schema = new Dictionary<string, object?> { ["name"] = "Thing" };

        SiteSchemas.Link(schema, Context);

        schema.Keys.ShouldBe(["name"]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RobotsDirectives_Empty_IsNull(string? value)
    {
        RobotsDirectives.Normalize(value).ShouldBeNull();
    }
}
