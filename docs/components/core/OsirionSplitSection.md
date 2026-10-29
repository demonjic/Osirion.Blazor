# OsirionSplitSection

Purpose
Copy beside an aside, such as text and a list next to an image, a card or a sample record: two equal columns from
768px, and below that the aside stacked under the copy. It renders without script.

Key parameters
- `Content`: the copy; it comes first in reading order
- `Aside`: the block beside the copy
- `AsidePosition`: `SplitAsidePosition.End` (default, on the right in a left-to-right page) or `Start`
- `Class`, `Style` and unmatched attributes go on the root `div`

Usage
```razor
<OsirionBaseSection Id="how-it-works" Eyebrow="How it works" Title="Every decision keeps its reasons">
    <OsirionSplitSection AsidePosition="SplitAsidePosition.Start">
        <Content><p>Each request is checked against your rules.</p></Content>
        <Aside><img src="/images/rules.png" alt="The rules editor" width="640" height="400" /></Aside>
    </OsirionSplitSection>
</OsirionBaseSection>
```

Notes
- The copy is always first in the markup; `AsidePosition` only moves the aside's column, so reading and keyboard
  order stay copy then aside.
- With one slot filled, it spans both columns.
- Classes: `osirion-split-section` (plus `osirion-split-section-aside-start`), `osirion-split-section-content`,
  `osirion-split-section-aside`.
