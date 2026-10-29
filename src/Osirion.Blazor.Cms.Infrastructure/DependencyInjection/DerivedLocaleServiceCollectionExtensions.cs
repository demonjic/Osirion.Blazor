using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Osirion.Blazor.Cms.Domain.Options;
using Osirion.Blazor.Cms.Infrastructure.Localization;
using Osirion.Blazor.Core.Extensions;

namespace Osirion.Blazor.Cms.Infrastructure.DependencyInjection;

/// <summary>
/// Registers what a content provider needs to derive locales by transliteration. Every provider registration calls
/// it, so the repositories always find a <see cref="DerivedLocaleGenerator"/>; a provider without derived locales
/// derives nothing.
/// </summary>
internal static class DerivedLocaleServiceCollectionExtensions
{
    /// <summary>
    /// Registers the transliterators, the <see cref="DerivedLocaleGenerator"/> and the validation of
    /// <typeparamref name="TOptions"/>'s derived locales, which also runs at start-up. Safe to call more than once.
    /// </summary>
    /// <typeparam name="TOptions">The provider's options type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    internal static IServiceCollection TryAddDerivedLocales<TOptions>(this IServiceCollection services)
        where TOptions : ContentProviderOptions
    {
        services.AddOsirionTransliteration();
        services.TryAddSingleton<DerivedLocaleGenerator>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<TOptions>, DerivedLocaleOptionsValidator<TOptions>>());
        services.AddOptions<TOptions>().ValidateOnStart();

        return services;
    }
}
