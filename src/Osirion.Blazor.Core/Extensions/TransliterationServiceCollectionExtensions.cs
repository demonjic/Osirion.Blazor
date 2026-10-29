using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Osirion.Blazor.Core.Transliteration;

namespace Osirion.Blazor.Core.Extensions;

/// <summary>
/// Extension methods for registering text transliterators.
/// </summary>
public static class TransliterationServiceCollectionExtensions
{
    /// <summary>
    /// Registers the built-in transliterators (<see cref="SerbianCyrillicToLatinTransliterator"/>) and
    /// <see cref="TextTransliterators"/>, which resolves a transliterator by its name.
    /// </summary>
    /// <remarks>
    /// Safe to call more than once. A site adds its own transliterator with
    /// <c>services.AddSingleton&lt;ITextTransliterator, MyTransliterator&gt;()</c>; registered after this call under a
    /// built-in name, it replaces the built-in one.
    /// </remarks>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOsirionTransliteration(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddEnumerable(ServiceDescriptor.Singleton<ITextTransliterator, SerbianCyrillicToLatinTransliterator>());
        services.TryAddSingleton<TextTransliterators>();

        return services;
    }
}
