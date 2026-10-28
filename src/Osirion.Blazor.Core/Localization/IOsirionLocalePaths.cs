using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Osirion.Blazor.Core.Localization;

/// <summary>
/// Reads and writes the culture prefix of site paths, for a site whose addresses carry the language in their first
/// path segment: <c>/contact</c> in the default culture and <c>/sr/contact</c> in Serbian. Registered as a singleton by
/// <c>AddOsirionLocalization</c> from <see cref="OsirionLocalizationOptions"/>.
/// </summary>
/// <remarks>
/// A site path starts with a single <c>/</c> and is relative to the application's path base. Prefixes are matched
/// ignoring letter case and written in lower case.
/// </remarks>
public interface IOsirionLocalePaths
{
    /// <summary>Gets the site's cultures, the default culture included, in the configured order.</summary>
    IReadOnlyList<OsirionLocaleCulture> Cultures { get; }

    /// <summary>Gets the culture served at the site root, without a prefix.</summary>
    OsirionLocaleCulture DefaultCulture { get; }

    /// <summary>
    /// Finds the culture whose prefix is the first segment of <paramref name="path"/>, ignoring letter case.
    /// </summary>
    /// <param name="path">A path that starts with <c>/</c>, without a query string, such as <c>/sr/contact</c>.</param>
    /// <param name="culture">The culture of the prefix, or <see langword="null"/> when the path has none.</param>
    /// <param name="remainder">
    /// The rest of the path as a site path, <c>/contact</c> for <c>/sr/contact</c> and <c>/</c> for <c>/sr</c>. Slashes
    /// and backslashes after the prefix collapse to one <c>/</c>, so the remainder never names another host.
    /// <paramref name="path"/> itself when there is no prefix.
    /// </param>
    /// <returns><see langword="true"/> when the first segment is a culture prefix.</returns>
    bool TryGetCulture(string? path, [NotNullWhen(true)] out OsirionLocaleCulture? culture, out string remainder);

    /// <summary>
    /// Gets the address of <paramref name="sitePath"/> in <paramref name="culture"/>: <c>/contact</c> gives
    /// <c>/sr/contact</c> for <c>sr</c> and stays <c>/contact</c> for the default culture, <c>/</c> gives <c>/sr</c>,
    /// and a path that already has a prefix gets the new one instead. The query string and fragment are kept.
    /// </summary>
    /// <remarks>
    /// Anything that is not a path on the site is returned unchanged: absolute and protocol-relative URLs,
    /// <c>mailto:</c> and <c>tel:</c> links and relative paths. A culture the site does not have is matched through its
    /// parents (<c>sr-Latn-RS</c>, then <c>sr-Latn</c>, then <c>sr</c>), and the default culture is used when none
    /// matches.
    /// </remarks>
    /// <param name="sitePath">A path on the site, for example <c>/contact?tab=2</c>.</param>
    /// <param name="culture">The culture of the address.</param>
    /// <returns>The localized address.</returns>
    string Localize(string sitePath, CultureInfo culture);

    /// <summary>
    /// Removes the culture prefix from <paramref name="sitePath"/>: <c>/sr/contact</c> gives <c>/contact</c>. A path
    /// without a prefix, and anything that is not a path on the site, is returned unchanged.
    /// </summary>
    /// <param name="sitePath">A path on the site.</param>
    /// <returns>The path without its prefix.</returns>
    string WithoutLocale(string sitePath);

    /// <summary>
    /// Gets the address of <paramref name="sitePath"/> in the culture of the request,
    /// <see cref="CultureInfo.CurrentUICulture"/>; see <see cref="Localize"/>.
    /// </summary>
    /// <param name="sitePath">A path on the site, for example <c>/contact</c>.</param>
    /// <returns>The localized address.</returns>
    string LocalizeForCurrentCulture(string sitePath);
}
