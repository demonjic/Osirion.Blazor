using System.Globalization;

namespace Osirion.Blazor.Cms.Web.Options;

/// <summary>
/// Names the home page as the first entry of the structured data breadcrumbs (<c>BreadcrumbList</c>) in a language,
/// so a page in Serbian does not call it <c>Home</c>. <c>AddOsirion</c> registers one that reads
/// <c>Osirion:Site:Labels:Home</c> for the request's language version; register your own to take it from elsewhere.
/// </summary>
public interface IBreadcrumbHomeText
{
    /// <summary>Gets the name of the home page in <paramref name="culture"/>.</summary>
    /// <param name="culture">The culture of the request.</param>
    /// <returns>The name, or <see langword="null"/> or an empty string to fall back to the SEO options.</returns>
    string? For(CultureInfo culture);
}
