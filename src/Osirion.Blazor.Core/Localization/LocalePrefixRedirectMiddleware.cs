using Microsoft.AspNetCore.Http;

namespace Osirion.Blazor.Core.Localization;

/// <summary>
/// Gives every page one address per language: a culture prefix written in another letter case (<c>/SR/contact</c>) or
/// an alias of it (<c>/sr_latn/contact</c>) is redirected to its canonical form (<c>/sr-latn/contact</c>), and the
/// default culture's prefix (<c>/en/contact</c>) to the address without one (<c>/contact</c>).
/// </summary>
internal sealed class LocalePrefixRedirectMiddleware(RequestDelegate next, IOsirionLocalePaths localePaths)
{
    public Task InvokeAsync(HttpContext context)
    {
        // A 301 makes browsers repeat a POST as a GET without its body, so only GET and HEAD are redirected.
        if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method))
        {
            return next(context);
        }

        // The whole first segment is compared, so an alias that starts with the prefix (sr-latn-rs) is not taken for it.
        var path = context.Request.Path.Value;
        if (!localePaths.TryGetCulture(path, out var culture, out var remainder)
            || (!culture.IsDefault && string.Equals(FirstSegment(path!), culture.Prefix, StringComparison.Ordinal)))
        {
            return next(context);
        }

        // The remainder is a site path with a single leading slash, so the target never names another host.
        context.Response.StatusCode = StatusCodes.Status301MovedPermanently;
        context.Response.Headers.Location = BuildLocation(context.Request, culture.AddPrefixTo(remainder));
        return Task.CompletedTask;
    }

    private static string FirstSegment(string path)
    {
        var end = path.IndexOf('/', 1);
        return end < 0 ? path[1..] : path[1..end];
    }

    // Keeps the application's path base and the request's query string, as the content redirects do.
    private static string BuildLocation(HttpRequest request, string target)
    {
        var location = request.PathBase.Add(new PathString(target)).ToUriComponent();
        return location + request.QueryString.ToUriComponent();
    }
}
