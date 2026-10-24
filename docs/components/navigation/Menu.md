# Menu

`Menu` renders site navigation without JavaScript, so it works the same under static SSR, interactive server
rendering and WebAssembly. Open and closed states are kept in visually hidden checkboxes; CSS does the rest.

- A **horizontal** menu is a bar: the brand, the items, then the `Actions` slot (a search box, a theme toggle or a
  call to action). Submenus open as dropdown panels; a mega submenu opens a wide panel centred under the bar.
- Below its **collapse breakpoint** the bar keeps the brand and a hamburger button. The items open in a drawer under
  the bar, submenus open in place as indented lists, and a tap on the dimmed page closes the drawer.
- A **vertical** menu stacks its items and opens submenus in place.

```razor
<Menu BrandLogo="/logo.png"
      BrandText="Contoso"
      Alignment="MenuAlignment.Right"
      CollapseBreakpoint="MenuCollapseBreakpoint.Large">
    <ChildContent>
        <MenuItem Text="Platform" HasSubmenu SubmenuVariant="SubmenuVariant.Mega">
            <ChildContent>
                <MenuGroup Label="Core platform" Description="Measure and automate.">
                    <MenuItem Text="Tracking" Description="Measurement down to ad level." Href="/platform/tracking" />
                    <MenuItem Text="Automation" Description="Protect margins as you scale." Href="/platform/automation" />
                </MenuGroup>
                <MenuGroup Label="Infrastructure" Description="Connect your stack.">
                    <MenuItem Text="Data and APIs" Description="Integrate what you already run." Href="/platform/data" />
                </MenuGroup>
                <MenuCallToAction Text="Platform overview" Description="Everything in one tour." Href="/platform" />
            </ChildContent>
            <SubmenuAside>
                <MenuImageCard ImageUrl="/images/customer.jpg" ImageAlt="A customer team at work"
                               Title="Customer stories" Href="/customers" />
            </SubmenuAside>
        </MenuItem>

        <MenuItem Text="Guides" HasSubmenu>
            <MenuItem Text="Getting started" Href="/guides/start" />
            <MenuItem Text="Integrations" HasSubmenu>
                <MenuItem Text="Payroll" Href="/guides/payroll" />
                <MenuItem Text="ERP" Href="/guides/erp" />
            </MenuItem>
        </MenuItem>

        <MenuItem Text="Pricing" Href="/pricing" />
    </ChildContent>
    <Actions>
        <SearchBox />
        <ThemeToggle />
    </Actions>
</Menu>
```

## Parameters

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `ChildContent` | `RenderFragment?` | | `MenuItem`, `MenuDivider` and other items. |
| `Actions` | `RenderFragment?` | | Content after the items: aligned to the end of the bar, and at the bottom of the drawer. |
| `Href` | `string?` | `"/"` | Destination of the brand link. |
| `BrandLogo`, `BrandText` | `string?` | | Brand image and text. |
| `BrandingTemplate` | `RenderFragment?` | | Replaces the brand link. |
| `Orientation` | `MenuOrientation` | `Horizontal` | `Horizontal` or `Vertical`. |
| `Alignment` | `MenuAlignment` | `Center` | Where a horizontal bar places its items: `Left`, `Center` or `Right`. In a right-aligned bar, dropdowns and nested flyouts open towards the page so they stay on screen. |
| `CollapseOnMobile` | `bool` | `true` | Shows the hamburger and drawer below the collapse breakpoint. |
| `CollapseBreakpoint` | `MenuCollapseBreakpoint` | `Mobile` | `Mobile` collapses at 768px and below, `Large` below 1200px, `ExtraLarge` below 1440px. Pick the narrowest breakpoint at which the bar still fits on one line. |
| `Sticky`, `StickyZIndex` | `bool`, `int` | `true`, `1000` | Keeps a horizontal menu at the top while the page scrolls. The default `z-index` comes from `.osirion-menu-sticky` in the stylesheet, so no `style` attribute is written (nothing for a Content Security Policy to allow); another value, or any `Style`, is written inline, after `Style`, so it still wins. |
| `AutoExpandActive` | `bool` | `true` | Vertical menus only. Keeps the submenu of the active item open; its toggle cannot close it while the item is active. |
| `AriaLabel`, `ToggleAriaLabel`, `Id` | `string` | | Accessible names and the id the generated ids derive from. |

`MenuItem`, `MenuGroup` and the mega menu building blocks are described in [MenuItems.md](MenuItems.md).

## Behaviour

- **Hover and keyboard.** In the bar a panel opens on hover. From the keyboard, Tab reaches the submenu toggle and
  Space opens the panel; Tab then moves through its links, and the panel closes when focus leaves it. A closed
  panel is skipped by Tab.
- **Parents without a page.** A `MenuItem` with `HasSubmenu` and no `Href` (or `Href="#"`) renders as the toggle
  itself, so a tap anywhere on it opens the submenu instead of following a dead link.
- **Nested submenus.** Inside a dropdown, a nested submenu flies out beside its row. In the drawer and in vertical
  menus, every level opens in place, indented under its parent.
- **Mega submenus.** Groups form a responsive column grid with the `SubmenuAside` on the right. In the drawer the
  groups become one list with small section labels; descriptions and the aside are left out to keep it scannable.
- **Icons.** `Icon` takes one of the built-in names (`home`, `blog`, `document`, `docs`, `documentation`, `user`,
  `profile`, `contact`, `about`, `info`, `settings`, `gear`, `dashboard`). Any other name renders no icon, rather
  than an empty slot; use `IconTemplate` for your own icon.

## Theming

The menu follows the Osirion theme tokens (`--osirion-background-primary`, `--osirion-text-primary`,
`--osirion-border-color`, `--osirion-link-color` and the framework adapters that set them). These hooks override
it, on the menu or on any ancestor:

| Variable | Default |
| --- | --- |
| `--osirion-menu-height` | `4rem` |
| `--osirion-menu-max-width` | `none`; set a length to centre the bar's content |
| `--osirion-menu-padding-x` | `1rem` |
| `--osirion-menu-logo-size` | `2.25rem` |
| `--osirion-menu-background`, `--osirion-menu-color`, `--osirion-menu-muted-color` | theme background, text and secondary text |
| `--osirion-menu-border-color` | theme border color |
| `--osirion-menu-accent` | the theme's link color (`--osirion-link-color`), which the framework adapters take from the framework; used for the active item, focus rings and links |
| `--osirion-menu-hover-background`, `--osirion-menu-active-background` | tints of the text and accent colors |
| `--osirion-menu-dropdown-background` | the menu background |
| `--osirion-menu-radius`, `--osirion-menu-panel-radius`, `--osirion-menu-mega-radius` | `0.5rem`, `0.75rem`, `1rem` |
| `--osirion-menu-shadow` | soft two-layer shadow for panels and the drawer |
