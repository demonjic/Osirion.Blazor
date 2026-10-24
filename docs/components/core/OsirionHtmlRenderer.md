# OsirionHtmlRenderer

Purpose
Render HTML content with optional Prism-based code highlighting, copy button, and line highlighting.

Key parameters
- HtmlContent (string)
- EnableSyntaxHighlighting (default true)
- ShowLineNumbers, EnableCopyButton, EnableLineHighlighting
- UseAccessibleTheme (WCAG-friendly): the theme is in css/osirion-html-renderer.min.css under the article's
  osirion-wcag-theme class (rules wrapped in :where(), so palettes still outrank them), not an inline style element;
  the class is added only when syntax highlighting is on
- LanguageClasses (obsolete, never read by the component; style code blocks with CSS selectors on the language-* classes)
- PrismBaseUrl (optional base address Prism is served from; empty uses the site-wide PrismOptions.BaseUrl, and without
  that the pinned cdnjs release 1.29.0 with Subresource Integrity)
- SanitizeHtml (default true): sanitize with the registered IHtmlContentSanitizer allow-list
- HtmlSanitizer (optional delegate that replaces the registered sanitizer)

Implementation notes
- Detects <pre><code> blocks and reads their language- class (on the code or pre element) to load the matching
  Prism language; the block markup itself is not rewritten
- Wraps blocks with header and copy button when enabled
- Adds the line-numbers class to the <pre> of each block if requested, keeping its other classes
- Loads Prism, its language components and the line-numbers plugin in order through osirion-script-loader.js, which
  sets integrity and crossorigin on the pinned cdnjs files (hashes from https://api.cdnjs.com/libraries/prism/1.29.0?fields=sri).
  Files from a custom PrismBaseUrl carry no integrity check
- services.AddOsirionPrism(options => options.BaseUrl = "/lib/prism/1.29.0/") sets the Prism folder for every renderer,
  including those inside the CMS content components; BaseUrl must be a path on the site or an https address (checked
  at startup), and the PrismBaseUrl parameter wins over it

Security
- Content is sanitized by default with an allow-list HTML sanitizer: script, event handlers, javascript:/data: URLs,
  forms, iframes, inline styles and style elements are removed; headings keep their id and code blocks keep their
  language class.
- Sanitization runs before code blocks are wrapped, so the copy button handler added by the component is kept.
- A custom HtmlSanitizer delegate must escape < and > inside attribute values, as the default sanitizer does:
  the copy button wrapper and ShowLineNumbers edit the sanitized markup with text patterns.
- Configure the allow-list with services.AddOsirionHtmlSanitizer(...), for example to allow a custom element.
- Set SanitizeHtml="false" only for markup the application itself produced.
