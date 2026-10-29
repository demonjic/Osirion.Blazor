using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace Osirion.Blazor.Cms.Infrastructure.Caching;

/// <summary>
/// Implements the stale-while-revalidate caching pattern for content repositories
/// </summary>
public class StaleWhileRevalidateCacheDecorator : IContentRepository
{
    private readonly IContentRepository _decorated;
    private readonly ILogger _logger;
    private readonly TimeSpan _staleTime;
    private readonly TimeSpan _maxAge;
    private readonly StaleWhileRevalidateStore _store;
    private readonly string _providerIdentifier;

    /// <summary>Initializes a cache decorator with stale-while-revalidate behavior.</summary>
    public StaleWhileRevalidateCacheDecorator(
        IContentRepository decorated,
        IMemoryCache cache,
        ILogger<StaleWhileRevalidateCacheDecorator> logger,
        TimeSpan staleTime,
        TimeSpan maxAge,
        string providerIdentifier)
    {
        _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
        ArgumentNullException.ThrowIfNull(cache);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _providerIdentifier = providerIdentifier ?? throw new ArgumentNullException(nameof(providerIdentifier));
        _staleTime = staleTime;
        _maxAge = maxAge;
        _store = new StaleWhileRevalidateStore(cache, logger, staleTime, maxAge);
    }

    /// <summary>Gets all content items, using the cache when available.</summary>
    public async Task<IReadOnlyList<ContentItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = $"content:all:{_providerIdentifier}";

        return await _store.GetOrLoadAsync(
            cacheKey,
            token => _decorated.GetAllAsync(token),
            cancellationToken);
    }

    /// <summary>Gets a content item by identifier.</summary>
    public async Task<ContentItem?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("ID cannot be empty", nameof(id));

        var cacheKey = $"content:{id}:{_providerIdentifier}";

        return await _store.GetOrLoadAsync(
            cacheKey,
            token => _decorated.GetByIdAsync(id, token),
            cancellationToken);
    }

    /// <summary>Gets a content item by path.</summary>
    public async Task<ContentItem?> GetByPathAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be empty", nameof(path));

        var cacheKey = $"content:path:{path}:{_providerIdentifier}";

        return await _store.GetOrLoadAsync(
            cacheKey,
            token => _decorated.GetByPathAsync(path, token),
            cancellationToken);
    }

    /// <summary>Gets a content item by URL.</summary>
    public async Task<ContentItem?> GetByUrlAsync(string url, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be empty", nameof(url));

        var cacheKey = $"content:url:{url}:{_providerIdentifier}";

        return await _store.GetOrLoadAsync(
            cacheKey,
            token => _decorated.GetByUrlAsync(url, token),
            cancellationToken);
    }

    /// <summary>Finds content items matching a query.</summary>
    public async Task<IReadOnlyList<ContentItem>?> FindByQueryAsync(ContentQuery query, CancellationToken cancellationToken = default)
    {
        // Generate a cache key based on the query properties
        var cacheKey = GenerateQueryCacheKey(query);

        return await _store.GetOrLoadAsync(
            cacheKey,
            token => _decorated.FindByQueryAsync(query, token),
            cancellationToken);
    }

    /// <summary>Gets content items in a directory.</summary>
    public async Task<IReadOnlyList<ContentItem>> GetByDirectoryAsync(string directoryId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"content:directory:{directoryId}:{_providerIdentifier}";

        return await _store.GetOrLoadAsync(
            cacheKey,
            token => _decorated.GetByDirectoryAsync(directoryId, token),
            cancellationToken);
    }

    /// <summary>Gets translations for a content item.</summary>
    public async Task<IReadOnlyList<ContentItem>> GetTranslationsAsync(string contentId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"content:translations:{contentId}:{_providerIdentifier}";

        return await _store.GetOrLoadAsync(
            cacheKey,
            token => _decorated.GetTranslationsAsync(contentId, token),
            cancellationToken);
    }

    /// <summary>Gets all content tags.</summary>
    public async Task<IReadOnlyList<ContentTag>> GetTagsAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = $"content:tags:{_providerIdentifier}";

        return await _store.GetOrLoadAsync(
            cacheKey,
            token => _decorated.GetTagsAsync(token),
            cancellationToken);
    }

    // Write operations should invalidate the cache
    /// <summary>Saves a content item and refreshes the cache.</summary>
    public async Task<ContentItem> SaveAsync(ContentItem entity, CancellationToken cancellationToken = default)
    {
        var result = await _decorated.SaveAsync(entity, cancellationToken);
        await RefreshCacheAsync(cancellationToken);
        return result;
    }

    /// <summary>Saves a content item with a commit message.</summary>
    public async Task<ContentItem> SaveWithCommitMessageAsync(ContentItem entity, string commitMessage, CancellationToken cancellationToken = default)
    {
        var result = await _decorated.SaveWithCommitMessageAsync(entity, commitMessage, cancellationToken);
        await RefreshCacheAsync(cancellationToken);
        return result;
    }

    /// <summary>Deletes a content item and refreshes the cache.</summary>
    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await _decorated.DeleteAsync(id, cancellationToken);
        await RefreshCacheAsync(cancellationToken);
    }

    /// <summary>Deletes a content item with a commit message.</summary>
    public async Task DeleteWithCommitMessageAsync(string id, string commitMessage, CancellationToken cancellationToken = default)
    {
        await _decorated.DeleteWithCommitMessageAsync(id, commitMessage, cancellationToken);
        await RefreshCacheAsync(cancellationToken);
    }

    /// <summary>
    /// Evicts only the entries this decorator created (never the host's shared cache) and refreshes the
    /// underlying repository.
    /// </summary>
    public async Task RefreshCacheAsync(CancellationToken cancellationToken = default)
    {
        _store.Clear();
        _logger.LogInformation("Cache cleared for provider: {ProviderId}", _providerIdentifier);

        await _decorated.RefreshCacheAsync(cancellationToken);

        // Readers that loaded from the repository while it was refreshing may have cached the old state.
        _store.Clear();
    }

    /// <summary>Maximum length of a readable query key before it is replaced by a hash.</summary>
    private const int MaxReadableKeyLength = 200;

    private string GenerateQueryCacheKey(ContentQuery query)
    {
        var keyBuilder = new StringBuilder();
        keyBuilder.Append("query:").Append(_providerIdentifier).Append(':');

        AppendText(keyBuilder, "path", query.Path);
        AppendText(keyBuilder, "dir", query.Directory);
        AppendText(keyBuilder, "dirid", query.DirectoryId);
        AppendText(keyBuilder, "slug", query.Slug);
        AppendText(keyBuilder, "url", query.Url);
        AppendText(keyBuilder, "cat", query.Category);
        AppendText(keyBuilder, "tag", query.Tag);
        AppendText(keyBuilder, "search", query.SearchQuery);
        AppendText(keyBuilder, "author", query.Author);
        AppendText(keyBuilder, "locale", query.Locale);
        AppendText(keyBuilder, "l10n", query.LocalizationId);
        AppendText(keyBuilder, "provider", query.ProviderId);
        AppendSet(keyBuilder, "tags", query.Tags);
        AppendSet(keyBuilder, "cats", query.Categories);
        AppendSet(keyBuilder, "inc", query.IncludeIds);
        AppendSet(keyBuilder, "exc", query.ExcludeIds);

        if (query.IsFeatured.HasValue)
            keyBuilder.Append("featured=").Append(query.IsFeatured.Value).Append(';');
        if (query.Status.HasValue)
            keyBuilder.Append("status=").Append(query.Status.Value).Append(';');
        if (query.DateFrom.HasValue)
            keyBuilder.Append("from=").Append(query.DateFrom.Value.Ticks).Append(';');
        if (query.DateTo.HasValue)
            keyBuilder.Append("to=").Append(query.DateTo.Value.Ticks).Append(';');

        keyBuilder.Append("unpub=").Append(query.IncludeUnpublished).Append(';');
        keyBuilder.Append("subdirs=").Append(query.IncludeSubdirectories).Append(';');
        keyBuilder.Append("raw=").Append(query.IncludeRawFiles).Append(';');
        keyBuilder.Append("sort=").Append(query.SortBy).Append(',').Append(query.SortDirection).Append(';');

        if (query.Skip.HasValue)
            keyBuilder.Append("skip=").Append(query.Skip.Value).Append(';');
        if (query.Take.HasValue)
            keyBuilder.Append("take=").Append(query.Take.Value).Append(';');

        var key = keyBuilder.ToString();
        if (key.Length <= MaxReadableKeyLength)
        {
            return key;
        }

        // Caller supplied text (search terms, id lists) must not make cache keys unbounded.
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key)));
        return $"query:{_providerIdentifier}:sha256:{hash}";
    }

    // Values are length-prefixed so that a value containing the separator cannot collide with another query.
    private static void AppendText(StringBuilder builder, string name, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        builder.Append(name).Append('=').Append(value.Length).Append(':').Append(value).Append(';');
    }

    private static void AppendSet(StringBuilder builder, string name, IEnumerable<string>? values)
    {
        if (values is null)
            return;

        var ordered = values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToList();

        if (ordered.Count == 0)
            return;

        builder.Append(name).Append("=[");
        foreach (var value in ordered)
        {
            builder.Append(value.Length).Append(':').Append(value).Append(',');
        }

        builder.Append("];");
    }
}
