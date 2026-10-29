using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using Osirion.Blazor.Core.Extensions;
using Osirion.Blazor.Internal;

namespace Osirion.Blazor.Extensions;

/// <summary>
/// Serves a derived site version's resource strings (<c>IStringLocalizer</c>) by transliteration, like its shell.
/// </summary>
public static class DerivedResourceLocalizationExtensions
{
    /// <summary>
    /// Wraps the registered <see cref="IStringLocalizerFactory"/> so that, for a culture an <c>Osirion:Site:Locales</c>
    /// entry derives (<c>"sr-Latn": { "DerivedFrom": "sr", "Transliteration": "serbian-cyrillic-to-latin" }</c>), a
    /// localizer returns the source culture's string transliterated, unless the resource has its own value for the
    /// derived culture (a <c>.sr-Latn.resx</c> entry). Placeholders such as <c>{0}</c> are kept. Every other culture is
    /// answered as before, and a site that does not call this method is not changed.
    /// </summary>
    /// <remarks>
    /// Call it after <c>AddLocalization</c> when the site sets localization options; without one, it adds the default
    /// resource localization. Safe to call more than once. The derived cultures are read from the site options, so the
    /// shell and the resources of a version are derived in one place.
    /// </remarks>
    /// <param name="services">The service collection.</param>
    /// <returns>The same <paramref name="services"/>, for chaining.</returns>
    public static IServiceCollection AddOsirionDerivedResourceLocalization(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (services.Any(descriptor => descriptor.ServiceType == typeof(DerivedResourceCultures)))
        {
            return services;
        }

        services.AddLocalization();
        services.AddOsirionTransliteration();
        services.TryAddSingleton<DerivedResourceCultures>();

        var inner = services.Last(descriptor => descriptor.ServiceType == typeof(IStringLocalizerFactory) && !descriptor.IsKeyedService);
        services.Remove(inner);
        services.Add(ServiceDescriptor.Describe(
            typeof(IStringLocalizerFactory),
            provider => new DerivedCultureStringLocalizerFactory(Create(provider, inner), provider.GetRequiredService<DerivedResourceCultures>()),
            inner.Lifetime));

        return services;
    }

    private static IStringLocalizerFactory Create(IServiceProvider provider, ServiceDescriptor descriptor) =>
        (IStringLocalizerFactory)(descriptor.ImplementationInstance
            ?? descriptor.ImplementationFactory?.Invoke(provider)
            ?? ActivatorUtilities.CreateInstance(provider, descriptor.ImplementationType!));
}
