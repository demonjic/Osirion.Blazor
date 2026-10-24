# OsirionContentNotFound

Purpose
Configurable empty-state/404 component with actions, suggestions, search slot and optional background pattern.

Key parameters
- ErrorCode (e.g., 404), Title, Message
- ShowIcon, UseDefaultIcon, Icon (RenderFragment) or IconUrl/IconAlt
- PrimaryButtonText/Url, SecondaryButtonText/Url, Actions (RenderFragment)
- ShowSuggestions, SuggestionsTitle, Suggestions: List of { Text, Url, Description?, Target }
- ShowSearchBox, SearchContent
- ShowContactInfo, ContactMessage, ContactEmail
- Variant: default, minimal, centered, hero
- Size: small, medium, large, full
- BackgroundPattern

Content Security Policy
- The button links drop their underline through the class `osirion-content-not-found-button`; the component writes no `style` attribute of its own.
