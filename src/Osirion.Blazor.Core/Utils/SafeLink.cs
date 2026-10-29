namespace Osirion.Blazor.Core.Utils;

/// <summary>
/// Decides whether a URL that may come from content can be written into an <c>href</c> or <c>src</c>.
/// </summary>
public static class SafeLink
{
    /// <summary>
    /// Gets whether <paramref name="url"/> is a path on the site (see <see cref="LocalUrl.IsLocalPath"/>) or an
    /// absolute http, https or mailto URL. Other schemes, such as <c>javascript:</c> and <c>data:</c>, and
    /// protocol-relative URLs are rejected.
    /// </summary>
    public static bool IsSafe(string? url) =>
        LocalUrl.IsLocalPath(url)
        || (Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeMailto));
}
