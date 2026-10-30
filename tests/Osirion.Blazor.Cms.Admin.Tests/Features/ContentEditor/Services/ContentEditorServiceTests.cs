using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Octokit;
using Osirion.Blazor.Cms.Admin.Core.Events;
using Osirion.Blazor.Cms.Admin.Features.ContentEditor.Services;
using Osirion.Blazor.Cms.Admin.Infrastructure.Adapters;
using Osirion.Blazor.Cms.Admin.Interfaces;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Models;
using Osirion.Blazor.Cms.Domain.Models.GitHub;
using Osirion.Blazor.Cms.Domain.Options.Configuration;
using Osirion.Blazor.Cms.Domain.ValueObjects;
using Osirion.Blazor.Cms.Admin.Tests.TestDoubles;
using Shouldly;

namespace Osirion.Blazor.Cms.Admin.Tests.Features.ContentEditor.Services;

public class ContentEditorServiceTests
{
    private readonly IContentRepositoryAdapter _repositoryAdapter;
    private readonly IAdminContentService _contentService;
    private readonly IEventPublisher _eventPublisher;
    private readonly CmsAdminOptions _options;
    private readonly RecordingLogger<ContentEditorService> _logger;
    private readonly ContentEditorService _service;

    public ContentEditorServiceTests()
    {
        _repositoryAdapter = Substitute.For<IContentRepositoryAdapter>();
        _contentService = Substitute.For<IAdminContentService>();
        _eventPublisher = Substitute.For<IEventPublisher>();
        _options = new CmsAdminOptions
        {
            ContentRules = new ContentRulesOptions
            {
                EnforceFrontMatterValidation = true,
                RequiredFrontMatterFields = new List<string> { "Title" },
                AllowedFileExtensions = new List<string> { ".md" },
                AllowFileDeletion = true
            }
        };
        _logger = new RecordingLogger<ContentEditorService>();

        var optionsWrapper = Microsoft.Extensions.Options.Options.Create(_options);
        _service = new ContentEditorService(
            _repositoryAdapter,
            _contentService,
            _eventPublisher,
            optionsWrapper,
            _logger
        );
    }

    [Fact]
    public async Task GetBlogPostAsync_ShouldReturnBlogPost()
    {
        // Arrange
        var path = "content/blog/post.md";
        var expectedPost = new ContentItem
        {
            Path = path,
            Content = "# Test Content",
            Metadata = FrontMatter.Create("Test Title", "Test Description", System.DateTime.Now),
            Sha = "test-sha"
        };

        _repositoryAdapter.GetBlogPostAsync(path).Returns(expectedPost);

        // Act
        var result = await _service.GetBlogPostAsync(path);

        // Assert
        result.ShouldBe(expectedPost);
        await _repositoryAdapter.Received(1).GetBlogPostAsync(path);

        _logger.HasEntry(LogLevel.Information, "Fetching blog post", path).ShouldBeTrue();

        _logger.HasEntry(LogLevel.Information, "Blog post fetched successfully", path).ShouldBeTrue();
    }

    [Fact]
    public async Task GetBlogPostAsync_WhenExceptionThrown_ShouldLogAndRethrow()
    {
        // Arrange
        var path = "content/blog/post.md";
        var exception = new InvalidOperationException("Test error");

        _repositoryAdapter.GetBlogPostAsync(path).Throws(exception);

        // Act & Assert
        var ex = await Should.ThrowAsync<InvalidOperationException>(
            async () => await _service.GetBlogPostAsync(path)
        );

        ex.ShouldBe(exception);

        _logger.HasEntry(LogLevel.Error, "Error fetching blog post", path).ShouldBeTrue();
        _logger.Entries.ShouldContain(entry => entry.Level == LogLevel.Error && entry.Exception == exception);

        _eventPublisher.Received(1).Publish(
            Arg.Is<ErrorOccurredEvent>(e =>
                e.Message.Contains("Failed to load file") &&
                e.Message.Contains(path) &&
                e.Exception == exception)
        );
    }

    [Fact]
    public async Task SaveBlogPostAsync_ShouldSaveAndPublishEvent()
    {
        // Arrange
        var post = new ContentItem
        {
            Path = "content/blog/post.md",
            Content = "# Test Content",
            Metadata = FrontMatter.Create("Test Title", "Test Description", System.DateTime.Now),
            Sha = "existing-sha"
        };

        var commitMessage = "Update post";

        var commitResponse = new GitHubFileCommitResponse
        {
            Content = new GitHubFileContent
            {
                Path = post.Path,
                Sha = "new-sha-123"
            }
        };

        _repositoryAdapter
            .SaveContentAsync(post.Path, Arg.Any<string>(), commitMessage, post.Sha)
            .Returns(commitResponse);

        // Act
        var result = await _service.SaveBlogPostAsync(post, commitMessage);

        // Assert
        result.ShouldBe(commitResponse);

        await _repositoryAdapter.Received(1).SaveContentAsync(
            post.Path,
            Arg.Any<string>(),
            commitMessage,
            post.Sha
        );

        _logger.HasEntry(LogLevel.Information, "Saving blog post", post.Path).ShouldBeTrue();

        _logger.HasEntry(LogLevel.Information, "Blog post saved successfully", post.Path).ShouldBeTrue();

        _eventPublisher.Received(1).Publish(
            Arg.Is<ContentSavedEvent>(e => e.Path == post.Path)
        );
    }

    [Fact]
    public async Task SaveBlogPostAsync_WhenValidationFails_ShouldThrowValidationException()
    {
        // Arrange
        var post = new ContentItem
        {
            Path = "content/blog/post.md",
            Content = "# Test Content",
            Metadata = new FrontMatter(), // Missing Title which is required
            Sha = "existing-sha"
        };

        var commitMessage = "Update post";

        // Act & Assert
        var ex = await Should.ThrowAsync<ValidationException>(
            async () => await _service.SaveBlogPostAsync(post, commitMessage)
        );

        ex.Message.ShouldContain("Required field 'Title'");

        await _repositoryAdapter.DidNotReceive().SaveContentAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>()
        );

        _eventPublisher.DidNotReceive().Publish(Arg.Any<ContentSavedEvent>());
    }

    [Fact]
    public async Task SaveBlogPostAsync_WithInvalidFileExtension_ShouldThrowValidationException()
    {
        // Arrange
        var post = new ContentItem
        {
            Path = "content/blog/post.txt", // Not an allowed extension
            Content = "# Test Content",
            Metadata = FrontMatter.Create("Test Title", "Test Description", System.DateTime.Now),
            Sha = "existing-sha"
        };

        var commitMessage = "Update post";

        // Act & Assert
        var ex = await Should.ThrowAsync<ValidationException>(
            async () => await _service.SaveBlogPostAsync(post, commitMessage)
        );

        ex.Message.ShouldContain("File extension");
        ex.Message.ShouldContain("not allowed");

        await _repositoryAdapter.DidNotReceive().SaveContentAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>()
        );

        _eventPublisher.DidNotReceive().Publish(Arg.Any<ContentSavedEvent>());
    }

    [Fact]
    public async Task DeleteBlogPostAsync_ShouldDeleteAndPublishEvent()
    {
        // Arrange
        var path = "content/blog/post.md";
        var sha = "test-sha";

        var commitResponse = new GitHubFileCommitResponse
        {
            Commit = new GitHubCommitInfo { Sha = "commit-sha" }
        };

        _repositoryAdapter
            .DeleteFileAsync(path, Arg.Any<string>(), sha)
            .Returns(commitResponse);

        // Act
        var result = await _service.DeleteBlogPostAsync(path, sha);

        // Assert
        result.ShouldBe(commitResponse);

        await _repositoryAdapter.Received(1).DeleteFileAsync(
            path,
            Arg.Any<string>(),
            sha
        );

        _logger.HasEntry(LogLevel.Information, "Deleting blog post", path).ShouldBeTrue();

        _logger.HasEntry(LogLevel.Information, "Blog post deleted successfully", path).ShouldBeTrue();

        _eventPublisher.Received(1).Publish(
            Arg.Is<ContentDeletedEvent>(e => e.Path == path)
        );
    }

    [Fact]
    public async Task DeleteBlogPostAsync_WhenDeletionDisabled_ShouldThrowException()
    {
        // Arrange
        _options.ContentRules.AllowFileDeletion = false;

        var path = "content/blog/post.md";
        var sha = "test-sha";

        // Act & Assert
        var ex = await Should.ThrowAsync<InvalidOperationException>(
            async () => await _service.DeleteBlogPostAsync(path, sha)
        );

        ex.Message.ShouldContain("File deletion is not allowed");

        await _repositoryAdapter.DidNotReceive().DeleteFileAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>()
        );

        _eventPublisher.DidNotReceive().Publish(Arg.Any<ContentDeletedEvent>());
    }

    [Fact]
    public void CreateNewBlogPost_ShouldReturnNewBlogPost()
    {
        // Arrange
        var path = "content/blog";
        var title = "New Test Post";

        // Act
        var result = _service.CreateNewBlogPost(path, title);

        // Assert
        result.ShouldNotBeNull();
        result.Path.ShouldBe("content/blog/new-test-post.md");
        result.Content.ShouldContain(title);
        result.Metadata.ShouldNotBeNull();
        result.Metadata.Title.ShouldBe(title);
    }

    [Fact]
    public void GenerateFileNameFromTitle_ShouldReturnValidFileName()
    {
        // Arrange & Act & Assert
        _service.GenerateFileNameFromTitle("Test Title").ShouldBe("test-title.md");
        _service.GenerateFileNameFromTitle("Special Ch@racters & Spaces").ShouldBe("special-chracters-spaces.md");
        _service.GenerateFileNameFromTitle("Multiple---Hyphens").ShouldBe("multiple-hyphens.md");
        _service.GenerateFileNameFromTitle("").ShouldBe("new-post.md");

        // Already has .md extension
        _service.GenerateFileNameFromTitle("Test.md").ShouldBe("test.md");
    }

    [Fact]
    public void ConvertToBlogPost_ShouldMapContentItemToBlogPost()
    {
        // Arrange
        var contentItem = Cms.Domain.Entities.ContentItem.Create(id: "123", title: "Test Title", content: "# Test Content", path: "content/blog/post.md", providerId: "provider-id");

        // Act
        var result = _service.ConvertToBlogPost(contentItem);

        // Assert
        result.ShouldNotBeNull();
        result.Path.ShouldBe(contentItem.Path);
        result.Content.ShouldBe(contentItem.Content);
        result.Sha.ShouldBe(contentItem.Sha);

        result.Metadata.ShouldNotBeNull();
        result.Metadata.Title.ShouldBe(contentItem.Title);
        result.Metadata.Description.ShouldBe(contentItem.Description);
        result.Metadata.Author.ShouldBe(contentItem.Author);
        result.Metadata.Date.ShouldBe(FrontMatterDates.Format(contentItem.DateCreated));
        result.Metadata.Tags.ShouldBe(contentItem.Tags.ToList());
        result.Metadata.Categories.ShouldBe(contentItem.Categories.ToList());
        result.Metadata.Slug.ShouldBe(contentItem.Slug);
        result.Metadata.IsFeatured.ShouldBe(contentItem.IsFeatured);
        result.Metadata.Published.ShouldBe(contentItem.IsPublished);
    }

    // Failed before the fix: the post was dated by its day alone.
    [Fact]
    public void ConvertToBlogPost_ItemPublishedAtATime_KeepsTheTime()
    {
        var contentItem = Cms.Domain.Entities.ContentItem.Create(id: "123", title: "Test Title", content: "# Test Content", path: "content/blog/post.md", providerId: "provider-id");
        contentItem.SetCreatedDate(new DateTime(2026, 10, 29, 17, 30, 0, DateTimeKind.Utc));

        var result = _service.ConvertToBlogPost(contentItem);

        result.Metadata.ShouldNotBeNull().Date.ShouldBe("2026-10-29T17:30:00Z");
    }
}