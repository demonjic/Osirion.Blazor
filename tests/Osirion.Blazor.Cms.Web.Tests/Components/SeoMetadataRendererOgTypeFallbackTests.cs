using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Osirion.Blazor.Cms.Domain.ValueObjects;
using Osirion.Blazor.Cms.Web.Components;
using Osirion.Blazor.Cms.Web.Options;
using Osirion.Blazor.Cms.Web.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.Components;

/// <summary>
/// Pins the type a content page declares when no schema type is given: the front matter type, then
/// <c>FallbackSchemaType</c>, then <c>WebPage</c>, so <c>og:type</c> says <c>article</c> only for an article.
/// </summary>
public class SeoMetadataRendererOgTypeFallbackTests : CmsWebComponentTestBase
{
    // Fails before the fix: SeoMetadata.Type defaulted to "Article", so every item with front matter and no type
    // said og:type "article" and published an Article, a contact page included.
    [Fact]
    public void SeoMetadataRenderer_FrontMatterWithoutTypeAndNoSchemaTypes_IsAWebsite()
    {
        var item = SeoTestContent.Article();
        item.Metadata = new FrontMatter();

        var cut = RenderPage(_ => { }, item);

        Property(cut, "og:type").ShouldBe("website");
        Meta(cut, "content-type").ShouldBe("WebPage");
        cut.FindAll("meta[property='article:published_time']").ShouldBeEmpty();
        cut.FindAll("script[type='application/ld+json']")[0].TextContent.ShouldContain("\"@type\": \"WebPage\"");
    }

    [Theory]
    [InlineData("BlogPosting", "article")]
    [InlineData("techarticle", "article")]
    [InlineData("ProfilePage", "profile")]
    public void SeoMetadataRenderer_FrontMatterType_StillSetsTheOgType(string declaredType, string expectedOgType)
    {
        var item = SeoTestContent.Article();
        item.Metadata = new FrontMatter { SeoProperties = new SeoMetadata { Type = declaredType } };

        var cut = RenderPage(_ => { }, item);

        Property(cut, "og:type").ShouldBe(expectedOgType);
    }

    [Fact]
    public void SeoMetadataRenderer_FallbackSchemaType_AppliesWhenNothingElseNamesAType()
    {
        var item = SeoTestContent.Article();
        item.Metadata = new FrontMatter();

        var cut = RenderPage(p => p.Add(c => c.FallbackSchemaType, SchemaType.Article), item);

        Property(cut, "og:type").ShouldBe("article");
        Meta(cut, "content-type").ShouldBe("Article");
        cut.FindAll("script[type='application/ld+json']")[0].TextContent.ShouldContain("\"@type\": \"Article\"");
    }

    [Fact]
    public void SeoMetadataRenderer_FallbackSchemaType_YieldsToTheFrontMatterType()
    {
        var item = SeoTestContent.Article();
        item.Metadata = new FrontMatter { SeoProperties = new SeoMetadata { Type = "ProfilePage" } };

        var cut = RenderPage(p => p.Add(c => c.FallbackSchemaType, SchemaType.Article), item);

        Property(cut, "og:type").ShouldBe("profile");
    }

    [Fact]
    public void SeoMetadataRenderer_FallbackSchemaType_YieldsToTheConfiguredDefaultSchemaTypes()
    {
        Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new SeoMetadataOptions { DefaultSchemaTypes = [SchemaType.Product] }));

        var cut = RenderPage(p => p.Add(c => c.FallbackSchemaType, SchemaType.Article));

        Property(cut, "og:type").ShouldBe("product");
    }

    private IRenderedComponent<SeoMetadataRenderer> RenderPage(
        Action<ComponentParameterCollectionBuilder<SeoMetadataRenderer>> parameters,
        Osirion.Blazor.Cms.Domain.Entities.ContentItem? item = null)
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo(SeoTestContent.PageUrl);
        return RenderStatic<SeoMetadataRenderer>(p =>
        {
            p.Add(c => c.Content, item ?? SeoTestContent.Article());
            parameters(p);
        });
    }

    private static string? Meta(IRenderedComponent<SeoMetadataRenderer> cut, string name) =>
        cut.FindAll($"meta[name='{name}']").ShouldHaveSingleItem().GetAttribute("content");

    private static string? Property(IRenderedComponent<SeoMetadataRenderer> cut, string property) =>
        cut.FindAll($"meta[property='{property}']").ShouldHaveSingleItem().GetAttribute("content");
}
