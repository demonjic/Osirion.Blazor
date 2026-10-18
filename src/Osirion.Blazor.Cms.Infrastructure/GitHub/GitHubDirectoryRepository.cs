using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Exceptions;
using Osirion.Blazor.Cms.Domain.Interfaces;
using Osirion.Blazor.Cms.Domain.Interfaces.Directory;
using Osirion.Blazor.Cms.Domain.Models.GitHub;
using Osirion.Blazor.Cms.Domain.Options;
using Osirion.Blazor.Cms.Domain.Repositories;
using Osirion.Blazor.Cms.Infrastructure.Directory;
using Osirion.Blazor.Cms.Infrastructure.Utilities;
using DirectoryNotFoundException = Osirion.Blazor.Cms.Domain.Exceptions.DirectoryNotFoundException;

namespace Osirion.Blazor.Cms.Infrastructure.GitHub;

/// <summary>
/// Repository implementation for GitHub directories
/// </summary>
public class GitHubDirectoryRepository : DirectoryRepositoryBase, IDirectoryRepository
{
    private readonly IGitHubApiClient _apiClient;
    private readonly GitHubOptions _options;
    private readonly string? _providerName;

    /// <summary>Performs the GitHubDirectoryRepository operation.</summary>
    public GitHubDirectoryRepository(
        IGitHubApiClientFactory apiClientFactory,
        IOptions<GitHubOptions> options,
        IDirectoryCacheManager cacheManager,
        IDirectoryMetadataProcessor metadataProcessor,
        IPathUtilities pathUtils,
        ILogger<GitHubDirectoryRepository> logger,
        string? providerName = null)
        : base(GetProviderId(options.Value, providerName), cacheManager, metadataProcessor, pathUtils, options, logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

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

        _providerName = providerName;
    }

    private static string GetProviderId(GitHubOptions options, string? providerName)
    {
        return options.ProviderId ?? $"github-{providerName}-{options.Owner}-{options.Repository}";
    }

    /// <inheritdoc/>
    public override async Task<DirectoryItem> SaveAsync(DirectoryItem entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        // Use default commit message
        var commitMessage = string.IsNullOrWhiteSpace(entity.ProviderSpecificId)
            ? $"Create directory {entity.Name}"
            : $"Update directory {entity.Name}";

        try
        {
            // Check if directory exists
            var existingDirectory = await GetByIdAsync(entity.Id, cancellationToken);
            if (existingDirectory is null)
            {
                // Create new directory
                return await CreateDirectoryAsync(entity, commitMessage, cancellationToken);
            }
            else
            {
                // Update existing directory
                return await UpdateDirectoryAsync(entity, commitMessage, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "saving", entity.Id);
            throw new ContentProviderException($"Failed to save directory: {ex.Message}", ex, ProviderId);
        }
    }

    /// <inheritdoc/>
    public override async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await DeleteRecursiveAsync(id, null, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DirectoryItem> MoveAsync(string id, string? newParentId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("ID cannot be empty", nameof(id));

        LogOperation("moving", id);

        try
        {
            // Get directory to move
            var directory = await GetByIdAsync(id, cancellationToken);
            if (directory is null)
                throw new DirectoryNotFoundException(id);

            // Get new parent directory if specified
            DirectoryItem? newParent = null;
            if (!string.IsNullOrWhiteSpace(newParentId))
            {
                newParent = await GetByIdAsync(newParentId, cancellationToken);
                if (newParent is null)
                    throw new DirectoryNotFoundException(newParentId);

                // Check for circular reference
                if (directory.IsAncestorOf(newParent))
                    throw new ContentValidationException("Parent", "Cannot move a directory to one of its descendants");
            }

            // Determine old and new paths
            var oldPath = directory.Path;
            // The root is the configured content folder, not the repository root.
            var parentPath = newParent?.Path ?? PathUtils.NormalizePath(_options.ContentPath);
            var newPath = Path.Combine(parentPath, Path.GetFileName(directory.Path)).Replace('\\', '/');

            // Move all files and directories
            await MoveDirectoryContentsAsync(oldPath, newPath, cancellationToken);

            // Update directory entity
            var updatedDirectory = directory.Clone();
            updatedDirectory.SetParent(newParent);
            updatedDirectory.SetPath(newPath);

            // Save updated directory metadata
            await SaveAsync(updatedDirectory, cancellationToken);

            // Refresh cache
            await RefreshCacheAsync(cancellationToken);

            return updatedDirectory;
        }
        catch (Exception ex)
        {
            LogError(ex, "moving", id);
            throw new ContentProviderException($"Failed to move directory: {ex.Message}", ex, ProviderId);
        }
    }

    /// <inheritdoc/>
    protected override async Task<Dictionary<string, DirectoryItem>> LoadDirectoriesAsync(
        CancellationToken cancellationToken)
    {
        var cache = new Dictionary<string, DirectoryItem>();
        var contentPath = PathUtils.NormalizePath(_options.ContentPath);

        // One recursive tree request replaces a listing request per directory and a probe request per _index.md.
        // Without a usable tree the directories are listed one by one.
        var tree = await TryGetTreeAsync(cancellationToken);
        var snapshot = tree is null || tree.Truncated ? null : new GitHubTreeSnapshot(tree);

        var contents = snapshot is not null
            ? snapshot.ListChildren(contentPath)
            : await _apiClient.GetRepositoryContentsAsync(contentPath, cancellationToken) ?? [];

        Task<List<GitHubItem>> ListChildren(string path) => snapshot is not null
            ? Task.FromResult(snapshot.ListChildren(path))
            : ListFromApiAsync(path, cancellationToken);

        // Process contents recursively to build directory structure
        await ProcessDirectoriesRecursivelyAsync(contents, cache, null, ListChildren, cancellationToken);

        // Process _index.md files for metadata
        await ProcessDirectoryMetadataAsync(cache, snapshot, cancellationToken);

        return cache;
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

    private async Task<List<GitHubItem>> ListFromApiAsync(string path, CancellationToken cancellationToken)
    {
        return await _apiClient.GetRepositoryContentsAsync(path, cancellationToken) ?? [];
    }

    /// <inheritdoc/>
    protected override async Task DeleteDirectoryInternalAsync(
        string id,
        bool recursive,
        string? commitMessage,
        CancellationToken cancellationToken)
    {
        // Get directory to get path and metadata
        var directory = await GetByIdAsync(id, cancellationToken);
        if (directory is null)
            throw new DirectoryNotFoundException(id);

        // Generate a default commit message if not provided
        var message = commitMessage ?? $"Delete directory {directory.Name}";

        // Get all files in the directory and its subdirectories
        var allFiles = await GetAllFilesInPathAsync(directory.Path, cancellationToken);

        // Delete all files in reverse order (deeper paths first)
        foreach (var file in allFiles.OrderByDescending(f => f.Path.Count(c => c == '/')))
        {
            await _apiClient.DeleteFileAsync(
                file.Path,
                message,
                file.Sha,
                cancellationToken);
        }
    }

    /// <summary>
    /// Process directories recursively with improved parent-child relationship handling
    /// </summary>
    private async Task ProcessDirectoriesRecursivelyAsync(
        List<GitHubItem> contents,
        Dictionary<string, DirectoryItem> directoryCache,
        DirectoryItem? parentDirectory,
        Func<string, Task<List<GitHubItem>>> listChildren,
        CancellationToken cancellationToken,
        HashSet<string>? processedPaths = null)
    {
        // Initialize path tracking if not provided
        processedPaths ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in contents.Where(i => i.IsDirectory))
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Skip if we've already processed this path to avoid cycles
            if (!processedPaths.Add(item.Path))
            {
                Logger.LogWarning("Skipping already processed directory path: {Path}", item.Path);
                continue;
            }

            // Create directory item with more robust ID generation
            var directoryId = IdGenerator.GenerateDirectoryId(item.Path, item.Name, GetProviderId(_options, _providerName));

            DirectoryItem directory;

            // Check if we already have this directory in cache
            if (directoryCache.TryGetValue(directoryId, out var existingDirectory))
            {
                directory = existingDirectory;

                // Only set parent if the directory doesn't already have one
                if (parentDirectory is not null && directory.Parent is null)
                {
                    try
                    {
                        parentDirectory.AddChild(directory);
                    }
                    catch (ContentValidationException ex)
                    {
                        Logger.LogWarning(ex,
                            "Cannot set parent-child relationship between '{Parent}' and '{Child}'",
                            parentDirectory.Name, directory.Name);
                    }
                }
            }
            else
            {
                // Create new directory
                directory = DirectoryItem.Create(
                    id: directoryId,
                    path: item.Path,
                    name: item.Name,
                    providerId: ProviderId);

                // Set parent if available
                if (parentDirectory is not null)
                {
                    try
                    {
                        parentDirectory.AddChild(directory);
                    }
                    catch (ContentValidationException ex)
                    {
                        Logger.LogWarning(ex,
                            "Cannot set parent-child relationship between '{Parent}' and '{Child}'",
                            parentDirectory.Name, directory.Name);
                    }
                }

                // Extract locale from path if enabled. The shared path utilities are configured from the file system
                // provider's options, which do not know this repository's content folder, so the folder is read
                // against this provider's own ContentPath and locales, as GitHubContentRepository reads files.
                if (_options.EnableLocalization)
                {
                    directory.SetLocale(UrlGenerator.ExtractLocaleFromPath(
                        item.Path,
                        _options.ContentPath,
                        _options.EnableLocalization,
                        _options.GetEffectiveSupportedLocales(),
                        _options.DefaultLocale));
                }
                else
                {
                    directory.SetLocale(_options.DefaultLocale);
                }

                // Add to cache
                directoryCache[directoryId] = directory;
            }

            // Process subdirectories
            var subContents = await listChildren(item.Path);
            await ProcessDirectoriesRecursivelyAsync(
                subContents,
                directoryCache,
                directory, // Use the directory we just processed
                listChildren,
                cancellationToken,
                processedPaths);
        }
    }

    private async Task ProcessDirectoryMetadataAsync(
        Dictionary<string, DirectoryItem> directoryCache,
        GitHubTreeSnapshot? snapshot,
        CancellationToken cancellationToken)
    {
        // Process _index.md files for each directory
        foreach (var directoryPath in directoryCache.Values.Select(d => d.Path))
        {
            try
            {
                var indexFilePath = Path.Combine(directoryPath, "_index.md").Replace('\\', '/');

                // With a tree, a directory without an _index.md costs no request at all
                if (snapshot is not null && !snapshot.ContainsFile(indexFilePath))
                {
                    continue;
                }

                var fileContent = await _apiClient.GetFileContentAsync(indexFilePath, cancellationToken);
                if (fileContent is null)
                {
                    continue;
                }

                var decodedContent = fileContent.GetDecodedContent();

                // Find the directory
                var directory = directoryCache.Values.FirstOrDefault(d => d.Path == directoryPath);
                if (directory is not null)
                {
                    // Process metadata
                    MetadataProcessor.ProcessMetadata(directory, decodedContent);

                    // Save SHA for updates
                    directory.SetProviderSpecificId(fileContent.Sha);
                }
            }
            catch (GitHubRateLimitException)
            {
                throw;
            }
            catch (Exception)
            {
                // _index.md might not exist - that's ok
                continue;
            }
        }
    }

    private async Task<DirectoryItem> CreateDirectoryAsync(DirectoryItem entity, string commitMessage, CancellationToken cancellationToken)
    {
        LogOperation("creating", entity.Id);

        // GitHub doesn't support empty directories, so we create a placeholder file
        var placeholderPath = Path.Combine(entity.Path, ".gitkeep").Replace('\\', '/');

        // Create placeholder file
        await _apiClient.CreateOrUpdateFileAsync(
            placeholderPath,
            "", // Empty content
            commitMessage,
            null,
            cancellationToken);

        // Create _index.md with directory metadata
        await SaveDirectoryMetadataAsync(entity, commitMessage, cancellationToken);

        // Refresh cache
        await RefreshCacheAsync(cancellationToken);

        return entity;
    }

    private async Task<DirectoryItem> UpdateDirectoryAsync(DirectoryItem entity, string commitMessage, CancellationToken cancellationToken)
    {
        LogOperation("updating", entity.Id);

        // Update _index.md with directory metadata
        await SaveDirectoryMetadataAsync(entity, commitMessage, cancellationToken);

        // Refresh cache
        await RefreshCacheAsync(cancellationToken);

        return entity;
    }

    private async Task SaveDirectoryMetadataAsync(DirectoryItem directory, string commitMessage, CancellationToken cancellationToken)
    {
        var indexPath = Path.Combine(directory.Path, "_index.md").Replace('\\', '/');

        // Generate metadata content
        var content = MetadataProcessor.GenerateMetadataContent(directory);

        // Check if _index.md already exists
        string? sha = directory.ProviderSpecificId;
        if (string.IsNullOrWhiteSpace(sha))
        {
            try
            {
                var existingFile = await _apiClient.GetFileContentAsync(indexPath, cancellationToken);
                sha = existingFile?.Sha;
            }
            catch
            {
                // File doesn't exist yet
            }
        }

        // Create or update _index.md
        var response = await _apiClient.CreateOrUpdateFileAsync(
            indexPath,
            content,
            commitMessage,
            sha,
            cancellationToken);

        if (response is null)
        {
            throw new InvalidOperationException($"GitHub did not return a commit response for '{indexPath}'.");
        }

        // Update the entity with the new SHA
        directory.SetProviderSpecificId(response.Content.Sha);
    }

    private async Task<List<GitHubItem>> GetAllFilesInPathAsync(string path, CancellationToken cancellationToken)
    {
        var result = new List<GitHubItem>();
        var contents = await _apiClient.GetRepositoryContentsAsync(path, cancellationToken) ?? [];

        foreach (var item in contents)
        {
            if (item.IsFile)
            {
                result.Add(item);
            }
            else if (item.IsDirectory)
            {
                // Recursively get files in subdirectory
                var subFiles = await GetAllFilesInPathAsync(item.Path, cancellationToken);
                result.AddRange(subFiles);
            }
        }

        return result;
    }

    private async Task MoveDirectoryContentsAsync(string oldPath, string newPath, CancellationToken cancellationToken)
    {
        // Get all files in the source directory
        var files = await GetAllFilesInPathAsync(oldPath, cancellationToken);

        // Commit message for the operations
        var commitMessage = $"Move directory from {oldPath} to {newPath}";

        // Create files at the new location
        foreach (var file in files)
        {
            // Calculate new file path
            var relativePath = file.Path.Substring(oldPath.Length);
            if (relativePath.StartsWith("/"))
                relativePath = relativePath.Substring(1);

            var newFilePath = Path.Combine(newPath, relativePath).Replace('\\', '/');

            // Get file content
            var fileContent = await _apiClient.GetFileContentAsync(file.Path, cancellationToken);
            if (fileContent is null)
            {
                continue;
            }

            var content = fileContent.GetDecodedContent();

            // Create file at new location
            await _apiClient.CreateOrUpdateFileAsync(
                newFilePath,
                content,
                commitMessage,
                null, // No SHA because it's a new file at this path
                cancellationToken);

            // Delete original file
            await _apiClient.DeleteFileAsync(
                file.Path,
                commitMessage,
                file.Sha,
                cancellationToken);
        }
    }
}
