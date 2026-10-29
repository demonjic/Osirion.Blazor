using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Extensions.Options;
using Osirion.Blazor.Core.Utils;

namespace Osirion.Blazor.Core.Localization;

/// <summary>The culture prefixes of <see cref="OsirionLocalizationOptions"/>, resolved once.</summary>
internal sealed class OsirionLocalePaths : IOsirionLocalePaths
{
    private readonly Dictionary<string, OsirionLocaleCulture> _byPrefix = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, OsirionLocaleCulture> _byName = new(StringComparer.OrdinalIgnoreCase);

    public OsirionLocalePaths(IOptions<OsirionLocalizationOptions> options)
    {
        // Reading Value runs the validator, so the options below are known to be valid.
        var settings = options.Value;
        var defaultCulture = CultureInfo.GetCultureInfo(settings.DefaultCulture, predefinedOnly: true);

        var cultures = new List<OsirionLocaleCulture>();
        if (!settings.IsListed(defaultCulture))
        {
            cultures.Add(Resolve(defaultCulture, new LocalizationCulture(), defaultCulture));
        }

        foreach (var entry in settings.Cultures)
        {
            cultures.Add(Resolve(CultureInfo.GetCultureInfo(entry.Name, predefinedOnly: true), entry, defaultCulture));
        }

        foreach (var culture in cultures)
        {
            _byPrefix.Add(culture.Prefix, culture);
            _byName.Add(culture.Culture.Name, culture);
        }

        Cultures = cultures;
        DefaultCulture = cultures.Single(culture => culture.IsDefault);
    }

    public IReadOnlyList<OsirionLocaleCulture> Cultures { get; }

    public OsirionLocaleCulture DefaultCulture { get; }

    public bool TryGetCulture(string? path, [NotNullWhen(true)] out OsirionLocaleCulture? culture, out string remainder)
    {
        remainder = path ?? string.Empty;
        culture = null;
        if (string.IsNullOrEmpty(path) || path[0] != '/')
        {
            return false;
        }

        var end = path.IndexOf('/', 1);
        var segment = end < 0 ? path[1..] : path[1..end];
        if (!_byPrefix.TryGetValue(segment, out culture))
        {
            return false;
        }

        remainder = end < 0 ? "/" : "/" + path[end..].TrimStart('/', '\\');
        return true;
    }

    public string Localize(string sitePath, CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(sitePath);
        ArgumentNullException.ThrowIfNull(culture);

        if (!LocalUrl.IsLocalPath(sitePath))
        {
            return sitePath;
        }

        var (path, suffix) = SplitSuffix(sitePath);
        if (TryGetCulture(path, out _, out var withoutPrefix))
        {
            path = withoutPrefix;
        }

        return Match(culture).AddPrefixTo(path) + suffix;
    }

    public string WithoutLocale(string sitePath)
    {
        ArgumentNullException.ThrowIfNull(sitePath);

        if (!LocalUrl.IsLocalPath(sitePath))
        {
            return sitePath;
        }

        var (path, suffix) = SplitSuffix(sitePath);
        return TryGetCulture(path, out _, out var withoutPrefix) ? withoutPrefix + suffix : sitePath;
    }

    public string LocalizeForCurrentCulture(string sitePath) => Localize(sitePath, CultureInfo.CurrentUICulture);

    private OsirionLocaleCulture Match(CultureInfo culture)
    {
        for (var candidate = culture; candidate.Name.Length > 0; candidate = candidate.Parent)
        {
            if (_byName.TryGetValue(candidate.Name, out var match))
            {
                return match;
            }
        }

        return DefaultCulture;
    }

    private static (string Path, string Suffix) SplitSuffix(string sitePath)
    {
        var end = sitePath.AsSpan().IndexOfAny('?', '#');
        return end < 0 ? (sitePath, string.Empty) : (sitePath[..end], sitePath[end..]);
    }

    private static OsirionLocaleCulture Resolve(CultureInfo culture, LocalizationCulture entry, CultureInfo defaultCulture) => new(
        culture,
        OsirionLocalizationOptions.PrefixOf(culture.Name, entry.Prefix),
        string.IsNullOrWhiteSpace(entry.DisplayName) ? NativeDisplayName(culture) : entry.DisplayName,
        string.Equals(culture.Name, defaultCulture.Name, StringComparison.OrdinalIgnoreCase));

    // Native names are written in lower case in many languages ("srpski", "français"); a picker shows them capitalized,
    // with the culture's own casing rules.
    private static string NativeDisplayName(CultureInfo culture)
    {
        var name = culture.NativeName;
        return name.Length == 0 ? culture.Name : culture.TextInfo.ToUpper(name[..1]) + name[1..];
    }
}
