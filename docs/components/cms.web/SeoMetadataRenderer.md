# SeoMetadataRenderer

Emit meta tags (title, description, og:*, twitter:*) from ContentItem metadata.

Structured data
- Custom JSON-LD from front matter (`seo_properties.json_ld`) is parsed as JSON and written back with `<`, `>`
  and `&` escaped, so it cannot end the surrounding script element.
- A value that is not a valid JSON object or array, including one whose text is not valid Unicode (for example
  half of a surrogate pair), is ignored with a warning log entry, and the generated Schema.org data is rendered
  instead.
- `SchemaType.TechArticle`, `SoftwareApplication` (with `ApplicationCategory`, default `DeveloperApplication`),
  `ProfilePage` (the author as `mainEntity`, tags that are absolute links as `sameAs`), `AboutPage` (the site's
  organization as `mainEntity`) and `ContactPage` describe whole pages.
- Without `SchemaTypes`, configured `DefaultSchemaTypes` or a front matter `type`, a page is a `WebPage` with
  `og:type` `website`; `FallbackSchemaType` changes that last step (`ArticlePage` passes `Article`).
- On a list page (`ContentItems`), `SchemaTypes` writes a `CollectionPage` and a `BreadcrumbList` when it lists
  them, and each item is an `Article`, or its front matter `type` when that is `BlogPosting` or `TechArticle`, or the
  first of those article types that `SchemaTypes` names. Empty values are left out instead of written as `null`.

## Referrer policy

- No `referrer` meta tag by default: a referrer meta tag replaces the policy of the `Referrer-Policy` response
  header, so the server's header (or the browser default, `strict-origin-when-cross-origin`) applies.
- `ReferrerPolicy` (parameter, or `SeoMetadataOptions.ReferrerPolicy`) writes the tag. Only the policy tokens of the
  Referrer Policy specification are written; any other value writes nothing and logs a warning.

## Canonical URL, FAQ data and mobile tags

- The canonical URL, `og:url` and the structured data URLs are the current address without its query string and fragment, so tracking parameters never create a second canonical URL. Collection pages get a canonical link too. Use `CanonicalQueryParameters` to keep named parameters (for example `page` for paginated lists); a canonical declared in front matter always wins.
- `EnableGeneratedFaq` (default `false`, also available on `SeoMetadataOptions`) turns on FAQ JSON-LD and `answers-to` meta tags derived from titles and descriptions. It requires `EnableAeoOptimization`. An explicit `SchemaType.FAQ` emits nothing on its own.
- `EmitMobileMetaTags` (default `false`) emits viewport, theme-color and the mobile web app tags. Leave it off when the host document already declares them.
- Metadata is recomputed whenever the parameters are set, so a reused instance never keeps the previous page's values.
- `AlternateLanguageUrls` entries must have the form `language|url`; malformed entries are skipped and logged.

## Language versions

- On a site with locale routing (`IOsirionLocalePaths` registered by `AddOsirionLocalization`), a page that passes no
  `AlternateLanguageUrls` gets its `hreflang` links written for it: each version named by its culture name, then
  `x-default` with the default culture's address, and nothing for fewer than two versions.
- A content item links its canonical address and each published translation (same `ContentId`) whose locale is a
  configured culture, at `/{url}` with that culture's prefix. A culture without a translation, and a `noindex`
  translation, is not linked. The translations are read through `IContentProviderManager` in `OnParametersSetAsync`;
  a failed read writes no links and logs a warning.
- A list page or a code page links its canonical path (`CanonicalPath` when set) in every culture through
  `IOsirionLocalePaths.Localize`. On such a site a page without `Locale` takes its language from the address prefix.
- `AlternateLanguageUrls` always wins; an empty list writes no links.

## Author, image size, canonical path and breadcrumbs

- An item without an author gets no `author` in any schema: a `Person` with an empty name is invalid.
- `og:image:width` and `og:image:height` are written only for the default image (`DefaultImageUrl`), with
  `DefaultImageWidth` and `DefaultImageHeight`; an item's own image has no known size, so none is declared.
- `CanonicalPath` applies to content items too, after front matter `canonical`; the page components pass theirs on.
- The first breadcrumb is named by `IBreadcrumbHomeText` for the request's language (`Osirion:Site:Labels:Home` with
  `AddOsirion`), then `SeoMetadataOptions.BreadcrumbHomeText`, then `Home`; a `BreadcrumbHomeText` parameter wins.

## Robots and site structured data

- `Robots` writes documented robots directives (`noindex, follow`, `max-snippet:50`, ...) as `robots`, `googlebot` and
  `bingbot`; front matter `robots` comes first, `AllowSearchIndexing=false` still writes `noindex, nofollow`, and an
  invalid value keeps the default and logs a warning.
- `SeoMetadataOptions.EnableSiteSchemas` writes the site's `WebSite` (`{SiteUrl}/#website`, with a `SearchAction`
  from `SearchUrlTemplate`) and `Organization` (`{SiteUrl}/#organization`, with `SameAs`) on the home page or a
  culture's root, and makes every page's `publisher` and a web page's `isPartOf` references to them. The home page
  drops its own `WebSite` and `Organization` schema types. Off by default.

## Pages that are not content items

- `Title`, `Description`, `ImageUrl` and `CanonicalPath` give a Razor page (no `ContentItem`) the full head: title,
  description, canonical, Open Graph (`og:type` `website`), Twitter, a JSON-LD `WebPage` (or the `ContactPage` or
  `AboutPage` named by `SchemaTypes` or `FallbackSchemaType`) and the `hreflang` links. No author, dates or article tags.
- They apply only when `Title` is set and neither `Content` nor `ContentItems` is; with `Content` or `ContentItems` they
  are ignored. `CanonicalPath` is a path on the site or an absolute http(s) URL; anything else is ignored and logged.
- Flat parameters were chosen over a page model to match `CollectionTitle` and `CollectionDescription`.
- Blazor renders only the last `HeadContent` of a page, so all head tags go through this one renderer.
