using System.Collections.Concurrent;
using System.Globalization;
using System.Resources;
using Microsoft.Extensions.Localization;

namespace Osirion.Blazor.Internal;

/// <summary>
/// Creates localizers that answer a derived culture with its source culture's strings, transliterated. Every other
/// culture gets the inner factory's localizer's answer unchanged.
/// </summary>
internal sealed class DerivedCultureStringLocalizerFactory(IStringLocalizerFactory inner, DerivedResourceCultures cultures)
    : IStringLocalizerFactory
{
    public IStringLocalizer Create(Type resourceSource) => new DerivedCultureStringLocalizer(inner.Create(resourceSource), cultures);

    public IStringLocalizer Create(string baseName, string location) => new DerivedCultureStringLocalizer(inner.Create(baseName, location), cultures);
}

/// <summary>
/// A localizer that, for a derived culture, returns the string the resource has for that culture (or a culture between
/// the request's and the derived one) when it has one, and otherwise the source culture's string transliterated, with
/// its placeholders kept.
/// </summary>
internal sealed class DerivedCultureStringLocalizer(IStringLocalizer inner, DerivedResourceCultures cultures) : IStringLocalizer
{
    // The names each culture's own resources hold, read once per culture
    private readonly ConcurrentDictionary<string, HashSet<string>> _ownNames = new(StringComparer.OrdinalIgnoreCase);

    public LocalizedString this[string name]
    {
        get
        {
            if (!TryGetDerived(name, out var derived))
            {
                return inner[name];
            }

            var source = InCulture(derived.Source, () => inner[name]);
            return new LocalizedString(name, derived.Transliterate(source.Value), source.ResourceNotFound, source.SearchedLocation);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            if (!TryGetDerived(name, out var derived))
            {
                return inner[name, arguments];
            }

            // The format is transliterated outside its placeholders, then formatted the way the resource localizer formats
            var source = InCulture(derived.Source, () => inner[name]);
            var value = string.Format(CultureInfo.CurrentCulture, derived.Transliterate(source.Value), arguments);
            return new LocalizedString(name, value, source.ResourceNotFound, source.SearchedLocation);
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        if (!includeParentCultures
            || !cultures.TryFind(CultureInfo.CurrentUICulture, out var derived, out var ownCultures))
        {
            return inner.GetAllStrings(includeParentCultures);
        }

        var own = ownCultures.SelectMany(culture => InCulture(culture, () => ReadOwnStrings())).ToList();
        var ownNames = own.Select(value => value.Name).ToHashSet(StringComparer.Ordinal);
        var sourceStrings = InCulture(derived.Source, () => inner.GetAllStrings(includeParentCultures: true).ToList());

        return own
            .DistinctBy(value => value.Name)
            .Concat(sourceStrings
                .Where(value => !ownNames.Contains(value.Name))
                .Select(value => new LocalizedString(value.Name, derived.Transliterate(value.Value), value.ResourceNotFound, value.SearchedLocation)))
            .ToList();
    }

    private bool TryGetDerived(string name, out DerivedResourceCulture derived)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (!cultures.TryFind(CultureInfo.CurrentUICulture, out var found, out var ownCultures)
            || ownCultures.Any(culture => OwnNames(culture).Contains(name)))
        {
            derived = null!;
            return false;
        }

        derived = found;
        return true;
    }

    private HashSet<string> OwnNames(CultureInfo culture) =>
        _ownNames.GetOrAdd(culture.Name, _ => InCulture(culture, () => ReadOwnStrings().Select(value => value.Name).ToHashSet(StringComparer.Ordinal)));

    // A culture without resources of its own has no own strings; the resource localizer reports that by throwing.
    private List<LocalizedString> ReadOwnStrings()
    {
        try
        {
            return inner.GetAllStrings(includeParentCultures: false).ToList();
        }
        catch (MissingManifestResourceException)
        {
            return [];
        }
    }

    // The resource localizer reads the request's UI culture, so a source string is read by switching it for the call.
    private static T InCulture<T>(CultureInfo culture, Func<T> read)
    {
        var current = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = culture;
        try
        {
            return read();
        }
        finally
        {
            CultureInfo.CurrentUICulture = current;
        }
    }
}
