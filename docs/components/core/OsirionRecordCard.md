# OsirionRecordCard

Purpose
A framed sample record, such as an order, a ticket or a decision, for a page that shows what a product produces: a
title, a status with a tone, labelled rows and a caption. It renders a `figure` with a description list, without
script.

Key parameters
- `Title` and `HeadingLevel` (`1` to `6`, default `3`)
- `Status`: the status text, shown as a badge
- `StatusTone`: `RecordStatusTone.Neutral` (default), `Positive`, `Negative` or `Warning`
- `Rows`: `IReadOnlyList<RecordCardRow>`, where `RecordCardRow(string Label, string Value)` lives in
  `Osirion.Blazor.Core.Models`
- `Footnote`: a short note such as `Sample data`, rendered as the `figcaption`
- `Class`, `Style` and unmatched attributes go on the `figure`

Usage
```razor
<OsirionRecordCard Title="Order A-1042"
                   Status="Approved"
                   StatusTone="RecordStatusTone.Positive"
                   Rows="@([new RecordCardRow("Customer", "Example Ltd"), new RecordCardRow("Amount", "1,250.00")])"
                   Footnote="Sample data" />
```

Notes
- The status is text; the tone only colors the badge (`--osirion-status-success-*`, `-danger-*`, `-warning-*`), so
  the meaning never rests on color alone. The dot in the badge is `aria-hidden`.
- Rows are a `dl`, each `dt` and `dd` grouped in a `div`; below 480px the label sits above its value.
- With no title, status, rows or footnote, nothing is rendered. All text is HTML-encoded.
