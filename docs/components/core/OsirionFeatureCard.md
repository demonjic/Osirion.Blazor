# OsirionFeatureCard

Purpose
Feature card with image, headings, description, read-more and category-style pillow buttons. Highly themeable.

Key parameters
- Title, Description, ShowDescription, ChildContent
- Url (the read more link's target, and the image link; see Notes)
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
- Without ImageUrl the card has no image column and no image link (`osirion-feature-card-without-image`, or
  `osirion-feature-card-with-icon` with an Icon); up to 4.4.0 it drew a placeholder, inside a link without a name
  when Url was set.
- Every link has a name: the image is a link only when its alternative text is not empty, and not when the read
  more link goes to the same place; a pillow button without text or an aria label is left out.
