using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Repositories;
using Osirion.Blazor.Cms.Domain.Services;
using Osirion.Blazor.Cms.Domain.ValueObjects;
using Osirion.Blazor.Cms.Web.Feeds;
using Osirion.Blazor.Core.Extensions;
using Osirion.Blazor.Core.Localization;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.Feeds;

public class SitemapLanguageVersionsTests
{
    private static readonly XNamespace Sitemap = "http://www.sitemaps.org/schemas/sitemap/0.9";
    private static readonly XNamespace Xhtml = "http://www.w3.org/1999/xhtml";

    private readonly IContentProviderManager _manager = Substitute.For<IContentProviderManager>();

    [Fact]
    public async Task Sitemap_Localized_ListsEachAdditionalUrlInEveryCultureWithAlternatesAndXDefault()
    {
        Returns();
        var options = new OsirionFeedOptions { SiteUrl = "https://example.com", AdditionalSitemapUrls = ["/", "/contact"] };

        var body = await InvokeAsync(options, localized: true);

        var root = XDocument.Parse(body).Root!;
        root.Attribute(XNamespace.Xmlns + "xhtml")!.Value.ShouldBe(Xhtml.NamespaceName);
        var urls = root.Elements(Sitemap + "url").ToList();
        urls.Select(Location).ShouldBe([
            "https://example.com/",
            "https://example.com/sr",
            "https://example.com/contact",
            "https://example.com/sr/contact",
        ]);
        foreach (var url in urls.Take(2))
        {
            Alternates(url).ShouldBe([
                ("en", "https://example.com/"),
                ("sr-Latn", "https://example.com/sr"),
                ("x-default", "https://example.com/"),
            ]);
        }

        Alternates(urls[3]).ShouldBe([
            ("en", "https://example.com/contact"),
            ("sr-Latn", "https://example.com/sr/contact"),
            ("x-default", "https://example.com/contact"),
        ]);
        urls[3].Element(Xhtml + "link")!.Attribute("rel")!.Value.ShouldBe("alternate");
    }

    [Fact]
    public async Task Sitemap_Localized_TranslationsCarryEachOtherAndAnUntranslatedItemHasNone()
    {
        Returns(
            Item("blog/post", "en", "post"),
            Item("blog/post", "sr", "post"),
            Item("blog/only-english", "en", "only-english"),
            Item("blog/german", "de", "german"));

        var body = await InvokeAsync(new OsirionFeedOptions { SiteUrl = "https://example.com" }, localized: true);

        var urls = XDocument.Parse(body).Root!.Elements(Sitemap + "url").ToList();
        urls.Select(Location).ShouldBe([
            "https://example.com/blog/post",
            "https://example.com/sr/blog/post",
            "https://example.com/blog/only-english",
            "https://example.com/blog/german",
        ]);
        (string, string)[] translations =
        [
            ("en", "https://example.com/blog/post"),
            ("sr-Latn", "https://example.com/sr/blog/post"),
            ("x-default", "https://example.com/blog/post"),
        ];
        Alternates(urls[0]).ShouldBe(translations);
        Alternates(urls[1]).ShouldBe(translations);
        urls[1].Element(Sitemap + "lastmod")!.Value.ShouldBe("2026-09-02T08:30:00Z");
        Alternates(urls[2]).ShouldBeEmpty();
        Alternates(urls[3]).ShouldBeEmpty();
    }

    [Fact]
    public async Task Sitemap_Localized_TranslationWithoutTheDefaultCultureHasNoXDefault()
    {
        Returns(Item("guide", "sr", "guide"), Item("guide", "fr", "guide"));

        var body = await InvokeAsync(new OsirionFeedOptions { SiteUrl = "https://example.com" }, localized: true, withFrench: true);

        var urls = XDocument.Parse(body).Root!.Elements(Sitemap + "url").ToList();
        Alternates(urls[0]).ShouldBe([
            ("sr-Latn", "https://example.com/sr/guide"),
            ("fr", "https://example.com/fr/guide"),
        ]);
    }

    [Fact]
    public async Task Sitemap_Localized_NoIndexTranslationIsNeitherListedNorAnAlternate()
    {
        var hidden = Item("blog/post", "sr", "post");
        hidden.Metadata = new FrontMatter { SeoProperties = new SeoMetadata { Robots = "noindex" } };
        Returns(Item("blog/post", "en", "post"), hidden);

        var body = await InvokeAsync(new OsirionFeedOptions { SiteUrl = "https://example.com" }, localized: true);

        var urls = XDocument.Parse(body).Root!.Elements(Sitemap + "url").ToList();
        urls.Select(Location).ShouldBe(["https://example.com/blog/post"]);
        Alternates(urls[0]).ShouldBeEmpty();
    }

    [Fact]
    public async Task Sitemap_Localized_AbsoluteAdditionalUrlIsListedOnceWithoutAlternates()
    {
        Returns();
        var options = new OsirionFeedOptions { SiteUrl = "https://example.com", AdditionalSitemapUrls = ["https://docs.example.com/start"] };

        var body = await InvokeAsync(options, localized: true);

        var urls = XDocument.Parse(body).Root!.Elements(Sitemap + "url").ToList();
        urls.Select(Location).ShouldBe(["https://docs.example.com/start"]);
        Alternates(urls[0]).ShouldBeEmpty();
    }

    // Guard test: a site without locale routing gets the same document, byte for byte, as before language versions.
    [Fact]
    public async Task Sitemap_WithoutLocalization_IsUnchanged()
    {
        Returns(Item("blog/post", "en", "post"), Item("blog/post", "sr", "post"));
        var options = new OsirionFeedOptions { SiteUrl = "https://example.com", AdditionalSitemapUrls = ["/", "/contact"] };

        var body = await InvokeAsync(options, localized: false);

        var newLine = Environment.NewLine;
        body.ShouldBe(string.Join(newLine,
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>",
            "<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">",
            "  <url>",
            "    <loc>https://example.com/</loc>",
            "  </url>",
            "  <url>",
            "    <loc>https://example.com/contact</loc>",
            "  </url>",
            "  <url>",
            "    <loc>https://example.com/blog/post</loc>",
            "    <lastmod>2026-09-02T08:30:00Z</lastmod>",
            "  </url>",
            "</urlset>"));
    }

    private void Returns(params ContentItem[] items) =>
        _manager.GetContentByQueryAsync(Arg.Any<ContentQuery>(), Arg.Any<CancellationToken>()).Returns(items.ToList());

    private async Task<string> InvokeAsync(OsirionFeedOptions options, bool localized, bool withFrench = false)
    {
        var services = new ServiceCollection();
        services.AddSingleton(_manager);
        if (localized)
        {
            services.AddOsirionLocalization(localization =>
            {
                localization.DefaultCulture = "en";
                localization.Cultures.Add(new LocalizationCulture { Name = "sr-Latn", Prefix = "sr" });
                if (withFrench)
                {
                    localization.Cultures.Add(new LocalizationCulture { Name = "fr" });
                }
            });
        }

        var context = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("request.example");
        var body = new MemoryStream();
        context.Response.Body = body;

        await ContentFeedEndpoints.HandleAsync(context, ContentFeedKind.Sitemap, options);

        return System.Text.Encoding.UTF8.GetString(body.ToArray());
    }

    private static string Location(XElement url) => url.Element(Sitemap + "loc")!.Value;

    private static List<(string HrefLang, string Href)> Alternates(XElement url) =>
        url.Elements(Xhtml + "link").Select(link => (link.Attribute("hreflang")!.Value, link.Attribute("href")!.Value)).ToList();

    private static ContentItem Item(string url, string locale, string localizationId)
    {
        var item = ContentItem.Create($"{locale}/{url}", $"Title {url}", "<p>Body</p>", $"{locale}/{url}.md", "test");
        item.Url = url;
        item.DateCreated = new DateTime(2026, 9, 2, 8, 30, 0, DateTimeKind.Utc);
        item.SetLocale(locale);
        item.SetContentId(localizationId);
        item.LastModified = null;
        return item;
    }
}
