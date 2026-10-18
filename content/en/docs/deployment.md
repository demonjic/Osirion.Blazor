---
id: 'deployment'
order: 5
layout: docs
title: "Deploy an Osirion.Blazor site"
permalink: /docs/deployment
description: "Publish an Osirion.Blazor site with its content, supply tokens through the environment, set the public address and run it behind HTTPS."
author: Dejan Demonjić
date: 2026-10-17
last_modified: 2026-10-17
featured_image: /assets/images/blazor-cover.png
categories:
- Documentation
- Guides
tags:
- deployment
- publish
- https
- reverse-proxy
- sitemap
- feeds
published: true
slug: deployment
lang: en
custom_fields:
  group: guides
seo_properties:
  title: 'Deploy an Osirion.Blazor site: content, HTTPS, feeds'
  description: 'What a deployed Osirion.Blazor site needs: the content folder or GitHub token, forwarded headers behind a proxy, the public address, sitemap and feeds.'
  image: /assets/images/blazor-cover.png
  canonical: 'https://getosirion.com/docs/deployment'
  robots: index, follow
---

# Deploy an Osirion.Blazor site

This guide takes a working Osirion.Blazor site from your machine to a server: what to publish, what the host must
provide, how to keep HTTPS and the public address right behind a proxy, and where the sitemap and feeds come from. It
is for developers who have a site that runs with `dotnet run`.

## Before you start

- A site that shows its content locally, set up with the [providers guide](/docs/providers).
- You know where settings and secrets go, from the [configuration guide](/docs/configuration).
- This guide is written for Osirion.Blazor 4.1.0. One setting, `Osirion:Seo:SiteUrl`, arrives in the release after
  4.1.0 and is marked where it appears.

An Osirion.Blazor site is an ASP.NET Core application that renders its pages on the server. The repository includes no
static export tool, so this guide covers hosting the application.

## Publish with the content

```bash
dotnet publish -c Release -o publish
```

`dotnet publish` does not copy Markdown files. If the site reads a local folder, add this to the project file, as the
templates do, or the published site starts with no content:

```xml
<ItemGroup>
  <Content Include="content\**" CopyToPublishDirectory="PreserveNewest" />
</ItemGroup>
```

A relative `BasePath` is resolved against the process's working directory, and the host's content root is that same
folder, so resolving the path against the content root in `Program.cs` does not change where it points. Start the app
from the publish folder, or set an absolute `Osirion__Cms__Web__FileSystem__BasePath` on the host (see [Give the host its settings](#give-the-host-its-settings)). A site that
reads a GitHub repository needs no content folder.

## Give the host its settings

Set these on the host as environment variables. Nothing here belongs in a file you deploy.

| Environment variable | Needed when |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT=Production` | Recommended. It selects `appsettings.Production.json` and keeps user secrets out; an unset variable already means `Production`, and setting it states the intent. |
| `Osirion__Cms__Web__GitHub__Blog__ApiToken` | The site uses a GitHub provider named `Blog`. Replace `Blog` with your provider's name, the key under `Osirion:Cms:Web:GitHub`. |
| `Osirion__Cms__Web__GitHub__Blog__WebhookSecret` | You register the GitHub webhook (see [Plan for refresh](#plan-for-refresh)). |
| `Osirion__Cms__Web__FileSystem__BasePath` | The content folder sits at an absolute path on the host. |
| `Feeds__SiteUrl` | The site maps the sitemap and feeds and binds a `Feeds` section, as the templates do (see [Set the public address](#set-the-public-address-then-check-the-sitemap-and-feeds)). From the release after 4.1.0 `Osirion__Seo__SiteUrl` can stand in for it. |
| `Osirion__Seo__SiteUrl` | You want canonical URLs, and from the release after 4.1.0 the sitemap and feeds, to use your public address. Arrives in the release after 4.1.0. |
| `Osirion__Email__Smtp__Password` or an API key | The contact form sends email. |

The double underscore stands for the colon in a configuration key. Without a GitHub token the provider loads no content
and the site renders empty lists; see the [providers guide](/docs/providers).

## Keep HTTPS and the scheme right behind a proxy

The templates redirect HTTP to HTTPS and send HSTS outside Development:

```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
```

The sitemap and feeds build their addresses from the request's scheme and host when no public address is configured.
Osirion does not configure forwarded headers itself, so a proxy that ends TLS and forwards plain HTTP makes the app
see `http`, and the sitemap lists `http://` addresses. The cookies the server sets are always `Secure`, whatever
scheme the app sees.

If a proxy terminates TLS in front of the app, tell ASP.NET Core to trust its `X-Forwarded-Proto` header:

```csharp
using Microsoft.AspNetCore.HttpOverrides;

builder.Services.Configure<ForwardedHeadersOptions>(options =>
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);

var app = builder.Build();

app.UseForwardedHeaders();
```

Register `UseForwardedHeaders` before the other middleware. The middleware ignores the headers from a proxy it does not
trust, which by default is only a proxy on the same machine; Microsoft's page on
[proxy servers and load balancers](https://learn.microsoft.com/aspnet/core/host-and-deploy/proxy-load-balancer) explains
how to list yours.

## Set the public address, then check the sitemap and feeds

`MapOsirionFeeds` maps three endpoints from your published content: `/sitemap.xml`, `/feed.xml` (RSS) and `/atom.xml`.
Its `SiteUrl` option is the public address every absolute URL in them starts with. The templates bind it from a
top-level `Feeds` section, so the address can differ per environment:

```csharp
using Osirion.Blazor.Cms.Web.Feeds;

app.MapOsirionFeeds(feeds => app.Configuration.GetSection("Feeds").Bind(feeds));
```

```json
{
  "Feeds": {
    "SiteUrl": "https://blog.example.com",
    "FeedDirectory": "blog",
    "AdditionalSitemapUrls": [ "/", "/blog" ]
  }
}
```

Without `SiteUrl`, the addresses follow whatever scheme and `Host` the request carried, so a forged `Host` header can
put another host into a response that a cache stores. Set it in production. From the release after 4.1.0 the feeds fall
back to `Osirion:Seo:SiteUrl` before they use the request, so a site that sets that address once needs no `Feeds`
section for it; in 4.1.0 set `Feeds:SiteUrl`. `MapOsirionFeeds` throws at startup if
`SiteUrl` is not an absolute `http` or `https` address. The options are on the
[feeds and sitemap page](/components/feeds-and-sitemap).

Responses carry `Cache-Control: public, max-age=600` by default (`CacheDuration`), so a shared cache may serve a
document for up to ten minutes after the content changes.

Point crawlers at the sitemap in `wwwroot/robots.txt`, with the same address:

```text
Sitemap: https://blog.example.com/sitemap.xml
```

## Plan for refresh

How content changes after the deployment depends on the provider:

- **File system.** The content is the folder you published. Deploying a new folder and restarting picks up changes;
  the file watcher also reloads files edited in place.
- **GitHub.** The content is read from the API and kept in the app's memory. A push reaches the running site through the
  webhook (`AddGitHubWebhookAndPolling` and `UseGitHubWebhook`, with `WebhookSecret` set, in the
  [providers guide](/docs/providers)). GitHub must be able to reach `https://your-site/api/github/webhook`, so do not
  put that path behind a sign-in. The server must also reach the GitHub API (`https://api.github.com` unless you set
  `ApiUrl`).

The cache lives in each instance's memory, and a webhook delivery goes to one instance. With more than one instance
behind a load balancer, only the instance that receives a push refreshes. Run one instance, or serve the content folder
from the deployment, if every instance must change together.

A GitHub provider reads its content on the first request that needs it. To read it at startup instead, so the first
visitor does not wait for the API, call `UseOsirionCms` after building the app:

```csharp
using Osirion.Blazor.Cms.Infrastructure.DependencyInjection;

app.UseOsirionCms();
```

It blocks startup until the providers are initialized, and it logs an initialization error instead of stopping the app.
`UseOsirionCmsAsync` does the same without blocking the thread.

## Choose where to host it

Osirion has no preference. It needs an ASP.NET Core host and, depending on the provider, a content folder or outbound
HTTPS to GitHub. The choices differ in who ends TLS and where the files live:

- **A server or virtual machine** running the app with Kestrel behind a reverse proxy such as nginx, Caddy or IIS. You
  manage the runtime, the proxy and the certificate. [Keep HTTPS and the scheme right behind a proxy](#keep-https-and-the-scheme-right-behind-a-proxy) applies.
- **A container**, run by an orchestrator or a container service. Publish into the image, pass the settings from
  [Give the host its settings](#give-the-host-its-settings) as environment variables, and let the platform's ingress end TLS.
  [Keep HTTPS and the scheme right behind a proxy](#keep-https-and-the-scheme-right-behind-a-proxy) applies.
- **A managed application host** for .NET. The platform builds or runs your publish output and usually ends TLS for you,
  so check what scheme the app sees. [Keep HTTPS and the scheme right behind a proxy](#keep-https-and-the-scheme-right-behind-a-proxy) applies unless the platform already
  sets the forwarded headers for the app.

Whichever you choose, run the published output, not the project folder, and give the process only the settings in
[Give the host its settings](#give-the-host-its-settings).

## Check that it works

Run the published app the way the host will, with the environment variables set:

```bash
cd publish
ASPNETCORE_ENVIRONMENT=Production dotnet MySite.dll
```

Then, from outside if you can:

```bash
curl -i https://blog.example.com/sitemap.xml
```

You should see status 200, `Content-Type: application/xml; charset=utf-8`, and a `<loc>` for every published page that
starts with your public address, not an internal host name or `http://`.

- A sitemap that holds only the paths you listed in `AdditionalSitemapUrls` means the provider has no content. For the
  file system, the content folder was not published or `BasePath` is wrong; for GitHub, the token is missing or
  wrong. The log names which one (see the [providers guide](/docs/providers)).
- `http://` addresses in the sitemap behind an HTTPS proxy: set `SiteUrl` (or, from the release after 4.1.0,
  `Osirion:Seo:SiteUrl`), and add the forwarded headers from
  [Keep HTTPS and the scheme right behind a proxy](#keep-https-and-the-scheme-right-behind-a-proxy).
- A 404 for `/sitemap.xml`: `MapOsirionFeeds` is not called in `Program.cs`.

## Next steps

- [Content providers](/docs/providers): tokens, the default provider and the webhook.
- [Configuration](/docs/configuration): environments and secrets.
- [Feeds and sitemap](/components/feeds-and-sitemap): every option of `MapOsirionFeeds`.
- [Getting started](/docs/getting-started): the template path to a first site.
