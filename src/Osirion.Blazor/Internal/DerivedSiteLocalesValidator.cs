using System.Globalization;
using Microsoft.Extensions.Options;
using Osirion.Blazor.Core.Localization;
using Osirion.Blazor.Core.Transliteration;
using Osirion.Blazor.Options;

namespace Osirion.Blazor.Internal;

/// <summary>
/// Fails startup when a derived site version names a transliterator that is not registered or a source the site does
/// not have, or, with <c>Osirion:Localization</c> registered, when it names a culture the site's routes do not serve.
/// It also fails a <see cref="OsirionSiteOptions.LanguagePicker"/> without <c>Osirion:Localization</c>. The checks that
/// need no service are in <see cref="OsirionSiteOptions.GetErrors"/>.
/// </summary>
internal sealed class DerivedSiteLocalesValidator : IValidateOptions<OsirionSiteOptions>
{
    private readonly TextTransliterators _transliterators;
    private readonly IOsirionLocalePaths? _localePaths;

    public DerivedSiteLocalesValidator(TextTransliterators transliterators)
    {
        _transliterators = transliterators;
    }

    public DerivedSiteLocalesValidator(TextTransliterators transliterators, IOsirionLocalePaths localePaths)
        : this(transliterators)
    {
        _localePaths = localePaths;
    }

    public ValidateOptionsResult Validate(string? name, OsirionSiteOptions options)
    {
        var errors = new List<string>();
        if (options.LanguagePicker != SiteLanguagePicker.None && _localePaths is null)
        {
            errors.Add($"{OsirionSiteOptions.Section}:LanguagePicker needs the site's cultures: add an {OsirionLocalizationOptions.Section} section, or call UseLocalization.");
        }

        foreach (var (culture, entry) in options.Locales)
        {
            if (entry?.DerivedFrom is { } from)
            {
                AddErrors($"{OsirionSiteOptions.Section}:Locales:{culture}", culture, from, entry.Transliteration, options, errors);
            }
        }

        return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }

    private void AddErrors(string key, string culture, string from, string? transliteration, OsirionSiteOptions options, List<string> errors)
    {
        if (!string.IsNullOrWhiteSpace(transliteration) && !_transliterators.TryGet(transliteration, out _))
        {
            errors.Add($"{key}:Transliteration '{transliteration}' is not a registered transliterator (registered: {string.Join(", ", _transliterators.Names.Order(StringComparer.Ordinal))}).");
        }

        var hasEntry = options.Locales.Keys.Any(locale => string.Equals(locale, from, StringComparison.OrdinalIgnoreCase));
        if (!hasEntry && !IsServed(from))
        {
            errors.Add(_localePaths is null
                ? $"{key}:DerivedFrom '{from}' has no entry in {OsirionSiteOptions.Section}:Locales."
                : $"{key}:DerivedFrom '{from}' has no entry in {OsirionSiteOptions.Section}:Locales and is not a culture of {OsirionLocalizationOptions.Section}.");
        }

        if (_localePaths is not null && !IsServed(culture))
        {
            errors.Add($"{key} is not a culture of {OsirionLocalizationOptions.Section}, so no address serves the derived version.");
        }
    }

    // A source without an entry is still a version of the site when the site's routes serve it, the default culture
    // included: its version is the site's own values.
    private bool IsServed(string culture)
    {
        if (_localePaths is null)
        {
            return false;
        }

        try
        {
            var name = CultureInfo.GetCultureInfo(culture).Name;
            return _localePaths.Cultures.Any(served => string.Equals(served.Culture.Name, name, StringComparison.OrdinalIgnoreCase));
        }
        catch (CultureNotFoundException)
        {
            return false;
        }
    }
}
