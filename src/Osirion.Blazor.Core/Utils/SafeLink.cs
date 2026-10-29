namespace Osirion.Blazor.Core.Utils;

/// <summary>
/// Decides whether a URL that may come from content can be written into an <c>href</c> or <c>src</c>.
/// </summary>
public static class SafeLink
{
    private const string TelephoneScheme = "tel";

    /// <summary>
    /// Gets whether <paramref name="url"/> is a path on the site (see <see cref="LocalUrl.IsLocalPath"/>), a fragment of
    /// the current page (<c>#pricing</c>), or an absolute http, https, mailto or tel URL. Other schemes, such as
    /// <c>javascript:</c>, <c>vbscript:</c> and <c>data:</c>, and protocol-relative URLs are rejected.
    /// </summary>
    public static bool IsSafe(string? url) =>
        LocalUrl.IsLocalPath(url)
        || IsFragment(url)
        || (Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeMailto
                || (uri.Scheme == TelephoneScheme && url!.Length > TelephoneScheme.Length + 1)));

    // A fragment only moves within the page; white space and control characters, which browsers drop, are refused.
    private static bool IsFragment(string? url) =>
        url is { Length: > 1 } && url[0] == '#' && !url.Any(character => char.IsWhiteSpace(character) || char.IsControl(character));
}
