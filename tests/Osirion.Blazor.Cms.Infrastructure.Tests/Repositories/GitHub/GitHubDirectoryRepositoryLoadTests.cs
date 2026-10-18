using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Exceptions;
using Osirion.Blazor.Cms.Domain.Interfaces;
using Osirion.Blazor.Cms.Domain.Interfaces.Directory;
using Osirion.Blazor.Cms.Domain.Models.GitHub;
using Osirion.Blazor.Cms.Domain.Options;
using Osirion.Blazor.Cms.Infrastructure.GitHub;
using Shouldly;

namespace Osirion.Blazor.Cms.Infrastructure.Tests.Repositories.GitHub;

/// <summary>
/// Covers how <see cref="GitHubDirectoryRepository"/> builds its directory cache from a repository tree or from
/// directory listings, and how it reads <c>_index.md</c> metadata.
/// </summary>
public class GitHubDirectoryRepositoryLoadTests
{
    private readonly IGitHubApiClient _apiClient = Substitute.For<IGitHubApiClient>();
    private readonly IDirectoryCacheManager _cacheManager = Substitute.For<IDirectoryCacheManager>();
    private readonly IDirectoryMetadataProcessor _metadataProcessor = Substitute.For<IDirectoryMetadataProcessor>();
    private readonly IPathUtilities _pathUtils = Substitute.For<IPathUtilities>();
    private readonly GitHubOptions _options = new()
    {
        Owner = "owner",
        Repository = "repo",
        Branch = "main",
        ContentPath = "content",
        ProviderId = "github-test",
        DefaultLocale = "en"
    };

    public GitHubDirectoryRepositoryLoadTests()
    {
        _pathUtils.NormalizePath(Arg.Any<string>()).Returns(call => call.Arg<string>());

        // Every read runs the repository's own loader, so each test sees a freshly built cache.
        _cacheManager
            .GetCachedDirectoriesAsync(
                Arg.Any<Func<CancellationToken, Task<Dictionary<string, DirectoryItem>>>>(),
                Arg.Any<CancellationToken>(),
                Arg.Any<bool>())
            .Returns(call => call.Arg<Func<CancellationToken, Task<Dictionary<string, DirectoryItem>>>>()(CancellationToken.None));
    }

    private GitHubDirectoryRepository CreateRepository()
    {
        var factory = Substitute.For<IGitHubApiClientFactory>();
        factory.GetDefaultClient().Returns(_apiClient);

        return new GitHubDirectoryRepository(
            factory,
            Options.Create(_options),
            _cacheManager,
            _metadataProcessor,
            _pathUtils,
            NullLogger<GitHubDirectoryRepository>.Instance);
    }

    private static GitHubItem Dir(string path) => new() { Path = path, Name = path[(path.LastIndexOf('/') + 1)..], Type = "dir" };

    private static GitHubItem File(string path) => new() { Path = path, Name = path[(path.LastIndexOf('/') + 1)..], Type = "file", Sha = path + "-sha" };

    private static GitHubFileContent IndexFile(string path, string content, string sha) => new()
    {
        Name = "_index.md",
        Path = path,
        Content = content,
        Sha = sha
    };

    private void SetTree(bool truncated, params (string Path, string Type)[] entries)
    {
        _apiClient.GetTreeAsync(Arg.Any<CancellationToken>()).Returns(new GitHubTree
        {
            Truncated = truncated,
            Entries = entries.Select(entry => new GitHubTreeEntry { Path = entry.Path, Type = entry.Type, Sha = entry.Path + "-sha" }).ToList()
        });
    }

    private void SetListing(string path, params GitHubItem[] items)
    {
        _apiClient.GetRepositoryContentsAsync(path, Arg.Any<CancellationToken>()).Returns(items.ToList());
    }

    [Fact]
    public async Task GetAllAsync_WithTree_BuildsHierarchyWithoutListingRequests()
    {
        SetTree(false,
            ("content/blog", "tree"),
            ("content/blog/2024", "tree"),
            ("content/blog/_index.md", "blob"),
            ("content/docs", "tree"),
            ("content/readme.md", "blob"),
            ("other/ignored", "tree"));
        _apiClient.GetFileContentAsync("content/blog/_index.md", Arg.Any<CancellationToken>())
            .Returns(IndexFile("content/blog/_index.md", "---\ntitle: Blog\n---", "blog-index-sha"));
        var repository = CreateRepository();

        var roots = await repository.GetAllAsync();

        roots.Select(d => d.Name).OrderBy(n => n).ShouldBe(["blog", "docs"]);
        var blog = roots.Single(d => d.Name == "blog");
        blog.Children.Select(c => c.Path).ShouldBe(["content/blog/2024"]);
        blog.ProviderSpecificId.ShouldBe("blog-index-sha");
        blog.Locale.ShouldBe("en");
        _metadataProcessor.Received(1).ProcessMetadata(blog, "---\ntitle: Blog\n---");
        await _apiClient.Received(1).GetFileContentAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _apiClient.DidNotReceive().GetRepositoryContentsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    // Fails before the fix: folder locales came from the path settings of the file system provider, which do not know
    // this repository's content folder, so a Serbian folder was read with the default locale.
    [Fact]
    public async Task GetAllAsync_LocalizedContent_TakesEachFolderLocaleFromItsFolderUnderTheContentPath()
    {
        _options.EnableLocalization = true;
        _options.SupportedLocales = ["en", "sr"];
        SetTree(false,
            ("content/en", "tree"),
            ("content/en/blog", "tree"),
            ("content/sr", "tree"),
            ("content/sr/blog", "tree"));
        var repository = CreateRepository();

        var roots = await repository.GetAllAsync();

        roots.Single(d => d.Name == "en").Locale.ShouldBe("en");
        var serbian = roots.Single(d => d.Name == "sr");
        serbian.Locale.ShouldBe("sr");
        serbian.Children.ShouldHaveSingleItem().Locale.ShouldBe("sr");
    }

    [Fact]
    public async Task GetAllAsync_WithoutTree_ListsDirectoriesAndProbesEveryIndexFile()
    {
        _apiClient.GetTreeAsync(Arg.Any<CancellationToken>()).Returns((GitHubTree?)null);
        SetListing("content", Dir("content/blog"), File("content/readme.md"));
        SetListing("content/blog", Dir("content/blog/2024"));
        _apiClient.GetRepositoryContentsAsync("content/blog/2024", Arg.Any<CancellationToken>()).Returns((List<GitHubItem>?)null);
        _apiClient.GetFileContentAsync("content/blog/_index.md", Arg.Any<CancellationToken>())
            .Returns(IndexFile("content/blog/_index.md", "blog metadata", "blog-index-sha"));
        _apiClient.GetFileContentAsync("content/blog/2024/_index.md", Arg.Any<CancellationToken>())
            .Returns((GitHubFileContent?)null);
        var repository = CreateRepository();

        var roots = await repository.GetAllAsync();

        var blog = roots.ShouldHaveSingleItem();
        blog.Path.ShouldBe("content/blog");
        blog.ProviderSpecificId.ShouldBe("blog-index-sha");
        var year = blog.Children.ShouldHaveSingleItem();
        year.Path.ShouldBe("content/blog/2024");
        year.ProviderSpecificId.ShouldBeNullOrEmpty();
        await _apiClient.Received(1).GetFileContentAsync("content/blog/2024/_index.md", Arg.Any<CancellationToken>());
        _metadataProcessor.Received(1).ProcessMetadata(Arg.Any<DirectoryItem>(), Arg.Any<string>());
    }

    [Fact]
    public async Task GetAllAsync_WithTruncatedTree_FallsBackToListing()
    {
        SetTree(true, ("content/from-tree", "tree"));
        SetListing("content", Dir("content/from-listing"));
        var repository = CreateRepository();

        var roots = await repository.GetAllAsync();

        roots.ShouldHaveSingleItem().Path.ShouldBe("content/from-listing");
    }

    [Fact]
    public async Task GetAllAsync_WhenTreeRequestFails_FallsBackToListing()
    {
        _apiClient.GetTreeAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new HttpRequestException("boom"));
        SetListing("content", Dir("content/listed"));
        var repository = CreateRepository();

        var roots = await repository.GetAllAsync();

        roots.ShouldHaveSingleItem().Path.ShouldBe("content/listed");
    }

    [Fact]
    public async Task GetAllAsync_WhenTreeRequestIsRateLimited_FailsWithoutListing()
    {
        _apiClient.GetTreeAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new GitHubRateLimitException(null, HttpStatusCode.TooManyRequests));
        var repository = CreateRepository();

        var exception = await Should.ThrowAsync<ContentProviderException>(() => repository.GetAllAsync());

        exception.InnerException.ShouldBeOfType<GitHubRateLimitException>();
        await _apiClient.DidNotReceive().GetRepositoryContentsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAllAsync_WhenIndexFileCannotBeRead_KeepsDirectoryWithoutMetadata()
    {
        SetTree(false, ("content/blog", "tree"), ("content/blog/_index.md", "blob"));
        _apiClient.GetFileContentAsync("content/blog/_index.md", Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("not found"));
        var repository = CreateRepository();

        var roots = await repository.GetAllAsync();

        roots.ShouldHaveSingleItem().ProviderSpecificId.ShouldBeNullOrEmpty();
        _metadataProcessor.DidNotReceive().ProcessMetadata(Arg.Any<DirectoryItem>(), Arg.Any<string>());
    }

    [Fact]
    public async Task GetAllAsync_WhenIndexFileIsRateLimited_FailsTheLoad()
    {
        SetTree(false, ("content/blog", "tree"), ("content/blog/_index.md", "blob"));
        _apiClient.GetFileContentAsync("content/blog/_index.md", Arg.Any<CancellationToken>())
            .ThrowsAsync(new GitHubRateLimitException(DateTimeOffset.UnixEpoch, HttpStatusCode.Forbidden));
        var repository = CreateRepository();

        var exception = await Should.ThrowAsync<ContentProviderException>(() => repository.GetAllAsync());

        exception.InnerException.ShouldBeOfType<GitHubRateLimitException>();
    }

    [Fact]
    public async Task GetAllAsync_WhenListingRepeatsAPath_ProcessesItOnce()
    {
        _apiClient.GetTreeAsync(Arg.Any<CancellationToken>()).Returns((GitHubTree?)null);
        SetListing("content", Dir("content/loop"));
        SetListing("content/loop", Dir("content/LOOP"));
        var repository = CreateRepository();

        var roots = await repository.GetAllAsync();

        var loop = roots.ShouldHaveSingleItem();
        loop.Children.ShouldBeEmpty();
        await _apiClient.Received(1).GetRepositoryContentsAsync("content/loop", Arg.Any<CancellationToken>());
        await _apiClient.DidNotReceive().GetRepositoryContentsAsync("content/LOOP", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAllAsync_WithLocalization_TakesLocaleFromPath()
    {
        // The folder is a locale when this provider lists it, the same rule its content files follow.
        _options.EnableLocalization = true;
        _options.SupportedLocales = ["en", "de"];
        SetTree(false, ("content/de", "tree"));
        var repository = CreateRepository();

        var roots = await repository.GetAllAsync();

        roots.ShouldHaveSingleItem().Locale.ShouldBe("de");
    }

    [Fact]
    public async Task GetAllAsync_WhenIdsCollide_ReusesTheCachedDirectoryAndAttachesItToItsNewParent()
    {
        // The identifier hashes "path|name|provider", so these two entries share one identifier.
        _apiClient.GetTreeAsync(Arg.Any<CancellationToken>()).Returns((GitHubTree?)null);
        SetListing("content",
            new GitHubItem { Path = "content/q|r", Name = "s", Type = "dir" },
            new GitHubItem { Path = "content/p", Name = "p", Type = "dir" });
        SetListing("content/p", new GitHubItem { Path = "content/q", Name = "r|s", Type = "dir" });
        var repository = CreateRepository();

        var roots = await repository.GetAllAsync();

        var parent = roots.ShouldHaveSingleItem();
        parent.Path.ShouldBe("content/p");
        parent.Children.ShouldHaveSingleItem().Path.ShouldBe("content/q|r");
        await _apiClient.Received(1).GetRepositoryContentsAsync("content/q", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAllAsync_WhenAChildCollidesWithItsParent_KeepsTheParentAsARoot()
    {
        _apiClient.GetTreeAsync(Arg.Any<CancellationToken>()).Returns((GitHubTree?)null);
        SetListing("content", new GitHubItem { Path = "content/a", Name = "b|c", Type = "dir" });
        SetListing("content/a", new GitHubItem { Path = "content/a|b", Name = "c", Type = "dir" });
        var repository = CreateRepository();

        var roots = await repository.GetAllAsync();

        var root = roots.ShouldHaveSingleItem();
        root.Path.ShouldBe("content/a");
        root.Children.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WhenCancelled_ThrowsWithoutListingChildren()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        _cacheManager
            .GetCachedDirectoriesAsync(
                Arg.Any<Func<CancellationToken, Task<Dictionary<string, DirectoryItem>>>>(),
                Arg.Any<CancellationToken>(),
                Arg.Any<bool>())
            .Returns(call => call.Arg<Func<CancellationToken, Task<Dictionary<string, DirectoryItem>>>>()(cancellation.Token));
        SetTree(false, ("content/blog", "tree"));
        var repository = CreateRepository();

        var exception = await Should.ThrowAsync<ContentProviderException>(() => repository.GetAllAsync());

        exception.InnerException.ShouldBeAssignableTo<OperationCanceledException>();
    }
}
