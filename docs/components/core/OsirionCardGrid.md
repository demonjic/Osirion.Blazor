# OsirionCardGrid

Purpose
Lays cards, or any other blocks, out in equal columns that collapse on narrow screens. It renders a plain `div` grid
without script.

Key parameters
- `ChildContent`: the cards, one per grid cell
- `Columns`: `1` to `4`, default `3`; a value outside the range moves to the nearest one
- `Class`, `Style` and unmatched attributes go on the grid `div`

Usage
```razor
<OsirionCardGrid Columns="3">
    <OsirionFeatureCard Title="Fast pages" Description="HTML on the first request." />
    <OsirionFeatureCard Title="Themed by tokens" Description="One set of colors for every framework." />
    <OsirionFeatureCard Title="Accessible markup" Description="Landmarks, headings and labels in place." />
</OsirionCardGrid>
```

Notes
- `Columns` columns from 992px, at most two from 640px, one below.
- The column count is a class (`osirion-card-grid-columns-1` to `-4`), so the grid writes no inline style.
- The gap is `--osirion-spacing-6`.
