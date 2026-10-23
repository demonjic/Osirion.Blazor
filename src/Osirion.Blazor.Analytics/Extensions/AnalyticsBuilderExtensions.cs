using Microsoft.Extensions.DependencyInjection;
using Osirion.Blazor.Analytics.Internal;
using Osirion.Blazor.Analytics.Options;
using Osirion.Blazor.Analytics.Providers;

namespace Osirion.Blazor.Analytics.Extensions;

/// <summary>
/// Extension methods for <see cref="IAnalyticsBuilder"/>.
/// </summary>
public static class AnalyticsBuilderExtensions
{
    /// <summary>
    /// Makes every tracker wait for the visitor's analytics consent, as recorded by
    /// <c>OsirionCookieConsent</c>. Without consent no tracker script is rendered at all, or, with
    /// <see cref="AnalyticsConsentOptions.Evaluation"/> set to <c>Browser</c>, none runs.
    /// Recommended for sites with visitors in the EU.
    /// </summary>
    /// <param name="builder">The analytics builder.</param>
    /// <param name="configure">Optional action to change the consent category or where consent is evaluated.</param>
    /// <returns>The builder for chaining.</returns>
    public static IAnalyticsBuilder RequireConsent(
        this IAnalyticsBuilder builder,
        Action<AnalyticsConsentOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.Configure<AnalyticsConsentOptions>(options =>
        {
            options.RequireConsent = true;
            configure?.Invoke(options);
        });

        return builder;
    }

    /// <summary>
    /// Adds Plausible Analytics. Render <c>PlausibleTracker</c> in the page head.
    /// </summary>
    /// <param name="builder">The analytics builder.</param>
    /// <param name="configure">Sets the site's script URL and optional overrides.</param>
    /// <returns>The builder for chaining.</returns>
    public static IAnalyticsBuilder AddPlausible(this IAnalyticsBuilder builder, Action<PlausibleOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (configure is not null)
        {
            builder.Services.Configure(configure);
        }

        AnalyticsProviderRegistration.Register<PlausibleProvider, PlausibleOptions, PlausibleOptionsValidator>(builder.Services);
        return builder;
    }

    /// <summary>
    /// Adds Umami analytics. Render <c>UmamiTracker</c> in the page head.
    /// </summary>
    /// <param name="builder">The analytics builder.</param>
    /// <param name="configure">Sets the website ID and, for a self-hosted instance, the script URL.</param>
    /// <returns>The builder for chaining.</returns>
    public static IAnalyticsBuilder AddUmami(this IAnalyticsBuilder builder, Action<UmamiOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (configure is not null)
        {
            builder.Services.Configure(configure);
        }

        AnalyticsProviderRegistration.Register<UmamiProvider, UmamiOptions, UmamiOptionsValidator>(builder.Services);
        return builder;
    }
}
