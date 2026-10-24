# TableOfContents

Generate a table of contents from headings in rendered content. Supports depth filtering and anchor linking.

When the content has no headings it renders nothing, so a sidebar shows no empty outline. Set `EmptyText` to render
the `nav` with that text instead:

```razor
<TableOfContents Content="@Item?.Content" EmptyText="No headings found." />
```
