using System.Text.Json;
using Bunit;
using Microsoft.AspNetCore.Components;
using Osirion.Blazor.Cms.Admin.Features.ContentEditor.Components.Shared;
using Osirion.Blazor.Cms.Admin.Tests.TestDoubles;
using Osirion.Blazor.Cms.Domain.ValueObjects;
using Shouldly;

namespace Osirion.Blazor.Cms.Admin.Tests.Features.ContentEditor.Components.Shared;

public class SeoMetadataFormTests : AdminComponentTestContext
{
    private readonly List<SeoMetadata> _changes = [];

    public SeoMetadataFormTests()
    {
        RenderInteractively();
    }

    private IRenderedComponent<SeoMetadataForm> RenderForm(SeoMetadata seo) =>
        Render<SeoMetadataForm>(parameters => parameters
            .Add(p => p.SeoMetadata, seo)
            .Add(p => p.SeoMetadataChanged, (SeoMetadata value) => _changes.Add(value)));

    private static SeoMetadata CreateSeo(string type)
    {
        var seo = SeoMetadata.Create("My Title", "My description");
        seo.Type = type;
        seo.Canonical = "https://example.com/post";
        seo.OgImageUrl = "https://example.com/og.png";
        return seo;
    }

    private JsonElement GenerateFor(string type)
    {
        var cut = RenderForm(CreateSeo(type));

        cut.Find("button.btn-outline-secondary").Click();

        return JsonDocument.Parse(_changes.ShouldHaveSingleItem().JsonLd).RootElement;
    }

    [Fact]
    public void SeoMetadataForm_StaticRender_ShowsCountersWithoutWarnings()
    {
        SetRendererInfo(new RendererInfo("Static", false));

        var cut = Render<SeoMetadataForm>(parameters => parameters.Add(p => p.SeoMetadata, SeoMetadata.Create("Short", "Desc")));

        var counters = cut.FindAll(".char-counter");
        counters[0].TextContent.ShouldContain("5 / 60");
        counters[0].ClassList.ShouldNotContain("warning");
        counters[0].ClassList.ShouldNotContain("danger");
        counters[1].TextContent.ShouldContain("4 / 160");
    }

    [Fact]
    public void SeoMetadataForm_LongValues_MarkCountersAsWarningOrDanger()
    {
        var cut = Render<SeoMetadataForm>(parameters => parameters
            .Add(p => p.SeoMetadata, SeoMetadata.Create(new string('t', 55), new string('d', 161))));

        var counters = cut.FindAll(".char-counter");
        counters[0].ClassList.ShouldContain("warning");
        counters[1].ClassList.ShouldContain("danger");
    }

    [Fact]
    public void SeoMetadataForm_NullMetadata_FallsBackToEmpty()
    {
        var cut = Render<SeoMetadataForm>(parameters => parameters.Add(p => p.SeoMetadata, null!));

        cut.Instance.SeoMetadata.ShouldNotBeNull();
        cut.Find("#metaTitle").GetAttribute("value").ShouldBe(string.Empty);
    }

    [Fact]
    public void SeoMetadataForm_EditingFields_RaisesChangedWithNewValues()
    {
        var cut = RenderForm(SeoMetadata.Create("t", "d"));

        cut.Find("#metaTitle").Input("Title");
        cut.Find("#metaDescription").Input("Description");
        cut.Find("#canonicalUrl").Input("https://example.com/c");
        cut.Find("#robots").Change("noindex, follow");
        cut.Find("#schemaType").Change("Product");
        cut.Find("#jsonLd").Input("{}");

        var last = _changes[^1];
        last.Title.ShouldBe("Title");
        last.Description.ShouldBe("Description");
        last.Canonical.ShouldBe("https://example.com/c");
        last.Robots.ShouldBe("noindex, follow");
        last.Type.ShouldBe("Product");
        last.JsonLd.ShouldBe("{}");
    }

    [Fact]
    public void SeoMetadataForm_EditingWithoutListener_UpdatesBoundValue()
    {
        var seo = SeoMetadata.Create("t", "d");
        var cut = Render<SeoMetadataForm>(parameters => parameters.Add(p => p.SeoMetadata, seo));

        cut.Find("#metaTitle").Input("local");

        seo.Title.ShouldBe("local");
    }

    [Theory]
    [InlineData("Article")]
    [InlineData("BlogPosting")]
    public void SeoMetadataForm_GenerateForArticleTypes_ProducesArticleJsonLd(string type)
    {
        var json = GenerateFor(type);

        json.GetProperty("type").GetString().ShouldBe(type);
        json.GetProperty("headline").GetString().ShouldBe("My Title");
        json.GetProperty("description").GetString().ShouldBe("My description");
        json.GetProperty("image").GetString().ShouldBe("https://example.com/og.png");
        json.GetProperty("url").GetString().ShouldBe("https://example.com/post");
        json.GetProperty("publisher").GetProperty("type").GetString().ShouldBe("Organization");
    }

    [Fact]
    public void SeoMetadataForm_GenerateForProduct_ProducesOffer()
    {
        var json = GenerateFor("Product");

        json.GetProperty("type").GetString().ShouldBe("Product");
        json.GetProperty("name").GetString().ShouldBe("My Title");
        json.GetProperty("offers").GetProperty("priceCurrency").GetString().ShouldBe("USD");
    }

    [Fact]
    public void SeoMetadataForm_GenerateForEvent_ProducesLocation()
    {
        var json = GenerateFor("Event");

        json.GetProperty("type").GetString().ShouldBe("Event");
        json.GetProperty("location").GetProperty("type").GetString().ShouldBe("Place");
        json.TryGetProperty("startDate", out _).ShouldBeTrue();
    }

    [Fact]
    public void SeoMetadataForm_GenerateForOrganization_UsesImageAsLogo()
    {
        var json = GenerateFor("Organization");

        json.GetProperty("type").GetString().ShouldBe("Organization");
        json.GetProperty("logo").GetString().ShouldBe("https://example.com/og.png");
        json.GetProperty("sameAs").GetArrayLength().ShouldBe(3);
    }

    [Fact]
    public void SeoMetadataForm_GenerateForPerson_ProducesPerson()
    {
        var json = GenerateFor("Person");

        json.GetProperty("type").GetString().ShouldBe("Person");
        json.GetProperty("image").GetString().ShouldBe("https://example.com/og.png");
        json.GetProperty("sameAs").GetArrayLength().ShouldBe(2);
    }

    [Theory]
    [InlineData("WebPage")]
    [InlineData("NewsArticle")]
    public void SeoMetadataForm_GenerateForOtherTypes_ProducesWebPage(string type)
    {
        var json = GenerateFor(type);

        json.GetProperty("type").GetString().ShouldBe("WebPage");
        json.GetProperty("name").GetString().ShouldBe("My Title");
        json.GetProperty("url").GetString().ShouldBe("https://example.com/post");
    }

    [Fact]
    public void SeoMetadataForm_Generate_KeepsTheSelectedSchemaType()
    {
        var cut = RenderForm(CreateSeo("Product"));

        cut.Find("button.btn-outline-secondary").Click();

        _changes.ShouldHaveSingleItem().Type.ShouldBe("Product");
        cut.Find("#schemaType").GetAttribute("value").ShouldBe("Product");
    }

    [Fact]
    public void SeoMetadataForm_Generate_ShowsJsonInTextarea()
    {
        var cut = RenderForm(CreateSeo("WebPage"));

        cut.Find("button.btn-outline-secondary").Click();

        cut.Find("#jsonLd").GetAttribute("value").ShouldNotBeNull().ShouldContain("\"name\": \"My Title\"");
    }

    [Fact]
    public void SeoMetadataForm_EmptyRobots_OffersADefaultOption()
    {
        // Fails before the fix: the select had no empty option, so an empty Robots value showed "Index, Follow"
        // and could not be chosen again once a robots value was picked.
        var cut = RenderForm(SeoMetadata.Create("t", "d"));

        var options = cut.FindAll("#robots option");
        options[0].GetAttribute("value").ShouldBe(string.Empty);
        options[0].TextContent.ShouldContain("Default");
        cut.Find("#robots").GetAttribute("value").ShouldBe(string.Empty);
    }

    [Fact]
    public void SeoMetadataForm_EmptySchemaType_OffersADefaultOption()
    {
        // Fails before the fix: the select had no empty option, so an empty schema type showed "Article" and could
        // not be chosen again once a type was picked.
        var cut = RenderForm(SeoMetadata.Create("t", "d"));

        var options = cut.FindAll("#schemaType option");
        options[0].GetAttribute("value").ShouldBe(string.Empty);
        options[0].TextContent.ShouldContain("Default");
        cut.Find("#schemaType").GetAttribute("value").ShouldBe(string.Empty);
    }
}
