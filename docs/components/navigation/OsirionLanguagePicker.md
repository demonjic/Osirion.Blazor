# OsirionLanguagePicker

Purpose
Links the current page in each of the site's languages, for a site whose addresses carry the culture in their first
path segment (`/contact` in the default culture, `/sr/contact` in Serbian). It works in static SSR and needs no script.

Requirements
- `Osirion:Localization` registered by `AddOsirion(configuration)`, or `services.AddOsirionLocalization(...)`, which
  provides `IOsirionLocalePaths`. Without it the component cannot be created.
- `app.UseOsirionLocalization()` in the pipeline, so each request has the culture of its address. See the locale
  routing guide (`content/en/docs/locale-routing.md`, `/docs/locale-routing` on the documentation site).

Key parameters
- AriaLabel: the accessible name of the `nav` landmark (default: `"Language"`). Translate it per language, for
  example from your own resources
- Compact: a `details` disclosure whose `summary` names the current language and opens the list (default: `false`,
  a row of links). It needs no script, opens with Enter or Space, and keeps a header on one line; the list opens below
  it, aligned to its end
- Class: extra CSS classes, added after `osirion-language-picker` (inherited from `OsirionComponentBase`)
- Style: inherited from `OsirionComponentBase`, written on the `nav`
- Theme: `ThemeMode.System` (default), `Light` or `Dark`; a non-System value is written as `data-theme` (or
  `data-bs-theme` when the Bootstrap framework is configured) on the `nav` (inherited)
- Attributes: any other attribute, captured and applied to the `nav` (inherited)

Usage
```razor
@using Osirion.Blazor.Navigation.Components

<OsirionLanguagePicker />
```

With `Osirion:Localization` set to `en` (default, display name `English`) and `sr-Latn` (prefix `sr`), at
`/sr/contact?tab=2` it renders:

```html
<nav class="osirion-language-picker" aria-label="Language">
    <ul class="osirion-language-picker-list">
        <li class="osirion-language-picker-item">
            <a class="osirion-language-picker-link" href="/contact?tab=2" hreflang="en" lang="en">English</a>
        </li>
        <li class="osirion-language-picker-item">
            <a class="osirion-language-picker-link" href="/sr/contact?tab=2" hreflang="sr-Latn" lang="sr-Latn" aria-current="true">Srpski (latinica)</a>
        </li>
    </ul>
</nav>
```

Notes
- Renders nothing when the site has fewer than two cultures.
- The links follow the configured order of `Cultures`, with the default culture first when it is not listed. Their
  text is each culture's `DisplayName`, which defaults to the culture's own name for itself (the exact text comes from
  the platform's culture data).
- The current page comes from `NavigationManager`: its path without a culture prefix (`WithoutLocale`), then each
  culture's prefix (`Localize`). The query string is kept and the fragment dropped. The default culture's link has no
  prefix. On a page the framework re-executes for a status code or an error (`UseStatusCodePagesWithReExecute`,
  `UseExceptionHandler`), the address the visitor asked for is used instead (`IStatusCodeReExecuteFeature`, then
  `IExceptionHandlerPathFeature`), as the culture provider does, so a not-found page under `/sr/` marks Serbian.
- `OsirionSiteLayout` renders it from `Osirion:Site:LanguagePicker`: compact in the header, a row in the footer.
- The current culture is the one of the address (its prefix, or the default culture without one), marked with
  `aria-current="true"`; the others have no `aria-current`.
- Links are written from the application's path base, never from the request's host, and leading slashes in the
  current address collapse, so the links always stay on the site.
- Under interactive rendering it follows navigation (`LocationChanged`) and unsubscribes when disposed.
- Styling uses `--osirion-*` tokens. Its own hooks: `--osirion-language-picker-gap`, `--osirion-language-picker-color`,
  `--osirion-language-picker-hover-color` and `--osirion-language-picker-active-color`, and for the compact list
  `--osirion-language-picker-background`. The current language is shown in a heavier weight and underlined, not by
  color alone.
