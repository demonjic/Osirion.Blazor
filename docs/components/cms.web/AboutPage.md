# AboutPage

Purpose
A whole about page from one content item, `about.md` (slug `about`) by default. The title is the page's `h1`, in an
`OsirionPageHeading` or, with `ShowHero`, a hero, with the description below it, then the Markdown body and whatever `ChildContent`
adds. It writes AboutPage structured data with the site's organization as its main entity, and answers 404 when the
content is missing. It renders without script.

Usage
```razor
@page "/about"
<AboutPage>
    <OsirionAuthorCard Name="Ana Author" Role="Maintainer" ProfileUrl="/authors/ana" />
</AboutPage>
```

Key parameters
- Content: `ItemSlug` (default `about`), `ItemPath`, `ItemUrl` or `Item`; `Locale`
- Heading: an `OsirionPageHeading` by default; `ShowHero` (false here) shows a `HeroSection` styled by the hero
  parameters of the page base instead
- `HeaderContent` (`RenderFragment<ContentItem>`): the page's own header above the body, in place of the heading or
  the hero, for example the title with a call to action. It must render the page's `h1`:

  ```razor
  <AboutPage>
      <HeaderContent>
          <h1>@context.Title</h1>
          <p>@context.Description</p>
          <a class="btn btn-primary" href="/contact">Book a call</a>
      </HeaderContent>
  </AboutPage>
  ```

- `ChildContent`: shown after the body, for example author cards
- Width: the header (or the default heading), the body and the child content are centered at
  `--osirion-about-page-width`, which defaults to `--osirion-container-lg` (960px when unset), so the h1 stays in line
  with the body. The hero, while `ShowHero` is on, keeps its own width. Set the property on the page, for example
  `.osirion-about-page { --osirion-about-page-width: 1140px; }`
- SEO: `SchemaTypes` (default `AboutPage`, plus a BreadcrumbList while `GenerateMultipleSchemas` is on),
  `WebsiteName`, `OrganizationName`, `SiteLogoUrl`, `AlternateLanguageUrls`, `SiteDescription`, `TwitterSite`, `TwitterCreator`, `FacebookAppId`, `DefaultImageUrl`, `DefaultImageWidth`, `DefaultImageHeight`, and the switches `AllowAiDiscovery`, `AllowAiTraining`, `AllowSearchIndexing`, `EnableGeoOptimization` and `EnableAeoOptimization` (each passed on only when you set it, so a switch you leave out keeps the configured value)
- Not found: `NotFoundTitle`, `NotFoundText`, and a button to `BackLinkUrl` labeled `BackLinkText` unless `ShowBackLink` is off

Notes
- An opening `# Title` in the Markdown is left out, since the heading already shows the title.
