using Osirion.Blazor.Cms.Domain.Entities;

namespace Osirion.Blazor.Cms.Domain.Interfaces.Content;

/// <summary>
/// Interface for localization-related operations on content
/// </summary>
/// <remarks>The locales a provider has content in are reported by <see cref="ILocalizationInfoProvider"/>.</remarks>
public interface IContentLocalization
{
    /// <summary>
    /// Gets translations for a specific content item
    /// </summary>
    Task<IReadOnlyList<ContentItem>> GetContentTranslationsAsync(string localizationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the default locale for the provider
    /// </summary>
    string DefaultLocale { get; }

    /// <summary>
    /// Gets the supported locales for the provider
    /// </summary>
    IReadOnlyList<string> SupportedLocales { get; }
}
