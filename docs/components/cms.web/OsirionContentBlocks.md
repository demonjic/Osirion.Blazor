# OsirionContentBlocks

Purpose
Renders content blocks, in order, each with the renderer registered for its type: a page written as front matter
(`blocks:`) instead of a Razor page. `ContentRenderer` and `LandingPage` use it for an item with blocks; use it directly
to place blocks in a page of your own. The block types, their keys and the validation rules are in the content blocks
guide (`/docs/content-blocks`, source `content/en/docs/content-blocks.md`).

Key parameters
- Blocks: `IReadOnlyList<ContentBlock>`, usually `ContentItem.Blocks`; renders nothing when empty
- SourcePath: `string?`, the item's path (`ContentItem.Path`), named in logs and in Development notices
- IncludeFaqStructuredData: `bool`, default `true`; the first `faq` block writes FAQPage structured data. Turn it off
  when the page writes its own FAQPage
- Class, Style and other attributes go on the wrapping `div.osirion-content-blocks`

Usage
```razor
@if (item.HasBlocks)
{
    <OsirionContentBlocks Blocks="@item.Blocks" SourcePath="@item.Path" />
}
```

Registration (optional)
```csharp
builder.Services.AddOsirionContentBlocks(blocks => blocks
    .AddRenderer<PricingTableRenderer>("pricing-table")
    .AddComponent<ProductTour>("product-tour")
    .AddIcon("shield", ShieldIcon)
    .UseFallbackRenderer<UnknownBlockRenderer>());
```
Without `AddOsirionContentBlocks` the library's renderers are used (`ContentBlockRegistry.Default`). A renderer derives
from `ContentBlockRenderer<TBlock>` (its block is `Model`, the page context is `Context`); a component registered with
`AddComponent` receives the block's `parameters` through `[Parameter] IReadOnlyDictionary<string, string> Parameters`.

Notes
- Nothing from the content is written as raw HTML. Inline Markdown (descriptions, subtitles, captions) and block
  Markdown (`markdown`, answers) go through the registered `IHtmlContentSanitizer`; titles and button texts are plain
  text.
- Site paths in links are localized with `IOsirionLocalePaths` for the culture of the request when it is registered,
  and left as written otherwise.
- A block that cannot render (a type without a renderer and no fallback, an unknown component name) logs an error and
  renders nothing; in Development (`IHostEnvironment.IsDevelopment()`) it renders a `div.osirion-content-block-problem`
  with `role="alert"` naming the file, the block and the reason.
- A card's `icon` names an icon registered with `AddIcon(name, RenderFragment)`; an unregistered name shows no icon.
- Works with static server-side rendering; no script.
