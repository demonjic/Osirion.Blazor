using Blazored.LocalStorage;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Osirion.Blazor.Cms.Domain.Interfaces;
using Osirion.Blazor.Cms.Domain.Interfaces.Content;
using Osirion.Blazor.Cms.Domain.Interfaces.Directory;
using Osirion.Blazor.Cms.Domain.Options;
using Osirion.Blazor.Cms.Domain.Services;
using Osirion.Blazor.Cms.Infrastructure.GitHub;
using Osirion.Blazor.Cms.Infrastructure.Localization;
using Osirion.Blazor.Cms.Infrastructure.Providers;
using Osirion.Blazor.Cms.Infrastructure.Services;
using Osirion.Blazor.Cms.Infrastructure.Directory;

namespace Osirion.Blazor.Cms.Infrastructure.DependencyInjection;

/// <summary>
/// Extension methods for registering GitHub providers with multi-provider support
/// </summary>
public static class GitHubProviderRegistrationExtensions
{
    /// <summary>
    /// Adds all GitHub providers from configuration
    /// </summary>
    public static IServiceCollection AddGitHubProvidersFromConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register the factory first
        services.TryAddSingleton<IGitHubApiClientFactory, GitHubApiClientFactory>();
        services.AddGitHubHttpClientCookiePolicy();

        // Register HTTP clients
        services.TryAddHttpClient<IGitHubTokenProvider, GitHubTokenProvider>();

        // Per-user services are scoped, so one user's GitHub state never reaches another user
        services.AddScopedAuthenticationService();
        services.AddScopedGitHubAdminService();

        // Load Web providers
        var webSection = configuration.GetSection("Osirion:Cms:Web:GitHub");
        if (webSection.Exists())
        {
            foreach (var providerSection in webSection.GetChildren())
            {
                var providerName = providerSection.Key;
                services.AddGitHubProvider(configuration, providerName, providerSection);
            }
        }

        // Load Admin providers
        var adminSection = configuration.GetSection("Osirion:Cms:Admin:GitHub");
        if (adminSection.Exists())
        {
            foreach (var providerSection in adminSection.GetChildren())
            {
                var providerName = providerSection.Key;

                // Skip if already registered from Web section
                if (!services.Any(s => s.ServiceType == typeof(IContentProvider) &&
                    s.ImplementationType == typeof(GitHubContentProvider) &&
                    s.ServiceKey?.ToString() == providerName))
                {
                    services.AddGitHubProvider(configuration, providerName, providerSection);
                }
            }
        }

        services.AddBlazoredLocalStorage();

        return services;
    }

    /// <summary>
    /// Adds a specific GitHub provider
    /// </summary>
    private static IServiceCollection AddGitHubProvider(
        this IServiceCollection services,
        IConfiguration configuration,
        string providerName,
        IConfigurationSection providerSection)
    {
        // Create provider-specific service keys
        var contentRepoKey = $"GitHubContentRepository_{providerName}";
        var directoryRepoKey = $"GitHubDirectoryRepository_{providerName}";
        var providerKey = $"GitHubContentProvider_{providerName}";

        services.TryAddDerivedLocales<GitHubOptions>();

        // Register repositories as keyed singletons for this specific provider
        services.AddKeyedSingleton<GitHubDirectoryRepository>(directoryRepoKey, (serviceProvider, key) =>
        {
            var apiClientFactory = serviceProvider.GetRequiredService<IGitHubApiClientFactory>();
            // Each provider gets its own directory cache: the shared default cache would answer this provider's
            // directory requests with whichever provider loaded first.
            var cacheManager = new DirectoryCacheManager(serviceProvider.GetRequiredService<ILogger<DirectoryCacheManager>>());
            var metadataProcessor = serviceProvider.GetRequiredService<IDirectoryMetadataProcessor>();
            var pathUtils = serviceProvider.GetRequiredService<IPathUtilities>();
            var logger = serviceProvider.GetRequiredService<ILogger<GitHubDirectoryRepository>>();

            // Create provider-specific options
            var options = new GitHubOptions();
            providerSection.Bind(options);
            var optionsWrapper = Options.Create(options);

            return new GitHubDirectoryRepository(
                apiClientFactory,
                optionsWrapper,
                cacheManager,
                metadataProcessor,
                pathUtils,
                logger,
                providerName);
        });

        services.AddKeyedSingleton(contentRepoKey, (serviceProvider, key) =>
        {
            var apiClientFactory = serviceProvider.GetRequiredService<IGitHubApiClientFactory>();
            var markdownProcessor = serviceProvider.GetRequiredService<IMarkdownProcessor>();
            var directoryRepo = serviceProvider.GetRequiredKeyedService<GitHubDirectoryRepository>(directoryRepoKey);
            var logger = serviceProvider.GetRequiredService<ILogger<GitHubContentRepository>>();
            var queryFilter = serviceProvider.GetRequiredService<IContentQueryFilter>();

            // Create provider-specific options
            var options = new GitHubOptions();
            providerSection.Bind(options);
            var optionsWrapper = Options.Create(options);

            return new GitHubContentRepository(
                apiClientFactory,
                markdownProcessor,
                queryFilter,
                optionsWrapper,
                directoryRepo,
                logger,
                serviceProvider.GetRequiredService<DerivedLocaleGenerator>(),
                providerName);
        });

        services.AddKeyedSingleton(providerKey, (serviceProvider, key) =>
        {
            var contentRepo = serviceProvider.GetRequiredKeyedService<GitHubContentRepository>(contentRepoKey);
            var directoryRepo = serviceProvider.GetRequiredKeyedService<GitHubDirectoryRepository>(directoryRepoKey);
            var apiClientFactory = serviceProvider.GetRequiredService<IGitHubApiClientFactory>();
            var memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
            var logger = serviceProvider.GetRequiredService<ILogger<GitHubContentProvider>>();

            // Create provider-specific options
            var options = new GitHubOptions();
            providerSection.Bind(options);
            var optionsWrapper = Options.Create(options);

            return new GitHubContentProvider(
                contentRepo,
                directoryRepo,
                optionsWrapper,
                apiClientFactory,
                memoryCache,
                logger,
                providerName);
        });

        // Register as IContentProvider
        services.AddSingleton<IContentProvider>(serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredKeyedService<GitHubContentProvider>(providerKey);
            var logger = serviceProvider.GetRequiredService<ILogger<GitHubContentProvider>>();

            // Create provider-specific options for logging
            var options = new GitHubOptions();
            providerSection.Bind(options);

            logger.LogInformation("Registered GitHub provider '{ProviderName}' with Owner='{Owner}', Repository='{Repository}'",
                providerName, options.Owner, options.Repository);

            return provider;
        });

        var registeredOptions = new GitHubOptions();
        providerSection.Bind(registeredOptions);

        // The polling service reads EnablePolling and PollingIntervalSeconds from here, not from IOptions<GitHubOptions>.
        services.AddSingleton(new GitHubPollingTarget(providerName, registeredOptions));

        // Register default setter if this provider is marked as default
        if (providerSection.GetValue<bool>("IsDefault"))
        {
            // The id the provider reports, not a second derivation: the registry matches that exact string.
            services.AddSingleton<IDefaultProviderSetter>(new DefaultProviderSetter(GitHubProviderId.For(registeredOptions), true));
        }

        return services;
    }
}
