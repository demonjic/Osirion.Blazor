# MenuItem

Purpose
A single entry of a `Menu`: a link with an optional icon and description, which can also own a submenu (a regular
list or a mega panel). Open and closed state of the submenu is kept in a visually hidden checkbox, so it works in
static SSR without script.

Key parameters
- Text: the item text (default: empty string)
- Description: optional supporting text under the item text
- Icon: name of a built-in icon, matched case-insensitively: `home`, `blog`, `document`, `docs`, `documentation`,
  `user`, `profile`, `contact`, `about`, `info`, `settings`, `gear`, `dashboard`. Any other name renders no icon
- IconTemplate: custom icon, a `RenderFragment`; it is used instead of `Icon` when both are set
- Href: destination URL (default: `"#"`). A value whose scheme is not `http`, `https`, `mailto` or `tel` (for example
  `javascript:`) is not rendered, and neither is the `href` of a disabled item
- IsActive: marks the item active (default `false`)
- AutoDetectActive: also marks the item active when the current URL matches `Href` (default `true`)
- Match: `NavLinkMatch.All` (default) compares the whole path; `NavLinkMatch.Prefix` also matches paths below `Href` on
  a segment boundary. Query string, fragment and leading or trailing slashes are ignored. An empty or `"#"` `Href`
  never matches
- Disabled: the item renders without `href` and `OnClick` is not invoked (default `false`)
- Target: how to open the link, for example `_blank`; not rendered when the item has no `href`
- OnClick: `EventCallback<MouseEventArgs>` invoked when the link is clicked and the item is not disabled
- HasSubmenu: renders the submenu container and its toggle (default `false`)
- ToggleSubmenuLabel: the toggle's accessible name, with `{0}` for `Text` (default: `"Toggle {0} submenu"`). Set it in
  the page's language, so the name matches it (WCAG 3.1.2); `OsirionSiteLayout` passes `Osirion:Site:Labels:ToggleSubmenu`
- ChildContent: the submenu content (`MenuItem`, `MenuGroup`, `MenuCallToAction` and so on)
- SubmenuVariant: `SubmenuVariant.Regular` (default, a compact vertical list) or `SubmenuVariant.Mega` (grouped content
  in a responsive grid)
- SubmenuAside: content for a dedicated aside column; rendered only when `SubmenuVariant` is `Mega`, typically a
  `MenuPanel` or `MenuImageCard`
- Id: base identifier for the submenu ids (default: eight characters of a GUID generated once per instance)
- Class: extra CSS classes, added to the link (or to the label of a toggle-only item), not to the wrapper (inherited
  from `OsirionComponentBase`)
- Style: inherited from `OsirionComponentBase`, written on the root element
- Theme: `ThemeMode.System` (default), `Light` or `Dark`; a non-System value is written as `data-theme` (or
  `data-bs-theme` when the Bootstrap framework is configured) on the wrapper (inherited)
- Attributes: any other attribute, applied to the wrapper `div` (inherited)

Usage
```razor
<Menu BrandText="Contoso">
    <MenuItem Text="Home" Icon="home" Href="/" />

    <MenuItem Text="Platform" HasSubmenu SubmenuVariant="SubmenuVariant.Mega">
        <ChildContent>
            <MenuGroup Label="Core platform">
                <MenuItem Text="Tracking" Description="Measurement down to ad level." Href="/platform/tracking" />
            </MenuGroup>
        </ChildContent>
        <SubmenuAside>
            <MenuImageCard ImageUrl="/images/customer.jpg" ImageAlt="A customer team" Title="Customer stories"
                           Href="/customers" />
        </SubmenuAside>
    </MenuItem>

    <MenuItem Text="Guides" HasSubmenu>
        <MenuItem Text="Getting started" Href="/guides/start" />
        <MenuItem Text="Integrations" Href="/guides/integrations" Match="NavLinkMatch.Prefix" />
    </MenuItem>

    <MenuItem Text="Docs" Href="https://docs.example.com" Target="_blank" />
</Menu>
```

Notes
- Root: `div.osirion-menu-item-wrapper`, plus `osirion-menu-item-has-submenu` when `HasSubmenu`,
  `osirion-menu-item-has-mega` when `HasSubmenu` and `Mega`, and `osirion-menu-item-wrapper-active` when active.
- Link: `a.osirion-menu-item` with `osirion-menu-item-active` when active, `osirion-menu-item-disabled` when
  disabled, then `Class`. It always carries `aria-disabled` (`"true"` or `"false"`) and `aria-current="page"` when
  active. Without an `href` (disabled, or a rejected URL) it gets `role="link"` and no `target` or `rel`.
  `rel="noopener noreferrer"` is added when `Target` is `_blank` (case-insensitive) and an `href` is rendered.
- Content: optional `span.osirion-menu-item-icon` (built-in icon as an `aria-hidden` 16px SVG, or `IconTemplate`), then
  `span.osirion-menu-item-content` with `span.osirion-menu-item-text` and, when `Description` is not blank,
  `span.osirion-menu-item-description`.
- Submenu: with `HasSubmenu` the item renders `input.osirion-submenu-toggler` (a checkbox, `aria-label="Toggle {Text}
  submenu"`, `aria-controls` the submenu id), `label.osirion-submenu-toggle` with a chevron, and
  `div.osirion-submenu.compact-submenu` with class `osirion-submenu-regular` or `osirion-submenu-mega`. Ids are
  `submenu-{Id or generated}` for the submenu and `submenu-{...}-toggle` for the checkbox.
- Mega: `ChildContent` goes in `div.osirion-submenu-mega-content`; `SubmenuAside`, when set, goes in
  `div.osirion-submenu-mega-aside`. `SubmenuVariant.Mega` without `HasSubmenu` adds no mega class.
- Toggle-only parents: when `HasSubmenu` is true, the item is not disabled, `OnClick` has no handler and `Href` is blank
  or `"#"`, the link is replaced by `label.osirion-menu-item.osirion-menu-item-toggle-only` that toggles the submenu.
- Interactivity: no JavaScript is needed. Under an interactive render mode, and when `AutoDetectActive` is true, the
  item subscribes to `NavigationManager.LocationChanged` and updates its active state; it unsubscribes on dispose.
  Under static SSR the active state is computed once per render.
- The component computes its active state internally and never writes it back into `IsActive`.
