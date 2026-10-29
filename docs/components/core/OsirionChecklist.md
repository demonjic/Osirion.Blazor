# OsirionChecklist

Purpose
Short phrases, each behind a check mark and with an optional line of description, under an optional heading. It
renders a plain `<ul>` without script.

Key parameters
- `Title`: the heading; it names the list through `aria-labelledby` with a generated id
- `HeadingLevel`: `1` to `6`, default `3`; a value outside the range moves to the nearest level
- `Items`: `IReadOnlyList<ChecklistItem>`, where `ChecklistItem(string Title, string? Description = null)` lives in
  `Osirion.Blazor.Core.Models`
- `Class`, `Style` and unmatched attributes go on the root `div`

Usage
```razor
<OsirionChecklist Title="Included in every plan"
                  Items="@([new ChecklistItem("Static SSR", "Every page arrives as complete HTML."), new ChecklistItem("Light and dark themes")])" />
```

Notes
- The list has `role="list"`, so it keeps its list semantics without bullets.
- Each check mark is an inline `svg` with `aria-hidden="true"` and `focusable="false"`.
- With no title and no items, nothing is rendered. All text is HTML-encoded.
- `HeroSection.Highlights` and `OsirionFeatureCard.Items` render through this component.
- Classes: `osirion-checklist`, `osirion-checklist-title`, `osirion-checklist-list`, `osirion-checklist-item`,
  `osirion-checklist-mark`, `osirion-checklist-text`, `osirion-checklist-item-title`,
  `osirion-checklist-item-description`.
