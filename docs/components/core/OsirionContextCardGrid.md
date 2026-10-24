# OsirionContextCardGrid

Purpose
Shows a section with a heading, optional intro text and a grid of labeled detail cards. Each card has a label, a short
description and an accent color on its top border.

Key parameters
- Title: string, default "Context by category". The section heading, at `HeadingLevel`
- HeadingLevel: int, default 2. The level of the section heading, 1 to 6; each card label is a heading one level
  below, up to `h6`. A value outside the range moves to the nearest level
- Description: string, optional. Supporting text under the heading
- Items: `OsirionContextCardGrid.ContextCardItem[]`, default empty. The cards; the card grid is omitted when the array
  is empty
- HeadingId: string, optional. The id of the heading, referenced by the section's `aria-labelledby`. Without a value
  each grid generates its own
- Id: string, optional. The section's `id`, for in-page links
- SectionClass: string, optional. Extra CSS classes on the section
- ContainerClass: string, default "container". CSS classes on the inner content container
- Theme: `ThemeMode` (default `System`), inherited from `OsirionComponentBase`. `Light`, `Dark` and `Inverted` add a
  `data-theme` attribute (`data-bs-theme` when the Bootstrap framework is configured)
- Class and Style: strings, inherited from `OsirionComponentBase`. The component's markup does not read them
- Attributes: captures unmatched attributes (`CaptureUnmatchedValues`) and renders them on the section

`ContextCardItem` is a record: `ContextCardItem(string Label, string Description, string Accent = "green")`.

Usage
```razor
<OsirionContextCardGrid Title="Context by role"
                        Description="Each role sees the same data through its own priorities."
                        Id="roles"
                        HeadingId="roles-heading"
                        Items="@roleItems" />

@code {
    private readonly OsirionContextCardGrid.ContextCardItem[] roleItems =
    [
        new("Finance", "Cost variance and forecast predictability.", "blue"),
        new("Operations", "Coverage reliability and exception response."),
        new("Security", "Access reviews and audit evidence.", "purple")
    ];
}
```

Notes
- Emits `section.osirion-context-card-grid` with `aria-labelledby` pointing at the heading (`h2` by default) inside
  `div.osirion-context-card-grid-heading`. The cards sit in `div.osirion-context-card-grid-items`, and each is an
  `article.osirion-context-card-grid-item.osirion-context-card-grid-item-{accent}` with a heading one level below
  (`h3` by default, the label) and a `p` (the description).
- The accent string is used as written in the class name. It colors the card's top border: the stylesheet defines
  `blue`, `purple`, `indigo`, `teal` and `amber`; any other value, including the default `green`, uses
  `--osirion-context-card-grid-accent` (default `#176b45`).
- The card label takes the card's text color, `--osirion-context-card-grid-item-heading-color`, falling back to
  `--osirion-context-card-grid-item-color` (default `#555`, 7.46:1 on the default white card). Up to 4.2.0 it took the
  accent color, which measured 2.2:1 to 4.3:1 on a card set to the dark background of any shipped palette. A site
  that changes `--osirion-context-card-grid-card-background` sets `--osirion-context-card-grid-item-color` with it,
  and the label follows.
- `--osirion-context-card-grid-heading-align` aligns the heading and description: `start`, `center` (the default)
  or `end`. It moves both the text and its 52rem-wide lines, so `start` lines them up with the cards.
- Works in static SSR; the component has no script.
- The cards fill the width with `repeat(auto-fit, minmax(min(100%, 15rem), 1fr))`.
- Colors and spacing can be overridden with CSS variables such as `--osirion-context-card-grid-background`,
  `--osirion-context-card-grid-card-background` and `--osirion-context-card-grid-card-shadow`.
- There is no reduced-motion handling because the component has no animation.
