using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Interfaces;
using Osirion.Blazor.Cms.Domain.Repositories;
using Osirion.Blazor.Cms.Infrastructure.DependencyInjection;
using Osirion.Blazor.Cms.Infrastructure.Directory;
using Osirion.Blazor.Cms.Infrastructure.Markdown;
using Osirion.Blazor.Cms.Infrastructure.Providers;
using Shouldly;

namespace Osirion.Blazor.Cms.Infrastructure.Tests.FileSystem;

/// <summary>
/// The other names items accept for <c>lang</c>, <c>id</c> and <c>permalink</c>, and a page at the site root, read by
/// a FileSystem provider registered the way a site registers it.
/// </summary>
public sealed class FrontMatterKeyAliasTests : IDisposable
{
    private readonly string _basePath = Path.Combine(Path.GetTempPath(), "osirion-aliases-" + Guid.NewGuid().ToString("N"));
    private readonly WarningRecorder _logs = new();
    private ServiceProvider? _services;

    public FrontMatterKeyAliasTests()
    {
        System.IO.Directory.CreateDirectory(_basePath);
    }

    public void Dispose()
    {
        _services?.Dispose();
        System.IO.Directory.Delete(_basePath, recursive: true);
    }

    [Theory]
    [InlineData("locale")]
    [InlineData("language")]
    public async Task OtherNameOfLang_SetsTheLanguage(string key)
    {
        Write("en/platform.md", $"title: Platform\n{key}: sr\n");

        var item = await SingleItemAsync(localized: true);

        item.Locale.ShouldBe("sr");
        item.Metadata!.Lang.ShouldBe("sr");
        _logs.Messages.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("localization_id")]
    [InlineData("content_id")]
    public async Task OtherNameOfId_SetsTheLocalizationId(string key)
    {
        Write("en/platform.md", $"title: Platform\n{key}: page-platform\n");

        var item = await SingleItemAsync(localized: true);

        item.ContentId.ShouldBe("page-platform");
        item.Metadata!.Id.ShouldBe("page-platform");
    }

    [Theory]
    [InlineData("platform")]
    [InlineData("/platform")]
    [InlineData("/platform/")]
    public async Task Url_SetsTheAddressLikePermalink(string url)
    {
        Write("pages/about-the-platform.md", $"title: Platform\nurl: \"{url}\"\n");

        var item = await SingleItemAsync(localized: false);

        item.Url.ShouldBe("platform");
        item.Metadata!.Permalink.ShouldBe(url);
        item.Metadata.Url.ShouldBeNull();
    }

    [Theory]
    [InlineData("lang: en\nlocale: sr\n", "lang", "locale")]
    [InlineData("lang: en\nlanguage: sr\n", "lang", "language")]
    [InlineData("locale: en\nlanguage: sr\n", "locale", "language")]
    [InlineData("id: page-a\nlocalization_id: page-b\n", "id", "localization_id")]
    [InlineData("id: page-a\ncontent_id: page-b\n", "id", "content_id")]
    [InlineData("localization_id: page-a\ncontent_id: page-b\n", "localization_id", "content_id")]
    [InlineData("permalink: /a\nurl: /b\n", "permalink", "url")]
    public async Task Conflict_TheCanonicalOrFirstNameWinsAndTheConflictIsLogged(string keys, string usedKey, string ignoredKey)
    {
        Write("en/page.md", $"title: Page\n{keys}");

        var item = await SingleItemAsync(localized: true);

        var message = _logs.Messages.ShouldHaveSingleItem();
        message.ShouldContain("en/page.md");
        message.ShouldContain($"sets {usedKey} to");
        message.ShouldContain($"and {ignoredKey} to");
        if (usedKey is "lang" or "locale")
        {
            item.Locale.ShouldBe("en");
        }
        else if (usedKey is "id" or "localization_id")
        {
            item.ContentId.ShouldBe("page-a");
        }
        else
        {
            item.Url.ShouldBe("a");
        }
    }

    [Theory]
    [InlineData("lang: sr\nlocale: SR\n")]
    [InlineData("permalink: /a\nurl: a/\n")]
    [InlineData("id: page-a\nlocalization_id: page-a\n")]
    public async Task SameValueUnderTwoNames_IsNoConflict(string keys)
    {
        Write("en/page.md", $"title: Page\n{keys}");

        await SingleItemAsync(localized: true);

        _logs.Messages.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("permalink: \"/\"\n")]
    [InlineData("url: \"\"\n")]
    [InlineData("url: \"/\"\n")]
    public async Task RootAddress_WithoutLocalization_IsTheSiteRootAndFoundByASlash(string keys)
    {
        Write("home.md", $"title: Home\n{keys}");
        Write("about.md", "title: About\n");
        var provider = CreateProvider(localized: false);

        var home = (await provider.GetAllItemsAsync()).Single(item => item.Title == "Home");

        home.Url.ShouldBe(string.Empty);
        (await provider.GetItemByUrlAsync("/"))!.Title.ShouldBe("Home");
        (await provider.GetItemsByQueryAsync(new ContentQuery { Url = "/" }))!.ShouldHaveSingleItem().Title.ShouldBe("Home");
        (await provider.GetItemByUrlAsync("/about"))!.Title.ShouldBe("About");
    }

    [Fact]
    public async Task RootAddress_WithLocalization_EachCultureHasItsOwnRootPage()
    {
        Write("en/home.md", "title: Home\npermalink: \"/\"\nid: home\n");
        Write("sr/pocetna.md", "title: Pocetna\nurl: \"\"\nlocalization_id: home\n");
        var provider = CreateProvider(localized: true);

        var english = (await provider.GetItemsByQueryAsync(new ContentQuery { Url = "/", Locale = "en" }))!.ShouldHaveSingleItem();
        var serbian = (await provider.GetItemsByQueryAsync(new ContentQuery { Url = "/", Locale = "sr" }))!.ShouldHaveSingleItem();

        english.Title.ShouldBe("Home");
        serbian.Title.ShouldBe("Pocetna");
        (english.Url, serbian.Url).ShouldBe((string.Empty, string.Empty));
        serbian.ContentId.ShouldBe(english.ContentId);
    }

    [Fact]
    public async Task PermalinkLeftEmpty_KeepsThePathAddress()
    {
        Write("pages/team.md", "title: Team\npermalink: \"\"\n");

        var item = await SingleItemAsync(localized: false);

        item.Url.ShouldBe("pages/team");
    }

    // Directory metadata writes locale and url, and reads them back the same way items do.
    [Fact]
    public void DirectoryMetadata_WrittenAndRead_KeepsLocaleAndUrl()
    {
        var processor = new DirectoryMetadataProcessor(
            Substitute.For<IFrontMatterExtractor>(),
            new MarkdownProcessor(new DefaultMarkdownRenderer(), Substitute.For<IMarkdownSanitizer>(), Substitute.For<IHtmlToMarkdownConverter>()),
            NullLogger<DirectoryMetadataProcessor>.Instance);
        var written = DirectoryItem.Create("guides", "sr/guides", "Vodici", "test");
        written.SetLocale("sr");
        written.SetUrl("vodici");

        var read = processor.ProcessMetadata(DirectoryItem.Create("guides", "sr/guides", "guides", "test"), processor.GenerateMetadataContent(written));

        read.Locale.ShouldBe("sr");
        read.Url.ShouldBe("vodici");
        read.Name.ShouldBe("Vodici");
    }

    private async Task<ContentItem> SingleItemAsync(bool localized) =>
        (await CreateProvider(localized).GetAllItemsAsync()).ShouldHaveSingleItem();

    private FileSystemContentProvider CreateProvider(bool localized)
    {
        var settings = new Dictionary<string, string?>
        {
            ["Osirion:Cms:Web:FileSystem:BasePath"] = _basePath,
            ["Osirion:Cms:Web:FileSystem:WatchForChanges"] = "false",
        };
        if (localized)
        {
            settings["Osirion:Cms:Web:FileSystem:EnableLocalization"] = "true";
            settings["Osirion:Cms:Web:FileSystem:DefaultLocale"] = "en";
            settings["Osirion:Cms:Web:FileSystem:SupportedLocales:0"] = "en";
            settings["Osirion:Cms:Web:FileSystem:SupportedLocales:1"] = "sr";
        }

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var services = new ServiceCollection();
        services.AddLogging(logging => logging.AddProvider(_logs));
        services.AddSingleton<IConfiguration>(configuration);
        services.AddCms(configuration);
        services.AddFileSystemContentProvider(configuration);

        _services = services.BuildServiceProvider();
        return _services.GetRequiredService<FileSystemContentProvider>();
    }

    private void Write(string relativePath, string frontMatter)
    {
        var fullPath = Path.Combine(_basePath, relativePath);
        System.IO.Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, $"---\n{frontMatter}---\n\nBody.\n");
    }

    // Keeps the warnings written about front matter keys.
    private sealed class WarningRecorder : ILoggerProvider, ILogger
    {
        private readonly List<string> _messages = [];

        public IReadOnlyList<string> Messages
        {
            get
            {
                lock (_messages)
                {
                    return _messages.Where(message => message.Contains("front matter", StringComparison.Ordinal)).Distinct().ToList();
                }
            }
        }

        public ILogger CreateLogger(string categoryName) => this;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (logLevel == LogLevel.Warning)
            {
                lock (_messages)
                {
                    _messages.Add(formatter(state, exception));
                }
            }
        }

        public void Dispose()
        {
        }
    }
}
