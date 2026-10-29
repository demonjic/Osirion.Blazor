using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Exceptions;
using Osirion.Blazor.Cms.Domain.Interfaces;
using Osirion.Blazor.Cms.Domain.Interfaces.Content;
using Osirion.Blazor.Cms.Domain.Models.GitHub;
using Osirion.Blazor.Cms.Domain.Options;
using Osirion.Blazor.Cms.Domain.Repositories;
using Osirion.Blazor.Cms.Infrastructure.Localization;
using Osirion.Blazor.Cms.Infrastructure.Repositories;
using Osirion.Blazor.Cms.Infrastructure.Utilities;
using System.IO;

namespace Osirion.Blazor.Cms.Infrastructure.GitHub;

/// <summary>
/// Repository implementation for GitHub content
/// </summary>
public class GitHubContentRepository : BaseContentRepository
{
    private readonly IGitHubApiClient _apiClient;
    private readonly IDirectoryRepository _directoryRepository;
    private readonly GitHubOptions _options;
    private string _lastKnownCommitSha = string.Empty;
    private DateTimeOffset? _rateLimitedUntil;

    /// <summary>Performs the GitHubContentRepository operation.</summary>
    public GitHubContentRepository(
        IGitHubApiClientFactory apiClientFactory,
        IMarkdownProcessor markdownProcessor,
        IContentQueryFilter queryFilter,
        IOptions<GitHubOptions> options,
        IDirectoryRepository directoryRepository,
        ILogger<GitHubContentRepository> logger,
        string? providerName = null)
        : base(GetProviderId(options.Value, providerName), markdownProcessor, queryFilter, logger)
    {
        _directoryRepository = directoryRepository ?? throw new ArgumentNullException(nameof(directoryRepository));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        // Set base class properties
        EnableLocalization = _options.EnableLocalization;
        DefaultLocale = _options.DefaultLocale;
        SupportedLocales = _options.GetEffectiveSupportedLocales();
        ContentPath = _options.ContentPath;

        // Get the API client from factory
        if (!string.IsNullOrWhiteSpace(providerName))
        {
            _apiClient = apiClientFactory.GetClient(providerName);
        }
        else
        {
            _apiClient = apiClientFactory.GetDefaultClient();
        }

        // Configure API client
        _apiClient.SetRepository(_options.Owner, _options.Repository);
        _apiClient.SetBranch(_options.Branch);

        if (!string.IsNullOrWhiteSpace(_options.ApiToken))
        {
            _apiClient.SetAccessToken(_options.ApiToken);
        }
    }

    /// <summary>
    /// Initializes a repository that adds the items of the provider's
    /// <see cref="ContentProviderOptions.DerivedLocales"/> on every load.
    /// </summary>
    /// <exception cref="OptionsValidationException">Thrown when a derived locale breaks a rule.</exception>
    public GitHubContentRepository(
        IGitHubApiClientFactory apiClientFactory,
        IMarkdownProcessor markdownProcessor,
        IContentQueryFilter queryFilter,
        IOptions<GitHubOptions> options,
        IDirectoryRepository directoryRepository,
        ILogger<GitHubContentRepository> logger,
        DerivedLocaleGenerator derivedLocales,
        string? providerName = null)
        : this(apiClientFactory, markdownProcessor, queryFilter, options, directoryRepository, logger, providerName)
    {
        UseDerivedLocales(derivedLocales, _options);
    }

    private static string GetProviderId(GitHubOptions options, string? providerName)
    {
        return options.ProviderId ?? $"github-{providerName}-{options.Owner}-{options.Repository}";
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Reads the whole branch with one Git Trees request and downloads only the files whose blob SHA differs from the
    /// cached item. When the tree is unavailable or truncated, the directories are listed instead (still skipping files
    /// whose SHA did not change). A failed load throws, so the previous cache stays in place; a rate-limit response
    /// stops the load and no further request is made before the limit resets.
    /// </remarks>
    protected override async Task<Dictionary<string, ContentItem>> LoadItemsIntoCache(CancellationToken cancellationToken)
    {
        ThrowIfStillRateLimited();

        var contentPath = UrlGenerator.NormalizePath(ContentPath);
        var previousByPath = IndexByPath(ItemCache.Values);

        try
        {
            Logger.LogInformation("Loading GitHub content from {Owner}/{Repository} path: {Path}",
                _options.Owner, _options.Repository, contentPath);

            var cache = await LoadFromTreeAsync(contentPath, previousByPath, cancellationToken)
                ?? await LoadByListingAsync(contentPath, previousByPath, cancellationToken);

            await UpdateLastKnownCommitShaAsync(cancellationToken);

            Logger.LogInformation("Loaded {Count} content items from GitHub repository {Owner}/{Repository}",
                cache.Count, _options.Owner, _options.Repository);

            return cache;
        }
        catch (GitHubRateLimitException ex)
        {
            _rateLimitedUntil = ex.ResetAt ?? DateTimeOffset.UtcNow.AddMinutes(1);
            Logger.LogWarning(
                "GitHub rate limit reached while loading {Owner}/{Repository}; keeping the current cache and not retrying before {ResetAt}",
                _options.Owner, _options.Repository, _rateLimitedUntil);
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading content from GitHub repository {Owner}/{Repository}; keeping the current cache",
                _options.Owner, _options.Repository);
            throw;
        }
    }

    private void ThrowIfStillRateLimited()
    {
        var until = _rateLimitedUntil;
        if (until is not null && until > DateTimeOffset.UtcNow)
        {
            throw new GitHubRateLimitException(until, System.Net.HttpStatusCode.TooManyRequests);
        }
    }

    private static Dictionary<string, ContentItem> IndexByPath(IEnumerable<ContentItem> items)
    {
        var index = new Dictionary<string, ContentItem>(StringComparer.Ordinal);
        foreach (var item in items)
        {
            if (!string.IsNullOrWhiteSpace(item.ProviderSpecificId))
            {
                index[item.Path] = item;
            }
        }

        return index;
    }

    private async Task UpdateLastKnownCommitShaAsync(CancellationToken cancellationToken)
    {
        try
        {
            var branches = await _apiClient.GetBranchesAsync(cancellationToken) ?? [];
            var branch = branches.FirstOrDefault(b => b.Name == _options.Branch);
            if (branch is not null)
            {
                _lastKnownCommitSha = branch.Commit.Sha;
                Logger.LogInformation("Updated commit SHA to {Sha}", _lastKnownCommitSha);
            }
        }
        catch (GitHubRateLimitException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to update commit SHA");
        }
    }

    private async Task<GitHubTree?> TryGetTreeAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _apiClient.GetTreeAsync(cancellationToken);
        }
        catch (GitHubRateLimitException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Logger.LogWarning(ex, "Could not read the repository tree; listing directories instead");
            return null;
        }
    }

    private async Task<Dictionary<string, ContentItem>?> LoadFromTreeAsync(
        string contentPath,
        Dictionary<string, ContentItem> previousByPath,
        CancellationToken cancellationToken)
    {
        var tree = await TryGetTreeAsync(cancellationToken);
        if (tree is null || tree.Truncated)
        {
            Logger.LogInformation("Repository tree unavailable or truncated for {Owner}/{Repository}; listing directories instead",
                _options.Owner, _options.Repository);
            return null;
        }

        var cache = new Dictionary<string, ContentItem>();
        var prefix = contentPath.Length == 0 ? string.Empty : contentPath + "/";

        foreach (var entry in tree.Entries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!entry.IsFile
                || !entry.Path.StartsWith(prefix, StringComparison.Ordinal)
                || !IsSupportedFile(System.IO.Path.GetFileName(entry.Path)))
            {
                continue;
            }

            var item = await LoadFileAsync(entry.Path, entry.Sha, previousByPath, cancellationToken);
            if (item is not null)
            {
                cache[item.Id] = item;
            }
        }

        return cache;
    }

    private async Task<Dictionary<string, ContentItem>> LoadByListingAsync(
        string contentPath,
        Dictionary<string, ContentItem> previousByPath,
        CancellationToken cancellationToken)
    {
        var cache = new Dictionary<string, ContentItem>();
        var contents = await _apiClient.GetRepositoryContentsAsync(contentPath, cancellationToken);
        await ProcessContentsRecursivelyAsync(contents ?? [], cache, previousByPath, cancellationToken);
        return cache;
    }

    /// <summary>
    /// Returns the cached item when the file's SHA is unchanged, otherwise downloads and processes the file.
    /// A file that cannot be downloaded keeps its previous item, so a transient error does not drop content.
    /// </summary>
    private async Task<ContentItem?> LoadFileAsync(
        string path,
        string sha,
        Dictionary<string, ContentItem> previousByPath,
        CancellationToken cancellationToken)
    {
        previousByPath.TryGetValue(path, out var previous);

        if (previous is not null && !string.IsNullOrEmpty(sha) && string.Equals(previous.ProviderSpecificId, sha, StringComparison.Ordinal))
        {
            return previous;
        }

        try
        {
            var fileContent = await _apiClient.GetFileContentAsync(path, cancellationToken);
            return await ProcessMarkdownFileAsync(fileContent, cancellationToken);
        }
        catch (GitHubRateLimitException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing file: {FileName}", path);
            return previous;
        }
    }

    /// <inheritdoc/>
    public override async Task<ContentItem> SaveWithCommitMessageAsync(ContentItem entity, string commitMessage, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        if (string.IsNullOrWhiteSpace(entity.Path))
            throw new ArgumentException("Path cannot be empty", nameof(entity));

        ThrowIfDerived(entity);
        LogOperation("saving", entity.Id);

        try
        {
            // Generate content with front matter
            var content = await GenerateMarkdownWithFrontMatterAsync(entity, cancellationToken);

            // Get SHA if updating
            string? sha = entity.ProviderSpecificId;

            // Create or update file
            var response = await _apiClient.CreateOrUpdateFileAsync(
                entity.Path,
                content,
                commitMessage,
                sha,
                cancellationToken);

            if (response is null)
            {
                throw new ContentProviderException("GitHub did not return a commit response.", ProviderId);
            }

            if (!response.Success)
            {
                throw new ContentProviderException($"Failed to save content: {response.ErrorMessage}", ProviderId);
            }

            // Update with provider-specific information
            entity.SetProviderSpecificId(response.Content.Sha);

            // Refresh cache
            await RefreshCacheAsync(cancellationToken);

            return entity;
        }
        catch (Exception ex)
        {
            LogError(ex, "saving", entity.Id);
            throw new ContentProviderException($"Failed to save content item: {ex.Message}", ex, ProviderId);
        }
    }

    /// <inheritdoc/>
    public override async Task DeleteWithCommitMessageAsync(string id, string commitMessage, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("ID cannot be empty", nameof(id));

        LogOperation("deleting", id);

        try
        {
            // Get item to get path and SHA
            var item = await GetByIdAsync(id, cancellationToken);
            if (item is null)
                throw new ContentItemNotFoundException(id, ProviderId);

            ThrowIfDerived(item);
            if (string.IsNullOrWhiteSpace(item.ProviderSpecificId))
                throw new ContentProviderException($"Content has no SHA; cannot delete", ProviderId);

            // Delete file
            var response = await _apiClient.DeleteFileAsync(
                item.Path,
                commitMessage,
                item.ProviderSpecificId,
                cancellationToken);

            if (!response.Success)
            {
                throw new ContentProviderException($"Failed to delete content: {response.ErrorMessage}", ProviderId);
            }

            // Refresh cache
            await RefreshCacheAsync(cancellationToken);
        }
        catch (ContentItemNotFoundException)
        {
            // Re-throw not found exception
            throw;
        }
        catch (Exception ex)
        {
            LogError(ex, "deleting", id);
            throw new ContentProviderException($"Failed to delete content item: {ex.Message}", ex, ProviderId);
        }
    }

    private async Task ProcessContentsRecursivelyAsync(
        List<GitHubItem> contents,
        Dictionary<string, ContentItem> contentItems,
        Dictionary<string, ContentItem> previousByPath,
        CancellationToken cancellationToken = default)
    {
        foreach (var item in contents)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (item.IsFile && IsSupportedFile(item.Name))
            {
                var contentItem = await LoadFileAsync(item.Path, item.Sha, previousByPath, cancellationToken);
                if (contentItem is not null)
                {
                    contentItems[contentItem.Id] = contentItem;
                }
            }
            else if (item.IsDirectory)
            {
                var subContents = await _apiClient.GetRepositoryContentsAsync(item.Path, cancellationToken) ?? [];
                await ProcessContentsRecursivelyAsync(subContents, contentItems, previousByPath, cancellationToken);
            }
        }
    }

    private async Task<ContentItem?> ProcessMarkdownFileAsync(GitHubFileContent? fileContent, CancellationToken cancellationToken = default)
    {
        if (fileContent is null || !IsSupportedFile(fileContent.Name))
            return null;

        //// Skip _index.md files - they're for directory metadata
        //if (fileContent.Name.Equals("_index.md", StringComparison.OrdinalIgnoreCase))
        //    return null;

        // Get file content
        string markdownContent = fileContent.GetDecodedContent();
        if (string.IsNullOrWhiteSpace(markdownContent))
            return null;

        var id = CreateItemId(fileContent.Path);

        // Create the content item
        var contentItem = ContentItem.Create(
            id: id,
            title: Path.GetFileNameWithoutExtension(fileContent.Name),
            content: string.Empty, // Will be set in ProcessMarkdownAsync
            path: fileContent.Path,
            providerId: ProviderId);

        // Store the provider-specific ID (SHA)
        contentItem.SetProviderSpecificId(fileContent.Sha);


        // Note: File history dates are set from frontmatter in ProcessMarkdownAsync
        // GitHub commit dates could be fetched here if needed for audit trails

        // Extract locale from path if enabled
        if (_options.EnableLocalization)
        {
            var locale = UrlGenerator.ExtractLocaleFromPath(fileContent.Path, ContentPath, EnableLocalization, SupportedLocales, DefaultLocale);
            contentItem.SetLocale(locale);
        }
        else
        {
            contentItem.SetLocale(_options.DefaultLocale);
        }

        // Set directory if found
        try
        {
            var directoryPath = UrlGenerator.GetDirectoryPath(fileContent.Path);
            var directory = await _directoryRepository.GetByPathAsync(directoryPath, cancellationToken);
            if (directory is not null)
            {
                contentItem.SetDirectory(directory);
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to set directory for content: {Path}", fileContent.Path);
        }

        // Process the markdown content
        await ProcessMarkdownAsync(markdownContent, contentItem, cancellationToken);

        // Asked for only after the front matter is read, so a page with a date costs no extra request, and
        // an unchanged file is not asked again because its item is reused by SHA.
        if (string.IsNullOrWhiteSpace(contentItem.Metadata?.Date))
        {
            await ApplyLastCommitDateAsync(contentItem, cancellationToken);
        }

        // Set URL
        var url = UrlGenerator.GenerateUrl(contentItem.Metadata?.Permalink ?? string.Empty, contentItem.Path, contentItem.Slug, EnableLocalization, SupportedLocales, ContentPath);
        contentItem.SetUrl(url);

        AssignDefaultLocalizationId(contentItem);

        return contentItem;
    }

    // Without a date in the front matter a page would take the time of the load and move on every restart. The date
    // of the file's last commit does not, so it stands in; a lookup that fails keeps the page and its load time.
    private async Task ApplyLastCommitDateAsync(ContentItem contentItem, CancellationToken cancellationToken)
    {
        try
        {
            var commitDate = await _apiClient.GetLastCommitDateAsync(contentItem.Path, cancellationToken);
            if (commitDate is null)
            {
                return;
            }

            contentItem.SetCreatedDate(commitDate.Value);
            if (string.IsNullOrWhiteSpace(contentItem.Metadata?.LastModified))
            {
                contentItem.SetLastModifiedDate(commitDate.Value);
            }
        }
        catch (GitHubRateLimitException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Logger.LogWarning(ex, "Could not read the last commit date of {Path}; the page keeps its load time", contentItem.Path);
        }
    }

    private bool IsSupportedFile(string fileName)
    {
        return _options.SupportedExtensions
            .Any(ext => fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
    }
}
