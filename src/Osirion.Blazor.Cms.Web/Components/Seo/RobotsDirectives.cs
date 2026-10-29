using System.Globalization;

namespace Osirion.Blazor.Cms.Web.Components.Seo;

/// <summary>
/// Reads a <c>robots</c> meta value written by a page: a comma-separated list of the directives search engines
/// document for the robots meta tag, such as <c>noindex, follow</c> or <c>max-snippet:50</c>.
/// </summary>
internal static class RobotsDirectives
{
    /// <summary>The directives that take no value.</summary>
    private static readonly HashSet<string> Flags = new(StringComparer.OrdinalIgnoreCase)
    {
        "all", "none", "index", "noindex", "follow", "nofollow", "noarchive", "nocache", "nosnippet", "notranslate",
        "noimageindex", "indexifembedded"
    };

    private static readonly HashSet<string> ImagePreviewSizes = new(StringComparer.OrdinalIgnoreCase) { "none", "standard", "large" };

    /// <summary>
    /// The directives of <paramref name="value"/> in lower case, joined by a comma and a space, or <see langword="null"/>
    /// when it is empty or any directive is not one search engines document (an unknown name, a value on a flag, a
    /// missing or malformed value), so the page keeps the default instead of a tag crawlers would misread.
    /// </summary>
    internal static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var directives = new List<string>();
        foreach (var part in value.Split(','))
        {
            var directive = NormalizeDirective(part.Trim());
            if (directive is null)
            {
                return null;
            }

            directives.Add(directive);
        }

        return string.Join(", ", directives);
    }

    private static string? NormalizeDirective(string directive)
    {
        var separator = directive.IndexOf(':', StringComparison.Ordinal);
        if (separator < 0)
        {
            return Flags.Contains(directive) ? directive.ToLowerInvariant() : null;
        }

        var name = directive[..separator].Trim().ToLowerInvariant();
        var setting = directive[(separator + 1)..].Trim();
        var valid = name switch
        {
            "max-snippet" or "max-video-preview" => int.TryParse(setting, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var length) && length >= -1,
            "max-image-preview" => ImagePreviewSizes.Contains(setting),
            "unavailable_after" => setting.Length > 0,
            _ => false
        };

        return valid ? $"{name}:{(name == "unavailable_after" ? setting : setting.ToLowerInvariant())}" : null;
    }
}
