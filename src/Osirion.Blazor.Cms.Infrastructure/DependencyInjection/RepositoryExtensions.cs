using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Osirion.Blazor.Cms.Domain.Interfaces;
using Osirion.Blazor.Cms.Domain.Options;
using Osirion.Blazor.Cms.Infrastructure.FileSystem;
using Osirion.Blazor.Cms.Infrastructure.GitHub;
using Osirion.Blazor.Cms.Infrastructure.Services;

namespace Osirion.Blazor.Cms.Infrastructure.DependencyInjection;

/// <summary>
/// Extension methods for registering repositories
/// </summary>
public static class RepositoryExtensions
{
    /// <summary>
    /// Adds repository services to the service collection
    /// </summary>
    public static IServiceCollection AddOsirionRepositories(this IServiceCollection services)
    {
        // Register GitHub repositories
        services.TryAddDerivedLocales<GitHubOptions>();
        services.TryAddScoped<GitHubContentRepository>();
        services.TryAddScoped<GitHubDirectoryRepository>();

        // The repositories read and write through the abstraction, so a host gets the real file system unless it registers its own
        services.TryAddSingleton<System.IO.Abstractions.IFileSystem, System.IO.Abstractions.FileSystem>();

        // Register FileSystem repositories
        services.TryAddDerivedLocales<FileSystemOptions>();
        services.TryAddSingleton<FileSystemContentRepository>();
        services.TryAddFileSystemDirectoryCache();
        services.TryAddSingleton(DirectoryExtensions.CreateFileSystemDirectoryRepository);

        // Register other repositories and services
        services.AddHttpClient<IGitHubApiClient, GitHubApiClient>();
        services.TryAddScoped<IGitHubTokenProvider, GitHubTokenProvider>();
        services.AddScopedAuthenticationService();
        services.AddScopedGitHubAdminService();

        return services;
    }
}
