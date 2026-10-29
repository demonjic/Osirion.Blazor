using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Repositories;
using Osirion.Blazor.Cms.Domain.Services;
using Osirion.Blazor.Core.Localization;

namespace Osirion.Blazor.Cms.Web.Redirects;

/// <summary>
/// Answers requests for an old URL with a redirect to the page's current URL. The map comes from the
/// <c>redirect_from</c> front matter of published content and from <see cref="OsirionRedirectOptions.Redirects"/>.
/// </summary>
internal sealed partial class ContentRedirectMiddleware
{
    private readonly RequestDelegate _next;
    private readonly OsirionRedirectOptions _options;
    private readonly SemaphoreSlim _rebuild = new(1, 1);
    private RedirectMap? _map;

    public ContentRedirectMiddleware(RequestDelegate next, OsirionRedirectOptions options)
    {
        _next = next;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method))
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        var path = RedirectPaths.Normalize(context.Request.Path.Value);
        if (path is null)
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        var map = await GetMapAsync(context).ConfigureAwait(false);
        if (!map.Targets.TryGetValue(path, out var target) && !TryGetUnderCulturePrefix(context, map, path, out target))
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        context.Response.StatusCode = _options.Permanent ? StatusCodes.Status301MovedPermanently : StatusCodes.Status302Found;
        context.Response.Headers.Location = BuildLocation(context.Request, target);
    }

    // A fixed redirect for /pricing answers /sr/pricing too, with a target on the site moved under the same prefix. Only
    // the fixed redirects: content lists its earlier addresses per language in its own front matter.
    private bool TryGetUnderCulturePrefix(HttpContext context, RedirectMap map, string path, out string target)
    {
        target = string.Empty;
        if (!_options.ApplyUnderCulturePrefixes
            || context.RequestServices.GetService<IOsirionLocalePaths>() is not { } localePaths
            || !localePaths.TryGetCulture(path, out var culture, out var remainder)
            || RedirectPaths.Normalize(remainder) is not { } source
            || !map.Fixed.TryGetValue(source, out var fixedTarget))
        {
            return false;
        }

        target = fixedTarget.StartsWith('/') ? localePaths.Localize(fixedTarget, culture.Culture) : fixedTarget;
        return true;
    }

    // A site path keeps the app's path base and, unless it has its own, the request's query string; an absolute URL is
    // used as given. Only the path is escaped: the target's query and #fragment are written as they are, after it, so
    // /platform#predict is not sent as /platform%23predict.
    private static string BuildLocation(HttpRequest request, string target)
    {
        if (!target.StartsWith('/'))
        {
            return target;
        }

        var fragmentStart = target.IndexOf('#', StringComparison.Ordinal);
        var fragment = fragmentStart >= 0 ? target[fragmentStart..] : string.Empty;
        var pathAndQuery = fragmentStart >= 0 ? target[..fragmentStart] : target;
        var queryStart = pathAndQuery.IndexOf('?', StringComparison.Ordinal);
        var path = queryStart >= 0 ? pathAndQuery[..queryStart] : pathAndQuery;
        var query = queryStart >= 0 ? pathAndQuery[queryStart..] : request.QueryString.ToUriComponent();

        return request.PathBase.Add(new PathString(path)).ToUriComponent() + query + fragment;
    }

    private async Task<RedirectMap> GetMapAsync(HttpContext context)
    {
        var timeProvider = context.RequestServices.GetService<TimeProvider>() ?? TimeProvider.System;
        var current = _map;
        if (current is not null && current.ExpiresAt > timeProvider.GetUtcNow())
        {
            return current;
        }

        await _rebuild.WaitAsync(context.RequestAborted).ConfigureAwait(false);
        try
        {
            current = _map;
            if (current is not null && current.ExpiresAt > timeProvider.GetUtcNow())
            {
                return current;
            }

            var logger = context.RequestServices.GetService<ILoggerFactory>()?.CreateLogger<ContentRedirectMiddleware>();
            var built = await BuildAsync(context, logger).ConfigureAwait(false);
            if (built is null)
            {
                // The content could not be read: keep serving the last good map, or the fixed redirects, without
                // caching the failure.
                var fixedRedirects = FixedRedirects();
                return current ?? new RedirectMap(fixedRedirects, fixedRedirects, DateTimeOffset.MinValue);
            }

            _map = built with { ExpiresAt = timeProvider.GetUtcNow() + _options.CacheDuration };
            return _map;
        }
        finally
        {
            _rebuild.Release();
        }
    }

    private Dictionary<string, string> FixedRedirects()
    {
        var targets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (source, target) in _options.Redirects)
        {
            if (RedirectPaths.Normalize(source) is { } path && RedirectPaths.IsValidTarget(target))
            {
                targets[path] = target;
            }
        }

        return targets;
    }

    private async Task<RedirectMap?> BuildAsync(HttpContext context, ILogger? logger)
    {
        var fixedRedirects = FixedRedirects();
        var targets = new Dictionary<string, string>(fixedRedirects, StringComparer.OrdinalIgnoreCase);
        var manager = context.RequestServices.GetService<IContentProviderManager>();
        if (manager is null)
        {
            return new RedirectMap(targets, fixedRedirects, DateTimeOffset.MinValue);
        }

        IReadOnlyList<ContentItem> items;
        try
        {
            items = await manager.GetContentByQueryAsync(new ContentQuery(), context.RequestAborted).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            if (logger is not null)
            {
                LogContentReadFailed(logger, exception);
            }

            return null;
        }

        var livePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var fromContent = new List<(string Source, string Target)>();
        foreach (var item in items)
        {
            var url = _options.ContentUrlFormatter?.Invoke(item) ?? "/" + item.Url.TrimStart('/');
            if (RedirectPaths.Normalize(url) is { } live)
            {
                livePaths.Add(live);
            }

            foreach (var old in item.Metadata?.RedirectFrom ?? [])
            {
                if (RedirectPaths.Normalize(old) is { } source)
                {
                    fromContent.Add((source, url));
                }
            }
        }

        foreach (var (source, target) in fromContent)
        {
            if (livePaths.Contains(source))
            {
                // A page that exists at the address wins over a redirect away from it.
                if (logger is not null)
                {
                    LogRedirectShadowed(logger, source);
                }

                continue;
            }

            if (!targets.TryAdd(source, target) && logger is not null)
            {
                LogDuplicateSource(logger, source, targets[source]);
            }
        }

        return new RedirectMap(targets, fixedRedirects, DateTimeOffset.MinValue);
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Could not read content to build the redirect map")]
    private static partial void LogContentReadFailed(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Ignoring redirect_from {Source}: a page is published at that address")]
    private static partial void LogRedirectShadowed(ILogger logger, string source);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Ignoring a second redirect from {Source}; it already redirects to {Target}")]
    private static partial void LogDuplicateSource(ILogger logger, string source, string target);

    private sealed record RedirectMap(Dictionary<string, string> Targets, Dictionary<string, string> Fixed, DateTimeOffset ExpiresAt);
}
