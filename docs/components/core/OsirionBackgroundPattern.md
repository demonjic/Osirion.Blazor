# OsirionBackgroundPattern

Purpose
Apply decorative background pattern with optional mask overlay to any section.

Key parameters
- BackgroundPattern: BackgroundPatternType?
- MaskImage: bool (default true)

Notes
- Emits osirion-bg-wrapper and osirion-bg-image-mask classes
- Combine with section or hero components for non-intrusive visual depth.
- The dark look applies under `data-osirion-theme="dark"` (set by `OsirionStyles`), `data-bs-theme="dark"` (added
  under Bootstrap) or `data-theme="dark"` (a component's `Theme` parameter) on the page or a parent element.
