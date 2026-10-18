using System.Collections.Concurrent;
using System.Globalization;
using Osirion.Blazor.Cms.Domain.Entities;

namespace Osirion.Blazor.Cms.Web.Components;

/// <summary>
/// Formats the dates the content components show in the language of the content they belong to, so an English page
/// reads "October 17, 2026" whatever culture the server runs under.
/// </summary>
internal static class ContentDates
{
    // Locales come from content folders and front matter, so the set is small; the cap only guards against a
    // provider that hands out unbounded distinct values.
    private const int MaxCachedLocales = 256;

    private const string MachineDateFormat = "yyyy-MM-dd";

    private static readonly ConcurrentDictionary<string, CultureInfo?> Cultures = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Returns the culture of <paramref name="item"/>'s locale, or the current culture when the item has no locale or
    /// its locale is not a known culture name.
    /// </summary>
    internal static CultureInfo CultureOf(ContentItem? item) => CultureOf(item?.Locale);

    /// <summary>
    /// Returns the culture named by <paramref name="locale"/>, or the current culture when it is empty or not a known
    /// culture name.
    /// </summary>
    internal static CultureInfo CultureOf(string? locale)
    {
        if (string.IsNullOrWhiteSpace(locale))
        {
            return CultureInfo.CurrentCulture;
        }

        var name = locale.Trim();
        if (!Cultures.TryGetValue(name, out var culture))
        {
            culture = Find(name);
            if (Cultures.Count < MaxCachedLocales)
            {
                Cultures.TryAdd(name, culture);
            }
        }

        // The fallback is read on every call, not cached, because the current culture can differ per request.
        return culture ?? CultureInfo.CurrentCulture;
    }

    /// <summary>Formats <paramref name="date"/> with <paramref name="format"/> in the culture of <paramref name="item"/>.</summary>
    internal static string Format(DateTime date, string format, ContentItem? item) =>
        date.ToString(format, CultureOf(item));

    /// <summary>
    /// Formats <paramref name="date"/> for the <c>datetime</c> attribute of a <c>time</c> element, which needs the
    /// Gregorian <c>yyyy-MM-dd</c> form in every culture.
    /// </summary>
    internal static string MachineReadable(DateTime date) => date.ToString(MachineDateFormat, CultureInfo.InvariantCulture);

    private static CultureInfo? Find(string name)
    {
        try
        {
            // Without predefinedOnly, ICU accepts any well-formed name such as "xx" and formats it like the invariant
            // culture, which would hide a typo in the content instead of falling back to the site's culture.
            return CultureInfo.GetCultureInfo(name, predefinedOnly: true);
        }
        catch (CultureNotFoundException)
        {
            return null;
        }
    }
}
