using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using NSubstitute;
using Osirion.Blazor.Cms.Domain.Interfaces;
using Osirion.Blazor.Cms.Domain.Options.Configuration;
using Osirion.Blazor.Cms.Infrastructure.DependencyInjection;
using Osirion.Blazor.Cms.Infrastructure.GitHub;
using Osirion.Blazor.Cms.Infrastructure.Services;
using Osirion.Blazor.Cms.Infrastructure.Tests.TestDoubles;
using Shouldly;

namespace Osirion.Blazor.Cms.Infrastructure.Tests.DependencyInjection;

public class GitHubAdminServiceRegistrationTests
{
    private const string AdminToken = "admin-pat";
    private const string UserToken = "user-token";

    private readonly IConfiguration _configuration = GitHubTestServices.CreateConfiguration();

    [Fact]
    public void AddGitHubProvidersFromConfiguration_RegistersPerUserServicesAsScoped()
    {
        var services = CreateServices();

        services.AddGitHubProvidersFromConfiguration(_configuration);

        services.Single(descriptor => descriptor.ServiceType == typeof(IAuthenticationService))
            .Lifetime.ShouldBe(ServiceLifetime.Scoped);
        services.Single(descriptor => descriptor.ServiceType == typeof(IGitHubAdminService))
            .Lifetime.ShouldBe(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddGitHubProvidersFromConfiguration_ResolvesOneAdminServicePerScope()
    {
        // Proves each circuit or request gets its own admin service, and that it can be constructed at all
        // (the earlier transient typed-client registration could not build it).
        var services = CreateServices();
        services.AddGitHubProvidersFromConfiguration(_configuration);
        using var provider = services.BuildServiceProvider();
        using var firstScope = provider.CreateScope();
        using var secondScope = provider.CreateScope();

        var first = firstScope.ServiceProvider.GetRequiredService<IGitHubAdminService>();

        first.ShouldBeSameAs(firstScope.ServiceProvider.GetRequiredService<IGitHubAdminService>());
        first.ShouldNotBeSameAs(secondScope.ServiceProvider.GetRequiredService<IGitHubAdminService>());
    }

    [Fact]
    public void AddGitHubContentProvider_ResolvesOneAuthenticationServicePerScope()
    {
        // Proves every injection site in one circuit shares one authentication state, and that circuits
        // never share it.
        var services = CreateServices();
        services.AddGitHubContentProvider(_configuration);
        using var provider = services.BuildServiceProvider();
        using var firstScope = provider.CreateScope();
        using var secondScope = provider.CreateScope();

        var first = firstScope.ServiceProvider.GetRequiredService<IAuthenticationService>();

        first.ShouldBeSameAs(firstScope.ServiceProvider.GetRequiredService<IAuthenticationService>());
        first.ShouldNotBeSameAs(secondScope.ServiceProvider.GetRequiredService<IAuthenticationService>());
    }

    [Fact]
    public void RegistrationMethods_AfterTransientTypedClients_LeaveOnlyScopedRegistrations()
    {
        // Proves registration order cannot bring back a transient per-user service.
        var services = CreateServices();
        services.AddHttpClient<IAuthenticationService, AuthenticationService>();
        services.AddHttpClient<IGitHubAdminService, GitHubAdminService>();

        services.AddGitHubContentProvider(_configuration);
        services.AddGitHubProvidersFromConfiguration(_configuration);

        services.Where(descriptor => descriptor.ServiceType == typeof(IAuthenticationService))
            .ShouldHaveSingleItem().Lifetime.ShouldBe(ServiceLifetime.Scoped);
        services.Where(descriptor => descriptor.ServiceType == typeof(IGitHubAdminService))
            .ShouldHaveSingleItem().Lifetime.ShouldBe(ServiceLifetime.Scoped);
    }

    [Theory]
    [InlineData("GitHub_Blog")]
    [InlineData("IGitHubApiClient")]
    [InlineData("IGitHubTokenProvider")]
    [InlineData("IAuthenticationService")]
    public void GitHubHttpClient_PrimaryHandler_DoesNotUseCookies(string clientName)
    {
        // Proves a pooled handler shared by all users can no longer replay a cookie GitHub set for one
        // user on another user's request.
        var services = CreateServices();
        services.AddGitHubProvidersFromConfiguration(_configuration);
        using var provider = services.BuildServiceProvider();

        var handler = provider.GetRequiredService<IHttpMessageHandlerFactory>().CreateHandler(clientName);

        UsesCookies(GetPrimaryHandler(handler)).ShouldBeFalse();
    }

    [Fact]
    public void GitHubHttpClient_WithHostDefaultPrimaryHandlerAddedLater_DoesNotUseCookies()
    {
        // Proves a primary handler the host configures for all clients after the Osirion registration cannot
        // bring back the cookie container shared by every user.
        var services = CreateServices();
        services.AddGitHubProvidersFromConfiguration(_configuration);
        services.ConfigureHttpClientDefaults(builder => builder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()));
        using var provider = services.BuildServiceProvider();

        var handler = provider.GetRequiredService<IHttpMessageHandlerFactory>().CreateHandler("GitHub_Blog");

        UsesCookies(GetPrimaryHandler(handler)).ShouldBeFalse();
    }

    [Fact]
    public void GitHubHttpClient_WithHostNamedPrimaryHandlerAddedLater_DoesNotUseCookies()
    {
        var services = CreateServices();
        services.AddGitHubProvidersFromConfiguration(_configuration);
        services.AddHttpClient("GitHub_Blog").ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler());
        using var provider = services.BuildServiceProvider();

        var handler = provider.GetRequiredService<IHttpMessageHandlerFactory>().CreateHandler("GitHub_Blog");

        UsesCookies(GetPrimaryHandler(handler)).ShouldBeFalse();
    }

    // Fails before the fix: the primary handler followed a redirect itself and dropped the token on the way, so
    // the API client never saw the redirect and every later request went out unauthenticated.
    [Theory]
    [InlineData("GitHub_Blog", false)]
    [InlineData("IGitHubApiClient", false)]
    [InlineData("IGitHubTokenProvider", true)]
    [InlineData("IAuthenticationService", true)]
    [InlineData("Unrelated", true)]
    public void HttpClientPolicy_LeavesRedirectsOfApiClientsToTheClient(string clientName, bool followsRedirects)
    {
        var services = CreateServices();
        services.AddGitHubProvidersFromConfiguration(_configuration);
        using var provider = services.BuildServiceProvider();

        var handler = provider.GetRequiredService<IHttpMessageHandlerFactory>().CreateHandler(clientName);

        FollowsRedirects(GetPrimaryHandler(handler)).ShouldBe(followsRedirects);
    }

    [Fact]
    public void UnrelatedHttpClient_PrimaryHandler_KeepsCookies()
    {
        var services = CreateServices();
        services.AddGitHubProvidersFromConfiguration(_configuration);
        using var provider = services.BuildServiceProvider();

        var handler = provider.GetRequiredService<IHttpMessageHandlerFactory>().CreateHandler("Unrelated");

        UsesCookies(GetPrimaryHandler(handler)).ShouldBeTrue();
    }

    [Fact]
    public async Task AdminScopeChanges_DoNotChangePublicSiteRequests()
    {
        // Proves, through the real registrations, that a user's token, repository and branch set in an admin
        // scope no longer leak into the public site's GitHub reads.
        var handler = new RecordingHttpMessageHandler(_ => "[]");
        var services = CreateServices();
        services.AddCms(_configuration);
        services.AddGitHubProvidersFromConfiguration(_configuration);
        services.ConfigureHttpClientDefaults(builder => builder.ConfigurePrimaryHttpMessageHandler(() => handler));
        using var provider = services.BuildServiceProvider();
        var publicRepository = provider.GetRequiredKeyedService<GitHubContentRepository>("GitHubContentRepository_Blog");

        using (var adminScope = provider.CreateScope())
        {
            var adminService = adminScope.ServiceProvider.GetRequiredService<IGitHubAdminService>();
            await adminService.SetAuthTokenAsync("user-token");
            adminService.SetRepository("evil/x");
            adminService.SetBranch("feature");
        }

        handler.Clear();
        await publicRepository.RefreshCacheAsync();

        handler.Requests.ShouldNotBeEmpty();
        handler.Requests.ShouldAllBe(request =>
            request.Uri.AbsolutePath.StartsWith("/repos/o/r/")
            && request.AuthorizationParameter == GitHubTestServices.ServerToken);
        handler.Requests.Single(request => request.Uri.AbsolutePath.StartsWith("/repos/o/r/contents/"))
            .Uri.Query.ShouldBe("?ref=main");
    }

    [Fact]
    public async Task SignOut_ThroughResolvedAuthenticationService_DropsUsersTokenFromAdminServiceOfSameScope()
    {
        // Proves, through the real registrations, that the admin service of a scope follows the sign-out of the
        // authentication service of that scope, so the user's token does not outlive the session.
        var handler = new RecordingHttpMessageHandler();
        var services = CreateServices();
        services.Configure<CmsAdminOptions>(options => options.Authentication.PersonalAccessToken = AdminToken);
        services.AddGitHubProvidersFromConfiguration(_configuration);
        services.ConfigureHttpClientDefaults(builder => builder.ConfigurePrimaryHttpMessageHandler(() => handler));
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var adminService = scope.ServiceProvider.GetRequiredService<IGitHubAdminService>();
        var authentication = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();
        (await authentication.SetAccessTokenAsync(UserToken)).ShouldBeTrue();
        await adminService.SetAuthTokenAsync(UserToken); // what the login view model does through the repository adapter

        await authentication.SignOutAsync();
        handler.Clear();
        await adminService.GetFileContentAsync("x.md");

        handler.Requests.ShouldHaveSingleItem().AuthorizationParameter.ShouldBe(AdminToken);
    }

    [Fact]
    public void AdminService_WithBrowserStateStorage_ResolvesOutsideBlazorRender()
    {
        // Proves a background job or minimal API endpoint can still resolve the admin service (and through it the
        // authentication service and its browser state storage) before any NavigationManager is initialized.
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(_configuration);
        services.AddSingleton(Substitute.For<IMarkdownProcessor>());
        services.AddSingleton(Substitute.For<IJSRuntime>());
        services.AddScoped<NavigationManager, UninitializedNavigationManager>();
        services.AddScoped<IStateStorageService, LocalStorageService>();
        services.AddGitHubProvidersFromConfiguration(_configuration);
        using var provider = services.BuildServiceProvider();

        Should.NotThrow(() =>
        {
            using var scope = provider.CreateScope();
            scope.ServiceProvider.GetRequiredService<IGitHubAdminService>();
        });
    }

    private IServiceCollection CreateServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(_configuration);
        services.AddSingleton(Substitute.For<IStateStorageService>());
        services.AddSingleton(Substitute.For<IMarkdownProcessor>());
        return services;
    }

    private static HttpMessageHandler GetPrimaryHandler(HttpMessageHandler handler)
    {
        while (handler is DelegatingHandler delegatingHandler)
        {
            handler = delegatingHandler.InnerHandler!;
        }

        return handler;
    }

    private static bool UsesCookies(HttpMessageHandler handler)
    {
        return handler switch
        {
            HttpClientHandler httpClientHandler => httpClientHandler.UseCookies,
            SocketsHttpHandler socketsHandler => socketsHandler.UseCookies,
            _ => throw new InvalidOperationException($"Unexpected primary handler {handler.GetType()}")
        };
    }

    private static bool FollowsRedirects(HttpMessageHandler handler)
    {
        return handler switch
        {
            HttpClientHandler httpClientHandler => httpClientHandler.AllowAutoRedirect,
            SocketsHttpHandler socketsHandler => socketsHandler.AllowAutoRedirect,
            _ => throw new InvalidOperationException($"Unexpected primary handler {handler.GetType()}")
        };
    }

    /// <summary>
    /// The navigation manager of a scope that no Blazor render has initialized, as in a background job.
    /// </summary>
    private sealed class UninitializedNavigationManager : NavigationManager
    {
    }
}
