using Osirion.Blazor.Cms.Domain.Common;

namespace Osirion.Blazor.Cms.Domain.ValueObjects;

/// <summary>
/// Represents SEO metadata for content items
/// </summary>
public class SeoMetadata : ValueObject
{
    /// <summary>
    /// Gets the meta title (SEO title)
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets the meta description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Open Graph image for social sharing (recommended: 1200x630).
    /// </summary>
    public string? Image { get; set; }

    /// <summary>
    /// Gets the canonical URL
    /// </summary>
    public string Canonical { get; set; } = string.Empty;

    /// <summary>
    /// Language code (e.g., "en-US").
    /// </summary>
    public string? Lang { get; set; }

    /// <summary>
    /// Gets the robots meta directive. Empty by default, so the renderer writes its own value.
    /// </summary>
    public string Robots { get; set; } = string.Empty;

    /// <summary>
    /// Gets the Open Graph title
    /// </summary>
    public string OgTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets the Open Graph description
    /// </summary>
    public string OgDescription { get; set; } = string.Empty;

    /// <summary>
    /// Gets the Open Graph image URL
    /// </summary>
    public string OgImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets the Open Graph type. Empty by default, so the renderer computes it from the schema type.
    /// </summary>
    public string OgType { get; set; } = string.Empty;

    /// <summary>
    /// Gets the JSON-LD structured data
    /// </summary>
    public string JsonLd { get; set; } = string.Empty;

    /// <summary>
    /// Gets the schema.org type from the front matter <c>type</c>. Empty by default, so the renderer uses the page's
    /// schema types, then its fallback type, then <c>WebPage</c>.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets the Twitter card type
    /// </summary>
    public string TwitterCard { get; set; } = "summary_large_image";

    /// <summary>
    /// Gets the Twitter title
    /// </summary>
    public string TwitterTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets the Twitter description
    /// </summary>
    public string TwitterDescription { get; set; } = string.Empty;

    /// <summary>
    /// Gets the Twitter image URL
    /// </summary>
    public string TwitterImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Additional meta tags.
    /// </summary>
    public Dictionary<string, string> MetaTags { get; set; } = new Dictionary<string, string>();

    /// <summary>Creates populated SEO metadata.</summary>
    public static SeoMetadata Create(
        string metaTitle,
        string metaDescription,
        string? canonicalUrl = null,
        string? robots = null,
        string? ogTitle = null,
        string? ogDescription = null,
        string? ogImageUrl = null,
        string? ogType = null,
        string? jsonLd = null,
        string? schemaType = null,
        string? twitterCard = null,
        string? twitterTitle = null,
        string? twitterDescription = null,
        string? twitterImageUrl = null)
    {
        var metadata = new SeoMetadata
        {
            Title = metaTitle,
            Description = metaDescription,
            Canonical = canonicalUrl ?? string.Empty,
            Robots = robots ?? string.Empty,
            OgTitle = ogTitle ?? metaTitle,
            OgDescription = ogDescription ?? metaDescription,
            OgImageUrl = ogImageUrl ?? string.Empty,
            OgType = ogType ?? string.Empty,
            JsonLd = jsonLd ?? string.Empty,
            Type = schemaType ?? string.Empty,
            TwitterCard = twitterCard ?? "summary_large_image",
            TwitterTitle = twitterTitle ?? metaTitle,
            TwitterDescription = twitterDescription ?? metaDescription,
            TwitterImageUrl = twitterImageUrl ?? ogImageUrl ?? string.Empty
        };

        return metadata;
    }

    /// <summary>Creates a copy with a different meta title.</summary>
    public SeoMetadata WithMetaTitle(string metaTitle)
    {
        var clone = Clone();
        clone.Title = metaTitle;
        return clone;
    }

    /// <summary>Creates a copy with a different meta description.</summary>
    public SeoMetadata WithMetaDescription(string metaDescription)
    {
        var clone = Clone();
        clone.Description = metaDescription;
        return clone;
    }

    /// <summary>Creates a copy with a different canonical URL.</summary>
    public SeoMetadata WithCanonicalUrl(string canonicalUrl)
    {
        var clone = Clone();
        clone.Canonical = canonicalUrl;
        return clone;
    }

    /// <summary>Creates a copy with a different robots directive.</summary>
    public SeoMetadata WithRobots(string robots)
    {
        var clone = Clone();
        clone.Robots = robots;
        return clone;
    }

    /// <summary>Creates a copy with Open Graph metadata.</summary>
    public SeoMetadata WithOpenGraph(string title, string description, string imageUrl, string type = "article")
    {
        var clone = Clone();
        clone.OgTitle = title;
        clone.OgDescription = description;
        clone.OgImageUrl = imageUrl;
        clone.OgType = type;
        return clone;
    }

    /// <summary>Creates a copy with Twitter card metadata.</summary>
    public SeoMetadata WithTwitterCard(string title, string description, string imageUrl, string cardType = "summary_large_image")
    {
        var clone = Clone();
        clone.TwitterTitle = title;
        clone.TwitterDescription = description;
        clone.TwitterImageUrl = imageUrl;
        clone.TwitterCard = cardType;
        return clone;
    }

    /// <summary>Creates a copy with JSON-LD metadata.</summary>
    public SeoMetadata WithJsonLd(string jsonLd, string schemaType = "Article")
    {
        var clone = Clone();
        clone.JsonLd = jsonLd;
        clone.Type = schemaType;
        return clone;
    }

    /// <summary>
    /// Creates a deep clone of this SEO metadata
    /// </summary>
    public SeoMetadata Clone()
    {
        return new SeoMetadata
        {
            Title = Title,
            Description = Description,
            Image = Image,
            Canonical = Canonical,
            Lang = Lang,
            Robots = Robots,
            OgTitle = OgTitle,
            OgDescription = OgDescription,
            OgImageUrl = OgImageUrl,
            OgType = OgType,
            JsonLd = JsonLd,
            Type = Type,
            TwitterCard = TwitterCard,
            TwitterTitle = TwitterTitle,
            TwitterDescription = TwitterDescription,
            TwitterImageUrl = TwitterImageUrl,
            MetaTags = new Dictionary<string, string>(MetaTags)
        };
    }

    /// <summary>Gets or sets the GetEqualityComponents value.</summary>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Title;
        yield return Description;
        yield return Canonical;
        yield return Robots;
        yield return OgTitle;
        yield return OgDescription;
        yield return OgImageUrl;
        yield return OgType;
        yield return JsonLd;
        yield return Type;
        yield return TwitterCard;
        yield return TwitterTitle;
        yield return TwitterDescription;
        yield return TwitterImageUrl;
    }
}
