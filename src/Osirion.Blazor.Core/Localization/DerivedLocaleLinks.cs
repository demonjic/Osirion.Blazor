using System.Globalization;
using Osirion.Blazor.Core.Utils;

namespace Osirion.Blazor.Core.Localization;

/// <summary>
/// Moves a link of a language version to the version derived from it, the rule derived content and a derived site
/// version share: a link to one of the site's pages in the source culture becomes the same page in the derived culture,
/// and every other address stays as it is.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Only a path on the site moves (<see cref="LocalUrl.IsLocalPath"/>): absolute, protocol-relative,
/// <c>mailto:</c>, <c>tel:</c> and fragment links and relative paths stay.</item>
/// <item>A link to a file, whose last path segment has an extension (<c>/sr/feed.xml</c>, an image), stays.</item>
/// <item>With <see cref="IOsirionLocalePaths"/> (from <c>Osirion:Localization</c>) and the derived culture among its
/// cultures, a path under the source culture's prefix, or a path without a prefix when the source is the default
/// culture, gets the derived culture's prefix: <c>/sr/contact</c> or, for a default <c>sr</c>, <c>/contact</c> becomes
/// <c>/sr-latn/contact</c>.</item>
/// <item>Without it, a path whose first segment is the source culture's name, in any letter case, gets the derived
/// culture's name in lower case, the prefix <c>Osirion:Localization</c> gives a culture by default: <c>/sr/contact</c>
/// becomes <c>/sr-latn/contact</c>. A path without that segment stays.</item>
/// </list>
/// The query string and the fragment are kept.
/// </remarks>
public static class DerivedLocaleLinks
{
    /// <summary>Moves <paramref name="address"/> from <paramref name="fromCulture"/> to <paramref name="toCulture"/>.</summary>
    /// <param name="address">An <c>href</c> or <c>src</c> of the source version.</param>
    /// <param name="fromCulture">The source culture, for example <c>sr</c>.</param>
    /// <param name="toCulture">The derived culture, for example <c>sr-Latn</c>.</param>
    /// <param name="localePaths">The site's culture prefixes, when <c>Osirion:Localization</c> is registered.</param>
    /// <returns>The address in the derived version; the same instance when it does not move.</returns>
    public static string Move(string address, string fromCulture, string toCulture, IOsirionLocalePaths? localePaths = null)
    {
        ArgumentNullException.ThrowIfNull(fromCulture);
        ArgumentNullException.ThrowIfNull(toCulture);

        if (!LocalUrl.IsLocalPath(address))
        {
            return address;
        }

        var suffixStart = address.IndexOfAny(['?', '#']);
        var path = suffixStart < 0 ? address : address[..suffixStart];
        if (Path.HasExtension(path[(path.LastIndexOf('/') + 1)..]))
        {
            return address;
        }

        if (localePaths is not null && TryGetCulture(localePaths, toCulture, out var derived))
        {
            return MoveWithPrefixes(address, path, fromCulture, derived, localePaths);
        }

        var firstSegmentEnd = path.IndexOf('/', 1);
        var firstSegment = firstSegmentEnd < 0 ? path[1..] : path[1..firstSegmentEnd];
        return firstSegment.Equals(fromCulture, StringComparison.OrdinalIgnoreCase)
            ? "/" + toCulture.ToLowerInvariant() + address[(1 + firstSegment.Length)..]
            : address;
    }

    private static string MoveWithPrefixes(
        string address, string path, string fromCulture, OsirionLocaleCulture derived, IOsirionLocalePaths localePaths)
    {
        var isSourcePage = localePaths.TryGetCulture(path, out var culture, out _)
            ? IsCulture(culture, fromCulture)
            : IsCulture(localePaths.DefaultCulture, fromCulture);

        return isSourcePage ? localePaths.Localize(address, derived.Culture) : address;
    }

    private static bool TryGetCulture(IOsirionLocalePaths localePaths, string name, out OsirionLocaleCulture culture)
    {
        culture = localePaths.Cultures.FirstOrDefault(candidate => IsCulture(candidate, name))!;
        return culture is not null;
    }

    private static bool IsCulture(OsirionLocaleCulture culture, string name) =>
        string.Equals(culture.Culture.Name, CanonicalName(name), StringComparison.OrdinalIgnoreCase);

    // sr-latn and sr-Latn name one culture; a name the platform does not know is compared as written.
    private static string CanonicalName(string name)
    {
        try
        {
            return CultureInfo.GetCultureInfo(name).Name;
        }
        catch (CultureNotFoundException)
        {
            return name;
        }
    }
}
