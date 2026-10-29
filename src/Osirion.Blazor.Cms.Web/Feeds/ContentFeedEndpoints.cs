using System.Globalization;
using System.Text;
using System.Xml;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Enums;
using Osirion.Blazor.Cms.Domain.Repositories;
using Osirion.Blazor.Cms.Domain.Services;
using Osirion.Blazor.Cms.Web.Components;
using Osirion.Blazor.Cms.Web.Components.Seo;
using Osirion.Blazor.Cms.Web.Options;
using Osirion.Blazor.Core.Localization;
using Osirion.Blazor.Core.Utils;

namespace Osirion.Blazor.Cms.Web.Feeds;

/// <summary>
/// Maps <c>/sitemap.xml</c>, an RSS 2.0 feed at <c>/feed.xml</c> and an Atom feed at <c>/atom.xml</c>, built from
/// the published content of the registered content providers.
/// </summary>
public static class ContentFeedEndpoints
{
    /// <summary>The path of the XML sitemap.</summary>
    public const string SitemapPath = "/sitemap.xml";

    /// <summary>The path of the RSS 2.0 feed.</summary>
    public const string RssPath = "/feed.xml";

    /// <summary>The path of the Atom feed.</summary>
    public const string AtomPath = "/atom.xml";

    // sitemaps.org caps one sitemap file at 50,000 URLs.
    private const int SitemapUrlLimit = 50_000;

    private const string SitemapNamespace = "http://www.sitemaps.org/schemas/sitemap/0.9";
    private const string AtomNamespace = "http://www.w3.org/2005/Atom";
    private const string XhtmlNamespace = "http://www.w3.org/1999/xhtml";

    private static readonly XmlWriterSettings WriterSettings = new()
    {
        Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
        Indent = true,
    };

    /// <summary>
    /// Maps the sitemap and feed endpoints that <paramref name="configure"/> leaves enabled.
    /// </summary>
    /// <param name="endpoints">The application's endpoint route builder.</param>
    /// <param name="configure">Configures the site address, feed title and which content the feeds list.</param>
    /// <returns>The same <paramref name="endpoints"/>, for chaining.</returns>
    /// <exception cref="ArgumentException">The configured options are not valid; see <see cref="OsirionFeedOptions.Validate"/>.</exception>
    /// <example>
    /// <code>
    /// app.MapOsirionFeeds(feeds =>
    /// {
    ///     feeds.SiteUrl = "https://example.com";
    ///     feeds.FeedDirectory = "blog";
    /// });
    /// </code>
    /// </example>
    public static IEndpointRouteBuilder MapOsirionFeeds(this IEndpointRouteBuilder endpoints, Action<OsirionFeedOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var options = new OsirionFeedOptions();
        configure?.Invoke(options);
        options.Validate();

        if (options.EnableSitemap)
        {
            endpoints.MapGet(SitemapPath, new RequestDelegate(context => HandleAsync(context, ContentFeedKind.Sitemap, options)));
        }

        if (options.EnableRss)
        {
            endpoints.MapGet(RssPath, new RequestDelegate(context => HandleAsync(context, ContentFeedKind.Rss, options)));
        }

        if (options.EnableAtom)
        {
            endpoints.MapGet(AtomPath, new RequestDelegate(context => HandleAsync(context, ContentFeedKind.Atom, options)));
        }

        return endpoints;
    }

    /// <summary>Writes the requested document to the response.</summary>
    internal static async Task HandleAsync(HttpContext context, ContentFeedKind kind, OsirionFeedOptions options)
    {
        var services = context.RequestServices;
        var manager = services.GetRequiredService<IContentProviderManager>();
        var seo = services.GetService<IOptions<SeoMetadataOptions>>()?.Value;
        var feed = new FeedContext(
            ResolveSiteUrl(context.Request, options, seo),
            options,
            seo,
            context.Request.Host.Host);

        var query = kind == ContentFeedKind.Sitemap
            ? new ContentQuery { Take = SitemapUrlLimit, IncludeUnpublished = false }
            : new ContentQuery
            {
                Directory = options.FeedDirectory,
                Locale = options.FeedLocale,
                Take = options.FeedItemCount,
                SortBy = SortField.Date,
                SortDirection = SortDirection.Descending,
                IncludeUnpublished = false,
            };
        var items = await manager.GetContentByQueryAsync(query, context.RequestAborted).ConfigureAwait(false);

        using var buffer = new MemoryStream();
        using (var writer = XmlWriter.Create(buffer, WriterSettings))
        {
            switch (kind)
            {
                case ContentFeedKind.Sitemap:
                    WriteSitemap(writer, feed, items, services.GetService<IOsirionLocalePaths>());
                    break;
                case ContentFeedKind.Rss:
                    WriteRss(writer, feed, Newest(items, options.FeedItemCount));
                    break;
                default:
                    WriteAtom(writer, feed, Newest(items, options.FeedItemCount), services.GetService<TimeProvider>() ?? TimeProvider.System);
                    break;
            }
        }

        var response = context.Response;
        response.ContentType = kind switch
        {
            ContentFeedKind.Sitemap => "application/xml; charset=utf-8",
            ContentFeedKind.Rss => "application/rss+xml; charset=utf-8",
            _ => "application/atom+xml; charset=utf-8",
        };
        response.Headers.CacheControl = $"public, max-age={(long)options.CacheDuration.TotalSeconds}";
        response.ContentLength = buffer.Length;
        await response.Body.WriteAsync(buffer.GetBuffer().AsMemory(0, (int)buffer.Length), context.RequestAborted).ConfigureAwait(false);
    }

    private static void WriteSitemap(
        XmlWriter writer, FeedContext feed, IReadOnlyList<ContentItem> items, IOsirionLocalePaths? localePaths)
    {
        writer.WriteStartDocument();
        writer.WriteStartElement("urlset", SitemapNamespace);
        if (localePaths is not null)
        {
            writer.WriteAttributeString("xmlns", "xhtml", null, XhtmlNamespace);
        }

        var written = new HashSet<string>(StringComparer.Ordinal);
        foreach (var path in feed.Options.AdditionalSitemapUrls)
        {
            if (localePaths is null || !LocalUrl.IsLocalPath(path))
            {
                WriteSitemapUrl(writer, written, feed.Absolute(path), lastModified: null, alternates: []);
                continue;
            }

            // A page that is not content exists in every language, at the address Localize gives it.
            var alternates = LanguageVersions.ForPage(localePaths, path, feed.Absolute);
            foreach (var culture in localePaths.Cultures)
            {
                var location = feed.Absolute(localePaths.Localize(path, culture.Culture));
                WriteSitemapUrl(writer, written, location, lastModified: null, alternates);
            }
        }

        var languageVersions = localePaths is null ? null : new ContentLanguageVersions(localePaths, feed, items);
        foreach (var item in items)
        {
            if (LanguageVersions.IsNoIndex(item))
            {
                continue;
            }

            var location = languageVersions?.Location(item) ?? feed.ItemUrl(item);
            WriteSitemapUrl(writer, written, location, item.LastModified ?? item.DateCreated, languageVersions?.Alternates(item) ?? []);
        }

        writer.WriteEndElement();
        writer.WriteEndDocument();
    }

    // Writes one url element, unless an earlier one has the same location. The alternates follow lastmod, where the
    // sitemap schema allows elements of other namespaces.
    private static void WriteSitemapUrl(
        XmlWriter writer,
        HashSet<string> written,
        string location,
        DateTime? lastModified,
        IReadOnlyList<(string HrefLang, string Href)> alternates)
    {
        if (!written.Add(location))
        {
            return;
        }

        writer.WriteStartElement("url", SitemapNamespace);
        writer.WriteElementString("loc", SitemapNamespace, location);
        if (lastModified is { } modified)
        {
            writer.WriteElementString("lastmod", SitemapNamespace, ContentUrls.FormatUtc(modified));
        }

        foreach (var (hrefLang, href) in alternates)
        {
            writer.WriteStartElement("xhtml", "link", XhtmlNamespace);
            writer.WriteAttributeString("rel", "alternate");
            writer.WriteAttributeString("hreflang", hrefLang);
            writer.WriteAttributeString("href", href);
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }

    private static void WriteRss(XmlWriter writer, FeedContext feed, IReadOnlyList<ContentItem> items)
    {
        writer.WriteStartDocument();
        writer.WriteStartElement("rss");
        writer.WriteAttributeString("version", "2.0");
        writer.WriteAttributeString("xmlns", "atom", null, AtomNamespace);
        writer.WriteStartElement("channel");
        writer.WriteElementString("title", Clean(feed.Title));
        writer.WriteElementString("link", feed.Absolute("/"));
        writer.WriteElementString("description", Clean(feed.Description));
        if (!string.IsNullOrWhiteSpace(feed.Options.FeedLocale))
        {
            writer.WriteElementString("language", Clean(feed.Options.FeedLocale));
        }

        // The self link lets feed readers and validators find the canonical feed address.
        writer.WriteStartElement("atom", "link", AtomNamespace);
        writer.WriteAttributeString("href", feed.Absolute(RssPath));
        writer.WriteAttributeString("rel", "self");
        writer.WriteAttributeString("type", "application/rss+xml");
        writer.WriteEndElement();

        foreach (var item in items)
        {
            var link = feed.ItemUrl(item);
            writer.WriteStartElement("item");
            writer.WriteElementString("title", Clean(item.Title));
            writer.WriteElementString("link", link);
            writer.WriteStartElement("guid");
            writer.WriteAttributeString("isPermaLink", "true");
            writer.WriteString(link);
            writer.WriteEndElement();
            writer.WriteElementString("pubDate", ToUtc(item.DateCreated).ToString("r", CultureInfo.InvariantCulture));
            if (!string.IsNullOrWhiteSpace(item.Description))
            {
                writer.WriteElementString("description", Clean(item.Description));
            }

            writer.WriteEndElement();
        }

        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndDocument();
    }

    private static void WriteAtom(XmlWriter writer, FeedContext feed, IReadOnlyList<ContentItem> items, TimeProvider timeProvider)
    {
        // Atom requires an updated date even for an empty feed; the current time is the honest value then.
        var updated = items.Count > 0
            ? items.Max(item => ToUtc(item.LastModified ?? item.DateCreated))
            : timeProvider.GetUtcNow().UtcDateTime;

        writer.WriteStartDocument();
        writer.WriteStartElement("feed", AtomNamespace);
        writer.WriteElementString("id", AtomNamespace, feed.Absolute("/"));
        writer.WriteElementString("title", AtomNamespace, Clean(feed.Title));
        writer.WriteElementString("subtitle", AtomNamespace, Clean(feed.Description));
        writer.WriteElementString("updated", AtomNamespace, ContentUrls.FormatUtc(updated));
        WriteAtomLink(writer, feed.Absolute(AtomPath), "self");
        WriteAtomLink(writer, feed.Absolute("/"), "alternate");

        // A feed-level author satisfies Atom's rule that every entry has one, for entries without their own.
        writer.WriteStartElement("author", AtomNamespace);
        writer.WriteElementString("name", AtomNamespace, Clean(feed.Title));
        writer.WriteEndElement();

        foreach (var item in items)
        {
            var link = feed.ItemUrl(item);
            writer.WriteStartElement("entry", AtomNamespace);
            writer.WriteElementString("id", AtomNamespace, link);
            writer.WriteElementString("title", AtomNamespace, Clean(item.Title));
            WriteAtomLink(writer, link, "alternate");
            writer.WriteElementString("published", AtomNamespace, ContentUrls.FormatUtc(item.DateCreated));
            writer.WriteElementString("updated", AtomNamespace, ContentUrls.FormatUtc(item.LastModified ?? item.DateCreated));
            if (!string.IsNullOrWhiteSpace(item.Author))
            {
                writer.WriteStartElement("author", AtomNamespace);
                writer.WriteElementString("name", AtomNamespace, Clean(item.Author));
                writer.WriteEndElement();
            }

            if (!string.IsNullOrWhiteSpace(item.Description))
            {
                writer.WriteElementString("summary", AtomNamespace, Clean(item.Description));
            }

            writer.WriteEndElement();
        }

        writer.WriteEndElement();
        writer.WriteEndDocument();
    }

    private static void WriteAtomLink(XmlWriter writer, string href, string relation)
    {
        writer.WriteStartElement("link", AtomNamespace);
        writer.WriteAttributeString("href", href);
        writer.WriteAttributeString("rel", relation);
        writer.WriteEndElement();
    }

    // Providers are asked for the newest items, but a provider may ignore sorting or Take; the feed contract does not.
    private static ContentItem[] Newest(IReadOnlyList<ContentItem> items, int count) =>
        items.OrderByDescending(item => ToUtc(item.DateCreated)).Take(count).ToArray();

    // The feed's own SiteUrl, then the site-wide Osirion:Seo:SiteUrl, then the request. The SEO address is used only
    // when it is an absolute http(s) address, the same rule the SEO metadata renderer applies.
    private static string ResolveSiteUrl(HttpRequest request, OsirionFeedOptions options, SeoMetadataOptions? seo)
    {
        if (!string.IsNullOrWhiteSpace(options.SiteUrl))
        {
            return options.SiteUrl.TrimEnd('/');
        }

        if (Uri.TryCreate(seo?.SiteUrl, UriKind.Absolute, out var seoSiteUrl)
            && (seoSiteUrl.Scheme == Uri.UriSchemeHttp || seoSiteUrl.Scheme == Uri.UriSchemeHttps))
        {
            return seoSiteUrl.GetLeftPart(UriPartial.Path).TrimEnd('/');
        }

        return $"{request.Scheme}://{request.Host.ToUriComponent()}{request.PathBase.ToUriComponent()}";
    }

    private static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Local => value.ToUniversalTime(),
        DateTimeKind.Utc => value,
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
    };

    // XmlWriter refuses characters XML cannot represent (most control characters, lone surrogates); content that
    // carries one should lose that character, not take the whole feed down.
    private static string Clean(string value)
    {
        StringBuilder? cleaned = null;
        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (XmlConvert.IsXmlChar(character))
            {
                cleaned?.Append(character);
            }
            else if (index + 1 < value.Length && XmlConvert.IsXmlSurrogatePair(value[index + 1], character))
            {
                cleaned?.Append(character).Append(value[index + 1]);
                index++;
            }
            else
            {
                cleaned ??= new StringBuilder(value, 0, index, value.Length);
            }
        }

        return cleaned?.ToString() ?? value;
    }

    /// <summary>
    /// The addresses of the sitemap's items on a site with language versions: each item in its own culture, and the
    /// indexable translations (items with the same <see cref="ContentItem.ContentId"/>) as its alternates, the first
    /// item of each culture winning as the content providers keep it.
    /// </summary>
    private sealed class ContentLanguageVersions
    {
        private readonly IOsirionLocalePaths _localePaths;
        private readonly FeedContext _feed;
        private readonly Dictionary<string, Dictionary<OsirionLocaleCulture, string>> _byLocalizationId = new(StringComparer.Ordinal);

        public ContentLanguageVersions(IOsirionLocalePaths localePaths, FeedContext feed, IReadOnlyList<ContentItem> items)
        {
            _localePaths = localePaths;
            _feed = feed;
            foreach (var item in items)
            {
                if (LanguageVersions.IsNoIndex(item)
                    || string.IsNullOrWhiteSpace(item.ContentId)
                    || LanguageVersions.CultureOf(localePaths, item.Locale) is not { } culture)
                {
                    continue;
                }

                if (!_byLocalizationId.TryGetValue(item.ContentId, out var versions))
                {
                    versions = [];
                    _byLocalizationId[item.ContentId] = versions;
                }

                versions.TryAdd(culture, Location(item)!);
            }
        }

        /// <summary>The item's address in its culture, or <see langword="null"/> when the site has no such culture.</summary>
        public string? Location(ContentItem item) =>
            LanguageVersions.CultureOf(_localePaths, item.Locale) is { } culture
                ? _feed.Absolute(_localePaths.Localize(ContentUrls.ItemUrl(item, _feed.Options.ContentUrlFormatter), culture.Culture))
                : null;

        public List<(string HrefLang, string Href)> Alternates(ContentItem item) =>
            !string.IsNullOrWhiteSpace(item.ContentId) && _byLocalizationId.TryGetValue(item.ContentId, out var versions)
                ? LanguageVersions.Ordered(_localePaths, versions)
                : [];
    }

    private sealed record FeedContext(string SiteUrl, OsirionFeedOptions Options, SeoMetadataOptions? Seo, string Host)
    {
        public string Title => FirstNonBlank(Options.Title, Seo?.SiteName, Host);

        public string Description => FirstNonBlank(Options.Description, Seo?.SiteDescription, Title);

        public string ItemUrl(ContentItem item) =>
            Absolute(Options.ContentUrlFormatter?.Invoke(item) ?? item.Url);

        public string Absolute(string path)
        {
            if (Uri.TryCreate(path, UriKind.Absolute, out var absolute)
                && (absolute.Scheme == Uri.UriSchemeHttp || absolute.Scheme == Uri.UriSchemeHttps))
            {
                return absolute.AbsoluteUri;
            }

            var combined = $"{SiteUrl}/{path.TrimStart('/')}";

            // Uri escapes spaces and non-ASCII characters, which sitemaps and feed readers expect percent-encoded.
            return Uri.TryCreate(combined, UriKind.Absolute, out var uri) ? uri.AbsoluteUri : combined;
        }

        private static string FirstNonBlank(params string?[] values) =>
            values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
    }
}
