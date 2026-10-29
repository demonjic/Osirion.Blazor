using Microsoft.Extensions.Logging;
using Osirion.Blazor.Cms.Domain.ContentBlocks;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Enums;
using Osirion.Blazor.Cms.Domain.Exceptions;
using Osirion.Blazor.Cms.Domain.Interfaces;
using Osirion.Blazor.Cms.Domain.Interfaces.Content;
using Osirion.Blazor.Cms.Domain.Repositories;
using Osirion.Blazor.Cms.Domain.Options;
using Osirion.Blazor.Cms.Domain.ValueObjects;
using Osirion.Blazor.Cms.Infrastructure.Localization;
using Osirion.Blazor.Cms.Infrastructure.Utilities;
using System.Globalization;
using System.Text;

namespace Osirion.Blazor.Cms.Infrastructure.Repositories;

/// <summary>
/// Base implementation for content repositories with simplified caching
/// </summary>
public abstract class BaseContentRepository : RepositoryBase<ContentItem, string>, IContentRepository
{
    /// <summary>Gets the markdown processor used by the repository.</summary>
    protected readonly IMarkdownProcessor MarkdownProcessor;
    /// <summary>Gets the content query filter.</summary>
    protected readonly IContentQueryFilter QueryFilter;
    /// <summary>Gets the lock protecting cache updates.</summary>
    protected readonly SemaphoreSlim CacheLock = new(1, 1);

    // Simplified cache - no time-based expiration
    /// <summary>Gets or sets the cached content items.</summary>
    protected volatile Dictionary<string, ContentItem> ItemCache = new();
    /// <summary>Gets or sets whether the cache has been loaded.</summary>
    protected volatile bool CacheLoaded = false;

    // Configuration properties set by derived classes
    /// <summary>Gets or sets whether localization is enabled.</summary>
    protected bool EnableLocalization = false;
    /// <summary>Gets or sets the default content locale.</summary>
    protected string DefaultLocale = "en";
    /// <summary>Gets or sets the content path.</summary>
    protected string ContentPath = string.Empty;
    /// <summary>Gets or sets the supported content locales.</summary>
    protected List<string> SupportedLocales = new() { "en" };

    // Single-flight load state: the load that is running (null when idle) and whether another run was requested meanwhile
    private readonly object _loadGate = new();
    private Task? _loadInFlight;
    private bool _rerunRequested;

    private volatile bool _lastRefreshSucceeded = true;
    private DateTime _coldRetryNotBeforeUtc = DateTime.MinValue;
    private static readonly TimeSpan ColdLoadRetryDelay = TimeSpan.FromSeconds(15);

    // Set by a repository whose provider can derive locales; null derives nothing
    private DerivedLocaleGenerator? _derivedLocaleGenerator;
    private ContentProviderOptions? _derivedLocaleOptions;

    /// <summary>Initializes a repository with its content services.</summary>
    protected BaseContentRepository(
        string providerId,
        IMarkdownProcessor markdownProcessor,
        IContentQueryFilter queryFilter,
        ILogger logger)
        : base(providerId, logger)
    {
        MarkdownProcessor = markdownProcessor ?? throw new ArgumentNullException(nameof(markdownProcessor));
        QueryFilter = queryFilter ?? throw new ArgumentNullException(nameof(queryFilter));
    }

    /// <summary>Loads content items into the repository cache.</summary>
    protected abstract Task<Dictionary<string, ContentItem>> LoadItemsIntoCache(CancellationToken cancellationToken);

    /// <summary>
    /// Makes every load add the items of the provider's <see cref="ContentProviderOptions.DerivedLocales"/>, after
    /// <see cref="LoadItemsIntoCache"/> has read the files. Validates the derived locales first.
    /// </summary>
    /// <typeparam name="TOptions">The provider's options type.</typeparam>
    /// <param name="generator">Makes the derived items.</param>
    /// <param name="options">The provider options.</param>
    /// <exception cref="Microsoft.Extensions.Options.OptionsValidationException">Thrown when a derived locale breaks a rule.</exception>
    private protected void UseDerivedLocales<TOptions>(DerivedLocaleGenerator generator, TOptions options)
        where TOptions : ContentProviderOptions
    {
        ArgumentNullException.ThrowIfNull(generator);
        ArgumentNullException.ThrowIfNull(options);

        generator.ThrowIfInvalid(options);
        _derivedLocaleGenerator = generator;
        _derivedLocaleOptions = options;
    }

    /// <summary>
    /// Refuses to write or delete an item derived from another locale: it has no file, and saving it would turn the
    /// transliteration into a file that then wins over every later change of its source.
    /// </summary>
    /// <param name="item">The item to write or delete.</param>
    /// <exception cref="ContentProviderException">Thrown when the item is derived.</exception>
    private protected void ThrowIfDerived(ContentItem item)
    {
        if (item.IsDerived)
        {
            throw new ContentProviderException(
                $"The item '{item.Path}' is derived from the locale '{item.DerivedFromLocale}' and is read only; change its source item instead.",
                ProviderId);
        }
    }

    /// <inheritdoc/>
    public override async Task<IReadOnlyList<ContentItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureCacheIsLoaded(cancellationToken);
            return ItemCache.Values.ToList();
        }
        catch (Exception ex)
        {
            LogError(ex, "getting all items", ProviderId);
            return Array.Empty<ContentItem>();
        }
    }

    /// <inheritdoc/>
    public override async Task<ContentItem?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("ID cannot be empty", nameof(id));

        try
        {
            await EnsureCacheIsLoaded(cancellationToken);

            if (ItemCache.TryGetValue(id, out var item))
            {
                return item;
            }

            return null;
        }
        catch (Exception ex)
        {
            LogError(ex, "getting item by ID", id);
            return null;
        }
    }

    /// <summary>Gets whether the most recent cache load succeeded. A failed load keeps the previous cache.</summary>
    public bool LastRefreshSucceeded => _lastRefreshSucceeded;

    /// <summary>
    /// Refreshes the repository cache from its content provider.
    /// </summary>
    /// <remarks>
    /// Refreshes are single-flight. A request that arrives while a load is running does not start a second
    /// concurrent load and is not dropped: it schedules exactly one more run after the current one, so a webhook
    /// for a newer commit is always reflected. The returned task completes when that run has finished.
    /// A failed run keeps the previous cache and is logged.
    /// </remarks>
    public Task RefreshCacheAsync(CancellationToken cancellationToken = default)
    {
        return RequestLoadAsync(forceRefresh: true, cancellationToken);
    }

    /// <summary>
    /// Ensures the cache is loaded. Concurrent callers on a cold cache all wait for the same load, so none of
    /// them reads an empty cache while the first load is still running.
    /// </summary>
    protected virtual Task EnsureCacheIsLoaded(CancellationToken cancellationToken, bool forceRefresh = false)
    {
        if (!forceRefresh && CacheLoaded)
        {
            return Task.CompletedTask;
        }

        return RequestLoadAsync(forceRefresh, cancellationToken);
    }

    private Task RequestLoadAsync(bool forceRefresh, CancellationToken cancellationToken)
    {
        // A caller that has already given up must not start or extend a load, and must see its cancellation even when a
        // load has just finished: WaitAsync returns a completed task without looking at the token.
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

        Task flight;
        lock (_loadGate)
        {
            if (_loadInFlight is null)
            {
                if (!forceRefresh && CacheLoaded)
                {
                    return Task.CompletedTask;
                }

                // A cold load that just failed is not retried by every reader; a refresh request still is.
                if (!forceRefresh && DateTime.UtcNow < _coldRetryNotBeforeUtc)
                {
                    return Task.CompletedTask;
                }

                var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                _loadInFlight = completion.Task;
                _rerunRequested = false;
                _ = Task.Run(() => RunLoadLoopAsync(completion));
            }
            else if (forceRefresh)
            {
                // The running load may have read the source before the change that triggered this request.
                _rerunRequested = true;
            }

            flight = _loadInFlight;
        }

        return flight.WaitAsync(cancellationToken);
    }

    private async Task RunLoadLoopAsync(TaskCompletionSource completion)
    {
        try
        {
            while (true)
            {
                try
                {
                    // The load is shared by every waiter, so it must not observe one caller's cancellation.
                    await LoadAndAssignCache(CancellationToken.None).ConfigureAwait(false);
                    _lastRefreshSucceeded = true;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error loading content cache for provider {ProviderId}; keeping the previous cache", ProviderId);
                    _lastRefreshSucceeded = false;

                    if (!CacheLoaded)
                    {
                        lock (_loadGate)
                        {
                            _coldRetryNotBeforeUtc = DateTime.UtcNow + ColdLoadRetryDelay;
                        }
                    }
                }

                lock (_loadGate)
                {
                    if (!_rerunRequested)
                    {
                        _loadInFlight = null;
                        break;
                    }

                    _rerunRequested = false;
                }
            }
        }
        finally
        {
            completion.TrySetResult();
        }
    }

    /// <summary>
    /// Loads items into the cache and assigns them. A failed load leaves the previous cache untouched.
    /// </summary>
    private async Task LoadAndAssignCache(CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        Logger.LogInformation("Loading content cache for provider {ProviderId}. Current cache has {ItemCount} items",
            ProviderId, ItemCache.Count);

        var newCache = await LoadItemsIntoCache(cancellationToken).ConfigureAwait(false);

        if (newCache is null)
        {
            Logger.LogWarning("LoadItemsIntoCache returned null for provider {ProviderId}, using empty dictionary", ProviderId);
            newCache = new Dictionary<string, ContentItem>();
        }

        if (_derivedLocaleGenerator is not null && _derivedLocaleOptions is not null)
        {
            _derivedLocaleGenerator.AddDerivedItems(newCache, _derivedLocaleOptions, ContentPath);
        }

        ItemCache = newCache;
        CacheLoaded = true;

        Logger.LogInformation("Cache loaded successfully for provider {ProviderId} in {Duration}ms. Items count: {ItemCount}",
            ProviderId, (DateTime.UtcNow - startTime).TotalMilliseconds, newCache.Count);
    }

    /// <inheritdoc/>
    public async Task<ContentItem?> GetByPathAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be empty", nameof(path));

        try
        {
            await EnsureCacheIsLoaded(cancellationToken);

            var normalizedPath = UrlGenerator.NormalizePath(path);
            return ItemCache.Values.FirstOrDefault(c => UrlGenerator.NormalizePath(c.Path) == normalizedPath);
        }
        catch (Exception ex)
        {
            LogError(ex, "getting item by path", path);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<ContentItem?> GetByUrlAsync(string url, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be empty", nameof(url));

        try
        {
            await EnsureCacheIsLoaded(cancellationToken);

            return ItemCache.Values.FirstOrDefault(c => c.Url == url);
        }
        catch (Exception ex)
        {
            LogError(ex, "getting item by URL", url);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ContentItem>?> FindByQueryAsync(ContentQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureCacheIsLoaded(cancellationToken);

            var filteredItems = ItemCache.Values.AsQueryable();

            // Apply query filters, sorting, and pagination - all handled by QueryFilter
            filteredItems = QueryFilter.ApplyFilters(filteredItems, query);

            return filteredItems.ToList();
        }
        catch (Exception ex)
        {
            LogError(ex, "finding items by query", ProviderId);
            return Array.Empty<ContentItem>();
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ContentItem>> GetByDirectoryAsync(string directoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureCacheIsLoaded(cancellationToken);

            return ItemCache.Values
                .Where(item => item.Directory is not null && item.Directory.Id == directoryId)
                .ToList();
        }
        catch (Exception ex)
        {
            LogError(ex, "getting items by directory", directoryId);
            return Array.Empty<ContentItem>();
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ContentItem>> GetTranslationsAsync(string contentId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(contentId))
            return new List<ContentItem>();

        try
        {
            await EnsureCacheIsLoaded(cancellationToken);

            return ItemCache.Values
                .Where(item => item.ContentId == contentId)
                .ToList();
        }
        catch (Exception ex)
        {
            LogError(ex, "getting translations", contentId);
            return Array.Empty<ContentItem>();
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ContentTag>> GetTagsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureCacheIsLoaded(cancellationToken);

            return ItemCache.Values
                .SelectMany(item => item.Tags)
                .GroupBy(tag => tag.ToLowerInvariant())
                .Select(group => ContentTag.Create(
                    name: group.First(),
                    slug: UrlGenerator.GenerateSlug(group.First()),
                    count: group.Count()))
                .OrderBy(tag => tag.Name)
                .ToList();
        }
        catch (Exception ex)
        {
            LogError(ex, "getting tags", ProviderId);
            return Array.Empty<ContentTag>();
        }
    }

    /// <inheritdoc/>
    public override async Task<ContentItem> SaveAsync(ContentItem entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        // Use default commit message for compatibility with version-controlled systems
        var commitMessage = string.IsNullOrWhiteSpace(entity.ProviderSpecificId)
            ? $"Create {Path.GetFileName(entity.Path)}"
            : $"Update {Path.GetFileName(entity.Path)}";

        return await SaveWithCommitMessageAsync(entity, commitMessage, cancellationToken);
    }

    /// <inheritdoc/>
    public override async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var commitMessage = $"Delete content {id}";
        await DeleteWithCommitMessageAsync(id, commitMessage, cancellationToken);
    }

    /// <summary>
    /// Saves a content item with a commit message
    /// </summary>
    public abstract Task<ContentItem> SaveWithCommitMessageAsync(ContentItem entity, string commitMessage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a content item with a commit message
    /// </summary>
    public abstract Task DeleteWithCommitMessageAsync(string id, string commitMessage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates the id of the content item stored at <paramref name="path"/> in this provider.
    /// </summary>
    /// <remarks>
    /// The id is a hash of the path and the provider id that is the same in every process and on every server
    /// instance, so caches, links and lookups by id survive restarts and scale-out.
    /// </remarks>
    /// <param name="path">The item's path relative to the provider root, with <c>/</c> separators.</param>
    /// <returns>A 16-character lowercase hexadecimal id.</returns>
    protected string CreateItemId(string path)
    {
        return IdGenerator.GenerateStableId(path, ProviderId);
    }

    /// <summary>
    /// Gives an item whose front matter has no <c>id</c> a localization id derived from its path without the locale
    /// segment and the file extension, so all translations of the same content share it.
    /// </summary>
    /// <remarks>
    /// The provider id is left out on purpose: the admin writes this id into the front matter when it saves an item,
    /// and translations that were never saved must still compute the same value if the provider id changes.
    /// </remarks>
    /// <param name="contentItem">The item to complete.</param>
    protected void AssignDefaultLocalizationId(ContentItem contentItem)
    {
        if (!EnableLocalization || !string.IsNullOrWhiteSpace(contentItem.ContentId))
            return;

        var pathWithoutLocale = UrlGenerator.RemoveLocaleFromPath(contentItem.Path, ContentPath, EnableLocalization, SupportedLocales);
        var pathWithoutExtension = Path.ChangeExtension(pathWithoutLocale, null);
        if (string.IsNullOrWhiteSpace(pathWithoutExtension))
            return;

        contentItem.SetContentId(IdGenerator.GenerateStableId(pathWithoutExtension));
    }

    // A front matter without a title has an empty one, which SetTitle rejects, so the page was dropped. A folder's
    // _index.md takes the folder's name; any other file keeps the file name the item was created with.
    private static string ResolveTitle(ContentItem contentItem)
    {
        var frontMatterTitle = contentItem.Metadata?.Title;
        if (!string.IsNullOrWhiteSpace(frontMatterTitle))
        {
            return frontMatterTitle;
        }

        var segments = contentItem.Path.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        var isSectionPage = segments.Length > 1
            && string.Equals(Path.GetFileNameWithoutExtension(segments[^1]), "_index", StringComparison.OrdinalIgnoreCase);

        return isSectionPage ? segments[^2] : contentItem.Title;
    }

    /// <summary>
    /// Processes a markdown file to create a content item
    /// </summary>
    protected Task ProcessMarkdownAsync(string markdown, ContentItem contentItem, CancellationToken cancellationToken) =>
        ProcessMarkdownAsync(markdown, contentItem, sourceLastModified: null, cancellationToken);

    /// <summary>
    /// Processes a markdown file to create a content item.
    /// </summary>
    /// <param name="markdown">The file's markdown, with its front matter.</param>
    /// <param name="contentItem">The item to fill.</param>
    /// <param name="sourceLastModified">
    /// When the source last changed (a file's write time), used when the front matter has no <c>last_modified</c>.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    protected async Task ProcessMarkdownAsync(string markdown, ContentItem contentItem, DateTime? sourceLastModified, CancellationToken cancellationToken)
    {
        try
        {
            var result = MarkdownProcessor.ExtractFrontMatterAndContent(markdown);
            // Files excluded from markdown rendering are stored as HTML, so they are sanitized as HTML
            var html = contentItem.IsRawFile
                ? MarkdownProcessor.SanitizeHtml(result.Content)
                : await MarkdownProcessor.RenderToHtmlAsync(result.Content, sanitizeHtml: true);

            contentItem.Metadata = result.FrontMatter;
            // A block that breaks the contract throws, so the item is left out like a file with broken YAML
            contentItem.SetBlocks(ContentBlockParser.Parse(result.FrontMatter, contentItem.Path));
            // Keep the source next to the sanitized HTML so a save writes the original markdown back,
            // not a conversion of the sanitized HTML (SetContent alone resets the source to null)
            contentItem.SetContent(html, result.Content);

            // Map all FrontMatter properties to ContentItem (in order of FrontMatter class)
            contentItem.SetContentId(contentItem.Metadata?.Id ?? contentItem.ContentId);
            contentItem.SetOrderIndex(contentItem.Metadata?.Order ?? contentItem.OrderIndex);
            // Layout and Permalink are stored in Metadata only
            contentItem.SetTitle(ResolveTitle(contentItem));
            contentItem.SetDescription(contentItem.Metadata?.Description ?? contentItem.Description);
            contentItem.SetAuthor(contentItem.Metadata?.Author ?? contentItem.Author);
            contentItem.SetFeaturedImage(contentItem.Metadata?.FeaturedImage ?? contentItem.FeaturedImageUrl);
            contentItem.SetFeatured(contentItem.Metadata?.IsFeatured ?? contentItem.IsFeatured);
            contentItem.SetSlug(contentItem.Metadata?.Slug ?? contentItem.Slug);
            // The item already has the locale of its folder or the provider's default, which a missing lang keeps
            if (!string.IsNullOrWhiteSpace(contentItem.Metadata?.Lang))
            {
                contentItem.SetLocale(contentItem.Metadata.Lang);
            }

            // Map Published to Status and IsPublished
            var isPublished = contentItem.Metadata?.Published ?? true;
            contentItem.IsPublished = isPublished;
            contentItem.SetStatus(isPublished ? ContentStatus.Published : ContentStatus.Draft);

            // CustomFields and SeoProperties are stored in Metadata only

            // Parse and set date
            var createdDate = ParseUtcDate(contentItem.Metadata?.Date) ?? contentItem.DateCreated;
            contentItem.SetCreatedDate(createdDate);

            // Add categories
            if (contentItem.Metadata?.Categories is not null && contentItem.Metadata.Categories.Any())
            {
                foreach (var category in contentItem.Metadata.Categories)
                {
                    contentItem.AddCategory(category);
                }
            }

            // Add tags
            if (contentItem.Metadata?.Tags is not null && contentItem.Metadata.Tags.Any())
            {
                foreach (var tag in contentItem.Metadata.Tags)
                {
                    contentItem.AddTag(tag);
                }
            }

            // Last, because every setter above moves LastModified to now. The front matter's last_modified is what the
            // author declared, so it wins over the source's date; without either, the item was last changed when made.
            contentItem.SetLastModifiedDate(ParseUtcDate(contentItem.Metadata?.LastModified) ?? sourceLastModified ?? createdDate);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing markdown for {Path}", contentItem.Path);
            throw;
        }
    }

    private static DateTime? ParseUtcDate(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed)
            ? parsed
            : null;

    /// <summary>
    /// Generates markdown content with front matter for saving
    /// </summary>
    /// <remarks>Blocks the calling thread while the HTML is converted. Use <see cref="GenerateMarkdownWithFrontMatterAsync"/>.</remarks>
    [Obsolete("Use GenerateMarkdownWithFrontMatterAsync, which does not block the calling thread.")]
    protected string GenerateMarkdownWithFrontMatter(ContentItem entity)
    {
        return GenerateMarkdownWithFrontMatterAsync(entity, CancellationToken.None).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Generates markdown content with front matter for saving
    /// </summary>
    /// <param name="entity">The item to serialize.</param>
    /// <param name="cancellationToken">Cancels the HTML to markdown conversion.</param>
    protected async Task<string> GenerateMarkdownWithFrontMatterAsync(ContentItem entity, CancellationToken cancellationToken = default)
    {
        var markdown = new StringBuilder();

        if (entity.Metadata is not null)
        {
            CopyEntityToMetadata(entity, entity.Metadata);
            markdown.Append(entity.Metadata.ToYaml());
        }
        else
        {
            AppendFrontMatter(markdown, entity);
        }

        markdown.Append(await GetMarkdownBodyAsync(entity, cancellationToken).ConfigureAwait(false));

        return markdown.ToString();
    }

    private static void CopyEntityToMetadata(ContentItem entity, FrontMatter metadata)
    {
        metadata.Id = entity.ContentId;
        metadata.Order = entity.OrderIndex;
        metadata.Title = entity.Title;
        metadata.Description = entity.Description;
        metadata.Author = entity.Author;
        metadata.Date = entity.DateCreated.ToString("yyyy-MM-dd");
        metadata.FeaturedImage = entity.FeaturedImageUrl;
        metadata.IsFeatured = entity.IsFeatured;
        metadata.Slug = entity.Slug;
        metadata.Lang = entity.Locale;
        metadata.Published = entity.IsPublished;
        metadata.Categories = entity.Categories.ToList();
        metadata.Tags = entity.Tags.ToList();
    }

    // Writes the front matter by hand for items that were never read with metadata. Keys keep this order.
    private void AppendFrontMatter(StringBuilder markdown, ContentItem entity)
    {
        markdown.AppendLine("---");

        AppendQuotedValue(markdown, "id", entity.ContentId);

        if (entity.OrderIndex > 0)
            markdown.AppendLine($"order: {entity.OrderIndex}");

        AppendQuotedValue(markdown, "title", entity.Title);
        AppendQuotedValue(markdown, "description", entity.Description);
        AppendQuotedValue(markdown, "author", entity.Author);
        markdown.AppendLine($"date: {entity.DateCreated:yyyy-MM-dd}");
        AppendQuotedValue(markdown, "featured_image", entity.FeaturedImageUrl);
        AppendQuotedList(markdown, "categories", entity.Categories);
        AppendQuotedList(markdown, "tags", entity.Tags);

        if (entity.IsFeatured)
            markdown.AppendLine("is_featured: true");

        if (!entity.IsPublished)
            markdown.AppendLine("published: false");

        AppendQuotedValue(markdown, "slug", entity.Slug);
        AppendQuotedValue(markdown, "lang", entity.Locale);

        markdown.AppendLine("---");
        markdown.AppendLine();
    }

    private void AppendQuotedValue(StringBuilder markdown, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            markdown.AppendLine($"{key}: \"{EscapeYamlString(value)}\"");
    }

    private void AppendQuotedList(StringBuilder markdown, string key, IReadOnlyList<string> values)
    {
        if (values.Count == 0)
            return;

        markdown.AppendLine($"{key}:");
        foreach (var value in values)
            markdown.AppendLine($"  - \"{EscapeYamlString(value)}\"");
    }

    // The original markdown is preferred; the HTML is converted only for items that never had a markdown source.
    private async Task<string> GetMarkdownBodyAsync(ContentItem entity, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(entity.OriginalMarkdown))
            return entity.OriginalMarkdown;

        if (string.IsNullOrWhiteSpace(entity.Content))
            return string.Empty;

        return await MarkdownProcessor.ConvertHtmlToMarkdownAsync(entity.Content, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Determines whether a key is a standard front matter key
    /// </summary>
    protected bool IsStandardFrontMatterKey(string key)
    {
        var standardKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "title", "author", "description", "date", "last_modified", "date_modified",
            "slug", "featured", "is_featured", "featured_image", "feature_image", "image",
            "content_id", "localization_id", "locale", "language", "status",
            "categories", "category", "tags", "tag"
        };

        return standardKeys.Contains(key);
    }

    /// <summary>
    /// Escapes special characters in a YAML string
    /// </summary>
    protected string EscapeYamlString(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var escaped = new StringBuilder(value.Length + 8);
        foreach (var character in value)
        {
            switch (character)
            {
                case '\\':
                    escaped.Append("\\\\");
                    break;
                case '"':
                    escaped.Append("\\\"");
                    break;
                case '\n':
                    escaped.Append("\\n");
                    break;
                case '\r':
                    escaped.Append("\\r");
                    break;
                case '\t':
                    escaped.Append("\\t");
                    break;
                // Other control characters and the Unicode line separators would end the quoted scalar or its line
                case var control when char.IsControl(control) || (int)control is 0x2028 or 0x2029:
                    escaped.Append("\\u").Append(((int)control).ToString("X4", CultureInfo.InvariantCulture));
                    break;
                default:
                    escaped.Append(character);
                    break;
            }
        }

        return escaped.ToString();
    }
}
