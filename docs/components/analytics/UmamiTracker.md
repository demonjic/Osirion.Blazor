Purpose
-------
Render the Umami tracker script when an Umami provider is registered with a website ID.

Parameters
----------
- `SiteId` (`string?`): overrides the configured website ID for this render.
- `Enabled` (`bool?`): overrides the configured `Enabled` option for this render.

Example
-------
```csharp
builder.Services.AddOsirionAnalytics(a => a.AddUmami(o => o.SiteId = "94db1cb1-74f4-4a40-ad6c-962362670409"));
```

```razor
<head>
    <UmamiTracker />
</head>
```

Notes
-----
- Renders one `<script defer>` with `data-website-id` and, when set, `data-host-url`, `data-domains`, `data-tag`,
  `data-exclude-search`, `data-exclude-hash`, `data-do-not-track` and `data-auto-track="false"`. No inline script,
  so a strict Content-Security-Policy only needs the script host allowed.
- `ScriptUrl` defaults to Umami Cloud (`https://cloud.umami.is/script.js`); point it at a self-hosted instance.
- The website ID must be a GUID; other values fail validation at startup and the tracker renders nothing.
- Page views after Blazor enhanced navigation are reported by the script itself (History API).
- With `RequireConsent()` the tracker renders only after the visitor accepted analytics; with `Evaluation` set to `Browser` it renders held (`type="text/plain"`) and runs only after consent.
- Configuration section: `Osirion:Analytics:Umami`.
