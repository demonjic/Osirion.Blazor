# GA4Tracker

Renders Google Analytics 4 tag. Configure via GA4Options (MeasurementId, SendPageView, etc.).

# MatomoTracker

Renders Matomo script and configuration. Configure via MatomoOptions (SiteId, EndpointUrl, etc.).

# YandexMetricaTracker

Renders Yandex Metrica tag. Configure via YandexMetricaOptions.

# ClarityTracker

Renders Microsoft Clarity tag. Configure via ClarityOptions (ProjectId, etc.).

# PlausibleTracker

Renders the Plausible script and its `plausible.init` call. Configure via PlausibleOptions (ScriptUrl, Endpoint, optional tracking switches). See PlausibleTracker.md.

# UmamiTracker

Renders the Umami script tag, configured by attributes. Configure via UmamiOptions (SiteId = website ID, ScriptUrl, HostUrl, Domains). See UmamiTracker.md.

# AnalyticsTracker

Generic wrapper that selects the configured provider and renders its tracker.

Usage
- Register AddOsirionAnalytics in Program.cs
- Configure desired provider and options via configuration or code
- Place tracker component in MainLayout or document head/body per provider requirements.

Consent on a static site
- With `RequireConsent` and `Evaluation` = `Browser` (`Osirion:Analytics:Consent:Evaluation`), every tracker renders its scripts held (`<script type="text/plain" data-osirion-consent="analytics">`, the original `type` in `data-osirion-type`) followed by `<script type="module">` for `_content/Osirion.Blazor.Core/js/osirion-consent.js`, which runs them in page order once the visitor's consent includes the category. Markup that is not a script is left out. See ConsentScripts.HoldUntilConsent and docs/components/core/CookieConsent.md.
