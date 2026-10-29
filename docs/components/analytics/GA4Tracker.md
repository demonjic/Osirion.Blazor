Purpose
-------
Render Google Analytics 4 (GA4) script when a configured GA4 provider is enabled.

Parameters
----------
`MeasurementId`, `DebugMode` and `Enabled` (all optional) override the configured `GA4Options` for that render. Everything else is configured through `GA4Options` in analytics services. With the consent gate on (`RequireConsent()`), the tag renders only after the visitor accepted the `analytics` category. With `Evaluation` set to `Browser` (a site exported to static files), it renders for every visitor held as `type="text/plain"` and `osirion-consent.js` runs it after consent.

Example
-------
```razor
<GA4Tracker />
```

Notes
-----
- See `src/Osirion.Blazor.Analytics/Providers/GA4Provider.cs` for configuration fields.
- This component inherits common behavior from the analytics base tracker.
