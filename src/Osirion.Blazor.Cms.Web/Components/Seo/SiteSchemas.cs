namespace Osirion.Blazor.Cms.Web.Components.Seo;

/// <summary>
/// The site's own structured data (<c>SeoMetadataOptions.EnableSiteSchemas</c>): a <c>WebSite</c> and an
/// <c>Organization</c>, each with an <c>@id</c> derived from the site address, written once on the home page, and the
/// references other pages make to them by that <c>@id</c>.
/// </summary>
internal static class SiteSchemas
{
    private const string SchemaContext = "https://schema.org";

    private const string SearchTermPlaceholder = "{search_term_string}";

    /// <summary>The types that describe a web page, which name the web site they belong to as <c>isPartOf</c>.</summary>
    private static readonly HashSet<string> WebPageTypes = new(StringComparer.Ordinal)
    {
        "WebPage", "ContactPage", "AboutPage", "CollectionPage", "ProfilePage"
    };

    /// <summary>The <c>@id</c> of the site's <c>WebSite</c>: the site address followed by <c>/#website</c>.</summary>
    internal static string WebSiteId(string baseUrl) => baseUrl + "/#website";

    /// <summary>The <c>@id</c> of the site's <c>Organization</c>: the site address followed by <c>/#organization</c>.</summary>
    internal static string OrganizationId(string baseUrl) => baseUrl + "/#organization";

    /// <summary>
    /// The site as a <c>WebSite</c>, published by the site's organization, with a <c>SearchAction</c> when
    /// <paramref name="searchUrlTemplate"/> holds <c>{search_term_string}</c>.
    /// </summary>
    internal static Dictionary<string, object?> WebSite(SeoSchemaContext context, string? description, string? searchUrlTemplate)
    {
        var schema = new Dictionary<string, object?>
        {
            ["@context"] = SchemaContext,
            ["@type"] = "WebSite",
            ["@id"] = WebSiteId(context.BaseUrl),
            ["url"] = context.BaseUrl + "/",
            ["name"] = context.SiteName,
            ["description"] = string.IsNullOrWhiteSpace(description) ? null : description,
            ["inLanguage"] = context.LanguageCode,
            ["publisher"] = Reference(OrganizationId(context.BaseUrl))
        };

        if (!string.IsNullOrWhiteSpace(searchUrlTemplate)
            && searchUrlTemplate.Contains(SearchTermPlaceholder, StringComparison.Ordinal))
        {
            schema["potentialAction"] = new Dictionary<string, object?>
            {
                ["@type"] = "SearchAction",
                ["target"] = new Dictionary<string, object?>
                {
                    ["@type"] = "EntryPoint",
                    ["urlTemplate"] = context.NormalizeImageUrl(searchUrlTemplate)
                },
                ["query-input"] = "required name=search_term_string"
            };
        }

        return schema;
    }

    /// <summary>
    /// The site's <c>Organization</c>: its name, the site address, the logo and the absolute http(s) profiles of
    /// <paramref name="sameAs"/>.
    /// </summary>
    internal static Dictionary<string, object?> Organization(SeoSchemaContext context, string name, IEnumerable<string> sameAs)
    {
        var profiles = sameAs
            .Where(profile => Uri.TryCreate(profile, UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return new Dictionary<string, object?>
        {
            ["@context"] = SchemaContext,
            ["@type"] = "Organization",
            ["@id"] = OrganizationId(context.BaseUrl),
            ["name"] = name,
            ["url"] = context.BaseUrl + "/",
            ["logo"] = new Dictionary<string, object?>
            {
                ["@type"] = "ImageObject",
                ["url"] = context.PublisherLogoUrl
            },
            ["sameAs"] = profiles.Length > 0 ? profiles : null
        };
    }

    /// <summary>
    /// Makes a page's schema name the site's nodes instead of repeating them: its <c>publisher</c> becomes a reference
    /// to the organization, and a web page type gets <c>isPartOf</c> the web site. A schema that is not a property
    /// map is returned as it is.
    /// </summary>
    internal static object Link(object schema, SeoSchemaContext context)
    {
        if (schema is not Dictionary<string, object?> properties)
        {
            return schema;
        }

        if (properties.ContainsKey("publisher"))
        {
            properties["publisher"] = Reference(OrganizationId(context.BaseUrl));
        }

        if (properties.TryGetValue("@type", out var type) && type is string typeName && WebPageTypes.Contains(typeName))
        {
            properties["isPartOf"] = Reference(WebSiteId(context.BaseUrl));
        }

        return properties;
    }

    private static Dictionary<string, object?> Reference(string id) => new() { ["@id"] = id };
}
