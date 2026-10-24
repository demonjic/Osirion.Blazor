Purpose
-------
Render a theme toggle (light/dark) control and initialize client-side theme JavaScript. Provides accessible markup.

Parameters
----------
- `Id` (optional): id of the button. No id is written by default, so several toggles can share a page.
- `OnThemeChanged` (optional): raised with `light` or `dark` after a switch, only when the toggle is interactive. Under Static SSR it is never raised; listen for the `osirion-theme-update` browser event instead.
- State and configuration come from `IThemeService` and `ThemingOptions`.

Example
-------
```razor
<ThemeToggle />
```

Notes
-----
- The component loads a script file that handles toggling through a click listener; it writes no inline script and no `onclick` attribute, so a strict Content Security Policy does not block it. The button needs JavaScript; there is no server-side fallback form.
- For customization, update `ThemingOptions` or the theme service implementation.
- Keyboard focus draws the focus ring every component draws, from `--osirion-focus-ring-width`,
  `--osirion-focus-ring-color` and `--osirion-focus-ring-offset` (falling back to `--osirion-primary-color`), without
  `!important`, so a site that sets the tokens once restyles this ring too. Just inside it the button's own 2px ring
  shows in `--osirion-primary-color` at full strength, so focus stays visible when a site sets the token to a
  faint color.
- The wrapper's padding is `--osirion-theme-toggle-padding` (10px). The `Menu` drawer sets it to `0` on its actions,
  so the toggle lines up with the menu items; set it yourself in any container that spaces its children.
