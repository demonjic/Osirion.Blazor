using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace Osirion.Blazor.Core.Localization;

/// <summary>
/// Takes the culture of a request from the first segment of its path alone: a culture prefix
/// (<c>/sr/contact</c>) gives that culture and any other path gives the default culture, so one address always answers
/// in one language. <c>app.UseOsirionLocalization()</c> makes it the only provider of the request localization
/// middleware.
/// </summary>
/// <remarks>
/// A request that the framework re-executes for an error or status code page (<c>UseExceptionHandler</c>,
/// <c>UseStatusCodePagesWithReExecute</c>) is read at the path the visitor asked for, so a not-found page under
/// <c>/sr/</c> is in Serbian.
/// </remarks>
/// <param name="localePaths">The site's culture prefixes.</param>
public sealed class OsirionPathRequestCultureProvider(IOsirionLocalePaths localePaths) : RequestCultureProvider
{
    /// <inheritdoc />
    public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var path = httpContext.Features.Get<IStatusCodeReExecuteFeature>()?.OriginalPath
            ?? httpContext.Features.Get<IExceptionHandlerPathFeature>()?.Path
            ?? httpContext.Request.Path.Value;
        var culture = localePaths.TryGetCulture(path, out var prefixed, out _) ? prefixed : localePaths.DefaultCulture;
        return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(culture.Culture.Name));
    }
}
