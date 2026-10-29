using System.Text.RegularExpressions;

namespace Osirion.Blazor.Cms.Infrastructure.Utilities;

/// <summary>
/// Utility class for generating URLs and slugs
/// </summary>
public static class UrlGenerator
{
    private const string IndexFileName = "_index.md";

    /// <summary>
    /// Generates a URL for content
    /// </summary>
    /// <param name="permalink">
    /// The item's <c>permalink</c> front matter. When it has text, it is the address, with outer slashes removed and
    /// no locale folder. A folder's <c>_index.md</c> ignores it here: the folder's own permalink is applied to the folder.
    /// </param>
    /// <param name="path">The file's path, with its file name.</param>
    /// <param name="slug">The item's slug, the last segment of an address built from the path.</param>
    /// <param name="enableLocalization">Whether a leading locale folder is left out of the address.</param>
    /// <param name="supportedLocales">The folder names that count as locales; null accepts any language code shape.</param>
    /// <param name="skipSegment">A leading folder, such as the content root, to leave out of the address.</param>
    /// <returns>The address, without a leading slash.</returns>
    public static string GenerateUrl(string permalink, string path, string slug, bool enableLocalization, List<string>? supportedLocales, string? skipSegment = null)
    {

        // Check if pathToTrim is null, whitespace, or just "/"
        bool skipPrefixRemoval = string.IsNullOrWhiteSpace(skipSegment) || skipSegment == "/";

        // Normalize path
        path = NormalizePath(path);

        // Step 1: Remove skipSegment from the beginning of the path if present
        if (!skipPrefixRemoval)
        {
            path = RemoveLeadingSegment(path, skipSegment!);
        }

        var isIndexFile = string.Equals(path[(path.LastIndexOf('/') + 1)..], IndexFileName, StringComparison.OrdinalIgnoreCase);

        // permalink: "/" normalizes to the empty address, the site root, and is used like any other permalink
        if (!isIndexFile && !string.IsNullOrWhiteSpace(permalink))
        {
            return NormalizePath(permalink.Trim());
        }

        // Step 2: Remove the filename from the path
        path = GetDirectoryPath(path);

        // If using localization, check if the first segment is a locale and remove it
        if (enableLocalization && path.Length > 0)
        {
            path = RemoveLeadingLocale(path, supportedLocales);
        }

        // Step 3: Append slug, with a slash if path is not empty
        if (string.IsNullOrWhiteSpace(path))
        {
            return slug;
        }

        return isIndexFile ? path : $"{path}/{slug}";
    }

    /// <summary>
    /// Generates a URL-friendly slug from text
    /// </summary>
    public static string GenerateSlug(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "untitled";

        // Convert to lowercase
        var slug = text.ToLowerInvariant();

        // Remove special characters
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");

        // Replace spaces with hyphens
        slug = Regex.Replace(slug, @"\s+", "-");

        // Remove consecutive hyphens
        slug = Regex.Replace(slug, @"-{2,}", "-");

        // Trim hyphens from ends
        slug = slug.Trim('-');

        return slug;
    }

    /// <summary>
    /// Gets the directory path from a file path
    /// </summary>
    public static string GetDirectoryPath(string filePath)
    {
        var lastSlashIndex = filePath.LastIndexOf('/');
        if (lastSlashIndex >= 0)
        {
            return filePath.Substring(0, lastSlashIndex);
        }
        return string.Empty;
    }

    /// <summary>
    /// Removes locale prefix from a path
    /// </summary>
    public static string RemoveLocaleFromPath(string path, string contentPath, bool enableLocalization, List<string>? supportedLocales)
    {
        if (!enableLocalization)
            return path;

        path = RemoveContentPath(path, contentPath);
        return RemoveLeadingLocale(path, supportedLocales);
    }

    /// <summary>
    /// Extracts locale from a path
    /// </summary>
    public static string ExtractLocaleFromPath(string path, string contentPath, bool enableLocalization, List<string>? supportedLocales, string defaultLocale)
    {
        // If localization is disabled, always return default locale
        if (!enableLocalization)
        {
            return defaultLocale;
        }

        path = RemoveContentPath(path, contentPath);

        // Try to extract locale from path format like "en/blog" or "es/articles"
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length > 0 && IsValidLocale(segments[0], supportedLocales))
        {
            return segments[0];
        }

        // No valid locale found, return default
        return defaultLocale;
    }

    /// <summary>
    /// Checks if a string is a valid locale
    /// </summary>
    public static bool IsValidLocale(string locale, List<string>? supportedLocales)
    {
        // Check against supported locales list if defined
        if (supportedLocales is not null && supportedLocales.Any())
        {
            return supportedLocales.Contains(locale, StringComparer.OrdinalIgnoreCase);
        }

        // Fallback to simple validation: 2-letter language code or language-region format
        return (locale.Length == 2 && locale.All(char.IsLetter)) ||
               (locale.Length == 5 && locale[2] == '-' &&
                locale.Substring(0, 2).All(char.IsLetter) &&
                locale.Substring(3, 2).All(char.IsLetter));
    }

    /// <summary>
    /// Normalizes a path for consistent comparison
    /// </summary>
    public static string NormalizePath(string path)
    {
        return path.Replace('\\', '/').Trim('/');
    }

    private static string RemoveContentPath(string path, string contentPath)
    {
        var normalizedContentPath = NormalizePath(contentPath);
        return string.IsNullOrWhiteSpace(normalizedContentPath)
            ? path
            : RemoveLeadingSegment(path, normalizedContentPath);
    }

    /// <summary>
    /// Removes <paramref name="prefix"/> from the start of <paramref name="path"/> only when it is a whole segment,
    /// that is followed by a slash or the end of the path.
    /// </summary>
    private static string RemoveLeadingSegment(string path, string prefix)
    {
        if (!path.StartsWith(prefix))
            return path;

        if (path.Length == prefix.Length)
            return string.Empty;

        return path[prefix.Length] == '/' ? path.Substring(prefix.Length + 1) : path;
    }

    private static string RemoveLeadingLocale(string path, List<string>? supportedLocales)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length > 0 && IsValidLocale(segments[0], supportedLocales))
        {
            return string.Join("/", segments.Skip(1));
        }

        return path;
    }
}
