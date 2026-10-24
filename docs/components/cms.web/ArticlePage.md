Purpose
-------
Page-level component used to render article detail pages. Composes metadata, hero, content, and optional sidebar sections.

Parameters
----------
- Many parameters are forwarded from `OsirionContentDetailPageBase` such as `Item`, `IsLoading`, `ShowHero`, `ShowSidebar`, `ShowBreadcrumbs`, and more.

Example
-------
```razor
<ArticlePage Item="@article" ShowSidebar="true" />
```

Notes
-----
- See `src/Osirion.Blazor.Cms.Web/Components/PageLevel/ArticlePage.razor` for the full parameter list and behaviour.

- The component supports `ShowRelatedContent`, `ShowNavigation`, and slotting of `SidebarContent` for related widgets.

- `ShowSeriesNavigation` (true) shows `ContentSeriesNavigation` above the body for an article with `series` front
  matter; nothing renders for other articles.

- `AuthorContent` (`RenderFragment<ContentItem>`) is shown after the body, for example
  `<AuthorContent><OsirionAuthorCard Name="@context.Author" /></AuthorContent>`. `CustomFooterContent` (HTML,
  sanitized) follows it. `RelatedContentTitle` names the related section.

- `HeaderContent` (`RenderFragment<ContentItem>`) replaces the hero with the site's own article header, for example a
  title, a lede and a metadata block. It must render the page's `h1`; an opening `# Title` line in the Markdown is
  left out of the body, as it is under the hero.

- `FooterContent` (`RenderFragment<ContentItem>`) is shown after the article and its sidebar, outside the article
  column, in `footer.osirion-article-page-footer`; nothing renders without it. Use it for a closing call to action
  or related links:

  ```razor
  <ArticlePage ItemSlug="@Slug" DirectoryName="blog">
      <FooterContent>
          <p>Questions about @context.Title? <a href="/contact">Get in touch</a>.</p>
      </FooterContent>
  </ArticlePage>
  ```

- Below 640px the sidebar hides its table of contents, and the whole sidebar takes no space when it has no other
  section (categories, tags or `SidebarContent`). For an article without headings, the table of contents section is
  hidden at every width, since `TableOfContents` renders nothing.

- With `ShowHero` (the default) the hero shows the title as the page's `h1`, so an opening `# Title` line in the
  Markdown is left out of the body. `HomePage` and `LandingPage` do the same while they show their hero, and
  `ContentView`, `DocumentPage` and `LocalizedContentView` (with its header) do it under their own title heading.
