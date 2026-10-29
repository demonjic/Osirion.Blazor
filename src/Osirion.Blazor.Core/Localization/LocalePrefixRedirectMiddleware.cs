using Microsoft.AspNetCore.Http;

namespace Osirion.Blazor.Core.Localization;

/// <summary>
/// Gives every page one address per language: a culture prefix written in another letter case (<c>/SR/contact</c>)
/// is redirected to its canonical form (<c>/sr/contact</c>), and the default culture's prefix (<c>/en/contact</c>) to the
/// address without one (<c>/contact</c>).
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

        // A matched prefix has the length of the culture's prefix, so comparing that many characters ordinally tells
        // whether it is already written in the canonical, lower-case form.
        var path = context.Request.Path.Value;
        if (!localePaths.TryGetCulture(path, out var culture, out var remainder)
            || (!culture.IsDefault && string.CompareOrdinal(path, 1, culture.Prefix, 0, culture.Prefix.Length) == 0))
        {
            return next(context);
        }

        // The remainder is a site path with a single leading slash, so the target never names another host.
        context.Response.StatusCode = StatusCodes.Status301MovedPermanently;
        context.Response.Headers.Location = BuildLocation(context.Request, culture.AddPrefixTo(remainder));
        return Task.CompletedTask;
    }

    // Keeps the application's path base and the request's query string, as the content redirects do.
    private static string BuildLocation(HttpRequest request, string target)
    {
        var location = request.PathBase.Add(new PathString(target)).ToUriComponent();
        return location + request.QueryString.ToUriComponent();
    }
}
