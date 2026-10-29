using System.Globalization;
using Microsoft.Extensions.Options;
using Osirion.Blazor.Cms.Web.Options;
using Osirion.Blazor.Options;

namespace Osirion.Blazor.Internal;

/// <summary>
/// Names the home page in the structured data breadcrumbs with <see cref="SiteLabels.Home"/> of the language version
/// of the request, the derived versions included.
/// </summary>
internal sealed class SiteBreadcrumbHomeText(IOptions<OsirionSiteOptions> options) : IBreadcrumbHomeText
{
    public string? For(CultureInfo culture) => options.Value.ForCulture(culture).Labels.Home;
}
