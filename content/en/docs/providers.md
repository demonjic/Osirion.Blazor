---
id: 'providers'
order: 4
layout: docs
title: "Content providers"
permalink: /docs/providers
description: "Read Markdown content from a local folder or a GitHub repository: the options and their defaults, the default provider, locales, and how content refreshes."
author: Dejan Demonjić
date: 2026-10-17
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/docs-providers.jpg'
categories:
- Documentation
- Guides
tags:
- providers
- content
- file-system
- github
- markdown
- localization
published: true
slug: providers
lang: en
custom_fields:
  group: guides
seo_properties:
  og_image_url: 'https://media.getosirion.com/og/docs-providers.jpg'
  title: 'Osirion.Blazor content providers: folder and GitHub'
  description: 'Set up the file system and GitHub content providers in Osirion.Blazor: options and defaults, the default provider, locales, tokens and refresh.'
  image: 'https://media.getosirion.com/og/docs-providers.jpg'
  canonical: 'https://getosirion.com/docs/providers'
  robots: index, follow
---

# Content providers

A content provider is where Osirion reads your Markdown from. This guide sets up the two providers that ship with the
library, a local folder and a GitHub repository, and explains which one the components use, how locales work and how
content refreshes. It is for developers who have [installed Osirion.Blazor](/docs/installation).

## Before you start

- `AddOsirion(builder.Configuration)` is registered, as in the [installation guide](/docs/installation).
- You know where `Osirion` settings go, from the [configuration guide](/docs/configuration).
- This guide is written for Osirion.Blazor 4.1.0.

## Choose a provider

| Provider | Reads from | Configured under | How many |
| --- | --- | --- | --- |
| File system | A folder next to the app | `Osirion:Cms:Web:FileSystem` | One |
| GitHub | A folder in a repository, through the GitHub API | `Osirion:Cms:Web:GitHub:<Name>` | One per child key; `<Name>` is only a label |

`AddOsirion` registers a provider for each of those sections that exists. Older single-provider spellings of these
keys exist (`Osirion:Cms:FileSystem:Web` and `Osirion:Cms:GitHub`); this guide uses the keys above. A provider of your
own implements
`IContentProvider` and is registered with `AddCustomContentProvider<TProvider>`; that is not covered here.

Use the file system provider when the content ships with the app, and the GitHub provider when editors change content
in a repository without a deployment.

## Read a local folder

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

| Option | Default | What it does |
| --- | --- | --- |
| `BasePath` | empty | The folder that holds the content. Used as written: a relative path is resolved against the process's working directory, so `dotnet run` from the project folder finds `content` next to `Program.cs`, and a published app started from another folder does not. |
| `WatchForChanges` | `true` | Watches `BasePath` and reloads the content when a Markdown file is created, changed, renamed or deleted. |
| `IncludeSubdirectories` | `true` | Reads subfolders too. |
| `ExcludePatterns` | `**/node_modules/**`, `**/bin/**`, `**/obj/**` | Globs for files to skip, matched against a file's path under `BasePath`, for example `**/drafts/**`. Applies from the release after 4.1.0; 4.1.0 loads every file. |
| `CreateDirectoriesIfNotExist` | `false` | Creates `BasePath` when it is missing. |
| `UseFileWriteTimeAsLastModified` | `false` | Uses a file's write time as the item's last-modified date when its front matter has no `last_modified`. Off by default because a fresh checkout gives every file the time of the checkout, and that date reaches the structured data and the sitemap's `lastmod`. |

If `BasePath` does not exist, the app starts and serves no content. The log shows the warning `Cannot set up file
watcher - base path does not exist` and, when content is first read, an error whose message names the folder:
`Base content directory does not exist`. The host's content root is the starting folder by default, so resolving the
path against it in `Program.cs`, as the templates do, makes it absolute but does not move it. To be independent of the
starting folder, set an absolute `BasePath` on the host (the [configuration guide](/docs/configuration) shows how) or
start the app from the folder that holds `content`.

## Read a GitHub repository

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

| Option | Default | What it does |
| --- | --- | --- |
| `Owner` | empty | The account or organization that owns the repository. |
| `Repository` | empty | The repository name. |
| `Branch` | `main` | The branch to read. |
| `ContentPath` | empty | The folder inside the repository that holds the content. |
| `ApiToken` | empty | The token the provider reads the repository with. Required; see below. |
| `ApiUrl` | `https://api.github.com` | The base address of the GitHub API. |
| `WebhookSecret` | none | The secret that verifies GitHub webhook deliveries; see [Refresh content](#refresh-content). |
| `EnablePolling` | `false` | Checks the branch for new commits on a timer; see [Refresh content](#refresh-content). |
| `PollingIntervalSeconds` | `86400` | Seconds between checks when `EnablePolling` is on, one day by default. |
| `UseBackgroundCacheUpdate` | `true` | Applies the cache updates that polling triggers in the background instead of waiting for them. Webhook updates always run in the background. |

### The token is required

The GitHub provider calls the API only with an `ApiToken`, even for a public repository. Without one, the provider
logs `Access token is required for GitHub API requests` for each call it would have made, reports itself as read-only,
and loads no content: lists are empty and article addresses answer not found. The app does not stop. This is the
first thing to check when a GitHub site shows no content.

Use a token that can read the repository's contents. Never put it in `appsettings.json`: set it as a user secret in
development and as the environment variable `Osirion__Cms__Web__GitHub__Blog__ApiToken` in production, with `Blog`
replaced by your provider's name. The [configuration guide](/docs/configuration) shows both.

If GitHub's rate limit is reached, the provider keeps the content it already has and does not start another load
before the limit resets.

## Choose the default provider

When more than one provider is registered, the components that show a page of content, such as `ContentList` and
`ContentPage`, ask the default provider. The sitemap and the feeds read every provider.

Set `IsDefault` to `true` on the provider you want. Without it, the first registered provider is the default, and
`AddOsirion` registers the GitHub providers first, in configuration order, then the file system provider. If two
providers set it, the one registered last wins.

`IsDefault` on a GitHub provider works with or without `ProviderId` from the release after 4.1.0. In 4.1.0 it takes
effect only if the provider also sets `ProviderId`: without it the setting is ignored and the first registered provider
stays the default. If you run 4.1.0, set one. A provider id is any string that is unique among your providers:

```json
{
  "Osirion": {
    "Cms": {
      "Web": {
        "GitHub": {
          "Docs": {
            "Owner": "your-account",
            "Repository": "your-docs-repository",
            "ProviderId": "docs",
            "IsDefault": true
          }
        }
      }
    }
  }
}
```

A file system provider needs no `ProviderId` for `IsDefault` to work. Without a `ProviderId`, its id is derived from
its `BasePath`, and a GitHub provider's from `Owner` and `Repository`.

## Localize content

If your files are split by language, with a folder per locale such as `content/en/blog` and `content/de/blog`, set
these options on the provider. They have the same names and defaults for both providers:

| Option | Default | What it does |
| --- | --- | --- |
| `EnableLocalization` | `false` | Reads the locale from the first folder below the content root. |
| `DefaultLocale` | `en` | The locale of content whose path has no locale folder, and of all content while localization is off. |
| `SupportedLocales` | `["en"]` | The folder names that count as locales. A first folder not in this list is not a locale. A list you configure replaces the default. This arrives in the release after 4.1.0; 4.1.0 adds your entries to `en`, so `["en", "sr"]` gives `en`, `en`, `sr`. |

```json
{
  "Osirion": {
    "Cms": {
      "Web": {
        "FileSystem": {
          "BasePath": "content",
          "IsDefault": true,
          "EnableLocalization": true,
          "DefaultLocale": "en",
          "SupportedLocales": [ "en", "de" ]
        }
      }
    }
  }
}
```

The content root is `BasePath` for the file system provider and `ContentPath` for the GitHub provider. See
[LocalizedContentView](/components/localized-content-view) for showing the translation that matches the visitor.

## Refresh content

How new content reaches a running site depends on the provider.

- **File system.** The watcher reloads the content when a Markdown file changes, as long as `WatchForChanges` is on.
  After a deployment that replaces the folder, the new files are read on the next start.
- **GitHub.** The content is read when it is first needed and kept in memory. To pick up a push without a restart,
  register the webhook endpoint:

  ```csharp
  using Osirion.Blazor.Cms.Infrastructure.Extensions;

  builder.Services.AddGitHubWebhookAndPolling();

  // after builder.Build()
  app.UseGitHubWebhook();
  ```

  Then set `WebhookSecret` on the provider and add a webhook in the repository settings that sends `push` events to
  `https://your-site/api/github/webhook` with content type `application/json` and the same secret. [Getting
  started](/docs/getting-started) walks through it. Deliveries are verified with the `X-Hub-Signature-256` header, and
  every delivery is rejected while no provider has a secret. Register the webhook only together with a GitHub
  provider. From the release after 4.1.0, `UseGitHubWebhook` throws at startup when no GitHub provider is configured,
  and the message names the missing provider. In 4.1.0 a site with just the file system provider fails differently: the
  app does not start in Development, and every request answers 500 in Production.

Polling is the alternative when GitHub cannot reach your site. Set `EnablePolling` to `true` on a provider, and
`PollingIntervalSeconds` to how often it should check the branch (default 86400, one day). Each provider under
`Osirion:Cms:Web:GitHub` polls with its own two settings from the release after 4.1.0, and the polling service is
started by `AddGitHubWebhookAndPolling()` as above. In 4.1.0 the polling service reads them only from the
single-provider section `Osirion:Cms:GitHub`, so for a named provider use the webhook.

## Check that it works

Run the app and request a page that lists content, or the sitemap at `/sitemap.xml` if you have mapped the feeds. You
should see your items. If you see none, read the log:

- `Access token is required for GitHub API requests`: the GitHub provider has no `ApiToken`.
- `Cannot set up file watcher - base path does not exist` or `Base content directory does not exist`: `BasePath`
  points at a folder that is not there. Check the working directory the app started in.
- Content from the wrong provider: more than one provider is registered and the default is not the one you meant.
  See [Choose the default provider](#choose-the-default-provider).

## Next steps

- [Deployment](/docs/deployment): the token, the content folder and the webhook on a real host.
- [Configuration](/docs/configuration): environments and secrets.
- [CMS web components](/docs/packages/cms-web): `ContentList`, `ArticlePage` and the rest.
- [ContentList](/components/content-list): list pages over the default provider.
