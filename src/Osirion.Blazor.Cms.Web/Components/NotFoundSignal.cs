using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;

namespace Osirion.Blazor.Cms.Web.Components;

/// <summary>
/// Tells the framework, once per component, that the requested content does not exist, so a statically rendered
/// response carries HTTP 404 instead of a 200 "soft 404". On .NET 10 this is <c>NavigationManager.NotFound()</c>; on
/// .NET 9 the status code is set on the request's <see cref="HttpContext"/>, which the component receives as a
/// cascading parameter. It has no effect once the response has started, which is why the components that use it do
/// not use streaming rendering, and none during interactive rendering, where the not-found markup the component
/// renders is all that can be shown.
/// </summary>
internal sealed class NotFoundSignal
{
    private bool _sent;

#if NET10_0_OR_GREATER
    /// <summary>Signals not found, unless this instance already did since the last <see cref="Reset"/>.</summary>
    /// <param name="navigationManager">The component's navigation manager.</param>
    public void Send(NavigationManager navigationManager)
    {
        if (TryMarkSent())
        {
            navigationManager.NotFound();
        }
    }
#else
    /// <summary>Signals not found, unless this instance already did since the last <see cref="Reset"/>.</summary>
    /// <param name="httpContext">The request context, or <see langword="null"/> outside static rendering.</param>
    public void Send(HttpContext? httpContext)
    {
        if (TryMarkSent() && httpContext is { Response.HasStarted: false })
        {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        }
    }
#endif

    /// <summary>Allows the next not-found result to signal again after content was found.</summary>
    public void Reset() => _sent = false;

    private bool TryMarkSent()
    {
        if (_sent)
        {
            return false;
        }

        _sent = true;
        return true;
    }
}
