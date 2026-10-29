# OsirionFaq and OsirionAccordion

Purpose
`OsirionAccordion` is a list of collapsible sections built on the native `<details>` element, so it opens and closes
without script and is keyboard accessible as is. `OsirionFaq` renders questions and answers with it and writes
FAQPage structured data (schema.org) that describes them to search engines and other readers of the page. Google no
longer shows FAQ rich results (since May 2026), so the data does not change how the page looks in Google results.

Key parameters
- OsirionAccordion: `AllowMultiple` (default true; false gives the items a shared `name`, so the browser closes the
  others when one opens), `ChildContent`, `Class`
- OsirionAccordionItem: `Title` (required), `Open` (default false), `ChildContent`, `Class`; works inside an
  accordion or on its own
- OsirionFaq: `Items` (`IReadOnlyList<FaqItem>`, a `FaqItem(Question, Answer)` record in `Osirion.Blazor.Core.Models`),
  `IncludeStructuredData` (default true), `AllowMultiple` (default true), `AnswerTemplate`
  (`RenderFragment<FaqItem>?`: renders each answer in place of the plain-text paragraph, for formatted answers such as
  sanitized Markdown; the structured data still uses `FaqItem.Answer`, so give it the plain text), `Class`

Usage
```razor
<OsirionFaq Items="faq" AllowMultiple="false" />

@code {
    private readonly FaqItem[] faq =
    [
        new("Does it need JavaScript?", "No. Every component renders with static server-side rendering."),
        new("Which frameworks are supported?", "Bootstrap, MudBlazor, Fluent UI and Radzen."),
    ];
}
```

Notes
- Answers are plain text; line breaks in the text are kept (`white-space: pre-line`). `AnswerTemplate` renders them
  another way; markup it writes is not sanitized by the component, so sanitize content before it reaches the template.
- The JSON-LD is built with System.Text.Json and its default encoder, which escapes `<`, `>` and `&`, so a question or
  answer cannot end the script element. Text that cannot be written as JSON (half of a surrogate pair) leaves out the
  structured data and still renders the questions.
- Turn `IncludeStructuredData` off when the page already describes these questions in its own structured data; Google
  expects one FAQPage per page.
- Exclusive accordions (`AllowMultiple="false"`) need a browser that supports `<details name>`; older browsers let
  several items stay open.
