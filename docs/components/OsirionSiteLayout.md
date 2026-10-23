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
      "CallToAction": { "Text": "Book a call", "Href": "https://booking.example.com/intro" },
      "FooterSections": [ { "Title": "Learn", "Links": [ { "Text": "Docs", "Href": "/docs" } ] } ],
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
- `CallToAction`: a link styled as the CSS framework's primary button, after the search box and theme toggle, with
  `Text` and `Href` (both required). `Href` is a path on the site (`/contact`) or an `http`, `https` or `mailto`
  address; an `http` or `https` address gets `rel="noopener"` and opens in the same tab. Without it there is no button
- `Announcement`: `Id`, `Text`, optional `LinkText` and `LinkUrl`; dismissing it needs `app.MapOsirionAnnouncements()`
- Footer: `FooterDescription`, `FooterSections` (title and links), `SocialLinks` (`Network` is `github`, `x`,
  `linkedin`, `nuget`, `rss`, `facebook` or `instagram`, in any letter case, for an icon; other networks show their
  `Label` as text) under `SocialTitle`
  (`Follow Us`), `LegalLinks`, and `Owner` and `OwnerUrl` for the copyright line (default `Name` and `HomeUrl`);
  `Copyright` replaces that line with your own markup
- `ShowCookieConsent` (false; needs `app.MapOsirionCookieConsent()`), `ShowScrollToTop` (true)

The options are checked when the application starts: a missing `Name`, a menu entry without text or address, a
menu nested more than one level, or a call to action without `Text` or `Href` or with an `Href` of another kind (such
as `javascript:` or `//other.example`) stops startup with a message naming the configuration key. Options set another
way, for example with `Services.Configure<OsirionSiteOptions>`, skip that check; the layout then renders no call to
action for such a value.

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
- `MenuActions`: more actions after the search box, theme toggle and call to action
- `MenuLabel`: the accessible name of the menu (`Main navigation`)

Notes
- The menu collapses behind its toggle below 1200px. The call to action then shows in the opened menu, on a row of
  its own below the search box and theme toggle, and follows them in keyboard order.
- `AddOsirion` registers the JS component services the page layout needs, so `AddJSComponents()` is no longer
  required in `Program.cs`; calling it as well does no harm.
