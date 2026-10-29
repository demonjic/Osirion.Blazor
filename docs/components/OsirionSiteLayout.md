# OsirionSiteLayout

Purpose
The whole site shell in one line, from the `Osirion.Blazor` package: a skip link, an optional announcement, the main
menu with the brand, search, theme toggle and an optional call to action button, the page, and the footer with link
columns, social profiles, legal links and the copyright. It also renders the theme styles, the scroll-to-top button
and, when turned on, the cookie consent banner. Everything comes from the `Osirion:Site` configuration section. It
renders without script; the menu and theme toggle add behavior where script runs.

Usage
```csharp
// Program.cs: AddOsirion(configuration) reads Osirion:Site when the section exists
builder.Services.AddOsirion(builder.Configuration);
```

```razor
@* Routes.razor *@
<RouteView RouteData="routeData" DefaultLayout="typeof(OsirionSiteLayout)" />
```

```json
{
  "Osirion": {
    "Site": {
      "Name": "My blog",
      "LogoUrl": "/favicon.png",
      "Menu": [
        { "Text": "Blog", "Href": "/blog" },
        { "Text": "Guides", "Items": [ { "Text": "Getting started", "Href": "/guides/start" } ] },
        { "Text": "About", "Href": "/about" }
      ],
      "CallToAction": { "Text": "Book a call", "Href": "https://booking.example.com/intro", "FooterSection": "Contact" },
      "FooterSections": [
        { "Title": "Learn", "Links": [ { "Text": "Docs", "Href": "/docs" } ] },
        { "Title": "Contact", "Links": [ { "Text": "Write to us", "Href": "mailto:hello@example.com" } ] }
      ],
      "SocialLinks": [ { "Network": "github", "Href": "https://github.com/me/blog" } ],
      "LegalLinks": [ { "Text": "Privacy", "Href": "/privacy-policy" } ]
    }
  }
}
```

The same options can be set in code with `builder.Services.AddOsirion(osirion => osirion.UseSite(site => ...))`.

Options (`OsirionSiteOptions`)
- `Name` (required), `LogoUrl`, `HomeUrl` (`/`)
- `Menu`: entries with `Text`, `Href`, `Description` and `MatchPrefix` (true: `/blog` stays marked on
  `/blog/a-post`; never for `/`), and one level of `Items` shown as a dropdown
- `ShowSearch` (false) with `SearchUrl` (`/search`) and `SearchPlaceholder`; turn it on when the site has a search page
- `ShowThemeToggle` (true)
- `CallToAction`: a link styled as the CSS framework's primary button, at the end of the bar after the search box,
  theme toggle and `MenuActions`, with `Text` and `Href` (both required). `Href` is a path on the site (`/contact`) or
  an `http`, `https` or `mailto` address; an `http` or `https` address gets `rel="noopener"` and opens in the same tab.
  `FooterSection` (optional) names the `Title` of one of `FooterSections`, and the call to action becomes the last
  link of that column too, so the footer does not repeat its address; the title must match exactly, letter case
  included. Without `CallToAction` there is no button
- `Announcement`: `Id`, `Text`, optional `LinkText` and `LinkUrl`; dismissing it needs `app.MapOsirionAnnouncements()`
- Footer: `FooterDescription`, `FooterSections` (title and links), `SocialLinks` (`Network` is `github`, `x`,
  `linkedin`, `nuget`, `rss`, `facebook` or `instagram`, in any letter case, for an icon; other networks show their
  `Label` as text) under `SocialTitle`
  (`Follow Us`), `LegalLinks`, and `Owner` and `OwnerUrl` for the copyright line (default `Name` and `HomeUrl`);
  `Copyright` replaces that line with your own markup
- `ShowCookieConsent` (false; needs `app.MapOsirionCookieConsent()`), `ShowScrollToTop` (true). The banner is rendered
  with `Evaluation` from `Osirion:Analytics:Consent:Evaluation`, the setting the trackers read: set it to `Browser`
  for a site exported to static files, and the banner records consent in the browser and needs no endpoint
  (see docs/components/core/CookieConsent.md, static hosting). In either mode a legal link to a page with
  `?customize-cookies=true`, such as `{ "Text": "Cookie settings", "Href": "/privacy-policy?customize-cookies=true" }`,
  reopens the banner, so visitors can change or withdraw consent from the footer
- `Labels`: the shell's own texts, English by default (the text each component used before): `SkipLink` (`Skip to
  main content`), `Menu` (`Main navigation`), `MenuToggle` (`Toggle navigation menu`), `SwitchToDarkTheme` and
  `SwitchToLightTheme` (`Switch to dark mode`, `Switch to light mode`), `Search` (`Search`, the field's label and the
  button's visible text), `SubmitSearch` (`Submit search`, the button's name; keep the words of `Search` in it),
  `LegalLinks` (`Footer legal navigation`), `Announcement` and `DismissAnnouncement`
- `Locales`: what each language version says differently, keyed by culture name in any letter case. An entry may set
  `Name`, `LogoUrl`, `HomeUrl`, `Menu`, `SearchUrl`, `SearchPlaceholder`, `CallToAction`, `Announcement`,
  `FooterDescription`, `FooterSections`, `SocialLinks`, `LegalLinks`, `Owner`, `OwnerUrl`, `Copyright`, `SocialTitle`
  and `Labels`. A value it sets replaces the site's as a whole (a `Menu` is the whole menu; `Labels` are all the
  labels, English for any it leaves out), and a value it leaves out is the site's. Whether search, the theme toggle,
  the cookie banner and the scroll-to-top button are shown is the same for every version. An entry may instead be
  derived from another culture by transliteration: `DerivedFrom` and `Transliteration`, see below

Language versions
```json
{
  "Osirion": {
    "Site": {
      "Name": "Mein Blog",
      "Menu": [ { "Text": "Über uns", "Href": "/ueber-uns" } ],
      "Labels": { "SkipLink": "Zum Inhalt springen", "Menu": "Hauptnavigation" },
      "Locales": {
        "en": {
          "Name": "My blog",
          "HomeUrl": "/en",
          "Menu": [ { "Text": "About us", "Href": "/en/about" } ],
          "Labels": { "Menu": "Main navigation" }
        }
      }
    }
  }
}
```
The layout takes the entry of `CultureInfo.CurrentUICulture`, which `app.UseRequestLocalization()` sets for each
request, or of its nearest parent culture with an entry (`sr-Latn`, then `sr`, for `sr-Latn-RS`); a culture with
neither renders the site's own values. Above, an English request gets the English name, menu and labels (the label it
sets, and English for every other), and every other request the German ones. Give a locale's `Labels` at least one
key: configuration has no empty sections, so `"Labels": {}` sets nothing and the version would keep the site's labels.
Choose the culture from the address (a path
prefix) rather than from `Accept-Language`, so one address always answers with the same page:
`Osirion:Localization` with `app.UseOsirionLocalization()` does that, and `Locales` then decides what the shell says
in the culture it sets (see the locale routing guide, `content/en/docs/locale-routing.md`).
`OsirionSiteOptions.ForCulture(culture)` returns the same view for your own code.

The options are checked when the application starts: a missing `Name`, a menu entry without text or address, a
menu nested more than one level, a call to action without `Text` or `Href` or with an `Href` of another kind (such
as `javascript:` or `//other.example`), or a `CallToAction:FooterSection` that is not the `Title` of any footer section
stops startup with a message naming the configuration key. A locale key that is not a culture name fails too, and so
does a problem in a value a locale sets, named under `Osirion:Site:Locales:<culture>`; its call to action is checked
against the footer sections that version shows. Options set another
way, for example with `Services.Configure<OsirionSiteOptions>`, skip that check; the layout then renders no call to
action for such a value.

Derived versions
```json
"Locales": {
  "sr": { "Name": "Пример", "HomeUrl": "/sr", "Menu": [ { "Text": "Контакт", "Href": "/sr/kontakt" } ] },
  "sr-Latn": { "DerivedFrom": "sr", "Transliteration": "serbian-cyrillic-to-latin", "Name": "Primer d.o.o." }
}
```
`sr-Latn` is the `sr` version (its entry over the site's values) with every text transliterated, the copyright line
as markup, and its links to `sr` pages moved under the `sr-Latn` prefix (`/sr/kontakt` becomes `/sr-latn/kontakt`,
or the prefix `Osirion:Localization` configures); other addresses stay. A key the entry writes itself wins, so the
name above is `Primer d.o.o.`. Startup fails, naming the key, for a `DerivedFrom` that is not a culture, is the entry's
own culture, has neither an entry nor a route in `Osirion:Localization`, or is itself derived, for a `Transliteration`
that is missing or not registered, and, with `Osirion:Localization` registered, for a derived culture it does not
serve. `builder.Services.AddOsirionDerivedResourceLocalization()` derives the version's `IStringLocalizer` strings the
same way (see the localization guide, `content/en/docs/localization.md`).

Parameters, when you wrap it in your own layout
```razor
@inherits LayoutComponentBase
<OsirionSiteLayout>
    <MenuActions><a href="/subscribe">Subscribe</a></MenuActions>
    <ChildContent>@Body</ChildContent>
</OsirionSiteLayout>
```
- `ChildContent`: the page (as the router's layout, the page comes from `Body`)
- `Header`: replaces the menu built from the options, its call to action included; the announcement stays
- `Footer`: replaces the footer built from the options
- `MenuActions`: more actions after the search box and theme toggle; like them they move into the menu's drawer below
  its breakpoint, while the call to action stays in the bar after them
- `MenuLabel`: the accessible name of the menu; unset, it is `Labels:Menu` of the request's language version

Notes
- The menu collapses behind its toggle below 1200px. The call to action stays in the bar, after the toggle (it is the
  menu's `PinnedActions`), rendered once, while the items, search box, theme toggle and `MenuActions` move into the
  drawer. Keyboard order is the brand, the toggle, the drawer's items when it is open, then the call to action. On a
  phone the brand shares the bar with the button, so a long site name is shortened with an ellipsis.
- `AddOsirion` registers the JS component services the page layout needs, so `AddJSComponents()` is no longer
  required in `Program.cs`; calling it as well does no harm.
