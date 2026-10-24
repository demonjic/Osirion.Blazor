namespace Osirion.Blazor.Core.Configuration;

/// <summary>
/// Chooses where <c>OsirionHtmlRenderer</c> loads Prism, the syntax highlighter, from, for every renderer of the site,
/// including those inside <c>ContentView</c>, <c>ArticlePage</c> and the other content components. Register it with
/// <c>AddOsirionPrism</c>.
/// </summary>
/// <remarks>
/// Without a <see cref="BaseUrl"/>, Prism 1.29.0 loads from cdnjs (<c>https://cdnjs.cloudflare.com</c>) and every file
/// is checked against its Subresource Integrity hash. A site whose Content Security Policy allows scripts only from
/// its own origin (<c>script-src 'self'</c>) serves the same files itself and sets <see cref="BaseUrl"/> to their
/// folder. A renderer's <c>PrismBaseUrl</c> parameter, when set, wins over this option.
/// </remarks>
public sealed class PrismOptions
{
    /// <summary>
    /// Gets or sets the folder Prism is loaded from: a path on the site (for example <c>/lib/prism/1.29.0/</c>) or an
    /// <c>https</c> address. The folder keeps the layout of the Prism release on cdnjs: <c>prism.min.js</c>,
    /// <c>components/prism-{language}.min.js</c>, <c>themes/prism-okaidia.min.css</c> and
    /// <c>plugins/line-numbers/prism-line-numbers.min.js</c> and <c>.min.css</c>. Files from this folder load without
    /// an integrity check, because the library cannot know their content. When <see langword="null"/> or empty, the
    /// pinned cdnjs release is used. Any other value (a relative path, a protocol-relative <c>//host</c>, <c>http</c>
    /// or another scheme) fails validation when the application starts.
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// Gets whether <paramref name="baseUrl"/> is empty, a path on the site or an absolute <c>https</c> address.
    /// </summary>
    internal static bool IsValidBaseUrl(string? baseUrl)
    {
        if (string.IsNullOrEmpty(baseUrl))
        {
            return true;
        }

        if (baseUrl.StartsWith('/'))
        {
            return !baseUrl.StartsWith("//", StringComparison.Ordinal) && !baseUrl.StartsWith("/\\", StringComparison.Ordinal);
        }

        return Uri.TryCreate(baseUrl, UriKind.Absolute, out var address) && address.Scheme == Uri.UriSchemeHttps;
    }
}
