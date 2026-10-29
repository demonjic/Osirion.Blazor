using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Osirion.Blazor.Cms.Domain.Interfaces;
using Osirion.Blazor.Cms.Domain.Interfaces.Directory;
using Osirion.Blazor.Cms.Domain.Options;
using Osirion.Blazor.Cms.Domain.Repositories;
using Osirion.Blazor.Cms.Domain.ValueObjects;
using Osirion.Blazor.Cms.Infrastructure.Content;
using Osirion.Blazor.Cms.Infrastructure.FileSystem;
using Shouldly;
using System.IO.Abstractions.TestingHelpers;

namespace Osirion.Blazor.Cms.Infrastructure.Tests.FileSystem;

/// <summary>
/// A <c>robots.txt</c> next to the locale folders becomes a content item at the address <c>robots</c> when the provider's
/// <c>SupportedExtensions</c> include <c>.txt</c>, which the robots generator needs. It is a raw file, not a page, so
/// queries leave it out unless they ask for raw files.
/// </summary>
public sealed class FileSystemRawFileTests : IDisposable
{
    private static readonly string ContentRoot = MockUnixSupport.Path(@"c:\site\content");

    private readonly MockFileSystem _fileSystem = new();
    private readonly FileSystemContentRepository _repository;

    public FileSystemRawFileTests()
    {
        var markdownProcessor = Substitute.For<IMarkdownProcessor>();
        markdownProcessor.ExtractFrontMatterAndContent(Arg.Any<string>()).Returns(call => ((FrontMatter?)null, call.Arg<string>()));
        markdownProcessor.RenderToHtmlAsync(Arg.Any<string>(), Arg.Any<bool>()).Returns(call => call.Arg<string>());
        markdownProcessor.SanitizeHtml(Arg.Any<string>()).Returns(call => call.Arg<string>());
        var pathUtilities = Substitute.For<IPathUtilities>();
        pathUtilities.NormalizePath(Arg.Any<string>()).Returns(call => call.Arg<string>());

        _fileSystem.AddFile(_fileSystem.Path.Combine(ContentRoot, "robots.txt"), new MockFileData("User-agent: *\nAllow: /"));
        _fileSystem.AddFile(_fileSystem.Path.Combine(ContentRoot, "sitemap.xml"), new MockFileData("<urlset />"));
        _fileSystem.AddFile(_fileSystem.Path.Combine(ContentRoot, "en", "about.md"), new MockFileData("# About"));

        _repository = new FileSystemContentRepository(
            markdownProcessor,
            new ContentQueryFilter(pathUtilities),
            Options.Create(new FileSystemOptions
            {
                BasePath = ContentRoot,
                WatchForChanges = false,
                SupportedExtensions = [".md", ".txt"],
            }),
            NullLogger<FileSystemContentRepository>.Instance,
            _fileSystem);
    }

    public void Dispose() => _repository.Dispose();

    [Fact]
    public async Task GetAllAsync_TextFileInTheContentRoot_IsLoadedAsARawFileAtItsName()
    {
        var items = await _repository.GetAllAsync();

        var robots = items.Single(item => item.Path.EndsWith("robots.txt", StringComparison.Ordinal));
        robots.Url.ShouldBe("robots");
        robots.IsRawFile.ShouldBeTrue();
        items.ShouldNotContain(item => item.Path.EndsWith("sitemap.xml", StringComparison.Ordinal));
    }

    [Fact]
    public async Task FindByQueryAsync_PageQuery_LeavesTheRawFileOut()
    {
        var items = await _repository.FindByQueryAsync(new ContentQuery());

        items!.Count.ShouldBe(1);
        items[0].Path.ShouldEndWith("about.md");
    }

    [Fact]
    public async Task FindByQueryAsync_IncludeRawFiles_FindsTheRobotsFileByAddress()
    {
        var items = await _repository.FindByQueryAsync(new ContentQuery { Url = "robots", IncludeRawFiles = true });

        items!.Single().OriginalMarkdown.ShouldBe("User-agent: *\nAllow: /");
    }
}
