namespace Osirion.Blazor.Core.Localization;

/// <summary>
/// A language version of the site, one entry of <see cref="OsirionLocalizationOptions.Cultures"/>.
/// </summary>
public sealed class LocalizationCulture
{
    /// <summary>Gets or sets the culture name, for example <c>sr-Latn</c>. Required.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the first path segment that selects the culture, for example <c>sr</c> for <c>/sr/contact</c>.
    /// Defaults to <see cref="Name"/> in lower case, so <c>sr-Latn</c> gives <c>sr-latn</c>. It must be lower-case
    /// letters and digits, in words joined by hyphens.
    /// </summary>
    public string? Prefix { get; set; }

    /// <summary>
    /// Gets or sets the name shown in a language picker. Defaults to the culture's own name for itself, starting with a
    /// capital letter, for example <c>Français</c> for <c>fr</c>.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets other first path segments that reach the culture, for example <c>sr-Latn-RS</c> or <c>sr_latn</c>
    /// for addresses a site used before. A GET or HEAD request under an alias is redirected (301) to the same address
    /// under <see cref="Prefix"/>. Matched ignoring letter case; letters, digits, hyphens and underscores. Empty by
    /// default.
    /// </summary>
    public List<string> Aliases { get; set; } = [];
}
