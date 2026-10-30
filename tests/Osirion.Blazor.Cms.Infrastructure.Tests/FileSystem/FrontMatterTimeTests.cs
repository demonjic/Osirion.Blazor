using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Osirion.Blazor.Cms.Domain.Enums;
using Osirion.Blazor.Cms.Domain.Repositories;
using Osirion.Blazor.Cms.Infrastructure.DependencyInjection;
using Osirion.Blazor.Cms.Infrastructure.Providers;
using Shouldly;
using SortDirection = Osirion.Blazor.Cms.Domain.Enums.SortDirection;

namespace Osirion.Blazor.Cms.Infrastructure.Tests.FileSystem;

/// <summary>
/// Posts of one day ordered by the time their <c>date</c> names, read by a FileSystem provider registered the way a site
/// registers it.
/// </summary>
public sealed class FrontMatterTimeTests : IDisposable
{
    private readonly string _basePath = Path.Combine(Path.GetTempPath(), "osirion-times-" + Guid.NewGuid().ToString("N"));
    private ServiceProvider? _services;

    public FrontMatterTimeTests()
    {
        System.IO.Directory.CreateDirectory(_basePath);
    }

    public void Dispose()
    {
        _services?.Dispose();
        System.IO.Directory.Delete(_basePath, recursive: true);
    }

    [Fact]
    public async Task PostsOfOneDay_NewestFirst_FollowTheirTimes()
    {
        // Written in another order than the times, and named so the file order is not the time order either
        Write("blog/a-evening.md", "title: Evening\ndate: \"2026-10-29T20:00:00+01:00\"\n");
        Write("blog/b-morning.md", "title: Morning\ndate: \"2026-10-29T08:00:00+01:00\"\n");
        Write("blog/c-noon.md", "title: Noon\ndate: 2026-10-29T11:00:00Z\n");
        Write("blog/d-day-before.md", "title: Day before\ndate: 2026-10-28\n");

        var items = await CreateProvider().GetItemsByQueryAsync(new ContentQuery
        {
            Directory = "blog",
            SortBy = SortField.Date,
            SortDirection = SortDirection.Descending,
        });

        items.ShouldNotBeNull().Select(item => item.Title).ShouldBe(["Evening", "Noon", "Morning", "Day before"]);
        items[0].DateCreated.ShouldBe(new DateTime(2026, 10, 29, 19, 0, 0));
    }

    private FileSystemContentProvider CreateProvider()
    {
        var settings = new Dictionary<string, string?>
        {
            ["Osirion:Cms:Web:FileSystem:BasePath"] = _basePath,
            ["Osirion:Cms:Web:FileSystem:WatchForChanges"] = "false",
        };

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var services = new ServiceCollection();
        services.AddLogging();
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
}
