using System.Text.Json.Nodes;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.ValueObjects;
using Osirion.Blazor.Cms.Web.Components;
using Osirion.Blazor.Cms.Web.Options;
using Osirion.Blazor.Cms.Web.Tests.Base;
using Osirion.Blazor.Core.Extensions;
using Osirion.Blazor.Core.Localization;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.Components;

/// <summary>
/// The robots directives a page passes to <see cref="SeoMetadataRenderer"/>, and the site's own structured data
/// (<see cref="SeoMetadataOptions.EnableSiteSchemas"/>).
/// </summary>
public class SeoMetadataRendererSiteSchemaTests : CmsWebComponentTestBase
{
    private const string Site = "https://example.com";
    private const string DefaultRobots = "index, follow, max-snippet:-1, max-image-preview:large, max-video-preview:-1";

    private NavigationManager Navigation => Services.GetRequiredService<NavigationManager>();

    [Theory]
    [InlineData("noindex, follow", "noindex, follow")]
    [InlineData("NoIndex,NoFollow", "noindex, nofollow")]
    [InlineData("max-snippet:50, max-image-preview:Standard", "max-snippet:50, max-image-preview:standard")]
    [InlineData("max-video-preview:-1, noarchive", "max-video-preview:-1, noarchive")]
    [InlineData("unavailable_after: 2026-12-31", "unavailable_after:2026-12-31")]
    public void SeoMetadataRenderer_CodePageRobots_WritesTheDirectives(string robots, string expected)
    {
        var cut = RenderStatic<SeoMetadataRenderer>(p => p.Add(c => c.Title, "Thanks").Add(c => c.Robots, robots));

        Meta(cut, "robots").ShouldBe(expected);
        Meta(cut, "googlebot").ShouldBe(expected);
        Meta(cut, "bingbot").ShouldBe(expected);
    }

    [Theory]
    [InlineData("noindex, sometimes")]
    [InlineData("max-snippet:many")]
    [InlineData("max-snippet:-2")]
    [InlineData("max-image-preview:huge")]
    [InlineData("noindex:yes")]
    [InlineData("unavailable_after:")]
    [InlineData("noindex,,follow")]
    public void SeoMetadataRenderer_InvalidRobots_KeepsTheDefault(string robots)
    {
        var cut = RenderStatic<SeoMetadataRenderer>(p => p.Add(c => c.Title, "Thanks").Add(c => c.Robots, robots));

        Meta(cut, "robots").ShouldBe(DefaultRobots);
    }

    [Fact]
    public void SeoMetadataRenderer_RobotsWithIndexingOff_WritesNoIndexNoFollow()
    {
        var cut = RenderStatic<SeoMetadataRenderer>(p => p
            .Add(c => c.Title, "Thanks")
            .Add(c => c.Robots, "noindex, follow")
            .Add(c => c.AllowSearchIndexing, false));

        Meta(cut, "robots").ShouldBe("noindex, nofollow");
    }

    [Fact]
    public void SeoMetadataRenderer_ContentItemRobots_WinOverTheParameter()
    {
        var item = Article("Post");
        item.Metadata!.SeoProperties!.Robots = "noindex, nofollow";

        var cut = RenderStatic<SeoMetadataRenderer>(p => p.Add(c => c.Content, item).Add(c => c.Robots, "noindex, follow"));

        Meta(cut, "robots").ShouldBe("noindex, nofollow");
    }

    [Fact]
    public void SeoMetadataRenderer_ContentItemWithoutRobots_TakesTheParameter()
    {
        var cut = RenderStatic<SeoMetadataRenderer>(p => p.Add(c => c.Content, Article("Post")).Add(c => c.Robots, "noindex, follow"));

        Meta(cut, "robots").ShouldBe("noindex, follow");
    }

    // Guard test: without Robots a page writes the same directive as before.
    [Fact]
    public void SeoMetadataRenderer_NoRobots_WritesTheDefault()
    {
        var cut = RenderStatic<SeoMetadataRenderer>(p => p.Add(c => c.Title, "Thanks"));

        Meta(cut, "robots").ShouldBe(DefaultRobots);
    }

    [Fact]
    public void SeoMetadataRenderer_SiteSchemasOnTheHomePage_WritesTheWebSiteAndTheOrganization()
    {
        UseSiteSchemas(options =>
        {
            options.SameAs = ["https://social.example/acme", "not a link", "https://social.example/acme"];
            options.SearchUrlTemplate = "/search?q={search_term_string}";
        });
        Navigation.NavigateTo("http://localhost/");

        var cut = RenderStatic<SeoMetadataRenderer>(p => p
            .Add(c => c.Title, "Home")
            .Add(c => c.SchemaTypes, [SchemaType.WebPage, SchemaType.WebSite, SchemaType.Organization]));

        var schemas = Schemas(cut);
        schemas.Select(Type).ShouldBe(["WebPage", "WebSite", "Organization"]);

        var page = schemas[0];
        page["publisher"]!["@id"]!.GetValue<string>().ShouldBe($"{Site}/#organization");
        page["isPartOf"]!["@id"]!.GetValue<string>().ShouldBe($"{Site}/#website");
        page["publisher"]!["name"].ShouldBeNull();

        var website = schemas[1];
        website["@id"]!.GetValue<string>().ShouldBe($"{Site}/#website");
        website["url"]!.GetValue<string>().ShouldBe($"{Site}/");
        website["name"]!.GetValue<string>().ShouldBe("Acme Docs");
        website["description"]!.GetValue<string>().ShouldBe("Guides for Acme.");
        website["inLanguage"]!.GetValue<string>().ShouldBe("en");
        website["publisher"]!["@id"]!.GetValue<string>().ShouldBe($"{Site}/#organization");
        website["potentialAction"]!["target"]!["urlTemplate"]!.GetValue<string>()
            .ShouldBe($"{Site}/search?q={{search_term_string}}");

        var organization = schemas[2];
        organization["@id"]!.GetValue<string>().ShouldBe($"{Site}/#organization");
        organization["name"]!.GetValue<string>().ShouldBe("Acme Inc.");
        organization["url"]!.GetValue<string>().ShouldBe($"{Site}/");
        organization["logo"]!["url"]!.GetValue<string>().ShouldBe($"{Site}/images/logo.png");
        organization["sameAs"]!.AsArray().Select(node => node!.GetValue<string>()).ShouldBe(["https://social.example/acme"]);
    }

    [Fact]
    public void SeoMetadataRenderer_SiteSchemasOnAContentHomePage_WritesEachSiteNodeOnce()
    {
        UseSiteSchemas();
        Navigation.NavigateTo("http://localhost/?utm_source=mail");

        var cut = RenderStatic<SeoMetadataRenderer>(p => p
            .Add(c => c.Content, Article("Home"))
            .Add(c => c.SchemaTypes, [SchemaType.WebPage, SchemaType.WebSite, SchemaType.Organization, SchemaType.BreadcrumbList]));

        var schemas = Schemas(cut);
        schemas.Select(Type).ShouldBe(["WebPage", "BreadcrumbList", "WebSite", "Organization"]);
        schemas.Where(schema => Type(schema) == "WebSite").ShouldHaveSingleItem()["@id"]!.GetValue<string>().ShouldBe($"{Site}/#website");
        schemas[0]["isPartOf"]!["@id"]!.GetValue<string>().ShouldBe($"{Site}/#website");
    }

    [Fact]
    public void SeoMetadataRenderer_SiteSchemasOffTheHomePage_KeepsThePageTypes()
    {
        UseSiteSchemas();
        Navigation.NavigateTo("http://localhost/about");

        var cut = RenderStatic<SeoMetadataRenderer>(p => p
            .Add(c => c.Content, Article("About"))
            .Add(c => c.SchemaTypes, [SchemaType.AboutPage, SchemaType.Organization]));

        Schemas(cut).Select(Type).ShouldBe(["AboutPage", "Organization"]);
    }

    [Fact]
    public void SeoMetadataRenderer_SiteSchemasWithoutSearchOrProfiles_LeavesThemOut()
    {
        UseSiteSchemas(options => options.SearchUrlTemplate = "/search?q=");
        Navigation.NavigateTo("http://localhost/");

        var cut = RenderStatic<SeoMetadataRenderer>(p => p.Add(c => c.Title, "Home"));

        var schemas = Schemas(cut);
        schemas.Single(schema => Type(schema) == "WebSite")["potentialAction"].ShouldBeNull();
        schemas.Single(schema => Type(schema) == "Organization")["sameAs"].ShouldBeNull();
    }

    [Fact]
    public void SeoMetadataRenderer_SiteSchemasOnAnArticle_ReferencesTheSiteWithoutRepeatingIt()
    {
        UseSiteSchemas();
        Navigation.NavigateTo("http://localhost/blog/post");

        var cut = RenderStatic<SeoMetadataRenderer>(p => p
            .Add(c => c.Content, Article("Post"))
            .Add(c => c.SchemaTypes, [SchemaType.Article, SchemaType.AboutPage]));

        var schemas = Schemas(cut);
        schemas.Select(Type).ShouldBe(["Article", "AboutPage"]);
        schemas[0]["publisher"]!.AsObject().Select(pair => pair.Key).ShouldBe(["@id"]);
        schemas[0]["publisher"]!["@id"]!.GetValue<string>().ShouldBe($"{Site}/#organization");
        schemas[1]["isPartOf"]!["@id"]!.GetValue<string>().ShouldBe($"{Site}/#website");
    }

    [Fact]
    public void SeoMetadataRenderer_SiteSchemasOnAListPage_ReferencesThePublisherOfEveryItem()
    {
        UseSiteSchemas();
        Navigation.NavigateTo("http://localhost/blog");

        var cut = RenderStatic<SeoMetadataRenderer>(p => p
            .Add(c => c.ContentItems, [Article("Post")])
            .Add(c => c.CollectionTitle, "Blog")
            .Add(c => c.SchemaTypes, [SchemaType.CollectionPage]));

        var schemas = Schemas(cut);
        schemas[0]["isPartOf"]!["@id"]!.GetValue<string>().ShouldBe($"{Site}/#website");
        schemas.ShouldAllBe(schema => schema["publisher"] == null || schema["publisher"]!["@id"] != null);
        schemas.ShouldNotContain(schema => Type(schema) == "WebSite");
    }

    [Fact]
    public void SeoMetadataRenderer_SiteSchemasOnACultureRoot_WritesTheSiteSchemas()
    {
        UseSiteSchemas();
        Services.AddOsirionLocalization(localization =>
        {
            localization.DefaultCulture = "en";
            localization.Cultures.Add(new LocalizationCulture { Name = "sr-Latn", Prefix = "sr" });
        });
        Navigation.NavigateTo("http://localhost/sr");

        var cut = RenderStatic<SeoMetadataRenderer>(p => p.Add(c => c.Title, "Početna"));

        var website = Schemas(cut).Single(schema => Type(schema) == "WebSite");
        website["inLanguage"]!.GetValue<string>().ShouldBe("sr-Latn");
    }

    [Fact]
    public void SeoMetadataRenderer_SiteSchemasWithCustomJsonLd_StillWritesTheSiteSchemasOnTheHomePage()
    {
        UseSiteSchemas();
        Navigation.NavigateTo("http://localhost/");
        var item = Article("Home");
        item.Metadata!.SeoProperties!.JsonLd = """{"@context":"https://schema.org","@type":"WebPage","name":"Home"}""";

        var cut = RenderStatic<SeoMetadataRenderer>(p => p.Add(c => c.Content, item));

        Schemas(cut).Select(Type).ShouldBe(["WebPage", "WebSite", "Organization"]);
    }

    // Guard test: with the option off, the home page writes its own WebSite as before and no ids.
    [Fact]
    public void SeoMetadataRenderer_SiteSchemasOff_WritesTheStructuredDataAsBefore()
    {
        Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new SeoMetadataOptions { SiteUrl = Site, SiteName = "Acme Docs" }));
        Navigation.NavigateTo("http://localhost/");

        var cut = RenderStatic<SeoMetadataRenderer>(p => p
            .Add(c => c.Content, Article("Home"))
            .Add(c => c.SchemaTypes, [SchemaType.WebPage, SchemaType.WebSite]));

        var schemas = Schemas(cut);
        schemas.Select(Type).ShouldBe(["WebPage", "WebSite"]);
        schemas[0]["isPartOf"].ShouldBeNull();
        schemas[0]["publisher"]!["name"]!.GetValue<string>().ShouldBe("Acme Docs");
        schemas[1]["@id"].ShouldBeNull();
    }

    private void UseSiteSchemas(Action<SeoMetadataOptions>? configure = null)
    {
        var options = new SeoMetadataOptions
        {
            SiteUrl = Site,
            SiteName = "Acme Docs",
            SiteDescription = "Guides for Acme.",
            OrganizationName = "Acme Inc.",
            SiteLogoUrl = "/images/logo.png",
            EnableSiteSchemas = true,
        };
        configure?.Invoke(options);
        Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(options));
    }

    private static string? Meta(IRenderedComponent<SeoMetadataRenderer> cut, string name) =>
        cut.Find($"meta[name='{name}']").GetAttribute("content");

    private static List<JsonNode> Schemas(IRenderedComponent<SeoMetadataRenderer> cut) =>
        cut.FindAll("script[type='application/ld+json']").Select(script => JsonNode.Parse(script.TextContent)!).ToList();

    private static string Type(JsonNode schema) => schema["@type"]!.GetValue<string>();

    private static ContentItem Article(string title)
    {
        var item = ContentItem.Create($"id-{title}", title, "<p>Body</p>", $"blog/{title}.md", "test");
        item.SetUrl("blog/post");
        item.SetDescription("Summary");
        item.Metadata = new FrontMatter { Title = title, SeoProperties = new SeoMetadata() };
        return item;
    }
}
