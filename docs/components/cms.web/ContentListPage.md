# ContentListPage

Purpose
A whole list page in one line: a blog index, a category page, a tag page or an author's page. It opens with the
title as the page's only `h1` and a short description, lists the items in cards with pagination, writes
CollectionPage structured data, and links the feed when there is one. It renders without script.

Usage
```razor
@page "/blog"
<ContentListPage Directory="blog" Title="Blog" Description="Guides and release notes." FeedUrl="/feed.xml" />

@page "/tag/{Tag}"
<ContentListPage Tag="@Tag" ShowTags />

@code {
    [Parameter] public string? Tag { get; set; }
}
```

Key parameters
- Filter: `Directory`, `Category`, `Tag`, `Author`, `Locale`
- Heading: `Title` and `Description`; without them both are built from the filter (`Tag: blazor`, the directory's
  name), the same text the title tag and meta description use
- `HeaderContent` (`RenderFragment`): the page's own header above the list, in `header.osirion-content-list-page-header`,
  in place of the title and description, for example the title with a strip of series links. It must render the
  page's `h1`; `Title` and `Description` still name the page in its head metadata:

  ```razor
  <ContentListPage Directory="blog" Title="Blog">
      <HeaderContent>
          <h1>Blog</h1>
          <nav aria-label="Series">
              <a href="/series/build-a-blog">Build a blog</a>
          </nav>
      </HeaderContent>
  </ContentListPage>
  ```

- List: `ShowPagination` (true), `ItemsPerPage` (12), `SortBy`, `SortDirection`, `NoContentText`,
  `ContentUrlFormatter`, `PaginationUrlFormatter`, `WebsiteName`
- Items: `ItemTemplate` (`RenderFragment<ContentItem>`) renders each item in an `ol.osirion-content-index` instead of
  the built-in cards, for a text-first index such as `<ItemTemplate><MyRow Item="context" /></ItemTemplate>`; paging,
  the `h1`, the feed and the structured data stay the same
- Feed: `FeedUrl` (for example `/feed.xml` from `MapOsirionFeeds`) adds `<link rel="alternate">` to the head and a
  link below the list; `FeedTitle` names both
- Sidebar: `ShowCategories` and `ShowTags` (both false) list them beside the list from 992px up, with the current
  category or tag marked; `CategoriesTitle`, `TagsTitle`, `CategoryUrlFormatter`, `TagUrlFormatter`, `SidebarLabel`

Notes
- The heading comes from `ContentList.ShowTitle`, so a page that composes `ContentList` itself can turn it on to get
  the same `h1`.
- The list keeps its `h1` when nothing matches and shows `NoContentText` below it. When a category, tag or author is
  set and no item matches, the response is HTTP 404 (`ContentList.NotFoundWhenFilterMatchesNothing`); on .NET 10 the
  router then renders its not-found page, if it has one. A directory with nothing in it stays a normal page.
