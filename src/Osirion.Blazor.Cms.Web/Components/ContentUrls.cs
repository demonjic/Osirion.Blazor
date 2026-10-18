using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Osirion.Blazor.Cms.Domain.Entities;

namespace Osirion.Blazor.Cms.Web.Components;

/// <summary>
/// Builds the URLs and stable identifiers the content components share, so tags, categories, canonical URLs
/// and pagination links are produced one way everywhere.
/// </summary>
internal static class ContentUrls
{
    /// <summary>
    /// Turns a tag or category name into a URL path segment: invariant lower case, white space collapsed to
    /// single hyphens, then percent-encoded so characters such as '#' or '/' cannot change the URL structure.
    /// </summary>
    internal static string Slug(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var builder = new StringBuilder(value.Length);
        var pendingSeparator = false;
        foreach (var character in value.Trim().ToLowerInvariant())
        {
            if (char.IsWhiteSpace(character))
            {
                pendingSeparator = builder.Length > 0;
                continue;
            }

            if (pendingSeparator)
            {
                builder.Append('-');
                pendingSeparator = false;
            }

            builder.Append(character);
        }

        return Uri.EscapeDataString(builder.ToString());
    }

    /// <summary>
    /// Builds the link to a content item: the page's <paramref name="formatter"/> when it has one, otherwise the
    /// item's URL from the site root.
    /// </summary>
    internal static string ItemUrl(ContentItem item, Func<ContentItem, string>? formatter = null) =>
        formatter?.Invoke(item) ?? "/" + item.Url.TrimStart('/');

    /// <summary>
    /// Builds the link to a folder: the address its <c>_index.md</c> declares, otherwise its path below the content
    /// folder, without the locale folder, as content URLs leave it out.
    /// </summary>
    internal static string DirectoryUrl(DirectoryItem directory)
    {
        if (!string.IsNullOrWhiteSpace(directory.Url))
        {
            return "/" + directory.Url.Trim('/');
        }

        var root = directory;
        while (root.Parent is not null)
        {
            root = root.Parent;
        }

        var rootPath = root.Path.Trim('/');
        var path = directory.Path.Trim('/');
        if (!path.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
        {
            return "/" + path;
        }

        var belowRoot = path[rootPath.Length..].TrimStart('/');
        if (!ReferenceEquals(root, directory) && IsLocaleFolder(root))
        {
            return "/" + belowRoot;
        }

        return "/" + string.Join('/', new[] { FolderName(rootPath), belowRoot }.Where(part => part.Length > 0));
    }

    /// <summary>
    /// Whether a folder only sorts content by language: a top-level folder named after its locale, such as
    /// <c>content/en</c>. It has no page of its own.
    /// </summary>
    /// <remarks>
    /// The name is the last segment of the path because providers differ in what a path holds: the file system
    /// provider's paths start below the content folder (<c>/en/</c>), the GitHub provider's start at the repository
    /// root (<c>content/en</c>). A top-level folder is always directly below the content folder.
    /// </remarks>
    internal static bool IsLocaleFolder(DirectoryItem directory) =>
        directory.Parent is null
        && !string.IsNullOrWhiteSpace(directory.Locale)
        && string.Equals(FolderName(directory.Path.Trim('/')), directory.Locale, StringComparison.OrdinalIgnoreCase);

    // A top-level folder's own name, which is its path below the content folder whatever folder the provider's
    // paths start from.
    private static string FolderName(string path) => path[(path.LastIndexOf('/') + 1)..];

    /// <summary>Builds the default tag page URL, optionally below a locale segment.</summary>
    internal static string TagUrl(string tag, string? locale = null) => $"{LocalePrefix(locale)}/tag/{Slug(tag)}";

    /// <summary>Builds the default category page URL, optionally below a locale segment.</summary>
    internal static string CategoryUrl(string category, string? locale = null) => $"{LocalePrefix(locale)}/category/{Slug(category)}";

    private static string LocalePrefix(string? locale) =>
        string.IsNullOrWhiteSpace(locale) ? string.Empty : "/" + Uri.EscapeDataString(locale.Trim('/'));

    /// <summary>
    /// Returns the absolute URL without its query string and fragment, keeping only the query parameters named
    /// in <paramref name="keptParameters"/>.
    /// </summary>
    internal static string StripQueryAndFragment(string absoluteUri, IEnumerable<string>? keptParameters = null)
    {
        var fragmentIndex = absoluteUri.IndexOf('#', StringComparison.Ordinal);
        var withoutFragment = fragmentIndex >= 0 ? absoluteUri[..fragmentIndex] : absoluteUri;
        var queryIndex = withoutFragment.IndexOf('?', StringComparison.Ordinal);
        if (queryIndex < 0)
        {
            return withoutFragment;
        }

        var path = withoutFragment[..queryIndex];
        var kept = keptParameters?.ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (kept is not { Count: > 0 })
        {
            return path;
        }

        var query = QueryHelpers.ParseQuery(withoutFragment[queryIndex..]);
        var url = path;
        foreach (var (key, values) in query)
        {
            if (!kept.Contains(key))
            {
                continue;
            }

            foreach (var value in values)
            {
                url = QueryHelpers.AddQueryString(url, key, value ?? string.Empty);
            }
        }

        return url;
    }

    /// <summary>
    /// Returns <paramref name="currentUri"/> (path and query) with the <paramref name="parameter"/> query value
    /// replaced, or removed when <paramref name="value"/> is null, so other parameters such as a search query survive.
    /// </summary>
    internal static string WithQueryParameter(string currentUri, string parameter, string? value)
    {
        var uri = new Uri(currentUri, UriKind.RelativeOrAbsolute);
        var pathAndQuery = uri.IsAbsoluteUri ? uri.PathAndQuery : currentUri;
        var queryIndex = pathAndQuery.IndexOf('?', StringComparison.Ordinal);
        var path = queryIndex >= 0 ? pathAndQuery[..queryIndex] : pathAndQuery;
        var query = queryIndex >= 0 ? QueryHelpers.ParseQuery(pathAndQuery[queryIndex..]) : [];

        var url = path;
        foreach (var (key, values) in query)
        {
            if (string.Equals(key, parameter, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            foreach (var existing in values)
            {
                url = QueryHelpers.AddQueryString(url, key, existing ?? string.Empty);
            }
        }

        if (value is not null)
        {
            url = QueryHelpers.AddQueryString(url, parameter, value);
        }

        return url;
    }

    /// <summary>
    /// A hash of the text that is identical in every process and on every run, unlike <see cref="string.GetHashCode()"/>
    /// (FNV-1a over the UTF-8 bytes).
    /// </summary>
    internal static uint StableHash(string value)
    {
        var hash = 2166136261u;
        foreach (var octet in Encoding.UTF8.GetBytes(value))
        {
            hash ^= octet;
            hash *= 16777619u;
        }

        return hash;
    }

    /// <summary>Formats a date as an ISO 8601 UTC instant; a value of unspecified kind is taken to already be UTC.</summary>
    internal static string FormatUtc(DateTime value)
    {
        var utc = value.Kind switch
        {
            DateTimeKind.Local => value.ToUniversalTime(),
            DateTimeKind.Utc => value,
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        return utc.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);
    }
}
