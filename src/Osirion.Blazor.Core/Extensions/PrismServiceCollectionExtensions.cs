using Microsoft.Extensions.DependencyInjection;
using Osirion.Blazor.Core.Configuration;

namespace Osirion.Blazor.Core.Extensions;

/// <summary>
/// Extension methods for choosing where the HTML renderer loads Prism from.
/// </summary>
public static class PrismServiceCollectionExtensions
{
    /// <summary>
    /// Configures <see cref="PrismOptions"/> for every <c>OsirionHtmlRenderer</c> of the site. The options are
    /// validated when the application starts.
    /// </summary>
    /// <example>
    /// Serve Prism from the site, so the Content Security Policy can keep <c>script-src 'self'</c>:
    /// <code>
    /// builder.Services.AddOsirionPrism(options =&gt; options.BaseUrl = "/lib/prism/1.29.0/");
    /// </code>
    /// </example>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Sets the options, for example <see cref="PrismOptions.BaseUrl"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOsirionPrism(this IServiceCollection services, Action<PrismOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddOptions<PrismOptions>()
            .Configure(configure)
            .Validate(
                options => PrismOptions.IsValidBaseUrl(options.BaseUrl),
                "Osirion PrismOptions.BaseUrl must be empty, a path on the site such as /lib/prism/1.29.0/, or an https address.")
            .ValidateOnStart();

        return services;
    }
}
