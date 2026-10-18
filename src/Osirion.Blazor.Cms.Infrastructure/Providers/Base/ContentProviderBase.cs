using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Enums;
using Osirion.Blazor.Cms.Domain.Exceptions;
using Osirion.Blazor.Cms.Domain.Interfaces.Content;
using Osirion.Blazor.Cms.Domain.Models;
using Osirion.Blazor.Cms.Domain.Options;
using Osirion.Blazor.Cms.Domain.Repositories;
using Osirion.Blazor.Cms.Domain.Services;
using System.Globalization;

namespace Osirion.Blazor.Cms.Infrastructure.Providers;

/// <summary>
/// Base class for content providers (simplified - no redundant caching)
/// </summary>
public abstract class ContentProviderBase : IContentProvider, ILocalizationInfoProvider, IDisposable
{
    /// <summary>Performs the Logger operation.</summary>
    protected readonly ILogger Logger;
    private readonly ContentProviderOptions _providerOptions;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentProviderBase"/> class.
    /// </summary>
    /// <param name="memoryCache">The memory cache (kept for compatibility but not used for main data)</param>
    /// <param name="options">Provider options, read for the locales <see cref="GetLocalizationInfoAsync"/> reports</param>
    /// <param name="logger">The logger</param>
    protected ContentProviderBase(
        IMemoryCache memoryCache,
        IOptions<ContentProviderOptions> options,
        ILogger logger)
    {
        // The memory cache is kept for compatibility but not used for main caching
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _providerOptions = options?.Value ?? new ContentProviderOptions();
    }

    /// <inheritdoc/>
    public abstract string ProviderId { get; }

    /// <inheritdoc/>
    public abstract string DisplayName { get; }

    /// <inheritdoc/>
    public abstract bool IsReadOnly { get; }

    /// <inheritdoc/>
    public abstract Task<IReadOnlyList<ContentItem>> GetAllItemsAsync(CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract Task<ContentItem?> GetItemByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract Task<ContentItem?> GetItemByPathAsync(string path, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract Task<ContentItem?> GetItemByUrlAsync(string url, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract Task<IReadOnlyList<ContentItem>?> GetItemsByQueryAsync(ContentQuery query, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract Task<IReadOnlyList<ContentCategory>> GetCategoriesAsync(CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract Task<IReadOnlyList<ContentTag>> GetTagsAsync(CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract Task<IReadOnlyList<DirectoryItem>> GetDirectoriesAsync(string? locale = null, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract Task<DirectoryItem?> GetDirectoryByPathAsync(string path, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract Task<DirectoryItem?> GetDirectoryByIdAsync(string id, string? locale = null, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract Task<DirectoryItem?> GetDirectoryByUrlAsync(string url, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public virtual async Task<IReadOnlyList<ContentItem>?> GetContentTranslationsAsync(string localizationId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(localizationId))
            return Array.Empty<ContentItem>();

        try
        {
            var query = new ContentQuery { LocalizationId = localizationId };
            return await GetItemsByQueryAsync(query, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving translations for localization ID {LocalizationId}", localizationId);
            return Array.Empty<ContentItem>();
        }
    }

    /// <inheritdoc/>
    public virtual async Task<Dictionary<string, ContentItem>> GetContentTranslationsAsync(string localizationId)
    {
        if (string.IsNullOrWhiteSpace(localizationId))
        {
            return new Dictionary<string, ContentItem>();
        }

        try
        {
            var translations = await GetContentTranslationsAsync(localizationId, CancellationToken.None);
            if (translations is null)
                return new Dictionary<string, ContentItem>();

            var byLocale = new Dictionary<string, ContentItem>();
            var duplicateLocales = new SortedSet<string>(StringComparer.Ordinal);
            foreach (var item in translations)
            {
                if (!byLocale.TryAdd(item.Locale, item))
                {
                    duplicateLocales.Add(item.Locale);
                }
            }

            if (duplicateLocales.Count > 0)
            {
                Logger.LogWarning(
                    "Several items share localization ID {LocalizationId} in the locale(s) {Locales}; keeping the first of each",
                    localizationId, string.Join(", ", duplicateLocales));
            }

            return byLocale;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving translations dictionary for localization ID {LocalizationId}", localizationId);
            return new Dictionary<string, ContentItem>();
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// The locales with content are read from the items <see cref="GetAllItemsAsync"/> returns, which the built-in
    /// providers serve from the cache they have already loaded, so no extra request reaches the content source. A
    /// locale is named by its .NET culture; a code that is not a predefined culture keeps the code as its names.
    /// </remarks>
    public virtual async Task<LocalizationInfo> GetLocalizationInfoAsync(CancellationToken cancellationToken = default)
    {
        var defaultLocale = _providerOptions.DefaultLocale;
        var codes = new List<string>();
        if (!string.IsNullOrWhiteSpace(defaultLocale))
        {
            codes.Add(defaultLocale);
        }

        if (_providerOptions.EnableLocalization)
        {
            var items = await GetAllItemsAsync(cancellationToken);

            // A locale whose items are all drafts has no page a visitor could open
            var localesWithContent = items
                .Where(item => item.Status == ContentStatus.Published && !string.IsNullOrWhiteSpace(item.Locale))
                .Select(item => item.Locale)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // A list configured as empty accepts any locale folder, so the locales found are the candidates
            var supportedLocales = _providerOptions.GetEffectiveSupportedLocales();
            IEnumerable<string> candidates = supportedLocales.Count > 0
                ? supportedLocales
                : localesWithContent.Order(StringComparer.OrdinalIgnoreCase);

            foreach (var code in candidates)
            {
                if (localesWithContent.Contains(code) && !codes.Contains(code, StringComparer.OrdinalIgnoreCase))
                {
                    codes.Add(code);
                }
            }
        }

        var info = new LocalizationInfo { DefaultLocale = defaultLocale };
        foreach (var code in codes)
        {
            info.AvailableLocales.Add(DescribeLocale(code, isDefault: string.Equals(code, defaultLocale, StringComparison.OrdinalIgnoreCase)));
        }

        return info;
    }

    private static LocaleInfo DescribeLocale(string code, bool isDefault)
    {
        var locale = new LocaleInfo { Code = code, Name = code, NativeName = code, IsDefault = isDefault };
        try
        {
            // Only predefined cultures, so a folder name that merely looks like a locale is not given invented names
            var culture = CultureInfo.GetCultureInfo(code, predefinedOnly: true);
            if (!string.IsNullOrWhiteSpace(culture.EnglishName))
            {
                locale.Name = culture.EnglishName;
            }

            if (!string.IsNullOrWhiteSpace(culture.NativeName))
            {
                locale.NativeName = culture.NativeName;
            }

            locale.Direction = culture.TextInfo.IsRightToLeft ? "rtl" : "ltr";
        }
        catch (CultureNotFoundException)
        {
            // The code stays as its names, which a page can replace through its own formatter
        }

        return locale;
    }

    /// <inheritdoc/>
    public virtual async Task RefreshCacheAsync(CancellationToken cancellationToken = default)
    {
        // Base implementation does nothing - derived classes handle their own cache refresh
        Logger.LogDebug("RefreshCacheAsync called on base provider: {ProviderId}", ProviderId);
        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    public virtual async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Initializing content provider: {ProviderId}", ProviderId);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Executes an operation with exception handling
    /// </summary>
    protected async Task<T?> ExecuteWithExceptionHandlingAsync<T>(Func<Task<T?>> operation, string operationName)
    {
        try
        {
            return await operation();
        }
        catch (ContentProviderException)
        {
            // Re-throw content provider exceptions
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in {OperationName} for provider {ProviderId}: {Message}",
                operationName, ProviderId, ex.Message);

            throw new ContentProviderException(
                $"Error in {operationName} for provider {DisplayName}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Disposes resources used by the provider
    /// </summary>
    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes resources used by the provider
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            // Dispose managed resources in derived classes
        }

        _disposed = true;
    }
}
