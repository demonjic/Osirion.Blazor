# Osirion.Blazor.Analytics

[![NuGet](https://img.shields.io/nuget/v/Osirion.Blazor.Analytics)](https://www.nuget.org/packages/Osirion.Blazor.Analytics)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Osirion.Blazor)](https://www.nuget.org/packages/Osirion.Blazor.Analytics)
[![License](https://img.shields.io/github/license/demonjic/Osirion.Blazor)](https://github.com/demonjic/Osirion.Blazor/blob/master/LICENSE.txt)

Analytics integration for Blazor applications, supporting multiple providers with SSR compatibility.

## Features

- **Multiple Providers**: Microsoft Clarity, Matomo, Google Analytics 4 and Yandex Metrica support out of the box
- **Provider Pattern**: Easily extend with your own analytics providers
- **SSR Compatible**: Works with Server-Side Rendering and Static SSG
- **Configuration-Driven**: Simple setup through dependency injection
- **Privacy-Focused**: Opt-in tracking with consent options

## Installation

```bash
dotnet add package Osirion.Blazor.Analytics
```

## Usage

### Quick Start

```csharp
// In Program.cs
using Osirion.Blazor.Analytics.Extensions;

builder.Services.AddOsirionAnalytics(analytics => {
    analytics
        .AddClarity(options => {
            options.SiteId = "your-clarity-site-id";
        })
        .AddMatomo(options => {
            options.SiteId = "1";
            options.TrackerUrl = "//analytics.example.com/";
        })
        .AddGA4(options => {
            options.MeasurementId = "G-XXXXXXXXXX";
            options.DebugMode = builder.Environment.IsDevelopment();
        })
        .AddYandexMetrica(options => {
            options.CounterId = "12345678";
            options.WebVisor = true;
            options.TrackLinks = true;
        });
});
```

```razor
@using Osirion.Blazor.Analytics.Components

<!-- In your layout -->
<ClarityTracker />
<MatomoTracker />
<GA4Tracker />
<YandexMetricaTracker />
```

### With Configuration

```json
// In appsettings.json
{
  "Osirion": {
    "Analytics": {
      "Clarity": {
        "SiteId": "your-clarity-site-id",
        "Enabled": true
      },
      "Matomo": {
        "SiteId": "1",
        "TrackerUrl": "//analytics.example.com/",
        "TrackLinks": true,
        "RequireConsent": false
      },
      "GA4": {
        "MeasurementId": "G-XXXXXXXXXX",
        "Enabled": true,
        "AnonymizeIp": true,
        "TrackOutboundLinks": true,
        "DebugMode": false
      },
      "YandexMetrica": {
        "CounterId": "12345678",
        "Enabled": true,
        "WebVisor": true,
        "ClickMap": true,
        "TrackLinks": true,
        "AccurateTrackBounce": true,
        "EcommerceEnabled": true,
        "EcommerceContainerName": "dataLayer"
      }
    }
  }
}
```

```csharp
// In Program.cs
builder.Services.AddOsirionAnalytics(builder.Configuration);

// Or when using the full Osirion.Blazor package:
builder.Services.AddOsirion(builder.Configuration);
```

### Manual Tracking

```csharp
@inject IAnalyticsService Analytics

// Track a page view
await Analytics.TrackPageViewAsync("/custom-path");

// Track an event
await Analytics.TrackEventAsync("category", "action", "label", value);
```

### Creating Custom Providers

```csharp
public class CustomProvider : IAnalyticsProvider
{
    public string ProviderId => "custom";
    public bool IsEnabled => true;
    public bool ShouldRender => true;

    public Task TrackEventAsync(string category, string action, string? label = null, object? value = null, CancellationToken cancellationToken = default)
    {
        // Custom implementation
        return Task.CompletedTask;
    }

    public Task TrackPageViewAsync(string? path = null, CancellationToken cancellationToken = default)
    {
        // Custom implementation
        return Task.CompletedTask;
    }

    public string GetScript()
    {
        return "<script>/* Custom tracking script */</script>";
    }
}

// Register the provider
builder.Services.AddOsirionAnalytics(analytics => {
    analytics.AddProvider<CustomProvider>();
});
```

## Configuration Options

### Clarity Options

- `SiteId`: Your Microsoft Clarity site ID
- `Enabled`: Whether the tracker is enabled (default: true)
- `TrackerUrl`: The URL for the Clarity script (default: "https://www.clarity.ms/tag/")
- `TrackUserAttributes`: Obsolete, no effect. Call `clarity("identify", ...)` from your own code
- `AutoTrackPageViews`: No effect for Clarity, which records every page (default: true)

### Matomo Options

- `SiteId`: Your Matomo site ID
- `Enabled`: Whether the tracker is enabled (default: true)
- `TrackerUrl`: The URL to your Matomo installation
- `TrackLinks`: Whether to track link clicks (default: true)
- `TrackDownloads`: Obsolete, no effect. Matomo tracks downloads together with outlinks when `TrackLinks` is on
- `RequireConsent`: Whether to require cookie consent (default: false)
- `AutoTrackPageViews`: `false` leaves out the script that reports enhanced navigations; the loaded page is always counted (default: true)

### Yandex Metrica Options

- `CounterId`: Your Yandex Metrica counter ID
- `Enabled`: Whether the tracker is enabled (default: true)
- `WebVisor`: Enable session replay recordings (default: false)
- `ClickMap`: Create a map of clicks on your site (default: true)
- `TrackLinks`: Track clicks on outbound links (default: true)
- `AccurateTrackBounce`: Accurately track bounce rate (default: true)
- `TrackHash`: Track hash changes in the address bar, written as `trackHash` when set (default: null). `HashTracking: true` does the same while this is not set
- `DeferLoad`: Disable automatic sending of page view data (default: false)
- `AlternativeCdn`: Use alternative CDN domain (optional)
- `Params`: Custom parameters to track (optional)
- `UserParams`: User parameters for tracking (optional)
- `EcommerceEnabled`: Enable e-commerce data layer (default: false)
- `EcommerceContainerName`: E-commerce data layer container name (default: "dataLayer")

### Plausible Options

- `ScriptUrl`: Your site's script from Plausible's Site installation settings, such as `https://plausible.io/js/pa-XXXXX.js`, or a proxied path such as `/js/script.js` (required)
- `Endpoint`: Where events are sent, for a proxy such as `/api/event` (optional)
- `HashBasedRouting`, `OutboundLinks`, `FileDownloads`, `FormSubmissions`, `CaptureOnLocalhost`: Override the site's tracking settings; left out of the markup unless set
- `AutoTrackPageViews`: `false` sends `autoCapturePageviews: false` so you report page views yourself (default: true)
- `Enabled`: Whether the tracker is enabled (default: true)

### Umami Options

- `SiteId`: Your Umami website ID, a GUID (required)
- `ScriptUrl`: The tracker script (default: "https://cloud.umami.is/script.js"; self-hosted: `https://your-umami-host/script.js`)
- `HostUrl`: Where data is sent (`data-host-url`, optional)
- `Domains`: Comma-separated host names the tracker runs on (`data-domains`, optional)
- `Tag`: Groups the site's events (`data-tag`, optional)
- `ExcludeSearch`, `ExcludeHash`: Leave query strings or fragments out of the collected URLs (default: false)
- `DoNotTrack`: Respect the browser's Do Not Track setting (default: false)
- `AutoTrackPageViews`: `false` writes `data-auto-track="false"`, which turns off all automatic tracking (default: true)
- `Enabled`: Whether the tracker is enabled (default: true)

## Integration with Privacy Modules

Trackers can wait for the visitor's consent, as recorded by `OsirionCookieConsent`. Turn the gate on with
`RequireConsent()` (or `Osirion:Analytics:Consent:RequireConsent`):

```csharp
builder.Services.AddOsirionAnalytics(analytics => analytics
    .AddMatomo(options => { options.SiteId = "1"; options.TrackerUrl = "https://analytics.example.com/"; })
    .RequireConsent());
```

By default consent is read from the consent cookie on the server, and a tracker renders nothing until the visitor
accepted the `analytics` category. A site exported to static files has no request to read it from, so evaluate consent
in the browser instead:

```csharp
builder.Services.AddOsirionAnalytics(analytics => analytics
    .AddGA4(options => options.MeasurementId = "G-XXXXXXXXXX")
    .RequireConsent(options => options.Evaluation = ConsentEvaluation.Browser));
```

or `"Osirion": { "Analytics": { "Consent": { "RequireConsent": true, "Evaluation": "Browser" } } }`. Each tracker then
renders its scripts held (`type="text/plain"`, with `data-osirion-consent="analytics"`) and `osirion-consent.js`, from
Osirion.Blazor.Core, runs them after consent; `OsirionSiteLayout` renders its banner in the same mode. See
`docs/components/core/CookieConsent.md` (static hosting) for the banner, the reopen link and the Content Security Policy.

## SSR Considerations

The analytics components are designed to work seamlessly in Server-Side Rendering (SSR) environments:

- Scripts are only rendered when needed
- No JavaScript interop is required for basic functionality
- Components properly handle non-interactive rendering scenarios

## License

This project is licensed under the MIT License - see the [LICENSE](https://github.com/demonjic/Osirion.Blazor/blob/master/LICENSE.txt) file for details.