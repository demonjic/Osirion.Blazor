using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Osirion.Blazor.Cms.Domain.Exceptions;
using Osirion.Blazor.Cms.Domain.Interfaces;
using Osirion.Blazor.Cms.Domain.Interfaces.Content;
using Osirion.Blazor.Cms.Domain.Models.GitHub;
using Osirion.Blazor.Cms.Domain.Options;
using Osirion.Blazor.Cms.Domain.Repositories;
using Osirion.Blazor.Cms.Infrastructure.GitHub;
using Osirion.Blazor.Cms.Infrastructure.Localization;
using Osirion.Blazor.Cms.Infrastructure.Markdown;
using Osirion.Blazor.Core.Transliteration;
using Shouldly;

namespace Osirion.Blazor.Cms.Infrastructure.Tests.Localization;

/// <summary>
/// A GitHub repository with a derived locale derives through the same step as the FileSystem one, over paths that
/// start at the repository root.
/// </summary>
public class GitHubDerivedLocaleTests
{
    private readonly IGitHubApiClient _client = Substitute.For<IGitHubApiClient>();
    private readonly IGitHubApiClientFactory _factory = Substitute.For<IGitHubApiClientFactory>();
    private readonly List<GitHubTreeEntry> _tree = [];

    public GitHubDerivedLocaleTests()
    {
        _factory.GetDefaultClient().Returns(_client);
        _client.GetBranchesAsync(Arg.Any<CancellationToken>()).Returns([]);
        _client.GetTreeAsync(Arg.Any<CancellationToken>()).Returns(_ => new GitHubTree { Entries = [.. _tree] });
    }

    [Fact]
    public async Task GetAllAsync_CyrillicPage_GetsALatinItemUnderTheContentFolder()
    {
        Serve("content/sr/vesti/prva.md", "---\ntitle: Прва вест\ndate: 2026-01-02\n---\nТекст са [везом](/sr/kontakt).\n");
        var repository = CreateRepository(DerivedLocales());

        var items = await repository.GetAllAsync();

        var source = items.Single(item => item.Locale == "sr");
        var derived = items.Single(item => item.Locale == "sr-Latn");
        derived.IsDerived.ShouldBeTrue();
        derived.DerivedFromLocale.ShouldBe("sr");
        derived.Title.ShouldBe("Prva vest");
        derived.ContentId.ShouldBe(source.ContentId);
        derived.Path.ShouldBe("content/sr-Latn/vesti/prva.md");
        derived.Url.ShouldBe("vesti/prva");
        derived.Url.ShouldBe(source.Url);
        derived.ProviderSpecificId.ShouldBeNull();
        derived.Content.ShouldContain("Tekst sa <a href=\"/sr-Latn/kontakt\">vezom</a>.");
    }

    [Fact]
    public async Task GetAllAsync_FileInTheDerivedLocaleFolder_Wins()
    {
        Serve("content/sr/vesti/prva.md", "---\ntitle: Прва вест\ndate: 2026-01-02\n---\nТекст.\n");
        Serve("content/sr-latn/vesti/prva.md", "---\ntitle: Prva vest (ručno)\ndate: 2026-01-02\n---\nTekst.\n");
        var repository = CreateRepository(DerivedLocales());

        var items = await repository.GetAllAsync();

        items.Count.ShouldBe(2);
        items.ShouldNotContain(item => item.IsDerived);
    }

    [Fact]
    public async Task GetAllAsync_RefreshWithUnchangedFiles_DerivesAgainWithoutDownloading()
    {
        Serve("content/sr/vesti/prva.md", "---\ntitle: Прва вест\ndate: 2026-01-02\n---\nТекст.\n");
        var repository = CreateRepository(DerivedLocales());
        await repository.GetAllAsync();

        await repository.RefreshCacheAsync();

        (await repository.GetAllAsync()).Count(item => item.IsDerived).ShouldBe(1);
        await _client.Received(1).GetFileContentAsync("content/sr/vesti/prva.md", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveAsync_DerivedItem_IsRefusedWithoutAnyRequest()
    {
        Serve("content/sr/vesti/prva.md", "---\ntitle: Прва вест\ndate: 2026-01-02\n---\nТекст.\n");
        var repository = CreateRepository(DerivedLocales());
        var derived = (await repository.GetAllAsync()).Single(item => item.IsDerived);

        await Should.ThrowAsync<ContentProviderException>(() => repository.SaveAsync(derived));

        await _client.DidNotReceive().CreateOrUpdateFileAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Constructor_InvalidDerivedLocale_Throws()
    {
        var derivedLocales = DerivedLocales();
        derivedLocales[0].From = "de";

        Should.Throw<OptionsValidationException>(() => CreateRepository(derivedLocales))
            .Failures.ShouldBe(["DerivedLocales[0].From 'de' is not one of SupportedLocales (sr, en)."]);
    }

    private static List<DerivedLocaleOptions> DerivedLocales() =>
    [
        new DerivedLocaleOptions
        {
            Locale = "sr-Latn",
            From = "sr",
            Transliteration = SerbianCyrillicToLatinTransliterator.TransliteratorName,
        },
    ];

    private GitHubContentRepository CreateRepository(List<DerivedLocaleOptions> derivedLocales)
    {
        var options = new GitHubOptions
        {
            Owner = "o",
            Repository = "r",
            Branch = "main",
            ContentPath = "content",
            ProviderId = "github-test",
            SupportedExtensions = [".md"],
            EnableLocalization = true,
            DefaultLocale = "sr",
            SupportedLocales = ["sr", "en"],
            DerivedLocales = derivedLocales,
        };
        var processor = new MarkdownProcessor(new DefaultMarkdownRenderer(), new MarkdownSanitizer(), new HtmlToMarkdownConverter());
        var generator = new DerivedLocaleGenerator(
            new TextTransliterators([new SerbianCyrillicToLatinTransliterator()]),
            NullLogger<DerivedLocaleGenerator>.Instance);

        return new GitHubContentRepository(
            _factory, processor, Substitute.For<IContentQueryFilter>(), Options.Create(options), Substitute.For<IDirectoryRepository>(),
            NullLogger<GitHubContentRepository>.Instance, generator);
    }

    private void Serve(string path, string markdown)
    {
        var sha = "sha-" + path;
        _tree.Add(new GitHubTreeEntry { Path = path, Sha = sha, Type = "blob" });
        _client.GetFileContentAsync(path, Arg.Any<CancellationToken>()).Returns(new GitHubFileContent
        {
            Name = Path.GetFileName(path),
            Path = path,
            Sha = sha,
            Encoding = "base64",
            Content = Convert.ToBase64String(Encoding.UTF8.GetBytes(markdown)),
        });
    }
}
