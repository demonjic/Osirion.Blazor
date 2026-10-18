---
title: "Osirion.Blazor 4.0: Security Hardening and Fluent UI v5"
author: "Dejan Demonjić"
date: "2026-10-11"
last_modified: 2026-10-11
description: "Osirion.Blazor 4.0.0 secures the CMS admin and webhooks, sanitizes content HTML, adds an analytics consent gate and supports Fluent UI v5."
slug: "osirion-blazor-4-0-release"
categories: [Releases]
tags: [Release Notes, Security, CMS, Analytics, Theming, Migration]
featured_image: 'https://media.getosirion.com/og/blog-osirion-blazor-4-0-release.jpg'
is_featured: false
published: true
seo_properties:
  og_image_url: 'https://media.getosirion.com/og/blog-osirion-blazor-4-0-release.jpg'
  title: "Osirion.Blazor 4.0 Release: Security Hardening and Fluent UI v5"
  description: "What changes in Osirion.Blazor 4.0.0, which changes break existing sites, and how to upgrade."
  type: "Article"
---

# Osirion.Blazor 4.0: security hardening and Fluent UI v5

Osirion.Blazor 4.0.0 is a security release. It closes holes in the CMS admin, the GitHub webhook and content
rendering, sanitizes all content HTML by default, and adds a consent gate for analytics. It also brings the Fluent UI
adapter to Fluent UI Blazor v5 and fixes the MudBlazor adapter. The packages target .NET 9 and .NET 10.

```bash
dotnet add package Osirion.Blazor --version 4.0.0
```

Most changes are behavioral, and several of them break existing sites on purpose. Read
[Breaking changes and how to upgrade](#breaking-changes-and-how-to-upgrade) before you update a site in production.

## CMS admin and GitHub access

- **Admin pages need a signed-in user.** Every routed admin page (`/osirion`, `/osirion/dashboard`,
  `/osirion/content`, `/osirion/content/edit`, `/osirion/settings` and `/osirion/login`) requires the authorization
  policy `"Osirion.Cms.Admin"` (`CmsAdminAuthorization.PolicyName`). The default policy accepts any authenticated
  user; a policy with that name in your application replaces it. Before 4.0 these pages, and the repository writes
  behind them, were open to anonymous visitors.
- **The server token stays on the server.** `Osirion:Cms:Admin:Authentication:PersonalAccessToken` no longer signs in
  every visitor of `/osirion/login` and is never sent to the browser. Admin services never use a content provider's
  `ApiToken`.
- **Per-user tokens are encrypted.** A user's own GitHub token is kept in `ProtectedLocalStorage`, encrypted with
  Data Protection under a purpose bound to the signed-in user, instead of plain-text localStorage.
- **GitHub OAuth** sends a single-use `state` and a PKCE S256 challenge, return URLs are limited to local paths, and
  login errors no longer show exception text.
- **Sessions are isolated.** GitHub API clients are no longer shared between admin sessions, and the admin event
  buses are scoped to one circuit or request, so one admin's events no longer reach another admin's editor.

## Webhooks and content paths

- The GitHub webhook (`UseGitHubWebhook`, `POST /api/github/webhook`) verifies `X-Hub-Signature-256` against the
  providers' `WebhookSecret`, rejects every delivery while no provider has a secret, refuses bodies over 5 MiB, and
  refreshes only the provider whose owner, repository, branch and secret match.
- GitHub paths and branch names are percent-encoded segment by segment, and empty, `.` or `..` segments are rejected
  before any request is sent.
- The FileSystem repositories no longer write, delete, create or move anything outside `FileSystemOptions.BasePath`,
  and the content command validators reject rooted paths, `.` and `..` segments and control characters.

## Content HTML is sanitized

Content HTML is cleaned with an allow-list (HtmlSanitizer on the AngleSharp HTML5 parser) when Markdown is rendered
or ingested, and again where it is written as raw markup: `OsirionHtmlRenderer`, `ContentView`,
`LocalizedContentView`, `DocumentPage`, `MarkdownPreview` and the admin preview. Scripts, event handler attributes,
`javascript:`, `vbscript:` and `data:` URLs, forms, iframes, inline styles, `<style>` and `svg` are removed; `class`,
`id`, `role`, `aria-*` and table cell alignment are kept. `OsirionHtmlRenderer.SanitizeHtml` now defaults to `true`.

If your content needs more, extend the allow-list instead of turning sanitization off:

```csharp
using Osirion.Blazor.Core.Extensions;

builder.Services.AddOsirionHtmlSanitizer(options =>
{
    options.AllowElement("iframe", "src", "width", "height", "title", "allow", "allowfullscreen");
});
```

Other output is encoded or checked too:

- Front matter JSON-LD is parsed and written again with `<`, `>` and `&` escaped; invalid JSON-LD is dropped.
- `HeroSection` and `OsirionBaseSection` CSS-escape background image URLs.
- Navigation links render no `href` for schemes other than relative, http, https, mailto and tel.
- Analytics trackers JSON-encode every configured value they write into scripts.
- The CMS query cache key includes every query field, so a draft query and a public query can no longer share a
  cached result.

## Cookie consent and analytics

- The cookie consent endpoint (`MapOsirionCookieConsent`) requires an antiforgery token, which the banner form now
  carries, and redirects only to local paths.
- A new **consent gate**, off by default, renders trackers only after the visitor accepted the analytics category in
  `OsirionCookieConsent`; GA4 then starts with Google Consent Mode defaults set to denied. Turn it on with
  `RequireConsent()` on the analytics builder or in configuration:

  ```json
  {
    "Osirion": {
      "Analytics": {
        "Consent": {
          "RequireConsent": true
        }
      }
    }
  }
  ```

- Matomo and Yandex Metrica report page views after Blazor enhanced navigation when `AutoTrackPageViews` is on.
- Invalid tracker settings, such as a malformed GA4 id, now fail at startup with a message that names the setting.

## Theming: Fluent UI v5 and MudBlazor

- The Fluent UI adapter reads the Fluent UI Blazor v5 tokens (`--colorNeutralBackground1`, `--colorBrandBackground`
  and others) and falls back to the v4 tokens, so v4 sites need no change.
- With `Framework` set to `FluentUI`, `OsirionStyles` passes Osirion's theme mode to Fluent UI v5, so Fluent
  components follow `ThemeToggle`.
- The MudBlazor, Fluent UI and Radzen adapters now win over Osirion's own light theme, so light mode uses the
  framework's palette; the MudBlazor adapter reads the correct background token and styles form inputs.
- Two new example sites: the CMS example on MudBlazor 9 and on Fluent UI Blazor 5.

## Content and SEO

- **Drafts.** A file without a `published` key is now published (`FrontMatter.Published` defaults to `true`, as in
  Jekyll and Hugo). Before, such files were silently hidden. Add `published: false` to keep a file hidden.
- **Stable ids.** Content, directory and localization ids are SHA-256 based and the same on every restart and every
  server instance; they used to come from `string.GetHashCode()`.
- **Canonical URLs.** `SeoMetadataRenderer` drops the query string and fragment from canonical, `og:url` and
  structured data URLs (keep chosen parameters with `CanonicalQueryParameters`), and viewport and theme-color tags
  are opt-in (`EmitMobileMetaTags`).
- **404s.** `ArticlePage`, `DocumentPage` and `HomePage` no longer stream and answer missing content with HTTP 404.
- **Fewer GitHub requests.** The GitHub provider reads the repository with one Git Trees request and downloads only
  changed files: 3 to 13 requests per refresh for 50 files in 10 folders, down from about 80.
- **Per-user services.** `IThemeService` and `ScrollToTopManager` are scoped, so one user's choices are no longer
  served to others.

The full list, including more than 80 fixes, is in the
[changelog](https://github.com/demonjic/Osirion.Blazor/blob/master/CHANGELOG.md).

## Breaking changes and how to upgrade

Skip the rows for features you do not use. The
[migration guide](https://github.com/demonjic/Osirion.Blazor/blob/master/docs/MIGRATION.md#upgrading-to-400)
has every step in detail.

| Area | What changed | What to do |
| --- | --- | --- |
| CMS admin | Pages require the `Osirion.Cms.Admin` policy | Add an authentication scheme and `UseAuthentication()`/`UseAuthorization()` |
| CMS admin | The configured token no longer signs anyone in | Rotate `PersonalAccessToken`; register `https://<host>/osirion/login` as the OAuth callback |
| Webhooks | Deliveries must be signed | Set `WebhookSecret` in the provider and in GitHub, then restart |
| Content | HTML is sanitized by default | Review content that used iframes, inline styles or scripts; extend the allow-list |
| Cookie consent | Antiforgery token required | Call `app.UseAntiforgery()` before the endpoints |
| Content paths | Rooted paths and `.`/`..` segments are rejected | Use paths relative to the content root |
| Drafts | Files without `published` are public | Add `published: false` to files that must stay hidden |
| Content ids | Ids are new stable hashes | Check translations saved through the admin with a short legacy `id` |
| API | Interface, constructor and lifetime changes | See step 7 of the migration guide |

For the admin, a host that maps the admin pages adds its own sign-in. A minimal setup with a cookie scheme:

```csharp
using Microsoft.AspNetCore.Authentication.Cookies;
using Osirion.Blazor.Cms.Admin.Features.Security;

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => options.LoginPath = "/account/login");

// Only editors may use the admin
builder.Services.AddAuthorization(options =>
    options.AddPolicy(CmsAdminAuthorization.PolicyName, policy => policy.RequireRole("Editor")));

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
```

`/osirion/login` is the GitHub credential step, not a sign-in page, so point `LoginPath` at a page of your own. Sites
that never map the admin pages need no change here.

For the webhook, store the secret outside source control:

```bash
dotnet user-secrets set "Osirion:Cms:Web:GitHub:<Name>:WebhookSecret" "<long random value>"
```

## Related

- [New in Osirion.Blazor 4.0: content components](/blog/osirion-blazor-4-0-components)
- [HTML renderer](/components/html-renderer)
- [Cookie consent](/components/cookie-consent)
- [Analytics](/docs/packages/analytics)
- [Theming](/docs/packages/theming)
- [Osirion.Blazor on NuGet](https://www.nuget.org/packages/Osirion.Blazor)
