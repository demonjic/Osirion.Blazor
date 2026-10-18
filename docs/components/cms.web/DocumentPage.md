# DocumentPage

Purpose
A documentation page: side navigation, breadcrumbs, the title and its metadata, an "On this page" table of contents,
the body, an edit link and previous and next links. It renders without script.

Usage
```razor
@page "/docs/{*Slug}"
<DocumentPage ContentPath="@($"docs/{Slug}.md")"
              EditUrlFormatter="@(item => $"https://github.com/owner/repo/edit/main/content/{item.Path}")" />
```

Key parameters
- Content: `ContentPath` or `Content`
- Side navigation: `ShowSideNavigation` (true), `NavigationTitle`, `NavigationItems`; without `NavigationItems` it
  lists the pages of the document's folder in their `order`, with the current page marked. On screens 768 pixels
  wide or less it folds behind a button labelled `SideNavigationToggleText` ("Documentation menu"), so the page starts
  with its content
- `ShowMetadata` (true): author and "Last updated", the front matter `last_modified` date or the publication date
- `ShowTableOfContents` (true), `TableOfContentsTitle`, `TocMinLevel`, `TocMaxLevel`, `TocShowNumbers` (true)
- `EditUrlFormatter`: builds the address of the page's source; when it returns one, an `EditLinkText` link follows
  the body
- `ShowNavigationLinks` (true): previous and next pages of the folder, linked by their URLs (`ContentUrlFormatter`
  overrides)
- SEO: `SchemaTypes` (default `TechArticle`, plus a BreadcrumbList while `GenerateMultipleSchemas` is on),
  `WebsiteName`, `OrganizationName`, `SiteLogoUrl`, `SiteDescription`, `TwitterSite`, `TwitterCreator`, `FacebookAppId`, `DefaultImageUrl`, `DefaultImageWidth`, `DefaultImageHeight`, and the switches `AllowAiDiscovery`, `AllowAiTraining`, `AllowSearchIndexing`, `EnableGeoOptimization` and `EnableAeoOptimization` (each passed on only when you set it, so a switch you leave out keeps the configured value)
- Not found: `NotFoundTitle`, `NotFoundText`, and a button to `BackLinkUrl` labeled `BackLinkText` unless `ShowBackLink` is off

Notes
- The page body is a `div`; the layout's `main` stays the page's only main landmark.
