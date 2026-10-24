# OsirionReadMoreLink

Consistent CTA link with optional icon, variants, sizes, and stretched behavior for cards.

Basic usage

```razor
@using Osirion.Blazor.Components

<OsirionReadMoreLink Href="/docs" Text="Read more" />
```

Variants and options

```razor
<OsirionReadMoreLink 
    Href="/download"
    Text="Download"
    Variant="ReadMoreVariant.Download"
    Size="LinkSize.Large"
    IconPosition="IconPosition.Left" />
```

Notes

- Stretched links for clickable cards.
- Framework-aware classes adapt to Bootstrap, MudBlazor, etc.
- Without `Style` the link has no `style` attribute at all (it wrote an empty `style=""` before), so a strict Content Security Policy needs nothing for it.
