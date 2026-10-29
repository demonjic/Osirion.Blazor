using System.Text.Json.Nodes;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Repositories;
using Osirion.Blazor.Cms.Domain.Services;
using Osirion.Blazor.Cms.Domain.ValueObjects;
using Osirion.Blazor.Cms.Web.Components;
using Osirion.Blazor.Cms.Web.Tests.Base;
using Osirion.Blazor.Core.Extensions;
using Osirion.Blazor.Core.Localization;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.Components;

/// <summary>
/// The hreflang links <see cref="SeoMetadataRenderer"/> writes on a site with locale routing, and the head it writes
/// for a page that is not a content item.
/// </summary>
public class SeoMetadataRendererLanguageVersionsTests : CmsWebComponentTestBase
{
    private const string Origin = "http://localhost";

    private readonly IContentProviderManager _manager = Substitute.For<IContentProviderManager>();

    public SeoMetadataRendererLanguageVersionsTests()
    {
        Services.AddSingleton(_manager);
    }

    private NavigationManager Navigation => Services.GetRequiredService<NavigationManager>();

    [Fact]
    public void SeoMetadataRenderer_CodePageLocalized_LinksThePageInEveryCultureAndXDefault()
    {
        UseLocalization();
        Navigation.NavigateTo($"{Origin}/sr/contact?utm_source=mail");

        var cut = RenderStatic<SeoMetadataRenderer>(p => p.Add(c => c.Title, "Kontakt"));

        Alternates(cut).ShouldBe([
            ("en", $"{Origin}/contact"),
            ("sr-Latn", $"{Origin}/sr/contact"),
            ("de", $"{Origin}/de/contact"),
            ("x-default", $"{Origin}/contact"),
        ]);
        cut.Find("meta[name='language']").GetAttribute("content").ShouldBe("sr-Latn");
        cut.Find("meta[property='og:locale']").GetAttribute("content").ShouldBe("sr");
    }

    [Fact]
    public void SeoMetadataRenderer_CodePageCanonicalPath_LinksAreBuiltFromIt()
    {
        UseLocalization();
        Navigation.NavigateTo($"{Origin}/contact-us");

        var cut = RenderStatic<SeoMetadataRenderer>(p => p
            .Add(c => c.Title, "Contact")
            .Add(c => c.CanonicalPath, "/contact"));

        cut.Find("link[rel='canonical']").GetAttribute("href").ShouldBe($"{Origin}/contact");
        Alternates(cut)[1].ShouldBe(("sr-Latn", $"{Origin}/sr/contact"));
    }

    [Fact]
    public void SeoMetadataRenderer_ListPageLocalized_LinksThePageInEveryCulture()
    {
        UseLocalization();
        Navigation.NavigateTo($"{Origin}/de/blog");

        var cut = RenderStatic<SeoMetadataRenderer>(p => p
            .Add(c => c.ContentItems, [Article("Post", "blog/post", "en", "post")])
            .Add(c => c.CollectionTitle, "Blog"));

        Alternates(cut).Select(link => link.Href).ShouldBe([$"{Origin}/blog", $"{Origin}/sr/blog", $"{Origin}/de/blog", $"{Origin}/blog"]);
    }

    [Fact]
    public void SeoMetadataRenderer_ContentItemLocalized_LinksOnlyTheCulturesWithATranslation()
    {
        UseLocalization();
        Navigation.NavigateTo($"{Origin}/blog/post");
        var english = Article("Post", "blog/post", "en", "post");
        var serbian = Article("Objava", "blog/post", "sr", "post");
        var hidden = Article("Beitrag", "blog/post", "de", "post");
        hidden.Metadata!.SeoProperties!.Robots = "noindex";
        ContentQuery? query = null;
        _manager.GetContentByQueryAsync(Arg.Do<ContentQuery>(q => query = q), Arg.Any<CancellationToken>())
            .Returns(new List<ContentItem> { english, serbian, hidden });

        var cut = RenderStatic<SeoMetadataRenderer>(p => p.Add(c => c.Content, english));

        Alternates(cut).ShouldBe([
            ("en", $"{Origin}/blog/post"),
            ("sr-Latn", $"{Origin}/sr/blog/post"),
            ("x-default", $"{Origin}/blog/post"),
        ]);
        query.ShouldNotBeNull();
        query.LocalizationId.ShouldBe("post");
        query.IncludeUnpublished.ShouldBeFalse();
    }

    [Fact]
    public void SeoMetadataRenderer_ContentItemWithoutTranslations_WritesNoLinks()
    {
        UseLocalization();
        var english = Article("Post", "blog/post", "en", "post");
        _manager.GetContentByQueryAsync(Arg.Any<ContentQuery>(), Arg.Any<CancellationToken>())
            .Returns(new List<ContentItem> { english });

        var cut = RenderStatic<SeoMetadataRenderer>(p => p.Add(c => c.Content, english));

        Alternates(cut).ShouldBeEmpty();
    }

    [Fact]
    public void SeoMetadataRenderer_TranslationsReadLater_LinksAppearWhenTheReadCompletes()
    {
        UseLocalization();
        Navigation.NavigateTo($"{Origin}/sr/blog/post");
        var serbian = Article("Objava", "blog/post", "sr", "post");
        var pending = new TaskCompletionSource<IReadOnlyList<ContentItem>>();
        _manager.GetContentByQueryAsync(Arg.Any<ContentQuery>(), Arg.Any<CancellationToken>()).Returns(pending.Task);

        var cut = RenderStatic<SeoMetadataRenderer>(p => p.Add(c => c.Content, serbian));
        Alternates(cut).ShouldBeEmpty();
        pending.SetResult([Article("Post", "blog/post", "en", "post"), serbian]);

        cut.WaitForAssertion(() => Alternates(cut).ShouldBe([
            ("en", $"{Origin}/blog/post"),
            ("sr-Latn", $"{Origin}/sr/blog/post"),
            ("x-default", $"{Origin}/blog/post"),
        ]));
    }

    [Fact]
    public void SeoMetadataRenderer_TranslationsCannotBeRead_WritesTheRestOfTheHead()
    {
        UseLocalization();
        _manager.GetContentByQueryAsync(Arg.Any<ContentQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<IReadOnlyList<ContentItem>>(new InvalidOperationException("offline")));

        var cut = RenderStatic<SeoMetadataRenderer>(p => p.Add(c => c.Content, Article("Post", "blog/post", "en", "post")));

        Alternates(cut).ShouldBeEmpty();
        cut.FindAll("link[rel='canonical']").Count.ShouldBe(1);
    }

    [Fact]
    public void SeoMetadataRenderer_ExplicitAlternateLanguageUrls_WinOverLocaleRouting()
    {
        UseLocalization();
        Navigation.NavigateTo($"{Origin}/contact");

        var cut = RenderStatic<SeoMetadataRenderer>(p => p
            .Add(c => c.Title, "Contact")
            .Add(c => c.AlternateLanguageUrls, ["fr|https://example.com/fr/contact"]));

        Alternates(cut).ShouldBe([("fr", "https://example.com/fr/contact")]);
    }

    [Fact]
    public void SeoMetadataRenderer_ExplicitEmptyAlternateLanguageUrls_WritesNoLinksAndReadsNoTranslations()
    {
        UseLocalization();

        var cut = RenderStatic<SeoMetadataRenderer>(p => p
            .Add(c => c.Content, Article("Post", "blog/post", "en", "post"))
            .Add(c => c.AlternateLanguageUrls, []));

        Alternates(cut).ShouldBeEmpty();
        _manager.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact]
    public void SeoMetadataRenderer_CodePage_WritesTheWholeHead()
    {
        Navigation.NavigateTo($"{Origin}/contact?utm_source=mail");

        var cut = RenderStatic<SeoMetadataRenderer>(p => p
            .Add(c => c.Title, "Contact")
            .Add(c => c.Description, "Write to the team.")
            .Add(c => c.ImageUrl, "/images/contact.png")
            .Add(c => c.SiteNameOverride, "Example"));

        Render(cut.FindComponent<PageTitle>().Instance.ChildContent!).Markup.ShouldBe("Contact | Example");
        cut.Find("meta[name='description']").GetAttribute("content").ShouldBe("Write to the team.");
        cut.Find("link[rel='canonical']").GetAttribute("href").ShouldBe($"{Origin}/contact");
        cut.Find("meta[property='og:title']").GetAttribute("content").ShouldBe("Contact");
        cut.Find("meta[property='og:description']").GetAttribute("content").ShouldBe("Write to the team.");
        cut.Find("meta[property='og:type']").GetAttribute("content").ShouldBe("website");
        cut.Find("meta[property='og:url']").GetAttribute("content").ShouldBe($"{Origin}/contact");
        cut.Find("meta[property='og:image']").GetAttribute("content").ShouldBe($"{Origin}/images/contact.png");
        cut.Find("meta[name='twitter:title']").GetAttribute("content").ShouldBe("Contact");
        cut.Find("meta[name='twitter:image']").GetAttribute("content").ShouldBe($"{Origin}/images/contact.png");
        cut.Find("meta[name='language']").GetAttribute("content").ShouldBe("en");
        cut.FindAll("meta[property^='article:']").ShouldBeEmpty();
        cut.FindAll("meta[name='publish_date']").ShouldBeEmpty();
        cut.FindAll("meta[name='author']").ShouldBeEmpty();
        Alternates(cut).ShouldBeEmpty();

        var schemas = cut.FindAll("script[type='application/ld+json']");
        schemas.Count.ShouldBe(1);
        var schema = JsonNode.Parse(schemas[0].TextContent)!;
        schema["@type"]!.GetValue<string>().ShouldBe("WebPage");
        schema["name"]!.GetValue<string>().ShouldBe("Contact");
        schema["description"]!.GetValue<string>().ShouldBe("Write to the team.");
        schema["url"]!.GetValue<string>().ShouldBe($"{Origin}/contact");
        schema["inLanguage"]!.GetValue<string>().ShouldBe("en");
        schema["image"]!.GetValue<string>().ShouldBe($"{Origin}/images/contact.png");
        schema["publisher"]!["name"]!.GetValue<string>().ShouldBe("Example");
        schema["datePublished"].ShouldBeNull();
        schema["author"].ShouldBeNull();
    }

    [Fact]
    public void SeoMetadataRenderer_CodePageSchemaTypes_UsesTheFirstWebPageType()
    {
        var cut = RenderStatic<SeoMetadataRenderer>(p => p
            .Add(c => c.Title, "Contact")
            .Add(c => c.SchemaTypes, [SchemaType.Article, SchemaType.ContactPage]));

        var schema = JsonNode.Parse(cut.Find("script[type='application/ld+json']").TextContent)!;
        schema["@type"]!.GetValue<string>().ShouldBe("ContactPage");
        cut.Find("meta[property='og:type']").GetAttribute("content").ShouldBe("website");
    }

    [Theory]
    [InlineData("https://example.com/contact", "https://example.com/contact")]
    [InlineData("//evil.example/contact", "http://localhost/contact-us")]
    public void SeoMetadataRenderer_CodePageCanonicalPath_AcceptsAbsoluteAddressesAndIgnoresOtherHosts(string canonicalPath, string expected)
    {
        Navigation.NavigateTo($"{Origin}/contact-us");

        var cut = RenderStatic<SeoMetadataRenderer>(p => p
            .Add(c => c.Title, "Contact")
            .Add(c => c.CanonicalPath, canonicalPath));

        cut.Find("link[rel='canonical']").GetAttribute("href").ShouldBe(expected);
    }

    // Guard test: the page parameters belong to pages that are not content items, so a content page that passes them
    // renders exactly what it renders without them.
    [Fact]
    public void SeoMetadataRenderer_ContentPageWithPageParameters_RendersAsWithoutThem()
    {
        Navigation.NavigateTo($"{Origin}/blog/post");
        var item = Article("Post", "blog/post", "en", "post");

        var without = RenderStatic<SeoMetadataRenderer>(p => p.Add(c => c.Content, item)).Markup;
        var with = RenderStatic<SeoMetadataRenderer>(p => p
            .Add(c => c.Content, item)
            .Add(c => c.Title, "Other")
            .Add(c => c.Description, "Other description")
            .Add(c => c.ImageUrl, "/other.png")
            .Add(c => c.CanonicalPath, "/other")).Markup;

        with.ShouldBe(without);
    }

    // Guard test: without locale routing a content page writes no hreflang link and never reads its translations.
    [Fact]
    public void SeoMetadataRenderer_WithoutLocaleRouting_WritesNoLinksAndReadsNoTranslations()
    {
        var cut = RenderStatic<SeoMetadataRenderer>(p => p.Add(c => c.Content, Article("Post", "blog/post", "en", "post")));

        Alternates(cut).ShouldBeEmpty();
        _manager.ReceivedCalls().ShouldBeEmpty();
    }

    // Guard test: with neither content nor a title the component still writes only an empty page title.
    [Fact]
    public void SeoMetadataRenderer_NoContentNoTitle_WritesNoTags()
    {
        UseLocalization();

        var cut = RenderStatic<SeoMetadataRenderer>(p => p.Add(c => c.Description, "Only a description"));

        cut.FindAll("meta").ShouldBeEmpty();
        cut.FindAll("link").ShouldBeEmpty();
    }

    // A derived locale (Serbian Latin transliterated from Cyrillic) is a translation like any other: the derived item
    // links its source and the other translations, and its source links it.
    [Fact]
    public void SeoMetadataRenderer_DerivedLocale_LinksTheDerivedItemAndItsSource()
    {
        Services.AddOsirionLocalization(localization =>
        {
            localization.DefaultCulture = "en";
            localization.Cultures.Add(new LocalizationCulture { Name = "sr-Cyrl", Prefix = "sr" });
            localization.Cultures.Add(new LocalizationCulture { Name = "sr-Latn", Prefix = "lat" });
        });
        Navigation.NavigateTo($"{Origin}/lat/blog/post");
        var english = Article("Post", "blog/post", "en", "post");
        var cyrillic = Article("Objava", "blog/post", "sr", "post");
        var latin = cyrillic.DeriveForLocale("sr-Latn-Objava", "sr-Latn", "sr-Latn/blog/post.md");
        latin.IsDerived.ShouldBeTrue();
        _manager.GetContentByQueryAsync(Arg.Any<ContentQuery>(), Arg.Any<CancellationToken>())
            .Returns(new List<ContentItem> { english, cyrillic, latin });

        var cut = RenderStatic<SeoMetadataRenderer>(p => p.Add(c => c.Content, latin));

        Alternates(cut).ShouldBe([
            ("en", $"{Origin}/blog/post"),
            ("sr-Cyrl", $"{Origin}/sr/blog/post"),
            ("sr-Latn", $"{Origin}/lat/blog/post"),
            ("x-default", $"{Origin}/blog/post"),
        ]);
        cut.Find("meta[name='language']").GetAttribute("content").ShouldBe("sr-Latn");
    }

    [Fact]
    public void SeoMetadataRenderer_ContentItemInACultureTheSiteDoesNotHave_WritesNoLinksAndReadsNoTranslations()
    {
        UseLocalization();

        var cut = RenderStatic<SeoMetadataRenderer>(p => p.Add(c => c.Content, Article("Article", "blog/post", "fr", "post")));

        Alternates(cut).ShouldBeEmpty();
        _manager.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact]
    public void SeoMetadataRenderer_ProviderAnswersNull_WritesNoLinks()
    {
        UseLocalization();
        _manager.GetContentByQueryAsync(Arg.Any<ContentQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>>(null!));

        var cut = RenderStatic<SeoMetadataRenderer>(p => p.Add(c => c.Content, Article("Post", "blog/post", "en", "post")));

        Alternates(cut).ShouldBeEmpty();
    }

    [Fact]
    public void SeoMetadataRenderer_CodePageWithGeneratedFaq_AsksTheTitleAndAnswersWithTheDescription()
    {
        var cut = RenderStatic<SeoMetadataRenderer>(p => p
            .Add(c => c.Title, "How to reach the team?")
            .Add(c => c.Description, "Write to the team through the form.")
            .Add(c => c.EnableGeneratedFaq, true));

        cut.Find("meta[name='answers-to']").GetAttribute("content").ShouldBe("How to reach the team?");
        cut.Find("meta[name='entity-summary']").GetAttribute("content").ShouldBe("Write to the team through the form.");
        var faq = cut.FindAll("script[type='application/ld+json']")
            .Select(script => JsonNode.Parse(script.TextContent)!)
            .Single(schema => schema["@type"]!.GetValue<string>() == "FAQPage");
        faq["mainEntity"]![0]!["acceptedAnswer"]!["text"]!.GetValue<string>().ShouldBe("Write to the team through the form.");
    }

    [Theory]
    [InlineData(SchemaType.AboutPage, "AboutPage")]
    [InlineData(SchemaType.Article, "WebPage")]
    public void SeoMetadataRenderer_CodePageFallbackSchemaType_UsedOnlyWhenItIsAWebPageType(SchemaType fallback, string expected)
    {
        var cut = RenderStatic<SeoMetadataRenderer>(p => p
            .Add(c => c.Title, "About")
            .Add(c => c.FallbackSchemaType, fallback));

        JsonNode.Parse(cut.Find("script[type='application/ld+json']").TextContent)!["@type"]!.GetValue<string>().ShouldBe(expected);
        cut.Find("meta[property='og:type']").GetAttribute("content").ShouldBe("website");
    }

    private void UseLocalization() =>
        Services.AddOsirionLocalization(localization =>
        {
            localization.DefaultCulture = "en";
            localization.Cultures.Add(new LocalizationCulture { Name = "sr-Latn", Prefix = "sr" });
            localization.Cultures.Add(new LocalizationCulture { Name = "de" });
        });

    private static List<(string HrefLang, string Href)> Alternates(IRenderedComponent<SeoMetadataRenderer> cut) =>
        cut.FindAll("link[rel='alternate']")
            .Select(link => (link.GetAttribute("hreflang")!, link.GetAttribute("href")!))
            .ToList();

    private static ContentItem Article(string title, string url, string locale, string localizationId)
    {
        var item = ContentItem.Create($"{locale}-{title}", title, "<p>Body</p>", $"{locale}/{url}.md", "test");
        item.SetUrl(url);
        item.SetDescription("Summary");
        item.SetLocale(locale);
        item.SetContentId(localizationId);
        item.Metadata = new FrontMatter { Title = title, SeoProperties = new SeoMetadata { Type = "Article" } };
        return item;
    }
}
