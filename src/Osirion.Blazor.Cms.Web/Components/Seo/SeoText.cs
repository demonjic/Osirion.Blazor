namespace Osirion.Blazor.Cms.Web.Components.Seo;

/// <summary>
/// Text helpers shared by the SEO meta tags and the structured data: truncation to the lengths search engines
/// display, URL normalization and name formatting.
/// </summary>
internal static class SeoText
{
    /// <summary>
    /// A period closer than this to the cut point ends the truncated description, so it keeps a whole sentence.
    /// </summary>
    private const int SentenceEndWindow = 50;

    private const string Ellipsis = "...";

    private const int MaxMetaNameLength = 64;

    /// <summary>
    /// Whether <paramref name="name"/> can be written unencoded as the <c>name</c> attribute of a meta element: a letter,
    /// then letters, digits, hyphens, underscores, periods and colons, at most 64 characters. Front matter is
    /// content, so a name with a quote, an angle bracket or white space could otherwise change the markup around it.
    /// </summary>
    internal static bool IsSafeMetaName(string? name)
    {
        if (string.IsNullOrEmpty(name) || name.Length > MaxMetaNameLength || !char.IsAsciiLetter(name[0]))
        {
            return false;
        }

        return name.All(character => char.IsAsciiLetterOrDigit(character) || character is '-' or '_' or '.' or ':');
    }

    /// <summary>Returns the first value that is not null, empty or white space.</summary>
    internal static string? FirstNonBlank(params string?[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

    /// <summary>Shortens a title to the last whole word before <paramref name="maxLength"/> and appends an ellipsis.</summary>
    internal static string? TruncateTitle(string? title, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Length <= maxLength)
        {
            return title;
        }

        var truncated = title[..maxLength].TrimEnd();
        var lastSpace = truncated.LastIndexOf(' ');

        return lastSpace > 0 ? truncated[..lastSpace] + Ellipsis : truncated + Ellipsis;
    }

    /// <summary>
    /// Shortens a description to <paramref name="maxLength"/>, ending at a sentence when one ends near the cut,
    /// otherwise at the last whole word followed by an ellipsis.
    /// </summary>
    internal static string? TruncateDescription(string? description, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(description) || description.Length <= maxLength)
        {
            return description;
        }

        var truncated = description[..maxLength].TrimEnd();
        var lastPeriod = truncated.LastIndexOf('.');
        if (lastPeriod > maxLength - SentenceEndWindow)
        {
            return truncated[..(lastPeriod + 1)];
        }

        var lastSpace = truncated.LastIndexOf(' ');
        return lastSpace > 0 ? truncated[..lastSpace] + Ellipsis : truncated + Ellipsis;
    }

    /// <summary>Makes a relative image URL absolute against <paramref name="baseUrl"/>; absolute URLs pass through.</summary>
    internal static string? NormalizeImageUrl(string? url, string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return url;
        }

        return $"{baseUrl}/{url.TrimStart('/')}";
    }

    /// <summary>Returns the host of <paramref name="url"/> without a leading <c>www.</c>.</summary>
    internal static string ExtractSiteName(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return string.Empty;
        }

        var host = new Uri(url).Host;
        return host.StartsWith("www.", StringComparison.OrdinalIgnoreCase) ? host[4..] : host;
    }

    /// <summary>
    /// Converts a BCP 47 language tag to the Open Graph form, language and territory: <c>en-US</c> to <c>en_US</c>,
    /// <c>sr-Latn-RS</c> to <c>sr_RS</c>. Open Graph has no place for a script subtag, so it is left out; a tag without a
    /// territory stays the bare language. Without a tag the locale is <c>en_US</c>.
    /// </summary>
    internal static string ToOpenGraphLocale(string? locale)
    {
        if (string.IsNullOrWhiteSpace(locale))
        {
            return "en_US";
        }

        var subtags = locale.Trim().Replace('_', '-').Split('-', StringSplitOptions.RemoveEmptyEntries);
        var language = subtags[0].ToLowerInvariant();
        var territory = subtags.Skip(1).FirstOrDefault(IsTerritory);
        return territory is null ? language : $"{language}_{territory.ToUpperInvariant()}";
    }

    // A region subtag: two letters (RS) or three digits (419); a script subtag (Latn) has four letters.
    private static bool IsTerritory(string subtag) =>
        (subtag.Length == 2 && subtag.All(char.IsAsciiLetter)) || (subtag.Length == 3 && subtag.All(char.IsAsciiDigit));

    /// <summary>Formats a slug-case path segment as Title Case words.</summary>
    internal static string FormatBreadcrumbSegment(string segmentName)
    {
        if (string.IsNullOrWhiteSpace(segmentName))
        {
            return string.Empty;
        }

        var words = segmentName
            .Replace('-', ' ')
            .Replace('_', ' ')
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return string.Join(' ', words.Select(word => char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant()));
    }
}
