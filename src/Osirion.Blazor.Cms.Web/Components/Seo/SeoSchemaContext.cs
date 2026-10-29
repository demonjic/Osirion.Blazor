using Osirion.Blazor.Cms.Domain.Entities;

namespace Osirion.Blazor.Cms.Web.Components.Seo;

/// <summary>
/// The page state the schema.org structured data is built from, resolved once by
/// <see cref="SeoMetadataRenderer"/> after its meta tags so the structured data agrees with them.
/// </summary>
internal sealed record SeoSchemaContext
{
    /// <summary>The content of a single content page, or <see langword="null"/> on a collection page.</summary>
    public ContentItem? Content { get; init; }

    /// <summary>The items of a collection page.</summary>
    public IReadOnlyList<ContentItem>? ContentItems { get; init; }

    public required string SiteName { get; init; }

    /// <summary>The site origin without a trailing slash.</summary>
    public required string BaseUrl { get; init; }

    /// <summary>The canonical URL of the page.</summary>
    public required string CurrentUrl { get; init; }

    public string? LanguageCode { get; init; }

    public string? SiteLogoUrl { get; init; }

    public required string BreadcrumbHomeText { get; init; }

    public IReadOnlyList<(string Name, string Url)>? CustomBreadcrumbs { get; init; }

    public string? CollectionTitle { get; init; }

    public string? CollectionDescription { get; init; }

    public required string MetaTitle { get; init; }

    public string? MetaDescription { get; init; }

    /// <summary>
    /// Whether the site's own structured data is on, so a page names the site's <c>Organization</c> and
    /// <c>WebSite</c> by <c>@id</c> instead of repeating them.
    /// </summary>
    public bool LinkToSite { get; init; }

    /// <summary>The title of a page that is not a content item, without the site name; null on every other page.</summary>
    public string? PageTitle { get; init; }

    /// <summary>The description of a page that is not a content item; null on every other page.</summary>
    public string? PageDescription { get; init; }

    /// <summary>The resolved Open Graph image, the fallback image of collection items.</summary>
    public string? OgImage { get; init; }

    /// <summary>The absolute address of a page that identifies the author, or <see langword="null"/>.</summary>
    public string? AuthorUrl { get; init; }

    /// <summary>The <c>applicationCategory</c> of the SoftwareApplication schema.</summary>
    public string ApplicationCategory { get; init; } = "DeveloperApplication";

    /// <summary>The publisher logo URL, <c>/logo.png</c> on the site when no logo is configured.</summary>
    public string PublisherLogoUrl => NormalizeImageUrl(SiteLogoUrl) ?? $"{BaseUrl}/logo.png";

    /// <summary>Makes a relative image URL absolute against <see cref="BaseUrl"/>.</summary>
    public string? NormalizeImageUrl(string? url) => SeoText.NormalizeImageUrl(url, BaseUrl);
}
