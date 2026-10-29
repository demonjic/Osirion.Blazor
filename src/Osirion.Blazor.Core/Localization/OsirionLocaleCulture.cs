using System.Globalization;

namespace Osirion.Blazor.Core.Localization;

/// <summary>
/// A language version of the site as <see cref="IOsirionLocalePaths"/> serves it, resolved from
/// <see cref="OsirionLocalizationOptions"/>.
/// </summary>
/// <param name="Culture">The culture.</param>
/// <param name="Prefix">The first path segment that selects the culture, in lower case, for example <c>sr</c>.</param>
/// <param name="DisplayName">The name shown in a language picker, for example <c>Français</c>.</param>
/// <param name="IsDefault">Whether the culture is served at the site root, without its prefix.</param>
public sealed record OsirionLocaleCulture(CultureInfo Culture, string Prefix, string DisplayName, bool IsDefault)
{
    /// <summary>
    /// Puts the culture's prefix in front of <paramref name="sitePath"/>, a path that starts with a single <c>/</c> and
    /// has no prefix; the default culture keeps the path as it is.
    /// </summary>
    internal string AddPrefixTo(string sitePath)
    {
        if (IsDefault)
        {
            return sitePath;
        }

        return sitePath == "/" ? "/" + Prefix : "/" + Prefix + sitePath;
    }
}
