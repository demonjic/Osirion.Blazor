using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Enums;
using Osirion.Blazor.Cms.Domain.Repositories;
using Osirion.Blazor.Cms.Infrastructure.Caching;
using Shouldly;
using SortDirection = Osirion.Blazor.Cms.Domain.Enums.SortDirection;

namespace Osirion.Blazor.Cms.Infrastructure.Tests.Caching;

public sealed class StaleWhileRevalidateCacheDecoratorReadWriteTests : IDisposable
{
    private readonly IContentRepository _decorated = Substitute.For<IContentRepository>();
    private readonly MemoryCache _memoryCache = new(new MemoryCacheOptions());
    private readonly StaleWhileRevalidateCacheDecorator _decorator;

    public StaleWhileRevalidateCacheDecoratorReadWriteTests()
    {
        _decorator = new StaleWhileRevalidateCacheDecorator(
            _decorated,
            _memoryCache,
            NullLogger<StaleWhileRevalidateCacheDecorator>.Instance,
            TimeSpan.FromMinutes(5),
            TimeSpan.FromMinutes(30),
            "provider");
    }

    public void Dispose() => _memoryCache.Dispose();

    private static ContentItem Item(string id) => ContentItem.Create(id, "Title " + id, "Body", id + ".md", "provider");

    [Fact]
    public void Constructor_WithNullArguments_Throws()
    {
        var logger = NullLogger<StaleWhileRevalidateCacheDecorator>.Instance;
        var stale = TimeSpan.FromMinutes(1);
        var max = TimeSpan.FromMinutes(2);

        Should.Throw<ArgumentNullException>(() => new StaleWhileRevalidateCacheDecorator(null!, _memoryCache, logger, stale, max, "p"));
        Should.Throw<ArgumentNullException>(() => new StaleWhileRevalidateCacheDecorator(_decorated, null!, logger, stale, max, "p"));
        Should.Throw<ArgumentNullException>(() => new StaleWhileRevalidateCacheDecorator(_decorated, _memoryCache, null!, stale, max, "p"));
        Should.Throw<ArgumentNullException>(() => new StaleWhileRevalidateCacheDecorator(_decorated, _memoryCache, logger, stale, max, null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task SingleItemReads_WithBlankKey_Throw(string blank)
    {
        await Should.ThrowAsync<ArgumentException>(() => _decorator.GetByIdAsync(blank));
        await Should.ThrowAsync<ArgumentException>(() => _decorator.GetByPathAsync(blank));
        await Should.ThrowAsync<ArgumentException>(() => _decorator.GetByUrlAsync(blank));
    }

    [Fact]
    public async Task GetByPathAsync_CachesResult()
    {
        _decorated.GetByPathAsync("a.md", Arg.Any<CancellationToken>()).Returns(Item("a"));

        await _decorator.GetByPathAsync("a.md");
        var result = await _decorator.GetByPathAsync("a.md");

        result!.Id.ShouldBe("a");
        await _decorated.Received(1).GetByPathAsync("a.md", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByUrlAsync_CachesResult()
    {
        _decorated.GetByUrlAsync("/a", Arg.Any<CancellationToken>()).Returns(Item("a"));

        await _decorator.GetByUrlAsync("/a");
        var result = await _decorator.GetByUrlAsync("/a");

        result!.Id.ShouldBe("a");
        await _decorated.Received(1).GetByUrlAsync("/a", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByDirectoryAsync_CachesEachDirectorySeparately()
    {
        _decorated.GetByDirectoryAsync("d1", Arg.Any<CancellationToken>()).Returns([Item("a")]);
        _decorated.GetByDirectoryAsync("d2", Arg.Any<CancellationToken>()).Returns([Item("b")]);

        await _decorator.GetByDirectoryAsync("d1");
        var first = await _decorator.GetByDirectoryAsync("d1");
        var second = await _decorator.GetByDirectoryAsync("d2");

        first.Single().Id.ShouldBe("a");
        second.Single().Id.ShouldBe("b");
        await _decorated.Received(1).GetByDirectoryAsync("d1", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetTranslationsAsync_CachesResult()
    {
        _decorated.GetTranslationsAsync("a", Arg.Any<CancellationToken>()).Returns([Item("a-de")]);

        await _decorator.GetTranslationsAsync("a");
        var result = await _decorator.GetTranslationsAsync("a");

        result.Single().Id.ShouldBe("a-de");
        await _decorated.Received(1).GetTranslationsAsync("a", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetTagsAsync_CachesResult()
    {
        var tags = new List<ContentTag> { ContentTag.Create("dotnet", "dotnet") };
        _decorated.GetTagsAsync(Arg.Any<CancellationToken>()).Returns(tags);

        await _decorator.GetTagsAsync();
        var result = await _decorator.GetTagsAsync();

        result.ShouldBe(tags);
        await _decorated.Received(1).GetTagsAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FindByQueryAsync_WithEveryOptionalFilter_CachesTheQuery()
    {
        var query = new ContentQuery
        {
            IsFeatured = true,
            Status = ContentStatus.Published,
            DateFrom = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            DateTo = new DateTime(2024, 12, 31, 0, 0, 0, DateTimeKind.Utc),
            Skip = 5,
            Take = 10,
            SortBy = SortField.Title,
            SortDirection = SortDirection.Ascending,
            Tags = [" ", "b", "a", "a"],
            Categories = []
        };
        _decorated.FindByQueryAsync(query, Arg.Any<CancellationToken>()).Returns([Item("a")]);

        await _decorator.FindByQueryAsync(query);
        var result = await _decorator.FindByQueryAsync(query);

        result!.Single().Id.ShouldBe("a");
        await _decorated.Received(1).FindByQueryAsync(query, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FindByQueryAsync_QueriesDifferingOnlyInPaging_AreCachedSeparately()
    {
        _decorated.FindByQueryAsync(Arg.Is<ContentQuery>(q => q.Skip == 0), Arg.Any<CancellationToken>()).Returns([Item("first")]);
        _decorated.FindByQueryAsync(Arg.Is<ContentQuery>(q => q.Skip == 1), Arg.Any<CancellationToken>()).Returns([Item("second")]);

        var first = await _decorator.FindByQueryAsync(new ContentQuery { Skip = 0, Take = 1 });
        var second = await _decorator.FindByQueryAsync(new ContentQuery { Skip = 1, Take = 1 });

        first!.Single().Id.ShouldBe("first");
        second!.Single().Id.ShouldBe("second");
    }

    // Fails without the raw file flag in the key: the robots generator's query (raw files included) and a page query
    // for the same address shared one cache entry, so whichever ran first answered both.
    [Fact]
    public async Task FindByQueryAsync_QueriesDifferingOnlyInRawFiles_AreCachedSeparately()
    {
        _decorated.FindByQueryAsync(Arg.Is<ContentQuery>(q => !q.IncludeRawFiles), Arg.Any<CancellationToken>()).Returns([]);
        _decorated.FindByQueryAsync(Arg.Is<ContentQuery>(q => q.IncludeRawFiles), Arg.Any<CancellationToken>()).Returns([Item("robots")]);

        var pages = await _decorator.FindByQueryAsync(new ContentQuery { Url = "robots" });
        var rawFiles = await _decorator.FindByQueryAsync(new ContentQuery { Url = "robots", IncludeRawFiles = true });

        pages!.ShouldBeEmpty();
        rawFiles!.Single().Id.ShouldBe("robots");
    }

    [Fact]
    public async Task SaveWithCommitMessageAsync_SavesAndEvictsCachedReads()
    {
        var item = Item("a");
        _decorated.SaveWithCommitMessageAsync(item, "msg", Arg.Any<CancellationToken>()).Returns(item);
        _decorated.GetAllAsync(Arg.Any<CancellationToken>()).Returns([item]);
        await _decorator.GetAllAsync();

        var result = await _decorator.SaveWithCommitMessageAsync(item, "msg");
        await _decorator.GetAllAsync();

        result.ShouldBeSameAs(item);
        await _decorated.Received(1).RefreshCacheAsync(Arg.Any<CancellationToken>());
        await _decorated.Received(2).GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteWithCommitMessageAsync_DeletesAndEvictsCachedReads()
    {
        _decorated.GetAllAsync(Arg.Any<CancellationToken>()).Returns([Item("a")]);
        await _decorator.GetAllAsync();

        await _decorator.DeleteWithCommitMessageAsync("a", "msg");
        await _decorator.GetAllAsync();

        await _decorated.Received(1).DeleteWithCommitMessageAsync("a", "msg", Arg.Any<CancellationToken>());
        await _decorated.Received(1).RefreshCacheAsync(Arg.Any<CancellationToken>());
        await _decorated.Received(2).GetAllAsync(Arg.Any<CancellationToken>());
    }
}
