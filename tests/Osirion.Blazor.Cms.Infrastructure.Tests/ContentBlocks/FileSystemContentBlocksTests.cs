using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Osirion.Blazor.Cms.Domain.ContentBlocks;
using Osirion.Blazor.Cms.Domain.Exceptions;
using Osirion.Blazor.Cms.Infrastructure.DependencyInjection;
using Osirion.Blazor.Cms.Infrastructure.Providers;
using Shouldly;

namespace Osirion.Blazor.Cms.Infrastructure.Tests.ContentBlocks;

/// <summary>
/// A FileSystem provider registered the way a site registers it reads the <c>blocks</c> of a file onto its item, leaves
/// out a file whose blocks break the contract, and loads a file without blocks as before.
/// </summary>
public sealed class FileSystemContentBlocksTests : IDisposable
{
    private readonly string _basePath = Path.Combine(Path.GetTempPath(), "osirion-blocks-" + Guid.NewGuid().ToString("N"));
    private readonly RecordingLoggerProvider _logs = new();
    private ServiceProvider? _services;

    public FileSystemContentBlocksTests()
    {
        System.IO.Directory.CreateDirectory(_basePath);
    }

    public void Dispose()
    {
        _services?.Dispose();
        System.IO.Directory.Delete(_basePath, recursive: true);
    }

    [Fact]
    public async Task GetAllItemsAsync_FileWithBlocks_HasTypedBlocks()
    {
        Write("home.md", "title: Home\nbody: after\nblocks:\n  - { type: hero, title: Welcome, primary_action: { text: Start, url: /start } }\n  - { type: markdown, markdown: \"Hi\" }\n", "Body text.\n");

        var item = (await CreateProvider().GetAllItemsAsync()).ShouldHaveSingleItem();

        item.HasBlocks.ShouldBeTrue();
        item.BodyPlacement.ShouldBe(ContentBodyPlacement.AfterBlocks);
        item.Blocks[0].ShouldBeOfType<HeroBlock>().PrimaryAction!.Url.ShouldBe("/start");
        item.Blocks[1].ShouldBeOfType<MarkdownBlock>().Markdown.ShouldBe("Hi");
        item.Content.ShouldContain("Body text.");
    }

    // Guard test: a file without blocks loads exactly as it did before blocks existed.
    [Fact]
    public async Task GetAllItemsAsync_FileWithoutBlocks_HasNoBlocks()
    {
        Write("about.md", "title: About\n", "About us.\n");

        var item = (await CreateProvider().GetAllItemsAsync()).ShouldHaveSingleItem();

        item.HasBlocks.ShouldBeFalse();
        item.Blocks.ShouldBeEmpty();
        item.Metadata!.Blocks.ShouldBeNull();
        item.Content.ShouldBe("<p>About us.</p>\n");
    }

    [Fact]
    public async Task GetAllItemsAsync_FileWithInvalidBlocks_IsLeftOutAndTheProblemIsLogged()
    {
        Write("good.md", "title: Good\n", "Fine.\n");
        Write("bad.md", "title: Bad\nblocks:\n  - { type: cta, primary_action: { text: Go, url: \"javascript:alert(1)\" } }\n", "Body.\n");

        var items = await CreateProvider().GetAllItemsAsync();

        items.ShouldHaveSingleItem().Title.ShouldBe("Good");
        var logged = _logs.Exceptions.OfType<ContentBlockException>().Distinct().ShouldHaveSingleItem();
        logged.SourcePath.ShouldBe("bad.md");
        logged.Message.ShouldContain("blocks[0].primary_action.url");
    }

    private FileSystemContentProvider CreateProvider()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Osirion:Cms:Web:FileSystem:BasePath"] = _basePath,
            ["Osirion:Cms:Web:FileSystem:WatchForChanges"] = "false",
        }).Build();
        var services = new ServiceCollection();
        services.AddLogging(logging => logging.AddProvider(_logs));
        services.AddSingleton<IConfiguration>(configuration);
        services.AddCms(configuration);
        services.AddFileSystemContentProvider(configuration);

        _services = services.BuildServiceProvider();
        return _services.GetRequiredService<FileSystemContentProvider>();
    }

    private void Write(string relativePath, string frontMatter, string body)
    {
        var fullPath = Path.Combine(_basePath, relativePath);
        System.IO.Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, $"---\n{frontMatter}---\n\n{body}");
    }

    private sealed class RecordingLoggerProvider : ILoggerProvider, ILogger
    {
        private readonly List<Exception> _exceptions = [];

        public IReadOnlyList<Exception> Exceptions
        {
            get
            {
                lock (_exceptions)
                {
                    return [.. _exceptions];
                }
            }
        }

        public ILogger CreateLogger(string categoryName) => this;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (exception is not null)
            {
                lock (_exceptions)
                {
                    _exceptions.Add(exception);
                }
            }
        }

        public void Dispose()
        {
        }
    }
}
