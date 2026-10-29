using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Osirion.Blazor.Core.Localization;

/// <summary>
/// The language versions of a site whose addresses carry the language in their first path segment, for example
/// <c>/contact</c> in the default culture and <c>/sr/contact</c> in Serbian. Bound from the <c>Osirion:Localization</c>
/// configuration section by <c>AddOsirionLocalization(configuration)</c>, or set in code with
/// <c>AddOsirionLocalization(options =&gt; ...)</c>.
/// </summary>
public sealed partial class OsirionLocalizationOptions
{
    /// <summary>The configuration section the options are read from.</summary>
    public const string Section = "Osirion:Localization";

    /// <summary>
    /// Gets or sets the culture served at the site root, without a prefix, for example <c>en</c>. Required.
    /// </summary>
    public string DefaultCulture { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the language versions of the site. The default culture may be listed, to give it a display name;
    /// when it is not listed it is added first, with the defaults of <see cref="LocalizationCulture"/>. Either way its
    /// prefix redirects to the address without one.
    /// </summary>
    public List<LocalizationCulture> Cultures { get; set; } = [];

    /// <summary>Gets the problems that keep the options from describing the site's cultures; empty when they are valid.</summary>
    /// <returns>One message per problem, each naming its configuration key.</returns>
    public IReadOnlyList<string> GetErrors()
    {
        var errors = new List<string>();
        CultureInfo? defaultCulture = null;
        if (string.IsNullOrWhiteSpace(DefaultCulture))
        {
            errors.Add($"{Section}:DefaultCulture is required.");
        }
        else if (!TryGetCultureInfo(DefaultCulture, out defaultCulture))
        {
            errors.Add($"{Section}:DefaultCulture '{DefaultCulture}' is not a known culture name.");
        }

        var prefixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (defaultCulture is not null && !IsListed(defaultCulture))
        {
            AddPrefixErrors(PrefixOf(defaultCulture.Name, prefix: null), $"{Section}:DefaultCulture", prefixes, errors);
        }

        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < Cultures.Count; index++)
        {
            var key = $"{Section}:Cultures:{index}";
            var name = Cultures[index]?.Name;
            if (string.IsNullOrWhiteSpace(name))
            {
                errors.Add($"{key}:Name is required.");
            }
            else if (!TryGetCultureInfo(name, out var culture))
            {
                errors.Add($"{key}:Name '{name}' is not a known culture name.");
            }
            else if (!names.Add(culture.Name))
            {
                errors.Add($"{key}:Name '{name}' is listed more than once.");
            }
            else
            {
                AddPrefixErrors(PrefixOf(culture.Name, Cultures[index].Prefix), $"{key}:Prefix", prefixes, errors);
            }
        }

        // Aliases are checked once every prefix is known, so an alias that is a later culture's prefix is caught too.
        for (var index = 0; index < Cultures.Count; index++)
        {
            var aliases = Cultures[index]?.Aliases ?? [];
            for (var aliasIndex = 0; aliasIndex < aliases.Count; aliasIndex++)
            {
                AddAliasErrors(aliases[aliasIndex], $"{Section}:Cultures:{index}:Aliases:{aliasIndex}", prefixes, errors);
            }
        }

        return errors;
    }

    private static void AddAliasErrors(string? alias, string key, HashSet<string> segments, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(alias) || !AliasPattern().IsMatch(alias.Trim()))
        {
            errors.Add($"{key} '{alias}' must be letters, digits, hyphens and underscores, for example 'sr_latn'.");
        }
        else if (!segments.Add(alias.Trim()))
        {
            errors.Add($"{key} '{alias}' is the prefix or an alias of another culture.");
        }
    }

    /// <summary>
    /// Gets the culture <paramref name="name"/> names; <see langword="false"/> for an unknown name and for the
    /// invariant culture. Only cultures the platform knows are accepted, so a misspelt name fails instead of becoming a custom
    /// culture without data.
    /// </summary>
    internal static bool TryGetCultureInfo(string name, [NotNullWhen(true)] out CultureInfo? culture)
    {
        try
        {
            culture = CultureInfo.GetCultureInfo(name, predefinedOnly: true);
        }
        catch (CultureNotFoundException)
        {
            culture = null;
            return false;
        }

        if (culture.Name.Length == 0)
        {
            culture = null;
            return false;
        }

        return true;
    }

    /// <summary>Gets the URL segment of a culture: <paramref name="prefix"/>, or the culture name, in lower case.</summary>
    internal static string PrefixOf(string cultureName, string? prefix) =>
        (string.IsNullOrWhiteSpace(prefix) ? cultureName : prefix).ToLowerInvariant();

    internal bool IsListed(CultureInfo culture) =>
        Cultures.Exists(entry => entry is not null
            && TryGetCultureInfo(entry.Name, out var listed)
            && string.Equals(listed.Name, culture.Name, StringComparison.OrdinalIgnoreCase));

    private static void AddPrefixErrors(string prefix, string key, HashSet<string> prefixes, List<string> errors)
    {
        if (!PrefixPattern().IsMatch(prefix))
        {
            errors.Add($"{key} '{prefix}' must be lower-case letters and digits, in words joined by single hyphens, for example 'sr-latn'.");
        }
        else if (!prefixes.Add(prefix))
        {
            errors.Add($"{key} '{prefix}' is the prefix of another culture.");
        }
    }

    [GeneratedRegex(@"^[A-Za-z0-9_]+(?:-[A-Za-z0-9_]+)*\z", RegexOptions.CultureInvariant)]
    private static partial Regex AliasPattern();

    // \z, not $, so a trailing line break does not pass.
    [GeneratedRegex(@"^[a-z0-9]+(?:-[a-z0-9]+)*\z", RegexOptions.CultureInvariant)]
    private static partial Regex PrefixPattern();
}
