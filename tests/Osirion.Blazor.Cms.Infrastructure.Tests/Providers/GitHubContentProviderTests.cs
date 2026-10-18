using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Interfaces;
using Osirion.Blazor.Cms.Domain.Interfaces.Content;
using Osirion.Blazor.Cms.Domain.Interfaces.Directory;
using Osirion.Blazor.Cms.Domain.Models.GitHub;
using Osirion.Blazor.Cms.Domain.Options;
using Osirion.Blazor.Cms.Domain.Repositories;
using Osirion.Blazor.Cms.Domain.ValueObjects;
using Osirion.Blazor.Cms.Infrastructure.GitHub;
using Osirion.Blazor.Cms.Infrastructure.Providers;
using Osirion.Blazor.Cms.Infrastructure.Tests.TestDoubles;
using Shouldly;

namespace Osirion.Blazor.Cms.Infrastructure.Tests.Providers;

/// <summary>
/// Drives <see cref="GitHubContentProvider"/> over real GitHub repositories whose API client is a substitute, so
/// each content load shows up as one Git Trees request on <see cref="_repositoryClient"/>.
/// </summary>
public class GitHubContentProviderTests : IDisposable
{
    private static readonly TimeSpan BackgroundTimeout = TimeSpan.FromSeconds(10);

    private readonly IGitHubApiClient _providerClient = Substitute.For<IGitHubApiClient>();
    private readonly IGitHubApiClient _repositoryClient = Substitute.For<IGitHubApiClient>();
    private readonly IGitHubApiClientFactory _providerClientFactory = Substitute.For<IGitHubApiClientFactory>();
    private readonly IContentQueryFilter _queryFilter = Substitute.For<IContentQueryFilter>();
    private readonly IDirectoryCacheManager _directoryCache = Substitute.For<IDirectoryCacheManager>();
    private readonly IMarkdownProcessor _markdown = Substitute.For<IMarkdownProcessor>();
    private readonly IPathUtilities _pathUtils = Substitute.For<IPathUtilities>();
    private readonly RecordingLogger<GitHubContentProvider> _logger = new();
    private readonly MemoryCache _memoryCache = new(new MemoryCacheOptions());
    private readonly DirectoryItem _blogDirectory;
    private readonly GitHubOptions _options = new()
    {
        Owner = "o",
        Repository = "r",
        Branch = "main",
        ContentPath = "content",
        ApiToken = "token",
        UseBackgroundCacheUpdate = false,
        SupportedExtensions = [".md"]
    };

    public GitHubContentProviderTests()
    {
        _providerClientFactory.GetDefaultClient().Returns(_providerClient);
        ReturnBranches(_providerClient, Branch("dev", "sha-dev"), Branch("main", "sha-1"));
        ReturnBranches(_repositoryClient);

        _repositoryClient.GetTreeAsync(Arg.Any<CancellationToken>()).Returns(new GitHubTree
        {
            Entries =
            [
                new GitHubTreeEntry { Path = "README.md", Type = "blob", Sha = "s-readme" },
                new GitHubTreeEntry { Path = "content/a.md", Type = "blob", Sha = "s-a" },
                new GitHubTreeEntry { Path = "content/b.md", Type = "blob", Sha = "s-b" }
            ]
        });
        ServeFile("content/a.md", "s-a", "a-source", new FrontMatter
        {
            Title = "Alpha",
            Lang = "en",
            Categories = ["Web", "Tools"],
            Tags = ["csharp"]
        });
        ServeFile("content/b.md", "s-b", "b-source", new FrontMatter
        {
            Title = "Beta",
            Lang = "fr",
            Categories = ["web"],
            Tags = ["CSharp", "blazor"]
        });
        _markdown.RenderToHtmlAsync(Arg.Any<string>(), Arg.Any<bool>())
            .Returns(call => $"<p>{call.Arg<string>()}</p>");

        _queryFilter.ApplyFilters(Arg.Any<IQueryable<ContentItem>>(), Arg.Any<ContentQuery>())
            .Returns(call => call.Arg<IQueryable<ContentItem>>());

        _pathUtils.NormalizePath(Arg.Any<string>()).Returns(call => call.Arg<string>());
        _blogDirectory = DirectoryItem.Create("dir-blog", "content/blog", "Blog", "github-o-r");
        _blogDirectory.SetUrl("blog");
        _directoryCache.GetCachedDirectoriesAsync(
                Arg.Any<Func<CancellationToken, Task<Dictionary<string, DirectoryItem>>>>(),
                Arg.Any<CancellationToken>(),
                Arg.Any<bool>())
            .Returns(_ => new Dictionary<string, DirectoryItem> { [_blogDirectory.Id] = _blogDirectory });
    }

    public void Dispose()
    {
        _memoryCache.Dispose();
    }

    [Fact]
    public void Constructor_WithNullContentRepository_Throws()
    {
        // Act
        var act = () => new GitHubContentProvider(
            null!, CreateDirectoryRepository(), Options.Create(_options), _providerClientFactory, _memoryCache,
            _logger);

        // Assert
        act.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("contentRepository");
    }

    [Fact]
    public void Constructor_WithNullDirectoryRepository_Throws()
    {
        // Act
        var act = () => new GitHubContentProvider(
            CreateContentRepository(), null!, Options.Create(_options), _providerClientFactory, _memoryCache,
            _logger);

        // Assert
        act.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("directoryRepository");
    }

    [Fact]
    public void Constructor_WithNullOptions_Throws()
    {
        // Act
        var act = () => new GitHubContentProvider(
            CreateContentRepository(), CreateDirectoryRepository(), null!, _providerClientFactory, _memoryCache,
            _logger);

        // Assert
        act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Properties_WithoutAProviderId_DescribeTheRepository()
    {
        // Arrange
        var provider = CreateProvider();

        // Assert
        provider.ProviderId.ShouldBe("github-o-r");
        provider.DisplayName.ShouldBe("GitHub: o/r");
        provider.IsReadOnly.ShouldBeFalse();
    }

    [Fact]
    public void Properties_WithAProviderIdAndNoToken_UseThatIdAndAreReadOnly()
    {
        // Arrange
        _options.ProviderId = "docs";
        _options.ApiToken = " ";
        var provider = CreateProvider();

        // Assert
        provider.ProviderId.ShouldBe("docs");
        provider.IsReadOnly.ShouldBeTrue();
    }

    [Fact]
    public async Task InitializeAsync_WithAProviderName_UsesTheNamedClient()
    {
        // Arrange
        var namedClient = Substitute.For<IGitHubApiClient>();
        ReturnBranches(namedClient, Branch("main", "sha-1"));
        _providerClientFactory.GetClient("Blog").Returns(namedClient);
        var provider = CreateProvider("Blog");

        // Act
        await provider.InitializeAsync();

        // Assert
        namedClient.Received(1).SetRepository("o", "r");
        _providerClientFactory.DidNotReceive().GetDefaultClient();
    }

    [Fact]
    public async Task InitializeAsync_ConfiguresTheClientAndLoadsContentAndDirectories()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        await provider.InitializeAsync();

        // Assert
        _providerClient.Received(1).SetRepository("o", "r");
        _providerClient.Received(1).SetBranch("main");
        _providerClient.Received(1).SetAccessToken("token");
        await _repositoryClient.Received(1).GetTreeAsync(Arg.Any<CancellationToken>());
        await _directoryCache.Received().InvalidateCacheAsync(Arg.Any<CancellationToken>());
        _logger.Contains(LogLevel.Information, "Latest commit SHA for branch main: sha-1").ShouldBeTrue();
        _logger.Contains(LogLevel.Information, "Initializing content provider: github-o-r").ShouldBeTrue();
    }

    [Fact]
    public async Task InitializeAsync_WithoutAToken_DoesNotSetOne()
    {
        // Arrange
        _options.ApiToken = string.Empty;
        var provider = CreateProvider();

        // Act
        await provider.InitializeAsync();

        // Assert
        _providerClient.DidNotReceive().SetAccessToken(Arg.Any<string>());
    }

    [Fact]
    public async Task ReadMethods_InitializeOnlyOnce()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        await provider.GetAllItemsAsync();
        await provider.GetDirectoriesAsync();
        await provider.InitializeAsync();

        // Assert
        await _providerClient.Received(1).GetBranchesAsync(Arg.Any<CancellationToken>());
        await _repositoryClient.Received(1).GetTreeAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReadMethods_CalledTogether_ShareOneInitialization()
    {
        // Arrange
        var branches = new TaskCompletionSource<List<GitHubBranch>?>(TaskCreationOptions.RunContinuationsAsynchronously);
        _providerClient.GetBranchesAsync(Arg.Any<CancellationToken>()).Returns(branches.Task);
        var provider = CreateProvider();

        // Act
        var first = provider.GetAllItemsAsync();
        var second = provider.GetAllItemsAsync();
        branches.SetResult([Branch("main", "sha-1")]);
        var results = await Task.WhenAll(first, second);

        // Assert
        await _providerClient.Received(1).GetBranchesAsync(Arg.Any<CancellationToken>());
        results.ShouldAllBe(items => items.Count == 2);
    }

    [Fact]
    public async Task InitializeAsync_WhenAlreadyCancelled_ThrowsAndInitializesOnTheNextCall()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        await Should.ThrowAsync<OperationCanceledException>(() => provider.InitializeAsync(new CancellationToken(true)));
        _providerClient.DidNotReceive().SetRepository(Arg.Any<string>(), Arg.Any<string>());
        await provider.InitializeAsync();

        // Assert
        _providerClient.Received(1).SetRepository("o", "r");
    }

    [Fact]
    public async Task InitializeAsync_WhenReadingBranchesFails_StillLoadsContent()
    {
        // Arrange
        _providerClient.GetBranchesAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new HttpRequestException("offline"));
        var provider = CreateProvider();

        // Act
        var items = await provider.GetAllItemsAsync();

        // Assert
        items.Count.ShouldBe(2);
        _logger.Contains(LogLevel.Warning, "Failed to get latest commit SHA during initialization").ShouldBeTrue();
    }

    [Fact]
    public async Task InitializeAsync_WhenTheBranchIsMissing_DoesNotTreatAnyShaAsCurrent()
    {
        // Arrange
        ReturnBranches(_providerClient, Branch("dev", "sha-1"));
        var provider = CreateProvider();
        await provider.InitializeAsync();

        // Act
        await provider.UpdateCacheAsync("sha-1");

        // Assert
        await _repositoryClient.Received(2).GetTreeAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InitializeAsync_WhenCancelledWhileLoading_LogsAndStaysInitialized()
    {
        // Arrange
        using var cancellation = new CancellationTokenSource();
        _providerClient.GetBranchesAsync(Arg.Any<CancellationToken>()).Returns(_ =>
        {
            cancellation.Cancel();
            return new List<GitHubBranch> { Branch("main", "sha-1") };
        });
        var provider = CreateProvider();

        // Act
        await provider.InitializeAsync(cancellation.Token);
        await provider.GetAllItemsAsync();

        // Assert
        _logger.Contains(LogLevel.Error, "Error loading initial content data").ShouldBeTrue();
        await _providerClient.Received(1).GetBranchesAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("item-id")]
    [InlineData("item-path")]
    [InlineData("item-url")]
    [InlineData("directory-path")]
    [InlineData("directory-id")]
    [InlineData("directory-url")]
    public async Task ReadMethods_WithABlankKey_ThrowWithoutInitializing(string method)
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        Func<Task> act = method switch
        {
            "item-id" => () => provider.GetItemByIdAsync(" "),
            "item-path" => () => provider.GetItemByPathAsync(""),
            "item-url" => () => provider.GetItemByUrlAsync(" "),
            "directory-path" => () => provider.GetDirectoryByPathAsync(""),
            "directory-id" => () => provider.GetDirectoryByIdAsync(" "),
            _ => () => provider.GetDirectoryByUrlAsync("")
        };

        // Assert
        await act.ShouldThrowAsync<ArgumentException>();
        _providerClient.DidNotReceive().SetRepository(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task GetAllItemsAsync_ReturnsEveryContentFileUnderTheContentPath()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var items = await provider.GetAllItemsAsync();

        // Assert
        items.Select(i => i.Path).OrderBy(p => p, StringComparer.Ordinal).ShouldBe(["content/a.md", "content/b.md"]);
        items.Single(i => i.Path == "content/a.md").Title.ShouldBe("Alpha");
    }

    [Fact]
    public async Task ItemLookups_FindTheItemByIdPathAndUrl()
    {
        // Arrange
        var provider = CreateProvider();
        var alpha = (await provider.GetAllItemsAsync()).Single(i => i.Path == "content/a.md");

        // Act
        var byId = await provider.GetItemByIdAsync(alpha.Id);
        var byPath = await provider.GetItemByPathAsync("content/a.md");
        var byUrl = await provider.GetItemByUrlAsync(alpha.Url);

        // Assert
        byId.ShouldBeSameAs(alpha);
        byPath.ShouldBeSameAs(alpha);
        byUrl.ShouldBeSameAs(alpha);
    }

    [Fact]
    public async Task GetItemsByQueryAsync_PassesTheQueryToTheFilter()
    {
        // Arrange
        var provider = CreateProvider();
        var query = new ContentQuery { Tag = "csharp" };

        // Act
        var items = await provider.GetItemsByQueryAsync(query);

        // Assert
        items.ShouldNotBeNull().Count.ShouldBe(2);
        _queryFilter.Received(1).ApplyFilters(Arg.Any<IQueryable<ContentItem>>(), query);
    }

    [Fact]
    public async Task GetContentTranslationsAsync_QueriesByLocalizationId()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var items = await provider.GetContentTranslationsAsync("loc-1", CancellationToken.None);

        // Assert
        items.ShouldNotBeNull().Count.ShouldBe(2);
        _queryFilter.Received(1).ApplyFilters(
            Arg.Any<IQueryable<ContentItem>>(),
            Arg.Is<ContentQuery>(q => q.LocalizationId == "loc-1"));
    }

    [Fact]
    public async Task DirectoryLookups_ReadTheDirectoryRepository()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var roots = await provider.GetDirectoriesAsync();
        var byPath = await provider.GetDirectoryByPathAsync("content/blog");
        var byId = await provider.GetDirectoryByIdAsync("dir-blog", "en");
        var byUrl = await provider.GetDirectoryByUrlAsync("blog");

        // Assert
        roots.ShouldHaveSingleItem().ShouldBeSameAs(_blogDirectory);
        byPath.ShouldBeSameAs(_blogDirectory);
        byId.ShouldBeSameAs(_blogDirectory);
        byUrl.ShouldBeSameAs(_blogDirectory);
    }

    [Fact]
    public async Task GetCategoriesAsync_GroupsCategoriesIgnoringCaseAndSortsByName()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var categories = await provider.GetCategoriesAsync();

        // Assert
        categories.Select(c => (c.Name, c.Slug, c.Count)).ShouldBe([("Tools", "tools", 1), ("Web", "web", 2)]);
    }

    [Fact]
    public async Task GetTagsAsync_GroupsTagsIgnoringCaseAndSortsByName()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var tags = await provider.GetTagsAsync();

        // Assert
        tags.Select(t => (t.Name, t.Slug, t.Count)).ShouldBe([("blazor", "blazor", 1), ("csharp", "csharp", 2)]);
    }

    [Fact]
    public async Task RefreshCacheAsync_ReloadsContentAndDirectories()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        await provider.RefreshCacheAsync();

        // Assert
        await _repositoryClient.Received(1).GetTreeAsync(Arg.Any<CancellationToken>());
        await _directoryCache.Received(1).InvalidateCacheAsync(Arg.Any<CancellationToken>());
        _logger.Contains(LogLevel.Information, "Cache refreshed successfully for GitHub provider: github-o-r").ShouldBeTrue();
    }

    [Fact]
    public async Task RefreshCacheAsync_WhenCancelled_LogsAndRethrows()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        await Should.ThrowAsync<OperationCanceledException>(() => provider.RefreshCacheAsync(new CancellationToken(true)));

        // Assert
        _logger.Contains(LogLevel.Error, "Error refreshing cache for GitHub provider: github-o-r").ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateCacheAsync_WithTheCurrentSha_SkipsTheRefresh()
    {
        // Arrange
        var provider = CreateProvider();
        await provider.InitializeAsync();

        // Act
        await provider.UpdateCacheAsync("sha-1");

        // Assert
        await _repositoryClient.Received(1).GetTreeAsync(Arg.Any<CancellationToken>());
        _logger.Contains(LogLevel.Debug, "SHA unchanged").ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateCacheAsync_WithANewSha_RefreshesAndThenTreatsThatShaAsCurrent()
    {
        // Arrange
        var provider = CreateProvider();
        await provider.InitializeAsync();

        // Act
        await provider.UpdateCacheAsync("sha-2");
        await provider.UpdateCacheAsync("sha-2");

        // Assert
        await _repositoryClient.Received(2).GetTreeAsync(Arg.Any<CancellationToken>());
        await _directoryCache.Received(2).InvalidateCacheAsync(Arg.Any<CancellationToken>());
        _logger.Contains(LogLevel.Information, "Content updated successfully to SHA sha-2").ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateCacheAsync_WithoutAShaAndAnEmptyQueue_DoesNothing()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        await provider.UpdateCacheAsync();

        // Assert
        await _repositoryClient.DidNotReceive().GetTreeAsync(Arg.Any<CancellationToken>());
        await _directoryCache.DidNotReceive().InvalidateCacheAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateCacheAsync_WhenTheRefreshFails_RetriesTheSameShaNextTime()
    {
        // Arrange
        var provider = CreateProvider();
        await provider.InitializeAsync();
        _repositoryClient.GetTreeAsync(Arg.Any<CancellationToken>()).Returns((GitHubTree?)null);
        _repositoryClient.GetRepositoryContentsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("offline"));

        // Act
        await provider.UpdateCacheAsync("sha-2");
        await provider.UpdateCacheAsync("sha-2");

        // Assert
        await _repositoryClient.Received(3).GetTreeAsync(Arg.Any<CancellationToken>());
        _logger.Contains(LogLevel.Warning, "Content refresh for SHA sha-2 failed; the next poll will retry").ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateCacheAsync_WhileInitializationHoldsTheLock_QueuesTheShaForTheNextRun()
    {
        // Arrange
        var branches = new TaskCompletionSource<List<GitHubBranch>?>(TaskCreationOptions.RunContinuationsAsynchronously);
        _providerClient.GetBranchesAsync(Arg.Any<CancellationToken>()).Returns(branches.Task);
        var provider = CreateProvider();
        var initialization = provider.InitializeAsync();

        // Act
        await provider.UpdateCacheAsync("sha-queued");
        branches.SetResult([Branch("main", "sha-1")]);
        await initialization;
        await provider.UpdateCacheAsync();

        // Assert
        _logger.Contains(LogLevel.Debug, "Another thread is already processing the queue").ShouldBeTrue();
        _logger.Contains(LogLevel.Information, "Content updated successfully to SHA sha-queued").ShouldBeTrue();
        await _repositoryClient.Received(2).GetTreeAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateCacheAsync_WithBackgroundUpdatesConfigured_RefreshesInTheBackground()
    {
        // Arrange
        _options.UseBackgroundCacheUpdate = true;
        var provider = CreateProvider();

        // Act
        await provider.UpdateCacheAsync("sha-2");
        await _logger.WaitForAsync(LogLevel.Information, "Content updated successfully to SHA sha-2", BackgroundTimeout);

        // Assert
        await _repositoryClient.Received(1).GetTreeAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateCacheAsync_WhenForcedWithTheCurrentSha_RefreshesAnyway()
    {
        // Arrange
        var provider = CreateProvider();
        await provider.InitializeAsync();

        // Act
        await provider.UpdateCacheAsync("sha-1", forceBackground: true);
        await _logger.WaitForAsync(LogLevel.Information, "Content updated successfully to SHA sha-1", BackgroundTimeout);

        // Assert
        await _repositoryClient.Received(2).GetTreeAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task HandleWebhookAsync_WithoutASha_IgnoresTheEvent(string sha)
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        await provider.HandleWebhookAsync(sha);

        // Assert
        _logger.Contains(LogLevel.Warning, "Received webhook with empty commit SHA").ShouldBeTrue();
        await _repositoryClient.DidNotReceive().GetTreeAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleWebhookAsync_WithASha_RefreshesInTheBackground()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        await provider.HandleWebhookAsync("sha-hook");
        await _logger.WaitForAsync(LogLevel.Information, "Content updated successfully to SHA sha-hook", BackgroundTimeout);

        // Assert
        _logger.Contains(LogLevel.Information, "Received webhook for commit SHA: sha-hook").ShouldBeTrue();
        await _repositoryClient.Received(1).GetTreeAsync(Arg.Any<CancellationToken>());
    }

    // Fails before the change: the provider had no API that reports its locales.
    [Fact]
    public async Task GetLocalizationInfoAsync_LocalizationEnabled_ListsTheLocaleFoldersOfTheLoadedTree()
    {
        // Arrange
        _options.EnableLocalization = true;
        _options.DefaultLocale = "en";
        _options.SupportedLocales = ["en", "de", "fr"];
        _repositoryClient.GetTreeAsync(Arg.Any<CancellationToken>()).Returns(new GitHubTree
        {
            Entries =
            [
                new GitHubTreeEntry { Path = "content/en/a.md", Type = "blob", Sha = "s-a" },
                new GitHubTreeEntry { Path = "content/fr/b.md", Type = "blob", Sha = "s-b" }
            ]
        });
        ServeFile("content/en/a.md", "s-a", "a-source", new FrontMatter { Title = "Alpha" });
        ServeFile("content/fr/b.md", "s-b", "b-source", new FrontMatter { Title = "Beta" });
        var provider = CreateProvider();

        // Act
        await provider.GetAllItemsAsync();
        var info = await provider.GetLocalizationInfoAsync();

        // Assert
        info.DefaultLocale.ShouldBe("en");
        info.AvailableLocales.Select(locale => locale.Code).ShouldBe(["en", "fr"]);
        info.AvailableLocales.Single(locale => locale.Code == "fr").NativeName.ShouldBe("français");
        await _repositoryClient.Received(1).GetTreeAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetLocalizationInfoAsync_LocalizationDisabled_ListsOnlyTheDefaultLocale()
    {
        // Arrange
        _options.EnableLocalization = false;
        _options.DefaultLocale = "en";
        var provider = CreateProvider();

        // Act
        var info = await provider.GetLocalizationInfoAsync();

        // Assert
        info.AvailableLocales.Select(locale => locale.Code).ShouldBe(["en"]);
        info.AvailableLocales.Single().IsDefault.ShouldBeTrue();
    }

    private GitHubContentProvider CreateProvider(string? providerName = null)
    {
        return new GitHubContentProvider(
            CreateContentRepository(),
            CreateDirectoryRepository(),
            Options.Create(_options),
            _providerClientFactory,
            _memoryCache,
            _logger,
            providerName);
    }

    private GitHubContentRepository CreateContentRepository()
    {
        return new GitHubContentRepository(
            CreateRepositoryClientFactory(),
            _markdown,
            _queryFilter,
            Options.Create(_options),
            Substitute.For<IDirectoryRepository>(),
            NullLogger<GitHubContentRepository>.Instance);
    }

    private GitHubDirectoryRepository CreateDirectoryRepository()
    {
        return new GitHubDirectoryRepository(
            CreateRepositoryClientFactory(),
            Options.Create(_options),
            _directoryCache,
            Substitute.For<IDirectoryMetadataProcessor>(),
            _pathUtils,
            NullLogger<GitHubDirectoryRepository>.Instance);
    }

    private IGitHubApiClientFactory CreateRepositoryClientFactory()
    {
        var factory = Substitute.For<IGitHubApiClientFactory>();
        factory.GetDefaultClient().Returns(_repositoryClient);
        return factory;
    }

    private void ServeFile(string path, string sha, string source, FrontMatter frontMatter)
    {
        _repositoryClient.GetFileContentAsync(path, Arg.Any<CancellationToken>()).Returns(new GitHubFileContent
        {
            Name = Path.GetFileName(path),
            Path = path,
            Sha = sha,
            Content = source
        });
        frontMatter.Id = "loc-1";
        _markdown.ExtractFrontMatterAndContent(source).Returns((frontMatter, $"{source} body"));
    }

    private static void ReturnBranches(IGitHubApiClient client, params GitHubBranch[] branches)
    {
        client.GetBranchesAsync(Arg.Any<CancellationToken>()).Returns(branches.ToList());
    }

    private static GitHubBranch Branch(string name, string sha) => new()
    {
        Name = name,
        Commit = new GitHubCommitRef { Sha = sha }
    };
}
