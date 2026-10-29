using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Exceptions;
using Osirion.Blazor.Cms.Domain.Interfaces;
using Osirion.Blazor.Cms.Domain.Interfaces.Content;
using Osirion.Blazor.Cms.Domain.Options;
using Osirion.Blazor.Cms.Infrastructure.Directory;
using Osirion.Blazor.Cms.Infrastructure.Localization;
using Osirion.Blazor.Cms.Infrastructure.Repositories;
using Osirion.Blazor.Cms.Infrastructure.Utilities;
using System.IO.Abstractions;
using DirectoryNotFoundException = Osirion.Blazor.Cms.Domain.Exceptions.DirectoryNotFoundException;

namespace Osirion.Blazor.Cms.Infrastructure.FileSystem
{
    /// <summary>
    /// Repository implementation for file system content
    /// </summary>
    public class FileSystemContentRepository : BaseContentRepository
    {
        private readonly FileSystemOptions _options;
        private readonly PathUtilities _globMatcher;
        private readonly Dictionary<string, string> _fileExtensionContentTypeMap;
        private readonly IFileSystem _fileSystem;
        private readonly FileSystemDirectoryRepository? _directoryRepository;
        private FileSystemWatcher? _fileWatcher;

        /// <summary>Performs the FileSystemContentRepository operation.</summary>
        public FileSystemContentRepository(
            IMarkdownProcessor markdownProcessor,
            IContentQueryFilter queryFilter,
            IOptions<FileSystemOptions> options,
            ILogger<FileSystemContentRepository> logger,
            IFileSystem fileSystem)
            : base(GetProviderId(options.Value), markdownProcessor, queryFilter, logger)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _fileSystem = fileSystem;

            // Set base class properties
            EnableLocalization = _options.EnableLocalization;
            DefaultLocale = _options.DefaultLocale;
            SupportedLocales = _options.GetEffectiveSupportedLocales();
            ContentPath = FileSystemConfiguration.GetContentRoot(_options);
            _globMatcher = new PathUtilities(options);

            // Initialize file extension to content type mapping
            _fileExtensionContentTypeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { ".md", "text/markdown" },
                { ".markdown", "text/markdown" },
                { ".txt", "text/plain" }
            };

            // Setup file watcher if enabled
            if (_options.WatchForChanges)
            {
                SetupFileWatcher();
            }
        }

        /// <summary>
        /// Initializes a new instance that links every loaded item to its directory, so directory queries (for example
        /// <c>ContentList Directory="blog"</c>) find FileSystem content the way they find GitHub content.
        /// </summary>
        public FileSystemContentRepository(
            IMarkdownProcessor markdownProcessor,
            IContentQueryFilter queryFilter,
            IOptions<FileSystemOptions> options,
            ILogger<FileSystemContentRepository> logger,
            IFileSystem fileSystem,
            FileSystemDirectoryRepository directoryRepository)
            : this(markdownProcessor, queryFilter, options, logger, fileSystem)
        {
            _directoryRepository = directoryRepository ?? throw new ArgumentNullException(nameof(directoryRepository));
        }

        /// <summary>
        /// Initializes a new instance that links every loaded item to its directory and adds the items of the
        /// provider's <see cref="ContentProviderOptions.DerivedLocales"/> on every load.
        /// </summary>
        /// <exception cref="OptionsValidationException">Thrown when a derived locale breaks a rule.</exception>
        public FileSystemContentRepository(
            IMarkdownProcessor markdownProcessor,
            IContentQueryFilter queryFilter,
            IOptions<FileSystemOptions> options,
            ILogger<FileSystemContentRepository> logger,
            IFileSystem fileSystem,
            FileSystemDirectoryRepository directoryRepository,
            DerivedLocaleGenerator derivedLocales)
            : this(markdownProcessor, queryFilter, options, logger, fileSystem, directoryRepository)
        {
            UseDerivedLocales(derivedLocales, _options);
        }

        // The patterns match the path under BasePath, so a content folder inside a bin or obj folder still loads.
        // The leading slash lets "**/name/**" match a first-level folder too.
        private bool IsExcluded(string filePath)
        {
            var relativePath = "/" + GetRelativePath(filePath);
            return _options.ExcludePatterns.Any(pattern => _globMatcher.MatchesGlobPattern(relativePath, pattern));
        }

        private static string GetProviderId(FileSystemOptions options)
        {
            return FileSystemProviderId.For(options);
        }

        /// <inheritdoc/>
        protected override async Task<Dictionary<string, ContentItem>> LoadItemsIntoCache(CancellationToken cancellationToken)
        {
            var cache = new Dictionary<string, ContentItem>();

            try
            {
                // Create base directory if it doesn't exist
                if (!_fileSystem.Directory.Exists(_options.BasePath))
                {
                    if (_options.CreateDirectoriesIfNotExist)
                    {
                        _fileSystem.Directory.CreateDirectory(_options.BasePath);
                    }
                    else
                    {
                        throw new DirectoryNotFoundException($"Base content directory does not exist: {_options.BasePath}");
                    }
                }

                // Find all markdown files
                var files = new List<string>();
                foreach (var extension in _options.SupportedExtensions)
                {
                    files.AddRange(_fileSystem.Directory.GetFiles(
                        _options.BasePath,
                        $"*{extension}",
                        _options.IncludeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
                }

                // Process each file
                foreach (var file in files.Where(file => !IsExcluded(file)))
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var contentItem = await ProcessMarkdownFileAsync(file, cancellationToken);
                        if (contentItem is not null)
                        {
                            cache[contentItem.Id] = contentItem;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Error processing file: {FileName}", file);
                    }
                }

                Logger.LogInformation("Loaded {Count} content items from file system at {BasePath}",
                    cache.Count, _options.BasePath);

                return cache;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading content from file system at {BasePath}", _options.BasePath);
                throw;
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
                // Build the full file path, refusing any path that leaves the content root
                var fullPath = ContentPathGuard.ResolveUnderRoot(_options.BasePath, entity.Path);

                // Ensure directory exists
                var directoryPath = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrWhiteSpace(directoryPath) && !_fileSystem.Directory.Exists(directoryPath))
                {
                    if (_options.CreateDirectoriesIfNotExist)
                    {
                        _fileSystem.Directory.CreateDirectory(directoryPath);
                    }
                    else
                    {
                        throw new DirectoryNotFoundException($"Directory does not exist: {directoryPath}");
                    }
                }

                // Generate content with front matter
                var content = await GenerateMarkdownWithFrontMatterAsync(entity, cancellationToken);

                // Write content to file
                await _fileSystem.File.WriteAllTextAsync(fullPath, content, cancellationToken);

                // Update provider-specific ID (file path in this case)
                entity.SetProviderSpecificId(fullPath);

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
                // Get item to get path
                var item = await GetByIdAsync(id, cancellationToken);
                if (item is null)
                    throw new ContentItemNotFoundException(id, ProviderId);

                ThrowIfDerived(item);
                var fullPath = item.ProviderSpecificId;
                if (string.IsNullOrWhiteSpace(fullPath))
                {
                    // If provider-specific ID is not set, try using path
                    fullPath = ContentPathGuard.ResolveUnderRoot(_options.BasePath, item.Path);
                }
                else
                {
                    // Callers can change the id on the cached item, so it must still point inside the root
                    ContentPathGuard.EnsureUnderRoot(_options.BasePath, fullPath);
                }

                if (!_fileSystem.File.Exists(fullPath))
                    throw new ContentProviderException($"Content file not found: {fullPath}", ProviderId);

                // Delete file
                _fileSystem.File.Delete(fullPath);

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

        private async Task<ContentItem?> ProcessMarkdownFileAsync(string filePath, CancellationToken cancellationToken)
        {
            try
            {
                // Read file content
                var content = await _fileSystem.File.ReadAllTextAsync(filePath, cancellationToken);
                if (string.IsNullOrWhiteSpace(content))
                    return null;

                var relativePath = GetRelativePath(filePath);
                var id = CreateItemId(relativePath);

                // Create the content item
                var contentItem = ContentItem.Create(
                    id: id,
                    title: Path.GetFileNameWithoutExtension(filePath),
                    content: string.Empty, // Will be set in ProcessMarkdownAsync
                    path: relativePath,
                    providerId: ProviderId);

                // Store the provider-specific ID (file path)
                contentItem.SetProviderSpecificId(filePath);

                var fileInfo = _fileSystem.FileInfo.New(filePath);
                contentItem.SetCreatedDate(fileInfo.CreationTimeUtc);

                // Extract locale from path if enabled
                if (_options.EnableLocalization)
                {
                    var locale = UrlGenerator.ExtractLocaleFromPath(relativePath, ContentPath, EnableLocalization, SupportedLocales, DefaultLocale);
                    contentItem.SetLocale(locale);
                }
                else
                {
                    contentItem.SetLocale(_options.DefaultLocale);
                }

                await AssignDirectoryAsync(contentItem, relativePath, cancellationToken);

                // Process the markdown content
                await ProcessMarkdownAsync(content, contentItem, _options.UseFileWriteTimeAsLastModified ? fileInfo.LastWriteTimeUtc : null, cancellationToken);

                // Set URL
                var url = UrlGenerator.GenerateUrl(contentItem.Metadata?.Permalink ?? string.Empty, contentItem.Path, contentItem.Slug, EnableLocalization, SupportedLocales, ContentPath);
                contentItem.SetUrl(url);

                AssignDefaultLocalizationId(contentItem);

                return contentItem;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error processing markdown file: {FilePath}", filePath);
                return null;
            }
        }

        private async Task AssignDirectoryAsync(ContentItem contentItem, string relativePath, CancellationToken cancellationToken)
        {
            if (_directoryRepository is null)
                return;

            var directoryPath = UrlGenerator.GetDirectoryPath(relativePath);
            if (string.IsNullOrEmpty(directoryPath))
                return;

            try
            {
                var directory = await _directoryRepository.GetByPathAsync(directoryPath, cancellationToken);
                if (directory is not null)
                {
                    contentItem.SetDirectory(directory);
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to set directory for content: {Path}", relativePath);
            }
        }

        private string GetRelativePath(string fullPath)
        {
            return Path.GetRelativePath(_options.BasePath, fullPath).Replace('\\', '/');
        }

        private void SetupFileWatcher()
        {
            try
            {
                if (!_fileSystem.Directory.Exists(_options.BasePath))
                {
                    if (_options.CreateDirectoriesIfNotExist)
                    {
                        _fileSystem.Directory.CreateDirectory(_options.BasePath);
                    }
                    else
                    {
                        Logger.LogWarning("Cannot set up file watcher - base path does not exist: {BasePath}", _options.BasePath);
                        return;
                    }
                }

                _fileWatcher = new FileSystemWatcher(_options.BasePath)
                {
                    IncludeSubdirectories = _options.IncludeSubdirectories,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.DirectoryName,
                    EnableRaisingEvents = true
                };

                // Add handler for all change events
                _fileWatcher.Changed += OnFileChanged;
                _fileWatcher.Created += OnFileChanged;
                _fileWatcher.Deleted += OnFileChanged;
                _fileWatcher.Renamed += OnFileChanged;

                Logger.LogInformation("File watcher set up for path: {BasePath}", _options.BasePath);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to set up file watcher for {BasePath}", _options.BasePath);
            }
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            var extension = Path.GetExtension(e.FullPath);
            if (_options.SupportedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                Logger.LogInformation("File change detected: {ChangeType} - {Path}", e.ChangeType, e.FullPath);

                // Invalidate cache
                _ = RefreshAfterChangeAsync();
            }
        }

        // Supervised: the failure of a background refresh is logged here instead of going unobserved.
        private async Task RefreshAfterChangeAsync()
        {
            try
            {
                await RefreshCacheAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error refreshing content after a file change");
            }
        }

        /// <summary>Releases resources held by the component or service.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fileWatcher?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
