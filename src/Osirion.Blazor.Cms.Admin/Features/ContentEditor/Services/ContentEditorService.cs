using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Osirion.Blazor.Cms.Admin.Core.Events;
using Osirion.Blazor.Cms.Admin.Infrastructure.Adapters;
using Osirion.Blazor.Cms.Admin.Interfaces;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Models.GitHub;
using Osirion.Blazor.Cms.Domain.Options.Configuration;
using Osirion.Blazor.Cms.Domain.ValueObjects;
using System.Text.RegularExpressions;

namespace Osirion.Blazor.Cms.Admin.Features.ContentEditor.Services;

/// <summary>
/// Service for editing content in the admin interface
/// </summary>
public class ContentEditorService : IContentEditorService
{
    private readonly IContentRepositoryAdapter _repositoryAdapter;
    private readonly IAdminContentService _contentService;
    private readonly IEventPublisher _eventPublisher;
    private readonly CmsAdminOptions _options;
    private readonly ILogger<ContentEditorService> _logger;

    /// <summary>Performs the ContentEditorService operation.</summary>
    public ContentEditorService(
        IContentRepositoryAdapter repositoryAdapter,
        IAdminContentService contentService,
        IEventPublisher eventPublisher,
        IOptions<CmsAdminOptions> options,
        ILogger<ContentEditorService> logger)
    {
        _repositoryAdapter = repositoryAdapter ?? throw new ArgumentNullException(nameof(repositoryAdapter));
        _contentService = contentService ?? throw new ArgumentNullException(nameof(contentService));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a blog post by path
    /// </summary>
    public async Task<ContentItem> GetBlogPostAsync(string path)
    {
        try
        {
            _logger.LogInformation("Fetching blog post: {Path}", path);

            // First try to get from the repository adapter directly
            var blogPost = await _repositoryAdapter.GetBlogPostAsync(path);

            _logger.LogInformation("Blog post fetched successfully: {Path}", path);
            return blogPost;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching blog post: {Path}", path);
            _eventPublisher.Publish(new ErrorOccurredEvent($"Failed to load file: {path}", ex));
            throw;
        }
    }

    /// <summary>
    /// Saves a blog post
    /// </summary>
    public async Task<GitHubFileCommitResponse> SaveBlogPostAsync(ContentItem post, string commitMessage)
    {
        try
        {
            _logger.LogInformation("Saving blog post: {Path}", post.Path);

            // Validate the post if front matter validation is enabled
            if (_options.ContentRules.EnforceFrontMatterValidation)
            {
                ValidateBlogPost(post);
            }

            var content = post.ToMarkdown();
            var message = string.IsNullOrWhiteSpace(commitMessage)
                ? $"Update {Path.GetFileName(post.Path)}"
                : commitMessage;

            var response = await _repositoryAdapter.SaveContentAsync(
                post.Path,
                content,
                message,
                post.Sha);

            _logger.LogInformation("Blog post saved successfully: {Path}", post.Path);
            _eventPublisher.Publish(new ContentSavedEvent(post.Path));

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving blog post: {Path}", post.Path);
            _eventPublisher.Publish(new ErrorOccurredEvent($"Failed to save file: {post.Path}", ex));
            throw;
        }
    }

    /// <summary>
    /// Deletes a blog post
    /// </summary>
    public async Task<GitHubFileCommitResponse> DeleteBlogPostAsync(string path, string sha)
    {
        if (!_options.ContentRules.AllowFileDeletion)
        {
            var message = "File deletion is not allowed by content rules";
            _logger.LogWarning(message);
            _eventPublisher.Publish(new ErrorOccurredEvent(message));
            throw new InvalidOperationException(message);
        }

        try
        {
            _logger.LogInformation("Deleting blog post: {Path}", path);

            var message = $"Delete {Path.GetFileName(path)}";
            var response = await _repositoryAdapter.DeleteFileAsync(path, message, sha);

            _logger.LogInformation("Blog post deleted successfully: {Path}", path);
            _eventPublisher.Publish(new ContentDeletedEvent(path));

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting blog post: {Path}", path);
            _eventPublisher.Publish(new ErrorOccurredEvent($"Failed to delete file: {path}", ex));
            throw;
        }
    }

    /// <summary>
    /// Creates a new blog post with default content
    /// </summary>
    public ContentItem CreateNewBlogPost(string path = "", string title = "New Post")
    {
        _logger.LogInformation("Creating new blog post with title: {Title}", title);

        var metadata = FrontMatter.Create(
            title,
            "Enter description here...",
            DateTime.Now);

        var content = $"## {title}\n\nStart writing your content here...";

        var fileName = GenerateFileNameFromTitle(title);

        var filePath = string.IsNullOrWhiteSpace(path)
            ? fileName
            : $"{path.TrimEnd('/')}/{fileName}";

        return new ContentItem
        {
            Metadata = metadata,
            Content = content,
            Path = filePath
        };
    }

    /// <summary>
    /// Generates a file name from a title
    /// </summary>
    public string GenerateFileNameFromTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return "new-post.md";
        }

        // Strip an existing extension first: the sanitising below would turn ".md" into "md"
        var baseName = title.EndsWith(".md", StringComparison.OrdinalIgnoreCase) ? title[..^3] : title;

        // Convert to kebab case (lowercase with hyphens)
        var fileName = Regex.Replace(baseName.ToLowerInvariant(), @"[^a-z0-9\s-]", "");
        fileName = Regex.Replace(fileName, @"\s+", "-");
        fileName = Regex.Replace(fileName, @"-{2,}", "-");
        fileName = fileName.Trim('-');

        if (fileName.Length == 0)
        {
            return "new-post.md";
        }

        fileName += ".md";

        return fileName;
    }

    /// <summary>
    /// Converts a ContentItem to a BlogPost
    /// </summary>
    public ContentItem ConvertToBlogPost(ContentItem item)
    {
        var frontMatter = new FrontMatter
        {
            Title = item.Title,
            Description = item.Description,
            Author = item.Author,
            Date = FrontMatterDates.Format(item.DateCreated),
            Tags = item.Tags.ToList(),
            Categories = item.Categories.ToList(),
            Slug = item.Slug,
            IsFeatured = item.IsFeatured,
            Published = item.IsPublished
        };

        return new ContentItem
        {
            Metadata = frontMatter,
            Content = item.Content,
            Path = item.Path,
            Sha = item.Sha
        };
    }

    /// <summary>
    /// Validates a blog post
    /// </summary>
    private void ValidateBlogPost(ContentItem post)
    {
        // Check for required front matter fields
        foreach (var field in _options.ContentRules.RequiredFrontMatterFields)
        {
            var propertyInfo = typeof(FrontMatter).GetProperty(field, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            if (propertyInfo is null)
            {
                continue;
            }

            var value = propertyInfo.GetValue(post.Metadata);

            if (value is null || (value is string strValue && string.IsNullOrWhiteSpace(strValue)))
            {
                throw new ValidationException($"Required field '{field}' is missing or empty in front matter");
            }
        }

        // Check for content
        if (string.IsNullOrWhiteSpace(post.Content))
        {
            throw new ValidationException("Content cannot be empty");
        }

        // Check file extension
        var extension = Path.GetExtension(post.Path);

        if (!_options.ContentRules.AllowedFileExtensions.Contains(extension.ToLowerInvariant()))
        {
            throw new ValidationException($"File extension '{extension}' is not allowed. Allowed extensions: {string.Join(", ", _options.ContentRules.AllowedFileExtensions)}");
        }

        // Auto-generate slug if enabled and not provided
        if (_options.ContentRules.AutoGenerateSlugs && post.Metadata is { } metadata && string.IsNullOrWhiteSpace(metadata.Slug))
        {
            metadata.Slug = GenerateSlugFromTitle(metadata.Title);
        }
    }

    /// <summary>
    /// Generates a slug from a title
    /// </summary>
    private string GenerateSlugFromTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return "post";
        }

        // Similar to file name generation but without extension
        var slug = Regex.Replace(title.ToLowerInvariant(), @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-{2,}", "-");
        slug = slug.Trim('-');

        return slug;
    }
}

/// <summary>
/// Exception thrown when blog post validation fails
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with a specified error message.
    /// </summary>
    /// <param name="message"></param>
    public ValidationException(string message) : base(message)
    {
    }
}
