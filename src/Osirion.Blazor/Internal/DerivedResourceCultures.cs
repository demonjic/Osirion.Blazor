using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Options;
using Osirion.Blazor.Core.Transliteration;
using Osirion.Blazor.Options;

namespace Osirion.Blazor.Internal;

/// <summary>
/// The cultures whose resource strings are derived by transliteration: the <c>Osirion:Site:Locales</c> entries that
/// name <see cref="SiteLocaleOptions.DerivedFrom"/>, so the shell and the resources of a version are derived in one
/// place.
/// </summary>
internal sealed class DerivedResourceCultures
{
    private readonly Lazy<Dictionary<string, DerivedResourceCulture>> _byName;

    public DerivedResourceCultures(IOptions<OsirionSiteOptions> siteOptions, TextTransliterators transliterators)
    {
        // Read on first use, after the options are configured and validated
        _byName = new Lazy<Dictionary<string, DerivedResourceCulture>>(() => Read(siteOptions.Value, transliterators));
    }

    /// <summary>
    /// Finds the derived culture <paramref name="culture"/> belongs to: the culture itself or its nearest parent that is
    /// derived, so <c>sr-Latn-RS</c> belongs to a derived <c>sr-Latn</c>.
    /// </summary>
    /// <param name="culture">The request's UI culture.</param>
    /// <param name="derived">The derived culture.</param>
    /// <param name="ownCultures">
    /// The cultures from <paramref name="culture"/> up to the derived one: a string any of them has in its own resources
    /// wins over the derived string.
    /// </param>
    /// <returns><see langword="true"/> when the culture is derived.</returns>
    public bool TryFind(CultureInfo culture, [NotNullWhen(true)] out DerivedResourceCulture? derived, out IReadOnlyList<CultureInfo> ownCultures)
    {
        var chain = new List<CultureInfo>();
        for (var current = culture; current.Name.Length > 0; current = current.Parent)
        {
            chain.Add(current);
            if (_byName.Value.TryGetValue(current.Name, out derived))
            {
                ownCultures = chain;
                return true;
            }
        }

        derived = null;
        ownCultures = [];
        return false;
    }

    private static Dictionary<string, DerivedResourceCulture> Read(OsirionSiteOptions options, TextTransliterators transliterators)
    {
        var byName = new Dictionary<string, DerivedResourceCulture>(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, entry) in options.Locales)
        {
            // The site options' validators refuse an entry that breaks a rule; one that slips through derives nothing.
            if (entry?.DerivedFrom is { } from
                && transliterators.TryGet(entry.Transliteration, out var transliterator)
                && TryGetCulture(name, out var culture)
                && TryGetCulture(from, out var source))
            {
                byName[culture.Name] = new DerivedResourceCulture(source, transliterator);
            }
        }

        return byName;
    }

    private static bool TryGetCulture(string name, [NotNullWhen(true)] out CultureInfo? culture)
    {
        try
        {
            culture = CultureInfo.GetCultureInfo(name);
            return culture.Name.Length > 0;
        }
        catch (CultureNotFoundException)
        {
            culture = null;
            return false;
        }
    }
}

/// <summary>A derived culture's source culture and the transliterator that writes its strings.</summary>
/// <param name="Source">The culture whose strings are transliterated.</param>
/// <param name="Transliterator">Writes the strings.</param>
internal sealed record DerivedResourceCulture(CultureInfo Source, ITextTransliterator Transliterator)
{
    /// <summary>
    /// Transliterates <paramref name="value"/> outside its placeholders: <c>{0}</c>, <c>{0:N2}</c> and <c>{name}</c> stay
    /// as written, and so do the escaped braces <c>{{</c> and <c>}}</c>.
    /// </summary>
    public string Transliterate(string value)
    {
        if (!value.Contains('{', StringComparison.Ordinal))
        {
            return Transliterator.Transliterate(value);
        }

        var result = new StringBuilder(value.Length);
        var text = new StringBuilder();
        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (character == '{' && index + 1 < value.Length && value[index + 1] == '{')
            {
                text.Append("{{");
                index++;
                continue;
            }

            var holeEnd = character == '{' ? value.IndexOf('}', index + 1) : -1;
            if (holeEnd < 0)
            {
                text.Append(character);
                continue;
            }

            result.Append(Transliterator.Transliterate(text.ToString())).Append(value, index, holeEnd - index + 1);
            text.Clear();
            index = holeEnd;
        }

        return result.Append(Transliterator.Transliterate(text.ToString())).ToString();
    }
}
