using Osirion.Blazor.Cms.Domain.Models;

namespace Osirion.Blazor.Cms.Domain.Interfaces.Content;

/// <summary>
/// Reports the locales a content provider serves, so a component such as a language selector can list them.
/// </summary>
/// <remarks>
/// The built-in providers implement it through <c>ContentProviderBase</c>. It is a separate interface rather than a
/// member of <see cref="IContentLocalization"/> or <c>IContentProvider</c>, so implementations written against those
/// interfaces keep compiling.
/// </remarks>
public interface ILocalizationInfoProvider
{
    /// <summary>
    /// Gets the default locale and the locales that have content. With localization enabled, the list holds the
    /// default locale first, then each configured supported locale that has published content, in the configured
    /// order (or, when the supported locales are configured as an empty list, each locale found, in ordinal order).
    /// With localization disabled, it holds only the default locale.
    /// </summary>
    /// <param name="cancellationToken">The token that cancels loading the content the list is computed from.</param>
    /// <returns>The localization information of the provider.</returns>
    Task<LocalizationInfo> GetLocalizationInfoAsync(CancellationToken cancellationToken = default);
}
