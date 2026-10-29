# OsirionFeatureCard

Purpose
Feature card with image, headings, description, read-more and category-style pillow buttons. Highly themeable.

Key parameters
- Title, Description, ShowDescription, ChildContent
- Url (card-level link)
- ImageUrl, ImageAlt, ImageLazyLoading, ImageTransformOnHover, ImagePosition (Top, Bottom, Left, Right)
- CardSize (XXS..XXL), ContentAlignment, ContentFontSize
- BorderTiming, BorderColor; ShadowTiming, ShadowColor; TransformTiming
- ShowReadMoreLink, ReadMoreText, ReadMoreHref, ReadMoreStretched
- ShowPillowButtons, PillowButtons
- Icon (from 4.3): a decorative `RenderFragment` above the title, in `osirion-feature-card-icon` with `aria-hidden`;
  without an ImageUrl it replaces the image column and the card takes `osirion-feature-card-with-icon`
- Items (from 4.3): short phrases rendered as an `OsirionChecklist` after the description (`osirion-feature-card-items`)

Notes
- Emits osirion-feature-card plus many BEM modifiers for size/effects
- ReadMoreHref takes precedence over Url for the CTA
- Provides sensible image placeholders when ImageUrl is missing.
