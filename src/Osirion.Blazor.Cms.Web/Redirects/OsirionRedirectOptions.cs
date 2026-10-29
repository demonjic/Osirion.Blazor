using Osirion.Blazor.Cms.Domain.Entities;

namespace Osirion.Blazor.Cms.Web.Redirects;

/// <summary>
/// Options for the redirect middleware that <see cref="ContentRedirectExtensions.UseOsirionRedirects"/> adds.
/// </summary>
public sealed class OsirionRedirectOptions
{
    /// <summary>
    /// Gets or sets fixed redirects from an old path to a new path or absolute URL, for pages that are not content
    /// items, for example <c>["/docs/components/core"] = "/components"</c>. Content items list their own earlier URLs
    /// in <c>redirect_from</c> front matter instead.
    /// </summary>
    public IDictionary<string, string> Redirects { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets how a content item's current URL is built. Defaults to <c>/{item.Url}</c>, the path the content
    /// lists and the sitemap use; set it when the site routes content elsewhere.
    /// </summary>
    public Func<ContentItem, string>? ContentUrlFormatter { get; set; }

    /// <summary>
    /// Gets or sets whether each of <see cref="Redirects"/> also applies under every culture prefix of
    /// <c>Osirion:Localization</c>: with <c>["/pricing"] = "/contact"</c>, <c>/sr/pricing</c> goes to <c>/sr/contact</c>.
    /// A target on the site gets the request's prefix; an absolute URL is used as given. A source written with a prefix
    /// (<c>["/sr/pricing"]</c>) wins over the one without. Defaults to <see langword="false"/>, because a page that
    /// exists at a prefixed address would otherwise be redirected away; it needs <c>AddOsirionLocalization</c>.
    /// </summary>
    public bool ApplyUnderCulturePrefixes { get; set; }

    /// <summary>
    /// Gets or sets whether redirects are permanent (301, the default, which moves search rankings to the new URL) or
    /// temporary (302).
    /// </summary>
    public bool Permanent { get; set; } = true;

    /// <summary>
    /// Gets or sets how long the redirect map built from the content is reused before it is built again. Defaults to
    /// 5 minutes.
    /// </summary>
    public TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Checks the options and throws when a fixed redirect cannot work.
    /// </summary>
    /// <exception cref="ArgumentException">A source is not a site path, or a target is neither a site path nor an
    /// absolute http or https URL.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><see cref="CacheDuration"/> is negative.</exception>
    public void Validate()
    {
        foreach (var (source, target) in Redirects)
        {
            if (RedirectPaths.Normalize(source) is null)
            {
                throw new ArgumentException($"Redirect source '{source}' must be a path on the site, such as /old-page.", nameof(Redirects));
            }

            if (!RedirectPaths.IsValidTarget(target))
            {
                throw new ArgumentException($"Redirect target '{target}' must be a path on the site or an absolute http or https URL.", nameof(Redirects));
            }
        }

        if (CacheDuration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(CacheDuration), CacheDuration, "CacheDuration cannot be negative.");
        }
    }
}
