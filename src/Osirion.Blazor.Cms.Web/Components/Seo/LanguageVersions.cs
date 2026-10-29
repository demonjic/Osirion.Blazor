using System.Globalization;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Core.Localization;

namespace Osirion.Blazor.Cms.Web.Components.Seo;

/// <summary>
/// The language versions of a page, as <c>hreflang</c> and address pairs, shared by the head links of
/// <see cref="SeoMetadataRenderer"/> and the sitemap of <c>MapOsirionFeeds</c> so both name the same alternates.
/// </summary>
internal static class LanguageVersions
{
    /// <summary>The <c>hreflang</c> value of the address for visitors whose language has no version of its own.</summary>
    internal const string DefaultHrefLang = "x-default";

    /// <summary>
    /// The configured culture an item's <paramref name="locale"/> names: the default culture for a blank locale, then
    /// the culture with that name or prefix (an <c>sr</c> folder for <c>sr-Latn</c> served at <c>/sr</c>), then the
    /// nearest parent culture (<c>sr-Latn-RS</c> for <c>sr-Latn</c>). <see langword="null"/> when the site has no
    /// such culture, so the item has no language version of its own.
    /// </summary>
    internal static OsirionLocaleCulture? CultureOf(IOsirionLocalePaths localePaths, string? locale)
    {
        if (string.IsNullOrWhiteSpace(locale))
        {
            return localePaths.DefaultCulture;
        }

        var named = localePaths.Cultures.FirstOrDefault(culture =>
            string.Equals(culture.Culture.Name, locale, StringComparison.OrdinalIgnoreCase)
            || string.Equals(culture.Prefix, locale, StringComparison.OrdinalIgnoreCase));
        if (named is not null)
        {
            return named;
        }

        CultureInfo cultureInfo;
        try
        {
            cultureInfo = CultureInfo.GetCultureInfo(locale, predefinedOnly: true);
        }
        catch (CultureNotFoundException)
        {
            return null;
        }

        for (var parent = cultureInfo.Parent; parent.Name.Length > 0; parent = parent.Parent)
        {
            var match = localePaths.Cultures.FirstOrDefault(culture =>
                string.Equals(culture.Culture.Name, parent.Name, StringComparison.OrdinalIgnoreCase));
            if (match is not null)
            {
                return match;
            }
        }

        return null;
    }

    /// <summary>
    /// The page at <paramref name="sitePath"/> in every configured culture, through
    /// <see cref="IOsirionLocalePaths.Localize"/>, each address made absolute by <paramref name="absolute"/>; see
    /// <see cref="Ordered"/>.
    /// </summary>
    internal static List<(string HrefLang, string Href)> ForPage(
        IOsirionLocalePaths localePaths, string sitePath, Func<string, string> absolute)
    {
        var versions = localePaths.Cultures.ToDictionary(
            culture => culture,
            culture => absolute(localePaths.Localize(sitePath, culture.Culture)));
        return Ordered(localePaths, versions);
    }

    /// <summary>
    /// The <paramref name="versions"/> in the configured order of their cultures, each named by its culture name, then
    /// <c>x-default</c> with the default culture's address when it has one. Empty with fewer than two versions: a page
    /// in one language has no alternates to name.
    /// </summary>
    internal static List<(string HrefLang, string Href)> Ordered(
        IOsirionLocalePaths localePaths, IReadOnlyDictionary<OsirionLocaleCulture, string> versions)
    {
        if (versions.Count < 2)
        {
            return [];
        }

        var links = localePaths.Cultures
            .Where(versions.ContainsKey)
            .Select(culture => (culture.Culture.Name, versions[culture]))
            .ToList();
        if (versions.TryGetValue(localePaths.DefaultCulture, out var defaultAddress))
        {
            links.Add((DefaultHrefLang, defaultAddress));
        }

        return links;
    }

    /// <summary>
    /// Whether the item's front matter <c>robots</c> keeps it out of the index, which also keeps it out of the sitemap
    /// and out of the alternates of its translations.
    /// </summary>
    internal static bool IsNoIndex(ContentItem item) =>
        item.Metadata?.SeoProperties?.Robots?.Contains("noindex", StringComparison.OrdinalIgnoreCase) == true;
}
