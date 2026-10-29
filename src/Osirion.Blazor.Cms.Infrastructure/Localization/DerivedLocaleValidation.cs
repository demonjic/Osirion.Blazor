using System.Globalization;
using Osirion.Blazor.Cms.Domain.Options;
using Osirion.Blazor.Core.Transliteration;

namespace Osirion.Blazor.Cms.Infrastructure.Localization;

/// <summary>
/// The rules a provider's <see cref="ContentProviderOptions.DerivedLocales"/> must follow. Each failure names the key it
/// is about, for example <c>DerivedLocales[0].From</c>.
/// </summary>
internal static class DerivedLocaleValidation
{
    private const string Key = nameof(ContentProviderOptions.DerivedLocales);

    /// <summary>Checks the derived locales of <paramref name="options"/>.</summary>
    /// <param name="options">The provider options.</param>
    /// <param name="transliterators">The registered transliterators.</param>
    /// <returns>The failures; empty when the derived locales are valid.</returns>
    internal static List<string> Validate(ContentProviderOptions options, TextTransliterators transliterators)
    {
        var failures = new List<string>();
        var derivedLocales = options.DerivedLocales ?? [];
        if (derivedLocales.Count == 0)
        {
            return failures;
        }

        if (!options.EnableLocalization)
        {
            failures.Add($"{Key} needs {nameof(ContentProviderOptions.EnableLocalization)} to be true.");
        }

        // Derived locales are added to the effective list, so the locales content is written in are the others
        var derivedNames = derivedLocales.Select(derived => derived?.Locale).OfType<string>().ToHashSet(StringComparer.OrdinalIgnoreCase);
        var writtenLocales = options.GetEffectiveSupportedLocales()
            .Where(locale => !derivedNames.Contains(locale))
            .ToList();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var index = 0; index < derivedLocales.Count; index++)
        {
            var entry = $"{Key}[{index}]";
            var derived = derivedLocales[index];
            if (derived is null)
            {
                failures.Add($"{entry} is empty.");
                continue;
            }

            var localeIsCulture = CheckCultureName(failures, $"{entry}.{nameof(DerivedLocaleOptions.Locale)}", derived.Locale);
            var fromIsCulture = CheckCultureName(failures, $"{entry}.{nameof(DerivedLocaleOptions.From)}", derived.From);

            if (localeIsCulture && fromIsCulture && string.Equals(derived.Locale, derived.From, StringComparison.OrdinalIgnoreCase))
            {
                failures.Add($"{entry}.{nameof(DerivedLocaleOptions.Locale)} '{derived.Locale}' must differ from {nameof(DerivedLocaleOptions.From)}.");
            }
            else if (fromIsCulture && derivedNames.Contains(derived.From))
            {
                failures.Add($"{entry}.{nameof(DerivedLocaleOptions.From)} '{derived.From}' is itself a derived locale; derive from a locale content is written in.");
            }
            else if (fromIsCulture && !writtenLocales.Contains(derived.From, StringComparer.OrdinalIgnoreCase))
            {
                failures.Add($"{entry}.{nameof(DerivedLocaleOptions.From)} '{derived.From}' is not one of {nameof(ContentProviderOptions.SupportedLocales)} ({string.Join(", ", writtenLocales)}).");
            }

            if (!transliterators.TryGet(derived.Transliteration, out _))
            {
                failures.Add($"{entry}.{nameof(DerivedLocaleOptions.Transliteration)} '{derived.Transliteration}' is not a registered transliterator (registered: {string.Join(", ", transliterators.Names.Order(StringComparer.Ordinal))}).");
            }

            if (localeIsCulture && !seen.Add(derived.Locale))
            {
                failures.Add($"{entry}.{nameof(DerivedLocaleOptions.Locale)} '{derived.Locale}' is derived more than once.");
            }
        }

        return failures;
    }

    private static bool CheckCultureName(List<string> failures, string key, string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            failures.Add($"{key} is required.");
            return false;
        }

        try
        {
            // Predefined only, so a typing error is not accepted as a custom culture
            CultureInfo.GetCultureInfo(name, predefinedOnly: true);
            return true;
        }
        catch (CultureNotFoundException)
        {
            failures.Add($"{key} '{name}' is not a culture name.");
            return false;
        }
    }
}
