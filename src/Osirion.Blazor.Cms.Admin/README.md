# Osirion.Blazor.Cms.Admin

[![NuGet](https://img.shields.io/nuget/v/Osirion.Blazor.Cms.Admin)](https://www.nuget.org/packages/Osirion.Blazor.Cms.Admin)
[![License](https://img.shields.io/github/license/demonjic/Osirion.Blazor)](https://github.com/demonjic/Osirion.Blazor/blob/master/LICENSE.txt)

Content Management System (CMS) Administration module for Osirion.Blazor, providing a robust, flexible admin interface for managing content across different providers.

## Features

- **Unified Content Management**: Manage content from various providers (GitHub, FileSystem, etc.)
- **Rich Editing Experience**: Advanced markdown editor with live preview
- **SSR Compatible**: Works with Server-Side Rendering and Static SSG
- **Policy-Protected Admin**: Every admin page requires the `Osirion.Cms.Admin` authorization policy, which the host can replace (see [Securing the Admin](#securing-the-admin))
- **Multi-Provider Support**: Seamless integration with existing Osirion.Blazor CMS providers

## Installation

```bash
dotnet add package Osirion.Blazor.Cms.Admin
```

## Quick Start

### Configuration

```csharp
// In Program.cs
using Osirion.Blazor.Cms.Admin.DependencyInjection;

builder.Services.AddOsirionCmsAdmin(options => 
{
    options.UseGitHubProvider(github => 
    {
        github.Owner = "your-username";
        github.Repository = "your-content-repo";
    });
});
```

The admin pages also need the host's authentication; see [Securing the Admin](#securing-the-admin). They also need interactive server components (`AddInteractiveServerComponents`), because the GitHub credential stores use protected browser storage.

Settings under `Osirion:Cms:Admin` in configuration (for example the `Authentication` keys below) are bound by `AddOsirionCmsAdmin(IConfiguration)` and by `AddOsirion(configuration)`. The overload shown above, which takes a builder action, does not bind them.

### Basic Usage

The package ships routed admin pages. Once the package assembly is added to routing (see [Securing the Admin](#securing-the-admin)), they are served at:

| Route | Page |
|-------|------|
| `/osirion`, `/osirion/dashboard`, `/osirion/content` | Dashboard: repository and branch selection, file browser |
| `/osirion/content/edit` | Content editor |
| `/osirion/settings` | Settings |
| `/osirion/login` | GitHub sign-in |

## Core Components

### ContentEditor
- Markdown editing with live preview
- Front matter editing (basic, advanced, SEO and social metadata forms)
- Commit panel for saving to the repository
- Unpublished posts are saved as drafts

### Repository and file components
- `RepositorySelector` and `BranchSelector` choose the repository and branch to work on
- `FileExplorer` browses the files of the selected repository

### Layout and security components
- `CmsAdminDashboard` and `OsirionAdminLayout` frame the admin pages
- `AuthGuard` shows its content only to users who satisfy an authorization policy

## Sites without the admin

A site that gets this package through `Osirion.Blazor` or `Osirion.Blazor.Cms` but renders no admin page can keep its
scoped styles (about 72 KB) out of the app's `{AssemblyName}.styles.css`:

```xml
<PropertyGroup>
  <OsirionCmsAdminStyles>false</OsirionCmsAdminStyles>
</PropertyGroup>
```

Leave it unset on a site that serves the admin: its components take their styles only from that bundle.

## Securing the Admin

Every routed admin page (`/osirion`, `/osirion/dashboard`, `/osirion/content`, `/osirion/content/edit`, `/osirion/settings` and `/osirion/login`) requires the authorization policy named `CmsAdminAuthorization.PolicyName` (`"Osirion.Cms.Admin"`, namespace `Osirion.Blazor.Cms.Admin.Features.Security`). `AddOsirionCmsAdmin` registers authorization, the cascading authentication state and a default policy that requires an authenticated user, unless the host has already defined a policy with that name. The library never adds an authentication scheme: who may sign in, and how, is the host's decision.

The host must add an authentication scheme and run the authentication and authorization middleware after routing:

```csharp
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => options.LoginPath = "/account/login");

// Recommended on any site where people can register: only editors may use the admin
builder.Services.AddAuthorization(options =>
    options.AddPolicy(CmsAdminAuthorization.PolicyName, policy => policy.RequireRole("Editor")));

builder.Services.AddOsirion(builder.Configuration);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(Osirion.Blazor.Cms.Admin._Imports).Assembly);
```

- A host policy registered under `CmsAdminAuthorization.PolicyName` replaces the default, whether it is registered before or after `AddOsirionCmsAdmin`.
- Without an authentication scheme, or without `UseAuthorization()` after routing, requests to admin pages fail with an error instead of rendering.
- When the whole app is interactive, route with `AuthorizeRouteView` so that navigation inside a circuit is checked too. `OsirionAdminLayout` also hides page content from users who do not satisfy the policy.
- Authorization is evaluated when a circuit starts. To revoke access during a session, register a revalidating `AuthenticationStateProvider`.
- `AuthGuard` hides content inside a page; it does not protect an endpoint. Admin components you embed in your own pages (`ContentEditor`, `FileExplorer`, `RepositorySelector` and so on) are only as protected as those pages, so give them `@attribute [Authorize(Policy = CmsAdminAuthorization.PolicyName)]`.

## GitHub Credentials

The admin acts on GitHub with the first of these that is available:

1. The signed-in user's own token: a personal access token typed on `/osirion/login`, or a token from GitHub OAuth.
2. The server credential `Osirion:Cms:Admin:Authentication:PersonalAccessToken`. It is used only by admin services on the server, behind the admin policy. It never signs anyone in and never reaches the browser; the login page only shows that one is configured.

A content provider's `ApiToken` is never used by the admin.

A user's token is kept between page loads in the browser's local storage, encrypted with ASP.NET Core Data Protection under a purpose bound to the signed-in host user (the `ClaimTypes.NameIdentifier` claim, else the user name). A value that cannot be decrypted for the current user, for example after a key ring change, is treated as signed out. When the user has neither claim, the token lasts for the current session only. The login page restores the stored token when it is opened; other admin pages do not, so after a reload without a server credential the user opens `/osirion/login` once to sign in again. Signing out removes the stored token; the Sign Out button on `/osirion/settings` does so whether or not the admin layout is interactive. Where only the admin pages are interactive, the layout renders statically and does not show its own sign-out control. A token that earlier versions kept in plain text under `github_auth_token` is removed when the user next opens the login page or signs out. Run more than one instance only with a shared Data Protection key ring.

### GitHub OAuth

Set `GitHubClientId` and `GitHubClientSecret` under `Osirion:Cms:Admin:Authentication`, and register `https://your-site/osirion/login` as the OAuth app's callback URL (or set `GitHubRedirectUri` to the URL you registered). Keep the secrets out of `appsettings.json`:

```bash
dotnet user-secrets set "Osirion:Cms:Admin:Authentication:GitHubClientId" "<client id>"
dotnet user-secrets set "Osirion:Cms:Admin:Authentication:GitHubClientSecret" "<client secret>"
```

Each sign-in sends GitHub a single-use `state` value and a PKCE `code_challenge` (S256), both kept for that browser tab in encrypted session storage. The code is exchanged only when GitHub sends back the same `state`, and only after the login page becomes interactive. The requested scope is `repo`.

The `returnUrl` query value of the login page must be a local path such as `/osirion/content`; any other value is replaced by `/osirion`.

## Configuration Options

```csharp
builder.Services.AddOsirionCmsAdmin(options => 
{
    // Provider configuration
    options.UseGitHubProvider(github => { ... });
    options.UseFileSystemProvider(fs => { ... });

    // UI Customization
    options.ConfigureTheme(theme => 
    {
        theme.UseDarkMode = true;
        theme.PrimaryColor = "#007b31";
    });

    // Content Restrictions
    options.ConfigureContentRules(rules => 
    {
        rules.RequireApproval = true;
        rules.MaximumDraftAge = 30;
    });
});
```

## Localization

```csharp
builder.Services.AddOsirionCmsAdmin(options => 
{
    options.UseLocalization(localization => 
    {
        localization.AddSupportedCultures("en-US", "fr-FR", "de-DE");
        localization.SetDefaultCulture("en-US");
    });
});
```

## Security Best Practices

- Replace the default admin policy (any authenticated user) on every site where people can register.
- Use an authentication provider with multi-factor authentication for admin accounts.
- Keep `GitHubClientSecret` and `PersonalAccessToken` in user secrets or a key vault, never in `appsettings.json`.
- Prefer a fine-grained personal access token limited to the content repository.
- If you ran an earlier version with `PersonalAccessToken` configured, rotate it: earlier versions signed every visitor of `/osirion/login` in with it and stored tokens in plain-text browser storage. Ask users who entered their own tokens to rotate them too.

## Extensibility

Custom providers and components can be added easily:

```csharp
public class CustomContentProvider : IContentProvider 
{
    // Implement provider logic
}

builder.Services.AddOsirionCmsAdmin(options => 
{
    options.AddProvider<CustomContentProvider>();
});
```

## Documentation

For more detailed documentation, see [CMS Admin component documentation](https://github.com/demonjic/Osirion.Blazor/tree/master/docs/components/cms.admin).

## License

This project is licensed under the MIT License - see the [LICENSE](https://github.com/demonjic/Osirion.Blazor/blob/master/LICENSE.txt) file for details.

## Contributing

We welcome contributions! Please see our [contributing guidelines](https://github.com/demonjic/Osirion.Blazor/blob/master/CONTRIBUTING.md).