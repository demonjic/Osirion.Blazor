using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Osirion.Blazor.Cms.Web.ContentBlocks;

namespace Osirion.Blazor.Cms.Web.DependencyInjection;

/// <summary>Registers how content blocks render.</summary>
public static class ContentBlockServiceCollectionExtensions
{
    /// <summary>
    /// Registers the <see cref="ContentBlockRegistry"/> with the library's renderers and the ones
    /// <paramref name="configure"/> adds. Call it again to add more; each call's configuration applies in order. Without
    /// this call, blocks render with the library's renderers alone.
    /// </summary>
    /// <example>
    /// <code>
    /// builder.Services.AddOsirionContentBlocks(blocks => blocks
    ///     .AddRenderer&lt;PricingTableRenderer&gt;("pricing-table")
    ///     .AddComponent&lt;ProductTour&gt;("product-tour")
    ///     .UseFallbackRenderer&lt;UnknownBlockRenderer&gt;());
    /// </code>
    /// </example>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Adds renderers, components and a fallback renderer.</param>
    /// <returns>The service collection, for chaining.</returns>
    public static IServiceCollection AddOsirionContentBlocks(this IServiceCollection services, Action<ContentBlockOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = services.AddOptions<ContentBlockOptions>();
        if (configure is not null)
        {
            options.Configure(configure);
        }

        services.TryAddSingleton(provider => new ContentBlockRegistry(provider.GetRequiredService<IOptions<ContentBlockOptions>>().Value));
        return services;
    }
}
