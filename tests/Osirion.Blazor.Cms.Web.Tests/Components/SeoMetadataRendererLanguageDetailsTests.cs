using System.Globalization;
using System.Text.Json.Nodes;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Web.Components;
using Osirion.Blazor.Cms.Web.Options;
using Osirion.Blazor.Cms.Web.Tests.Base;
using Osirion.Blazor.Core.Extensions;
using Osirion.Blazor.Core.Localization;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.Components;

/// <summary>
/// The author of the structured data and the name of the home page in its breadcrumbs.
/// </summary>
public class SeoMetadataRendererLanguageDetailsTests : CmsWebComponentTestBase
{
    private NavigationManager Navigation => Services.GetRequiredService<NavigationManager>();

    // Fails before the fix: every type wrote "author": {"@type": "Person", "name": ""} for an item without an author.
    [Theory]
    [InlineData(SchemaType.Article)]
    [InlineData(SchemaType.WebPage)]
    [InlineData(SchemaType.Recipe)]
    [InlineData(SchemaType.VideoObject)]
    [InlineData(SchemaType.Review)]
    public void SeoMetadataRenderer_ItemWithoutAuthor_WritesNoAuthor(SchemaType schemaType)
    {
        var item = SeoTestContent.Article();
        item.Author = " ";

        var cut = RenderStatic<SeoMetadataRenderer>(p => p.Add(c => c.Content, item).Add(c => c.SchemaTypes, [schemaType]));

        Schemas(cut).ShouldAllBe(schema => schema["author"] == null);
        cut.FindAll("meta[name='author']").ShouldBeEmpty();
    }

    [Fact]
    public void SeoMetadataRenderer_ListedItemWithoutAuthor_WritesNoAuthor()
    {
        var item = SeoTestContent.Article();
        item.Author = string.Empty;

        var cut = RenderStatic<SeoMetadataRenderer>(p => p.Add(c => c.ContentItems, [item]).Add(c => c.CollectionTitle, "Blog"));

        Schemas(cut).ShouldAllBe(schema => schema["author"] == null);
    }

    [Fact]
    public void SeoMetadataRenderer_ItemWithAuthor_StillNamesThem()
    {
        var cut = RenderStatic<SeoMetadataRenderer>(p => p
            .Add(c => c.Content, SeoTestContent.Article())
            .Add(c => c.SchemaTypes, [SchemaType.Recipe]));

        Schemas(cut)[0]["author"]!["name"]!.GetValue<string>().ShouldBe("Jane Doe");
    }

    // Fails before the fix: the first breadcrumb was "Home" in every language.
    [Fact]
    public void SeoMetadataRenderer_SiteLabelForTheAddressCulture_NamesTheHomeBreadcrumb()
    {
        Services.AddOsirionLocalization(localization =>
        {
            localization.DefaultCulture = "en";
            localization.Cultures.Add(new LocalizationCulture { Name = "sr-Latn", Prefix = "sr" });
        });
        Services.AddSingleton<IBreadcrumbHomeText>(new Labels(culture => culture.Name == "sr-Latn" ? "Početna" : null));
        Navigation.NavigateTo("http://localhost/sr/blog/guides/my-post");

        var cut = RenderStatic<SeoMetadataRenderer>(p => p.Add(c => c.Content, SeoTestContent.Article()));

        HomeBreadcrumb(cut).ShouldBe("Početna");
    }

    [Fact]
    public void SeoMetadataRenderer_NoSiteLabel_UsesTheConfiguredName()
    {
        Services.AddSingleton<IBreadcrumbHomeText>(new Labels(_ => null));
        Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new SeoMetadataOptions { BreadcrumbHomeText = "Start" }));

        var cut = RenderStatic<SeoMetadataRenderer>(p => p.Add(c => c.Content, SeoTestContent.Article()));

        HomeBreadcrumb(cut).ShouldBe("Start");
    }

    [Fact]
    public void SeoMetadataRenderer_SiteLabelWithoutLocaleRouting_ReadsTheUiCulture()
    {
        Services.AddSingleton<IBreadcrumbHomeText>(new Labels(culture => culture.Name == "de-DE" ? "Startseite" : null));
        var previous = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("de-DE");
        try
        {
            var cut = RenderStatic<SeoMetadataRenderer>(p => p.Add(c => c.Content, SeoTestContent.Article()));

            HomeBreadcrumb(cut).ShouldBe("Startseite");
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    [Fact]
    public void SeoMetadataRenderer_BreadcrumbHomeTextParameter_WinsOverTheSiteLabel()
    {
        Services.AddSingleton<IBreadcrumbHomeText>(new Labels(_ => "Početna"));

        var cut = RenderStatic<SeoMetadataRenderer>(p => p
            .Add(c => c.Content, SeoTestContent.Article())
            .Add(c => c.BreadcrumbHomeText, "Naslovna"));

        HomeBreadcrumb(cut).ShouldBe("Naslovna");
    }

    // Guard test: without labels or options the first breadcrumb stays "Home".
    [Fact]
    public void SeoMetadataRenderer_NothingConfigured_NamesTheHomeBreadcrumbHome()
    {
        var cut = RenderStatic<SeoMetadataRenderer>(p => p.Add(c => c.Content, SeoTestContent.Article()));

        HomeBreadcrumb(cut).ShouldBe("Home");
    }

    private static string HomeBreadcrumb(IRenderedComponent<SeoMetadataRenderer> cut) =>
        Schemas(cut).Single(schema => schema["@type"]!.GetValue<string>() == "BreadcrumbList")
            ["itemListElement"]![0]!["name"]!.GetValue<string>();

    private static List<JsonNode> Schemas(IRenderedComponent<SeoMetadataRenderer> cut) =>
        cut.FindAll("script[type='application/ld+json']").Select(script => JsonNode.Parse(script.TextContent)!).ToList();

    private sealed class Labels(Func<CultureInfo, string?> home) : IBreadcrumbHomeText
    {
        public string? For(CultureInfo culture) => home(culture);
    }
}
