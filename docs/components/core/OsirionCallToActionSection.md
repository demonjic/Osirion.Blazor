# OsirionCallToActionSection

Purpose
The closing call to action at the end of a page: a heading, a short text and up to two links styled as buttons. It
renders a `<section>` labelled by its heading, without script, and the buttons take the classes of the configured
CSS framework.

Key parameters
- `Title`: the heading; it names the section through `aria-labelledby` with a generated id
- `HeadingLevel`: `1` to `6`, default `2`; a value outside the range moves to the nearest level
- `Description`: the paragraph under the heading
- `ChildContent`: after the description and before the buttons
- `PrimaryButtonText` and `PrimaryButtonUrl`, `SecondaryButtonText` and `SecondaryButtonUrl`: each button is
  rendered only with a label and a safe URL
- `Class`, `Style` and unmatched attributes go on the `section`

Usage
```razor
<OsirionCallToActionSection Title="Ready to build your site?"
                            Description="Install the package and publish your first page."
                            PrimaryButtonText="Get started"
                            PrimaryButtonUrl="/docs/getting-started"
                            SecondaryButtonText="Browse components"
                            SecondaryButtonUrl="/components" />
```

Notes
- A button URL must be a path starting with a single `/` (not `//` or `/\`), or an absolute http, https or mailto
  address. Any other URL, such as `javascript:` or a protocol-relative `//host`, leaves that button out.
- The buttons get `GetPrimaryButtonClass()` and `GetSecondaryButtonClass()` (`btn btn-primary` and
  `btn btn-secondary` under Bootstrap) plus `osirion-call-to-action-button` and a `-primary` or `-secondary` hook.
  Those classes come from `ButtonClasses.Primary` and `ButtonClasses.Secondary`, which `HeroSection` uses too.
- With no title, description, child content or renderable button, nothing is rendered.
- Title, description and button labels are plain text and HTML-encoded.
- `LandingPage` in Osirion.Blazor.Cms.Web still renders its own CTA block with the `osirion-landing-cta` classes.
