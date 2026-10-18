---
id: 'guide-migration'
order: 12
layout: docs
title: "Migration"
permalink: /docs/migration
description: "Upgrade Osirion.Blazor from 3.x to 4.0 and from 4.0 to 4.1: what changed, who must act and what to change in your code."
author: Dejan Demonjić
date: 2026-10-17
last_modified: 2026-10-17
featured_image: /assets/images/blazor-cover.png
categories:
- Documentation
- Guides
tags:
- migration
- upgrade
- breaking-changes
- guides
published: true
slug: migration
lang: en
custom_fields:
  group: guides
seo_properties:
  title: 'Migrate Osirion.Blazor from 3.x to 4.x'
  description: 'Upgrade steps from Osirion.Blazor 3.x to 4.0 and from 4.0 to 4.1, built from the changelog: breaking changes, removed members and the code to change.'
  image: /assets/images/blazor-cover.png
  canonical: 'https://getosirion.com/docs/migration'
  robots: index, follow
---

# Migration

This guide takes a site from Osirion.Blazor 3.x to 4.0, and from 4.0 to 4.1. It lists the changes that can break a
build or change what a site does, with the code to change. It is for developers who run an existing site; a new site
needs none of it.

Every item comes from `CHANGELOG.md` and was checked against the code at the tags `v3.0.1`, `v4.0.0` and `v4.1.0`.
The full list of 4.0 changes, with more detail on the admin and on custom implementations, is in
[`docs/MIGRATION.md`](https://github.com/demonjic/Osirion.Blazor/blob/master/docs/MIGRATION.md) in the repository.

## Before you start

- A clean working tree and a build that passes on your current version.
- Both 3.0.1 and 4.x target `net9.0` and `net10.0`, so the upgrade does not change your target framework.
- Read the steps for the features you use and skip the rest. Steps are marked with who must act.

## Part 1. From 3.x to 4.0

### Update the packages

```bash
dotnet add package Osirion.Blazor --version 4.1.0
```

Version 4.1.0 is the current release. If you reference single modules instead of `Osirion.Blazor`, update each one to
the same version.

### Decide which content is a draft (every site)

A content file without a `published` key in its front matter is now published. In 3.x it was hidden as a draft.
Add the key to every file that must stay hidden:

```yaml
---
title: "Not ready yet"
published: false
---
```

### Review HTML sanitizing (every site that renders content)

Rendered Markdown and ingested content are sanitized with an allow-list. `OsirionHtmlRenderer.SanitizeHtml` now defaults
to `true` (it was `false`), which also covers HTML you pass yourself, for example the custom content of a home page.
Script, event handler attributes, `javascript:`, `vbscript:` and `data:` URLs, forms, iframes, inline styles,
`<style>`, `svg` and the `name` attribute are removed. `class`, `id`, `role`, `aria-*` and table cell `text-align` stay.

If content that used to render now looks different, extend the allow-list instead of turning sanitizing off:

```csharp
using Osirion.Blazor.Core.Extensions;

builder.Services.AddOsirionHtmlSanitizer(options =>
    options.AllowElement("chart", "type", "title", "caption", "colors"));
```

Set `SanitizeHtml="false"` only for markup your own code builds. Read [Security](/docs/security) first.

### Add antiforgery (sites that use cookie consent)

`MapOsirionCookieConsent` requires an antiforgery token, which the banner form carries. Call
`UseAntiforgery` after `UseAuthentication` and `UseAuthorization`, and before the endpoints. The Blazor Web App template
already does. Razor Pages and MVC hosts that render the banner must add it:

```csharp
app.UseAntiforgery();
app.MapOsirionCookieConsent();
```

Custom forms that post to the consent endpoint must include `<AntiforgeryToken />` and post a local path as
`returnUrl`; any other value redirects to `/`. `UseOsirionCookieConsent` no longer writes a `Permissions-Policy`
header: set one yourself if you relied on it.

### Set the webhook secret (sites that call `UseGitHubWebhook`)

The webhook verifies `X-Hub-Signature-256` against the provider's `WebhookSecret` and rejects every delivery while no
provider has one. Set the secret and use the same value in the webhook settings on GitHub, with content type
`application/json`:

```bash
dotnet user-secrets set "Osirion:Cms:Web:GitHub:Site:WebhookSecret" "<long random value>"
```

A verified delivery that matches no provider now answers 202 instead of 400, and tags and deleted branches no longer
refresh content.

### Add authentication to the admin (sites that map the admin pages)

Every routed admin page requires the authorization policy `CmsAdminAuthorization.PolicyName` (`"Osirion.Cms.Admin"`).
The default policy lets in any authenticated user. The library adds no authentication scheme, so add one, and replace
the policy where anyone can register:

```csharp
using Microsoft.AspNetCore.Authentication.Cookies;
using Osirion.Blazor.Cms.Admin.Features.Security;

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => options.LoginPath = "/account/login");

builder.Services.AddAuthorization(options =>
    options.AddPolicy(CmsAdminAuthorization.PolicyName, policy => policy.RequireRole("Editor")));

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
```

The configured `Osirion:Cms:Admin:Authentication:PersonalAccessToken` no longer signs in every visitor of
`/osirion/login`. Rotate it: earlier versions applied it to every visitor and wrote it to their browser storage. Admin
services never use a content provider's `ApiToken`. GitHub sign-in with OAuth now redirects to `{BaseUri}osirion/login`
(it was `/osirion/auth/callback`, which has no route): register `https://<your host>/osirion/login` as the callback of
your OAuth app, or set `Osirion:Cms:Admin:Authentication:GitHubRedirectUri` to the address you registered. A site that never maps the admin needs none of
this.

### Move the FileSystem section

The local folder provider reads `Osirion:Cms:Web:FileSystem` (`FileSystemOptions.WebSection`). The old section
`Osirion:Cms:FileSystem:Web` is still read when the new one is missing, so nothing breaks, but change it:

```json
{
  "Osirion": {
    "Cms": {
      "Web": {
        "FileSystem": {
          "BasePath": "content",
          "IsDefault": true
        }
      }
    }
  }
}
```

The provider marked `IsDefault` is now the default even when you do not call `UseOsirionCms`; before, the first
registered provider won. A folder's `_index.md` is served at the folder's URL, as the GitHub provider already did:
remove `_index.md` files you do not want published.

### Fix code that breaks the build or the startup

| If your code does this | Change it to |
| --- | --- |
| Reads `IsServerSide` in a component (now `[Obsolete]`, an error where warnings are errors) | `IsInteractive`, or `IsRunningInBrowser` for WebAssembly |
| Injects `IThemeService` or `ScrollToTopManager` into a singleton | Both are scoped now. Inject them into components or scoped services; a singleton that takes either fails at startup where scope validation is on |
| Calls `INavigationService.ScrollToTopAsync` or `ScrollToElementAsync` (now `[Obsolete]`; they only logged) | Remove the call |
| Sets `OsirionHtmlRenderer.LanguageClasses` (obsolete, never read) | Style code blocks with CSS on the `language-*` classes |
| Relies on `ScrollToTopOptions.Enabled = false` leaving the button visible (it was always shown) | Remove the setting, or set `Enabled` to `true` |
| Sets `ScrollToTop.CustomIcon` to inline SVG (now sanitized, SVG removed) | Use `IconCssClass` |
| Finds `theme-toggle` or `scrollPositionInput` ids in markup or script | Give `ThemeToggle` an `Id` |
| Calls `IMarkdownProcessor.RenderToHtml` expecting unsanitized HTML | It defaults `sanitizeHtml` to `true`; pass `false` only for markup you produced |
| Implements `IGitHubTokenProvider` | Implement the new `ExchangeCodeForTokenAsync(code, clientId, clientSecret, redirectUri, codeVerifier, cancellationToken)` |
| Uses `AuthGuard.ShowLoginForm`, `AuthGuard.OnAuthResult` or `AdminPage.ShowLoginForm` | Remove them; they do nothing and are `[Obsolete]` |
| Calls `LoginViewModel.LoginWithGitHubAsync(code)` or `IAuthenticationService.AuthenticateWithGitHubAsync(code)` (both `[Obsolete]`) | The first always fails: use `StartGitHubLoginAsync()` and `LoginWithGitHubAsync(code, state)`. The second still works but skips the OAuth `state` check and PKCE: check the state, exchange the code with `IGitHubTokenProvider`, then call `SetAccessTokenAsync` |
| Creates `LoginViewModel` with `new` | The constructor also takes `IGitHubTokenProvider`, `IGitHubTokenStore` and `IGitHubOAuthStateStore`. Resolution through DI is unaffected |
| Keeps one `IGitHubApiClientFactory.GetClient` or `GetDefaultClient` result and expects other callers to see its changes | Each call now returns a new client the caller owns: configure the client you use |
| Registers a custom `IAuthenticationService` or `IGitHubAdminService`, or resolves the admin event buses (`IEventPublisher`, `IEventSubscriber`, `CmsEventMediator`) from the root provider | These are scoped now, and the Osirion registration methods replace earlier registrations: register yours after the Osirion calls and resolve scoped services inside a scope |
| Passes a rooted path, or a path with `.`, `..` or empty segments, to content save, directory operations or the GitHub provider | These are rejected: the FileSystem provider and the command validators reject them, and the GitHub provider throws `ArgumentException` before it sends a request. Use paths relative to `BasePath` |
| Registers a custom content provider (`AddContentProvider<T>`, `ContentBuilder.AddProvider<T>`) that depends on scoped services | The FileSystem provider and these providers are singletons now: resolve scoped services through `IServiceScopeFactory` |
| Uses a removed admin type: `ContentRepositoryAdapter`, `RepositoryAdapterBase`, `AdminContentRepository`, `StatePersistenceService`, `EditableComponent`, `LoadableComponentBase`, `ContentValidators`, `ComponentExtensions`, `ExceptionHandlingMiddleware` | Remove the use; nothing in the library used them |
| Passes a `Theme` string to `LoginPage`, `Login`, `Settings`, `AdminLayout`, `AdminPage`, `CmsAdminDashboard` or `CmsLayoutEditor` | Remove it; the duplicate parameter is gone |

The changelog also deprecates `DomainEntityBase<TId>`, `CommandDispatcher`, `StateManager`, `ErrorHandlingService`,
`CmsAdminStartupService` and `BaseContentRepository.GenerateMarkdownWithFrontMatter`. They still compile. The first five
will be removed in the next major release; use `GenerateMarkdownWithFrontMatterAsync` instead of the last one.

### Check markup and CSS you depend on

If you wrote CSS or script against Osirion markup, check these:

- `OsirionCookieConsent`: the form has the class `osirion-cookie-consent-form` and the buttons sit in a
  `div.osirion-cookie-consent-actions`. Target `.osirion-cookie-consent-form`.
- `DocumentPage`: the body is a `div`, not a second `main`. Update CSS that targeted `main.osirion-document-main`.
- `OsirionFooter` section titles are `h2`, not `h3`. Inside the layout's footer slot the footer renders a `div`.
- `Menu` ids changed from `osirion-menu-{orientation}` to `osirion-menu-{orientation}-{suffix}`: set `Id` if CSS or
  script targets the old one. Submenus no longer use `role="menu"`.
- `ArticlePage` tag badges are `a.osirion-tag-badge` links, not `img.osirion-tag-shield`; default tag and category URLs
  are URL-encoded slugs (`C#` becomes `c%23`).
- Media queries now use literal breakpoint values; a custom property in a media condition never matched.

### Check ids, analytics and SEO output

- **Stored ids.** Content, directory, localization and FileSystem provider ids are now SHA-256 based and stable. They
  used to change on every restart, so a site that stored them (bookmarks, database rows, cache keys) must look them up
  again. No data migration is needed.
- **Navigation.** Configured `Osirion:Navigation:Enhanced` values now apply when the `EnhancedNavigation` parameter is
  not set, so `PreserveScrollForSamePageNavigation` set to `false` now scrolls same-page navigations to the top.
- **Analytics.** An invalid tracker setting stops the app at startup with an `OptionsValidationException` that names
  the key. The consent gate (`Osirion:Analytics:Consent:RequireConsent`) is off by default. Yandex Metrica options bind
  from `Osirion:Analytics:YandexMetrica`; the old `YandexMetrica` key is still read. Matomo and Yandex Metrica add an
  external script when `AutoTrackPageViews` is on.
- **SEO.** `SeoMetadataRenderer` drops the query string from canonical and `og:url` addresses (keep chosen parameters
  with `CanonicalQueryParameters`), emits viewport and theme-color tags only with `EmitMobileMetaTags`, and writes
  generated FAQ data only with `EnableGeneratedFaq`.
- **404.** `ArticlePage`, `DocumentPage` and `HomePage` answer missing content with status 404. On .NET 10 the router's
  `NotFoundPage` renders, so set one. The page needs its own route, for example `@page "/not-found"`; the router
  throws `InvalidOperationException` for a type without a `Route` attribute:

```razor
<Router AppAssembly="typeof(Program).Assembly" NotFoundPage="typeof(Pages.NotFound)">
    <Found Context="routeData">
        <RouteView RouteData="routeData" DefaultLayout="typeof(Layout.MainLayout)" />
    </Found>
</Router>
```

## Part 2. From 4.0 to 4.1

The changelog lists no removed members for 4.1.0. Four changes can alter what a site does:

### Link your app's stylesheet (every site)

`OsirionStyles` no longer links `Osirion.Blazor.Core.bundle.scp.css`. Your app's own `{App}.styles.css` already imports
the scoped styles of every referenced package, so 4.0 downloaded that bundle twice. Make sure the head of your
`App.razor` links the app stylesheet; the Blazor template does:

```razor
<head>
    <link rel="stylesheet" href="@Assets["MySite.styles.css"]" />
    <HeadOutlet />
</head>
```

Use the name of your project in place of `MySite`. Without the link, components lose their own styles.

### Decide how last-modified dates are set (sites that use the local folder provider)

A file's write time no longer stands in for the last-modified date. Without `last_modified` in front matter, an item
is last modified on its `date`. A fresh checkout gives every file the time of the checkout, and that time had reached
`dateModified`, `article:modified_time` and the sitemap's `lastmod`. Add `last_modified` to the front matter of files
you edit, or turn the old behavior back on where files are edited in place:

```json
{
  "Osirion": {
    "Cms": {
      "Web": {
        "FileSystem": {
          "UseFileWriteTimeAsLastModified": true
        }
      }
    }
  }
}
```

### Check the SEO output

- `SeoMetadataRenderer` now applies `AllowAiDiscovery`, `AllowAiTraining`, `AllowSearchIndexing`,
  `EnableGeoOptimization`, `EnableAeoOptimization` and `GenerateMultipleSchemas` from `SeoMetadataOptions` unless the
  page sets the parameter. Before, the component's own default of `true` won, so `AllowAiTraining: false` was ignored.
  Check the values in your `Osirion:Seo` section.
- The `<title>` is no longer cut at 60 characters.
- Structured data no longer carries an author URL guessed from the name. Set `AuthorUrl` to a page that identifies the
  author, such as `/about`, to get one.
- The publisher logo no longer declares a fixed width and height, and the share image is no longer preloaded.

### Expect focusable code blocks and different list matches

- `OsirionHtmlRenderer` gives code blocks and tables `tabindex="0"`, so a keyboard user can scroll them. Set
  `FocusableScrollBlocks="false"` to turn it off.
- A `Category` or `Tag` query matches the name (ignoring case) or the slug exactly. Before, "Dev" also listed "Web Dev",
  and a category with a hyphen of its own listed nothing at its address.
- In the dark theme, the active navigation item, danger and info messages use lighter tokens
  (`--osirion-nav-active-text`, `--osirion-status-danger-text`, `--osirion-status-info-text`). If you override these,
  check your values.

## Check that it works

- `dotnet build` passes with no obsolete warnings from the table in
  [Fix code that breaks the build or the startup](#fix-code-that-breaks-the-build-or-the-startup).
- The site starts, and the log has no "Access token is required" or analytics validation errors.
- A draft file with `published: false` is not listed; a file without the key is.
- The cookie banner accepts a choice and redirects back (it returns 400 without `UseAntiforgery`).
- Pages look as before: if component styles are missing, revisit
  [Link your app's stylesheet](#link-your-apps-stylesheet-every-site).

## Next steps

- [Security](/docs/security): what sanitizing, antiforgery and the webhook secret protect.
- [Troubleshooting](/docs/troubleshooting): symptoms after an upgrade and their fixes.
- [Getting started](/docs/getting-started): the registration and configuration a current site uses.
- [Changelog](https://github.com/demonjic/Osirion.Blazor/blob/master/CHANGELOG.md): every change, release by release.
