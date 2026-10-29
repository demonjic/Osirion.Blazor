namespace Osirion.Blazor.Cms.Domain.Options;

/// <summary>
/// A content locale made from another by transliteration when content loads, so a site writes its content once and
/// serves it in a second script: for example <c>sr-Latn</c> from <c>sr</c> with <c>serbian-cyrillic-to-latin</c>.
/// </summary>
/// <remarks>
/// Every item of <see cref="From"/> without an item of the same localization id in <see cref="Locale"/> gets a derived
/// item in <see cref="Locale"/>; a file written for <see cref="Locale"/> always wins. Derived items are read only.
/// </remarks>
public sealed class DerivedLocaleOptions
{
    /// <summary>Gets or sets the culture name of the derived locale, for example <c>sr-Latn</c>.</summary>
    public string Locale { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the culture name of the locale the content is written in, for example <c>sr</c>. It must be one of
    /// the provider's <see cref="ContentProviderOptions.SupportedLocales"/>.
    /// </summary>
    public string From { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the registered transliterator that writes the text, for example
    /// <c>serbian-cyrillic-to-latin</c>.
    /// </summary>
    public string Transliteration { get; set; } = string.Empty;
}
