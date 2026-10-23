Purpose
-------
Render the Plausible Analytics script when a Plausible provider is registered with a script URL.

Parameters
----------
- `Enabled` (`bool?`): overrides the configured `Enabled` option for this render.

Example
-------
```csharp
builder.Services.AddOsirionAnalytics(a => a.AddPlausible(o => o.ScriptUrl = "https://plausible.io/js/pa-XXXXX.js"));
```

```razor
<head>
    <PlausibleTracker />
</head>
```

Notes
-----
- `ScriptUrl` is your site's own script from Plausible's Site installation settings, or the path your proxy serves
  it from (`/js/script.js`, with `Endpoint = "/api/event"`).
- The markup is Plausible's snippet: the external script plus a short inline script that queues calls and runs
  `plausible.init({...})` with only the options you set (`Endpoint`, `HashBasedRouting`, `OutboundLinks`,
  `FileDownloads`, `FormSubmissions`, `CaptureOnLocalhost`, and `autoCapturePageviews: false` when
  `AutoTrackPageViews` is false). Values are JSON encoded.
- Page views after Blazor enhanced navigation are reported by the script itself (History API).
- With `RequireConsent()` the tracker renders only after the visitor accepted analytics; with `Evaluation` set to `Browser` it renders held (`type="text/plain"`) and runs only after consent.
- Configuration section: `Osirion:Analytics:Plausible`.
