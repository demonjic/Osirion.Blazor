namespace Osirion.Blazor.Cms.Domain.ContentBlocks;

/// <summary>
/// The link rules of content blocks: which destinations a block may link to and which addresses an image may load.
/// </summary>
/// <remarks>
/// A link is a site-relative path (<c>/platform</c>, <c>/platform#govern</c>), a fragment of the same page
/// (<c>#pricing</c>), an absolute <c>https:</c> URL, <c>mailto:</c> or <c>tel:</c>. An image is a site-relative path or
/// an <c>https:</c> URL. Anything else is refused, including <c>javascript:</c>, <c>data:</c>, <c>http:</c>, a
/// protocol-relative <c>//host</c> and a path with a backslash, which browsers read as a slash (<c>/\host</c>).
/// White space and control characters are refused anywhere in the value, because browsers drop them and would read
/// <c>java&#9;script:</c> as a script URL.
/// </remarks>
public static class ContentBlockLinks
{
    /// <summary>The allowed forms of a link, for error messages.</summary>
    public const string AllowedLinkForms = "a site-relative path (/page), a fragment (#section), an https: URL, mailto: or tel:";

    /// <summary>The allowed forms of an image address, for error messages.</summary>
    public const string AllowedImageForms = "a site-relative path (/images/a.png) or an https: URL";

    /// <summary>Returns whether <paramref name="value"/> is an allowed link destination.</summary>
    /// <param name="value">The link as written.</param>
    public static bool IsAllowedLink(string? value) =>
        HasNoUnsafeCharacters(value)
        && (IsSitePath(value!) || IsFragment(value!) || IsHttpsUrl(value!)
            || HasSchemeAndValue(value!, "mailto:") || HasSchemeAndValue(value!, "tel:"));

    /// <summary>Returns whether <paramref name="value"/> is an allowed image address.</summary>
    /// <param name="value">The address as written.</param>
    public static bool IsAllowedImageSource(string? value) =>
        HasNoUnsafeCharacters(value) && (IsSitePath(value!) || IsHttpsUrl(value!));

    /// <summary>
    /// Returns whether <paramref name="value"/> is a path on the site: it starts with one <c>/</c>, the second
    /// character is not <c>/</c>, and it has no backslash.
    /// </summary>
    /// <param name="value">The link as written.</param>
    public static bool IsSitePath(string value) =>
        value.StartsWith('/') && !value.StartsWith("//", StringComparison.Ordinal) && !value.Contains('\\');

    private static bool IsFragment(string value) => value.Length > 1 && value[0] == '#';

    private static bool IsHttpsUrl(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && uri.Scheme == Uri.UriSchemeHttps
        && uri.Host.Length > 0
        && value.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

    private static bool HasSchemeAndValue(string value, string scheme) =>
        value.Length > scheme.Length && value.StartsWith(scheme, StringComparison.OrdinalIgnoreCase);

    private static bool HasNoUnsafeCharacters(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        foreach (var character in value)
        {
            if (char.IsWhiteSpace(character) || char.IsControl(character))
            {
                return false;
            }
        }

        return true;
    }
}
