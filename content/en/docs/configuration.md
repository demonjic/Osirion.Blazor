---
id: 'configuration'
order: 3
layout: docs
title: "Configure Osirion.Blazor"
permalink: /docs/configuration
description: "Configure Osirion.Blazor from the Osirion section of appsettings.json or in code, per environment, with secrets kept out of source control."
author: Dejan Demonjić
date: 2026-10-17
last_modified: 2026-10-23
featured_image: 'https://media.getosirion.com/og/docs-configuration.jpg'
categories:
- Documentation
- Guides
tags:
- configuration
- appsettings
- secrets
- options
- validation
published: true
slug: configuration
lang: en
custom_fields:
  group: guides
seo_properties:
  og_image_url: 'https://media.getosirion.com/og/docs-configuration.jpg'
  title: 'Configure Osirion.Blazor: appsettings, code, secrets'
  description: 'Which keys the Osirion section binds, how to set them in appsettings.json or in code, per environment, and how to keep secrets out of the repository.'
  image: 'https://media.getosirion.com/og/docs-configuration.jpg'
  canonical: 'https://getosirion.com/docs/configuration'
  robots: index, follow
---

# Configure Osirion.Blazor

This guide shows where each Osirion setting lives, how to set it in `appsettings.json` or in code, how to vary it per
environment, and where secrets such as a GitHub token belong. It is for developers who have installed the packages.

## Before you start

- Osirion.Blazor is installed and `AddOsirion` is registered, as in the [installation guide](/docs/installation).
- This guide is written for Osirion.Blazor 4.1.0. Two keys arrive later and are marked where they appear:
  `Osirion:Seo:SiteUrl` in the release after 4.1.0, and `Osirion:Site:CallToAction` in the release after 4.2.0.

## Know the sections

Everything binds from the `Osirion` section. `AddOsirion(builder.Configuration)` configures a module only when its
section exists, so a site that sets nothing for a module gets that module's defaults.

| Section | Bound to | Read about it |
| --- | --- | --- |
| `Osirion:Site` | `OsirionSiteOptions`: name, menu, call to action, footer, social links, announcement | [OsirionSiteLayout](https://github.com/demonjic/Osirion.Blazor/blob/master/docs/components/OsirionSiteLayout.md) |
| `Osirion:Theming` | `ThemingOptions`: `Framework`, `DefaultMode`, `DefaultPalette`, `AllowPaletteSelection`, `UseDefaultStyles`, `CustomVariables` | [Theming](/docs/packages/theming) |
| `Osirion:Cms:Web:FileSystem` | `FileSystemOptions`: one local folder | [Content providers](/docs/providers) |
| `Osirion:Cms:Web:GitHub:<Name>` | `GitHubOptions`: one provider per child key | [Content providers](/docs/providers) |
| `Osirion:Seo` | `SeoMetadataOptions`: `SiteName`, `SiteDescription`, `SiteLogoUrl`, `AllowSearchIndexing` and more | [SeoMetadataRenderer](/components/seo-metadata-renderer) |
| `Osirion:Email` | `EmailOptions`: `Provider`, `ToEmail`, and a subsection each for `Smtp`, `SendGrid` and `Infobip` | [Contact form](/components/contact-form) |
| `Osirion:Analytics:<Service>` | One options class per service: `Clarity`, `Matomo`, `GA4`, `YandexMetrica`, `Plausible`, `Umami` | [Analytics](/docs/packages/analytics) |
| `Osirion:Analytics:Consent` | `AnalyticsConsentOptions`: `RequireConsent`, `Category`, `Evaluation` (`Server` or `Browser`, which also sets the mode of `OsirionSiteLayout`'s banner) | [Analytics](/docs/packages/analytics) |
| `Osirion:Navigation:Enhanced` | `EnhancedNavigationOptions` | [Navigation](/docs/packages/navigation) |
| `Osirion:Navigation:ScrollToTop` | `ScrollToTopOptions` | [Navigation](/docs/packages/navigation) |

The sitemap and feeds are not under `Osirion`. `MapOsirionFeeds` takes its own options object, and the templates fill
it from a top-level `Feeds` section that their `Program.cs` binds. The [deployment guide](/docs/deployment) shows how.

## Configure in appsettings.json

A small site that reads Markdown from a folder, uses Osirion's own colors and renders its shell with
`OsirionSiteLayout`:

```json
{
  "Osirion": {
    "Theming": {
      "Framework": "None",
      "DefaultMode": "System"
    },
    "Site": {
      "Name": "My Blog",
      "Menu": [
        { "Text": "Blog", "Href": "/blog", "MatchPrefix": true },
        { "Text": "About", "Href": "/about" }
      ],
      "CallToAction": { "Text": "Book a call", "Href": "https://booking.example.com/intro" }
    },
    "Cms": {
      "Web": {
        "FileSystem": {
          "BasePath": "content",
          "IsDefault": true
        }
      }
    },
    "Seo": {
      "SiteName": "My Blog",
      "SiteDescription": "Notes, guides and ideas, written in Markdown."
    }
  }
}
```

Key names are not case sensitive, as with any ASP.NET Core configuration. A key that matches no property is ignored
without a message, so check a new key by looking at its effect.

`Osirion:Seo:SiteUrl` names the site's public address. When it is an absolute `http` or `https` address, the canonical
URL, `og:url` and the addresses in structured data use it instead of the host that served the request. The sitemap and
the feeds use it too when `Feeds:SiteUrl` is not set. It arrives in the release after 4.1.0.

`Osirion:Site:CallToAction` adds a button at the end of the menu, after the search box and theme toggle, styled as the
primary button of the CSS framework in `Osirion:Theming:Framework`. It needs a `Text` and an `Href`: a path on the site
such as `/contact`, or an `http`, `https` or `mailto` address. An `http` or `https` address gets `rel="noopener"` and
opens in the same tab. Below 1200px the button stays in the bar after the menu toggle, so it is visible on a phone.
`FooterSection`, set to the `Title` of one of `Osirion:Site:FooterSections`, adds the button's link to the end of that
footer column as well, so the footer does not repeat the address. Leave the key out for no button. It arrives in the
release after 4.2.0.

`Osirion:Site:Labels` sets the shell's own texts, such as the skip link, the menu's name and the theme toggle, in the
language of the site; each defaults to the English text. `Osirion:Site:Locales` holds what each language version says
differently, keyed by culture name (`"en"`, `"sr-Latn-RS"`), and the layout picks the entry of the request's UI
culture, which `app.UseRequestLocalization()` sets. The
[OsirionSiteLayout reference](https://github.com/demonjic/Osirion.Blazor/blob/master/docs/components/OsirionSiteLayout.md)
lists every key. Both arrive in the release after 4.2.0. A `Locales` entry with `DerivedFrom` and `Transliteration`
(`"sr-Latn": { "DerivedFrom": "sr", "Transliteration": "serbian-cyrillic-to-latin" }`) is another version written in
a second script; [Localization](/docs/localization#derive-the-shell-and-the-resource-strings-too) explains it. `Osirion:Localization` decides which culture a request has,
from the first segment of its address; [Locale routing](/docs/locale-routing) sets it up.

## Configure in code

Two ways, depending on whether the values come from `appsettings.json` too.

To set everything in code, call the overload of `AddOsirion` that takes a builder. It does not read the `Osirion`
section; each module takes the values you set in the delegate:

```csharp
using Osirion.Blazor.Components;
using Osirion.Blazor.Extensions;
using Osirion.Blazor.Options;

builder.Services.AddOsirion(osirion => osirion
    .UseTheming(theming => theming
        .UseFramework(CssFramework.Bootstrap)
        .Configure(options => options.DefaultMode = ThemeMode.Dark))
    .UseSeoMetadata(seo => seo.SiteName = "My Blog")
    .UseSite(site =>
    {
        site.Name = "My Blog";
        site.Menu.Add(new SiteMenuItem { Text = "Blog", Href = "/blog" });
    })
    .UseContent(content => content.AddFileSystem(files =>
    {
        files.BasePath = "content";
        files.IsDefault = true;
    })));
```

In this form the content builder offers `AddFileSystem` for a local folder. It starts from the
`Osirion:Cms:Web:FileSystem` section when that exists, then runs your delegate. The named GitHub providers under
`Osirion:Cms:Web:GitHub` are registered only by `AddOsirion(builder.Configuration)`, so a site with a GitHub provider
uses the configuration overload (see [Content providers](/docs/providers)).

To keep `appsettings.json` and still change one value in code, register with `AddOsirion(builder.Configuration)` and
then use the options pipeline. `PostConfigure` runs after every other registration, so it has the last word:

```csharp
using Osirion.Blazor.Cms.Domain.Options;
using Osirion.Blazor.Components;
using Osirion.Blazor.Extensions;

builder.Services.AddOsirion(builder.Configuration);

builder.Services.PostConfigure<FileSystemOptions>(options =>
    options.BasePath = Path.GetFullPath(options.BasePath, builder.Environment.ContentRootPath));

builder.Services.PostConfigure<ThemingOptions>(options => options.DefaultMode = ThemeMode.Dark);
```

The first `PostConfigure` is the one the templates use: it turns a relative `BasePath` into an absolute path under the
host's content root and leaves an absolute path as it is. The content root is the folder the process starts in, so this
does not make the path independent of the starting folder; the [providers guide](/docs/providers) says what does.

## Vary settings per environment

ASP.NET Core reads `appsettings.json` first, then `appsettings.{Environment}.json`, where the environment comes from
`ASPNETCORE_ENVIRONMENT` (`Development` under `dotnet run` with the default launch profile, `Production` when it is
unset). A later file overrides only the keys it contains.

A site that reads a local folder while you write and a GitHub repository in production keeps neither provider in
`appsettings.json`. `appsettings.Development.json`:

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

`appsettings.Production.json`:

```json
{
  "Osirion": {
    "Cms": {
      "Web": {
        "GitHub": {
          "Blog": {
            "Owner": "your-account",
            "Repository": "your-content-repository",
            "Branch": "main",
            "ContentPath": "content",
            "IsDefault": true
          }
        }
      }
    }
  }
}
```

Providers are registered from whatever sections exist, so each environment gets exactly one.

## Keep secrets out of appsettings.json

These keys are secrets. They never belong in a file under source control:

| Key | Holds |
| --- | --- |
| `Osirion:Cms:Web:GitHub:<Name>:ApiToken` | The GitHub token the provider reads the repository with |
| `Osirion:Cms:Web:GitHub:<Name>:WebhookSecret` | The secret that signs GitHub webhook deliveries |
| `Osirion:Email:Smtp:Password` | The SMTP password |
| `Osirion:Email:SendGrid:ApiKey`, `Osirion:Email:Infobip:ApiKey` | Email service API keys |

In development, use user secrets. They live in your user profile, outside the project:

```bash
dotnet user-secrets init
dotnet user-secrets set "Osirion:Cms:Web:GitHub:Blog:ApiToken" "<your token>"
```

User secrets are loaded only when the environment is `Development`. In production, use environment variables. An
environment variable name writes the key path with a double underscore where the key has a colon, so the same token
is:

```bash
export Osirion__Cms__Web__GitHub__Blog__ApiToken="<your token>"
```

In PowerShell:

```powershell
$env:Osirion__Cms__Web__GitHub__Blog__ApiToken = "<your token>"
```

List items take their index as a path segment, for example `Osirion__Site__Menu__0__Text`. Environment variables
override both JSON files and user secrets, so a value set on the host wins over the same key in a file. A secret store
that your host maps to environment variables works the same way.

## Know what is checked at startup

Only some options are validated, and each fails differently:

- `Osirion:Site` is validated when the application starts. A site without a `Name`, a menu item without a `Text`, an
  `Href` or child `Items`, a `CallToAction` without a `Text` or an `Href`, or with an `Href` of another kind such as
  `javascript:`, or a `CallToAction:FooterSection` that names no footer section, stops startup with an
  `OptionsValidationException` whose messages name the key, for example `Osirion:Site:Name is required.`
- Each analytics service under `Osirion:Analytics` is validated at startup when it is enabled, for example a GA4
  `MeasurementId` that does not look like `G-XXXXXXXXXX`.
- The feed options are checked when `MapOsirionFeeds` runs: a `SiteUrl` that is not an absolute `http` or `https`
  address throws `ArgumentException`.
- Content providers, theming, SEO and email are not validated. A missing GitHub token, for instance, shows up as a
  logged error when content is first requested, not as a startup failure. The [providers guide](/docs/providers) says
  what you see.

## Check that it works

Start the app and look at the effect of one key from each section you set. A wrong `Osirion:Site` value fails
immediately with the message above. For the others, an unchanged page after you set a key usually means the key path is
misspelled, or an environment variable or user secret with a different value overrides it. To see the value the app
resolved, inject `IOptions<T>` for the options class in the table, for example `IOptions<ThemingOptions>`, and read its
`Value`.

## Next steps

- [Content providers](/docs/providers): the `Osirion:Cms` keys and their defaults.
- [Deployment](/docs/deployment): what a deployed site needs in its environment.
- [Theming](/docs/packages/theming): every `Osirion:Theming` option.
- [Analytics](/docs/packages/analytics): the services under `Osirion:Analytics`.
