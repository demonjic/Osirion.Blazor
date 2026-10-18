using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Interfaces;
using Osirion.Blazor.Cms.Domain.Interfaces.Content;
using Osirion.Blazor.Cms.Domain.Interfaces.Directory;
using Osirion.Blazor.Cms.Domain.Options;
using Osirion.Blazor.Cms.Domain.Repositories;
using Osirion.Blazor.Cms.Domain.ValueObjects;
using Osirion.Blazor.Cms.Infrastructure.FileSystem;
using Osirion.Blazor.Cms.Infrastructure.Providers;
using Shouldly;

namespace Osirion.Blazor.Cms.Infrastructure.Tests.Providers;

/// <summary>
/// Runs <see cref="FileSystemContentProvider.GetLocalizationInfoAsync"/> over a real temporary folder of locale
/// subfolders, the layout a localized site keeps its Markdown in.
/// </summary>
public sealed class FileSystemContentProviderLocalesTests : IDisposable
{
    private readonly string _basePath = Path.Combine(Path.GetTempPath(), "osirion-locales-" + Guid.NewGuid().ToString("N"));
    private readonly MemoryCache _memoryCache = new(new MemoryCacheOptions());
    private readonly List<IDisposable> _repositories = [];

    public FileSystemContentProviderLocalesTests()
    {
        System.IO.Directory.CreateDirectory(_basePath);
    }

    public void Dispose()
    {
        foreach (var repository in _repositories)
        {
            repository.Dispose();
        }

        _memoryCache.Dispose();
        System.IO.Directory.Delete(_basePath, recursive: true);
    }

    // Fails before the change: the provider had no API that reports its locales.
    [Fact]
    public async Task GetLocalizationInfoAsync_LocalizationEnabled_ListsTheConfiguredLocalesThatHaveContent()
    {
        WriteContent("en/intro.md", "Intro");
        WriteContent("sr/intro.md", "Uvod");
        System.IO.Directory.CreateDirectory(Path.Combine(_basePath, "de"));
        WriteContent("fr/draft.md", "published: false");
        var provider = CreateProvider(enableLocalization: true, defaultLocale: "en", supportedLocales: ["en", "de", "fr", "sr"]);

        var info = await provider.GetLocalizationInfoAsync();

        info.DefaultLocale.ShouldBe("en");
        info.AvailableLocales.Select(locale => locale.Code).ShouldBe(["en", "sr"]);
        info.AvailableLocales.Single(locale => locale.IsDefault).Code.ShouldBe("en");
    }

    [Fact]
    public async Task GetLocalizationInfoAsync_DefaultLocaleWithoutContent_StillListsItFirst()
    {
        WriteContent("fr/intro.md", "Intro");
        WriteContent("en/intro.md", "Intro");
        var provider = CreateProvider(enableLocalization: true, defaultLocale: "sr", supportedLocales: ["en", "fr", "sr"]);

        var info = await provider.GetLocalizationInfoAsync();

        info.DefaultLocale.ShouldBe("sr");
        info.AvailableLocales.Select(locale => locale.Code).ShouldBe(["sr", "en", "fr"]);
    }

    [Fact]
    public async Task GetLocalizationInfoAsync_LocalizationDisabled_ListsOnlyTheDefaultLocale()
    {
        WriteContent("en/intro.md", "Intro");
        WriteContent("sr/intro.md", "Uvod");
        var provider = CreateProvider(enableLocalization: false, defaultLocale: "en", supportedLocales: ["en", "sr"]);

        var info = await provider.GetLocalizationInfoAsync();

        info.AvailableLocales.Select(locale => locale.Code).ShouldBe(["en"]);
    }

    [Fact]
    public async Task GetLocalizationInfoAsync_SupportedLocalesConfiguredEmpty_ListsTheLocaleFoldersFound()
    {
        WriteContent("sr/intro.md", "Uvod");
        WriteContent("de/intro.md", "Einleitung");
        WriteContent("en/intro.md", "Intro");
        var provider = CreateProvider(enableLocalization: true, defaultLocale: "en", supportedLocales: []);

        var info = await provider.GetLocalizationInfoAsync();

        info.AvailableLocales.Select(locale => locale.Code).ShouldBe(["en", "de", "sr"]);
    }

    [Fact]
    public async Task GetLocalizationInfoAsync_NamesEachLocaleByItsCultureAndFallsBackToTheCode()
    {
        WriteContent("de/intro.md", "Einleitung");
        WriteContent("not_a_culture/intro.md", "Intro");
        var provider = CreateProvider(enableLocalization: true, defaultLocale: "de", supportedLocales: ["de", "not_a_culture"]);

        var info = await provider.GetLocalizationInfoAsync();

        var german = info.AvailableLocales.Single(locale => locale.Code == "de");
        german.Name.ShouldBe("German");
        german.NativeName.ShouldBe("Deutsch");
        var unknown = info.AvailableLocales.Single(locale => locale.Code == "not_a_culture");
        unknown.Name.ShouldBe("not_a_culture");
        unknown.NativeName.ShouldBe("not_a_culture");
    }

    private FileSystemContentProvider CreateProvider(bool enableLocalization, string defaultLocale, List<string> supportedLocales)
    {
        var options = Options.Create(new FileSystemOptions
        {
            BasePath = _basePath,
            WatchForChanges = false,
            EnableLocalization = enableLocalization,
            DefaultLocale = defaultLocale,
            SupportedLocales = supportedLocales,
        });

        var markdownProcessor = Substitute.For<IMarkdownProcessor>();
        markdownProcessor.ExtractFrontMatterAndContent(Arg.Any<string>())
            .Returns(call => (ReadFrontMatter(call.Arg<string>()), call.Arg<string>()));
        markdownProcessor.RenderToHtmlAsync(Arg.Any<string>(), Arg.Any<bool>())
            .Returns(call => call.Arg<string>());

        var queryFilter = Substitute.For<IContentQueryFilter>();
        queryFilter.ApplyFilters(Arg.Any<IQueryable<ContentItem>>(), Arg.Any<ContentQuery>())
            .Returns(call => call.Arg<IQueryable<ContentItem>>());

        var directoryCache = Substitute.For<IDirectoryCacheManager>();
        directoryCache.GetCachedDirectoriesAsync(
                Arg.Any<Func<CancellationToken, Task<Dictionary<string, DirectoryItem>>>>(),
                Arg.Any<CancellationToken>(),
                Arg.Any<bool>())
            .Returns(_ => new Dictionary<string, DirectoryItem>());

        var contentRepository = new FileSystemContentRepository(
            markdownProcessor,
            queryFilter,
            options,
            NullLogger<FileSystemContentRepository>.Instance,
            new System.IO.Abstractions.FileSystem());
        var directoryRepository = new FileSystemDirectoryRepository(
            options,
            directoryCache,
            Substitute.For<IDirectoryMetadataProcessor>(),
            Substitute.For<IPathUtilities>(),
            NullLogger<FileSystemDirectoryRepository>.Instance);
        _repositories.Add(contentRepository);
        _repositories.Add(directoryRepository);

        return new FileSystemContentProvider(
            contentRepository,
            directoryRepository,
            options,
            _memoryCache,
            NullLogger<FileSystemContentProvider>.Instance);
    }

    private void WriteContent(string relativePath, string body)
    {
        var fullPath = Path.Combine(_basePath, relativePath);
        System.IO.Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, body);
    }

    /// <summary>The substituted markdown processor reads a <c>published: false</c> line as front matter.</summary>
    private static FrontMatter ReadFrontMatter(string markdown) => new()
    {
        Title = "Title",
        Published = !markdown.Contains("published: false", StringComparison.Ordinal),
    };
}
