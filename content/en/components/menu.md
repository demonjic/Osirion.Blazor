---
id: 'osirion-menu'
order: 3
layout: docs
title: Menu
permalink: /components/menu
description: Menu renders a responsive site menu with submenus, mega menus and a mobile drawer that open with CSS only. Parameters of the menu and its parts.
author: Dejan Demonjić
date: 2026-10-17
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-menu.jpg'
categories:
- Components
- Navigation Components
- Navigation
tags:
- blazor
- menu
- mega-menu
- navigation
- static-ssr
- osirion-blazor-navigation
is_featured: true
published: true
slug: menu
lang: en
custom_fields:
  group: navigation
seo_properties:
  title: 'Menu: a site menu with mega menus for Blazor'
  description: 'Menu renders a responsive site menu with submenus, mega menus and a mobile drawer that open with CSS only. Parameters of the menu and its parts.'
  image: 'https://media.getosirion.com/og/components-menu.jpg'
  canonical: 'https://getosirion.com/components/menu'
  lang: en
  robots: index, follow
  og_title: 'Menu - Osirion.Blazor'
  og_description: 'A responsive site menu with submenus and mega menus that works under static SSR without script.'
  og_image_url: 'https://media.getosirion.com/og/components-menu.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'Menu - Osirion.Blazor'
  twitter_description: 'A responsive site menu with submenus and mega menus that works under static SSR without script.'
  twitter_image_url: 'https://media.getosirion.com/og/components-menu.jpg'
  meta_tags: {}
---

# Menu

`Menu` renders a site menu: a `<nav>` with an optional brand link, your items, an optional actions slot and optional
pinned actions that stay in the bar. Submenus,
mega menus and the mobile drawer open with CSS only, so the menu works under static SSR without any script. The parts
that fill it (`MenuItem`, `MenuGroup`, `MenuDivider`, `MenuCallToAction`, `MenuPanel`, `MenuPanelLink` and
`MenuImageCard`) are described on this page too.

Package `Osirion.Blazor.Navigation`, namespace `Osirion.Blazor.Navigation.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Navigation/Components/Menu.razor).

```demo
menu
```

The demo is a horizontal menu with `Sticky` off, so it stays inside the page. Hover the Docs item, or tab to it and
press Space, to open its submenu. Below 768px the items collapse behind a hamburger.

## Usage

Add `@using Osirion.Blazor.Navigation.Components` to `_Imports.razor`. The menu needs no registration. A horizontal
menu with a brand, a link with an icon and a submenu:

```razor
@using Osirion.Blazor.Navigation.Components

<Menu AriaLabel="Main navigation" BrandText="My site" BrandLogo="/favicon.png"
      Alignment="MenuAlignment.Right">
    <MenuItem Text="Home" Href="/" Icon="home" />
    <MenuItem Text="Docs" Href="/docs" Icon="docs" HasSubmenu="true">
        <MenuItem Text="Getting started" Href="/docs/getting-started" />
        <MenuItem Text="Components" Href="/docs/components" />
    </MenuItem>
    <MenuItem Text="GitHub" Href="https://github.com/demonjic/Osirion.Blazor" Target="_blank" />
</Menu>
```

A mega menu has groups of links, a call to action and an aside:

```razor
<MenuItem Text="Product" Href="/product" HasSubmenu="true" SubmenuVariant="SubmenuVariant.Mega">
    <ChildContent>
        <MenuGroup Label="Build" FooterText="All features" FooterHref="/features">
            <MenuItem Text="Content" Description="Markdown from GitHub or the file system." Href="/features/content" />
            <MenuItem Text="SEO" Description="Metadata, sitemaps and feeds." Href="/features/seo" />
        </MenuGroup>
        <MenuCallToAction Text="Get started" Description="Install the package." Href="/docs" />
    </ChildContent>
    <SubmenuAside>
        <MenuPanel Title="New in this release">
            <MenuPanelLink Text="Changelog" Href="/changelog" />
        </MenuPanel>
    </SubmenuAside>
</MenuItem>
```

The `Actions` slot holds what belongs at the end of the bar, such as a search box or a theme toggle. A horizontal menu
aligns it to the trailing edge, and a collapsed menu shows it at the bottom of the drawer. The `PinnedActions` slot
holds what must stay visible at every width, such as the site's main call to action: a horizontal menu shows it after
`Actions`, and a collapsed menu keeps it in the bar after the toggle instead of moving it into the drawer. It is
rendered once, so a screen reader meets the link once. Keep it short, because on a phone it shares the bar with the
brand and the toggle (a long brand name is shortened with an ellipsis):

```razor
<Menu AriaLabel="Main navigation" BrandText="My site" CollapseBreakpoint="MenuCollapseBreakpoint.Large">
    <MenuItem Text="Pricing" Href="/pricing" />
    <Actions>
        <ThemeToggle />
    </Actions>
    <PinnedActions>
        <a class="osirion-button osirion-button-primary" href="/contact">Book a call</a>
    </PinnedActions>
</Menu>
```

## Parameters

Every part of the menu also takes `Class`, `Style` and `Theme` from the Osirion component base, and passes unmatched
attributes to its root element. `Style` is written on the root element of every part. `Class` goes on the container of
`Menu`, on the link (or toggle label) of `MenuItem`, and on the root element of the other parts, so on a `MenuItem` the
two reach different elements: `Style` lands on its wrapper, where a CSS variable such as `--osirion-menu-accent` is
inherited by the link and the submenu. A sticky `Menu` takes its default `z-index` of 1000 from its stylesheet and writes
no `style` attribute, so a strict Content Security Policy needs nothing for it; a `StickyZIndex` other than 1000, or a
`Style`, is written inline, with `Style` before the sticky `z-index`, which still wins.

### Menu

<!-- osirion:parameters Menu -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `ChildContent` | `RenderFragment?` | `null` | The menu items: `MenuItem`, `MenuDivider` and the other menu parts. |
| `Href` | `string?` | `"/"` | The destination of the brand link. A scheme other than a relative URL, `http`, `https`, `mailto` or `tel` leaves the link without an `href`. |
| `BrandLogo` | `string?` | `null` | The address of the logo image, shown before `BrandText` inside the brand link. The image is drawn square at `--osirion-menu-logo-size` (2.25rem by default) and declares 36 by 36 pixels, the size of that default, so the bar does not shift while it loads. Beside `BrandText` its alternative text is empty, because the text already names the link; without `BrandText` the logo is the whole link and its alternative text is `Home`. |
| `BrandText` | `string?` | `null` | The brand name, rendered as the text of the brand link. Without a logo it is the whole link. |
| `BrandingTemplate` | `RenderFragment?` | `null` | Your own brand markup. It replaces the brand link, `BrandLogo` and `BrandText` and is not wrapped in a link. |
| `Actions` | `RenderFragment?` | `null` | Content shown at the end of the bar, after the menu items: a search box, a theme toggle or a call to action button. A horizontal menu aligns it to the trailing edge; a collapsed menu shows it at the bottom of its drawer. Put content that must stay visible when the menu collapses in `PinnedActions`. |
| `PinnedActions` | `RenderFragment?` | `null` | Content that stays in the bar at every width, for example the site's main call to action. A horizontal menu shows it after `Actions`; below the collapse breakpoint it stays in the bar, drawn after the toggle, instead of moving into the drawer. It renders once, after the drawer in keyboard order. Keep it short: on a phone it shares the bar with the brand and the toggle. |
| `Sticky` | `bool` | `true` | Whether a horizontal menu stays at the top of the page while it scrolls. It has no effect on a vertical menu. Turn it off for a menu inside a page. |
| `StickyZIndex` | `int` | `1000` | The container's `z-index` while the menu is sticky. The default, 1000, comes from the stylesheet; another value, or any `OsirionComponentBase.Style`, is written to the container's inline style, after `Style`, so it wins over a `z-index` there. |
| `Orientation` | `MenuOrientation` | `MenuOrientation.Horizontal` | The layout: `Horizontal`, a bar with dropdown submenus, or `Vertical`, a stack whose submenus open in place. |
| `Alignment` | `MenuAlignment` | `MenuAlignment.Center` | Where a horizontal bar places its items: `Left`, `Center` or `Right`. A vertical menu ignores it. |
| `CollapseOnMobile` | `bool` | `true` | Whether the items collapse behind a hamburger toggle and a drawer below the breakpoint in `CollapseBreakpoint`. |
| `CollapseBreakpoint` | `MenuCollapseBreakpoint` | `MenuCollapseBreakpoint.Mobile` | The viewport width below which a collapsible menu shows the toggle: `Mobile` (768px and below), `Large` (below 1200px) or `ExtraLarge` (below 1440px). Pick the narrowest width at which the bar still fits on one line. It applies only while `CollapseOnMobile` is on. |
| `AutoExpandActive` | `bool` | `true` | Whether a vertical menu keeps the submenu of the active item open. The menu gets the `osirion-menu-auto-expand` class on its container, and the stylesheet shows the submenu of an active item, its toggle then no longer closes it. The other submenus open and close from their toggle as usual. A horizontal menu ignores it. |
| `AriaLabel` | `string` | `"Navigation menu"` | The accessible name of the `nav` landmark. Give each menu on a page a different one. |
| `ToggleAriaLabel` | `string` | `"Toggle navigation menu"` | The accessible name of the hamburger toggle. |
| `Id` | `string?` | `null` | The id of the container; the toggle and navigation ids derive from it. Without it each menu gets a unique generated id, so two menus on one page do not clash. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

### MenuItem

`MenuItem` is a link, or a parent that opens a submenu. It marks itself active (`aria-current="page"`) when the
current path matches `Href`; the comparison ignores the query string, the fragment and surrounding slashes. Unmatched
attributes go on the item's wrapper element.

<!-- osirion:parameters MenuItem -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Text` | `string` | `""` | The link text. |
| `Description` | `string?` | `null` | Supporting text under the link text. |
| `Icon` | `string?` | `null` | The name of a built-in icon: `home`, `blog`, `document`, `docs`, `documentation`, `user`, `profile`, `contact`, `about`, `info`, `settings`, `gear` or `dashboard`. Any other name renders no icon. `IconTemplate` takes precedence. |
| `IconTemplate` | `RenderFragment?` | `null` | Your own icon markup, rendered instead of `Icon`. |
| `Href` | `string` | `"#"` | The link address. Only a relative URL or the `http`, `https`, `mailto` and `tel` schemes get an `href`; with another scheme, such as `javascript:`, the link is rendered without one. A parent with `HasSubmenu` and no address (empty or `#`) renders as the submenu toggle itself instead of a dead link. |
| `IsActive` | `bool` | `false` | Whether the item is forced into the active state, with the `aria-current="page"` attribute and the active style, whatever the current URL is. |
| `Disabled` | `bool` | `false` | Whether the item is disabled: it renders no `href`, sets `aria-disabled` and ignores `OnClick`. |
| `HasSubmenu` | `bool` | `false` | Whether `ChildContent` is rendered as a submenu behind a disclosure toggle. |
| `ChildContent` | `RenderFragment?` | `null` | The submenu content: menu items, or groups and other parts for a mega submenu. It is rendered only when `HasSubmenu` is on. |
| `SubmenuAside` | `RenderFragment?` | `null` | Optional aside content for mega submenus, rendered as a dedicated right-hand column. Typically a `MenuPanel` or `MenuImageCard`. Only used when `SubmenuVariant` is Mega. |
| `SubmenuVariant` | `SubmenuVariant` | `SubmenuVariant.Regular` | The submenu presentation: `Regular` for a list or `Mega` for a grid of groups. |
| `Match` | `NavLinkMatch` | `NavLinkMatch.All` | How the current path is compared with `Href`: `All` needs the same path, `Prefix` also matches child paths on a segment boundary. The query string, the fragment and surrounding slashes are ignored. |
| `Target` | `string?` | `null` | The `target` attribute of the link. `_blank` also adds `rel="noopener noreferrer"`. |
| `OnClick` | `EventCallback<MouseEventArgs>` | - | The callback raised when the link is clicked. It runs only in an interactive render mode. A parent that has it renders as a link, not as the submenu toggle, even without an `Href`. |
| `Id` | `string?` | `null` | The base of the submenu and toggle ids, `submenu-{Id}`. Without it a generated value is used. |
| `AutoDetectActive` | `bool` | `true` | Whether the active state is detected from the current URL. Under static SSR it is computed once per request; in an interactive render mode the item also follows later navigations. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

### MenuGroup

`MenuGroup` groups items in a submenu column, with an optional label, description and footer link. In the drawer a
group becomes a list with a small uppercase label, and its description is left out.

<!-- osirion:parameters MenuGroup -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Label` | `string?` | `null` | The heading of the group. Without it the group has no heading and the items are not labelled. |
| `Description` | `string?` | `null` | Supporting text under the label, which describes the column in a mega submenu. |
| `FooterText` | `string?` | `null` | The text of the footer link, for example `All features`. The link is rendered only when `FooterHref` also gives a safe address. |
| `FooterHref` | `string?` | `null` | The address of the footer link. A scheme other than a relative URL, `http`, `https`, `mailto` or `tel` leaves the footer link out. |
| `FooterTarget` | `string?` | `null` | The `target` attribute of the footer link. `_blank` also adds `rel="noopener noreferrer"`. |
| `Collapsible` | `bool` | `false` | Whether the label opens and closes the group. The label becomes the `summary` of a `details` element with a chevron, so a click, Enter or Space toggles the items without script, also under static SSR. It needs a `Label`: without one the group stays open. |
| `Expanded` | `bool` | `true` | Whether a collapsible group starts open. It is the initial state only: the reader's toggle is not reset when the parent renders again with the same value. It has no effect unless `Collapsible` is on. |
| `ChildContent` | `RenderFragment?` | `null` | The items of the group, rendered in a `role="group"` container. |
| `Id` | `string?` | `null` | The id of the group; the ids of its label and item container derive from it. Without it a unique id is generated. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

### MenuDivider

`MenuDivider` draws a separator (`role="separator"`): a horizontal rule in lists and the drawer, and a short vertical
rule between the items of a horizontal bar.

<!-- osirion:parameters MenuDivider -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Label` | `string?` | `null` | An optional text shown with the separator. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

### MenuCallToAction

`MenuCallToAction` is a highlighted link with an arrow, for the end of a menu or mega menu. In a mega submenu it spans
every column, below the groups.

<!-- osirion:parameters MenuCallToAction -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Text` | `string` | `""` | The visible text of the link. |
| `Description` | `string?` | `null` | Supporting text under `Text`. |
| `Href` | `string` | `"#"` | The link address. Only a relative URL or the `http`, `https`, `mailto` and `tel` schemes get an `href`. |
| `Target` | `string?` | `null` | The `target` attribute. `_blank` also adds `rel="noopener noreferrer"`. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

### MenuPanel

`MenuPanel` is a high-contrast promotional aside for a mega menu, holding `MenuPanelLink` items. It renders an
`<aside>`. It defaults to the inverted surface tokens, and `--osirion-menu-panel-background` and
`--osirion-menu-panel-color` rebrand it.

<!-- osirion:parameters MenuPanel -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Title` | `string?` | `null` | The heading of the panel. It is also the accessible name unless `AriaLabel` is set. |
| `Description` | `string?` | `null` | Supporting text under the heading. |
| `AriaLabel` | `string?` | `null` | The accessible name of the `aside`. Falls back to `Title`. |
| `ChildContent` | `RenderFragment?` | `null` | The links of the panel, typically `MenuPanelLink` items. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

### MenuPanelLink

`MenuPanelLink` is a pill-style featured link inside a `MenuPanel`. Hovering it reveals a trailing arrow.

<!-- osirion:parameters MenuPanelLink -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Text` | `string` | `""` | The visible text of the link. |
| `Href` | `string` | `"#"` | The link address. Only a relative URL or the `http`, `https`, `mailto` and `tel` schemes get an `href`. |
| `Target` | `string?` | `null` | The `target` attribute. `_blank` also adds `rel="noopener noreferrer"`. |
| `IconTemplate` | `RenderFragment?` | `null` | An icon shown before the text, inside an `aria-hidden` span. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

### MenuImageCard

`MenuImageCard` is a linked image with a caption over a dark scrim, so the text stays readable in both themes. Use it
as a mega menu aside for a customer story or a news item. Pass a real `ImageAlt`.

<!-- osirion:parameters MenuImageCard -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `ImageUrl` | `string` | `""` | The address of the image, written as given. It loads lazily. |
| `ImageAlt` | `string?` | `null` | The alternative text of the image. Without it the image is rendered with `alt=""`, which marks it as decorative so a screen reader skips it. Set it when the image says something the caption does not. |
| `Title` | `string` | `""` | The caption title, shown over a dark scrim at the bottom of the image. |
| `Description` | `string?` | `null` | Supporting text under `Title`. |
| `Href` | `string` | `"#"` | The link address of the whole card. Only a relative URL or the `http`, `https`, `mailto` and `tel` schemes get an `href`. |
| `Target` | `string?` | `null` | The `target` attribute. `_blank` also adds `rel="noopener noreferrer"`. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

## Behavior

- **Hover and keyboard.** In a horizontal bar a submenu opens on hover. From the keyboard, Tab reaches the submenu
  toggle and Space opens the panel; Tab then moves through its links, and the panel closes when focus leaves it. A
  closed panel is skipped by Tab.
- **Collapse.** Below the collapse breakpoint the bar keeps the brand, the pinned actions and a hamburger. The items
  and `Actions` open in a drawer under the bar, submenus open in place as indented lists, and a tap on the dimmed page
  closes the drawer. The toggle and every submenu are visually hidden checkboxes with labels, so they work from the
  keyboard and without JavaScript.
- **Keyboard order when collapsed.** Tab reaches the brand first, then the toggle, which shows a focus ring; Space opens
  the drawer, and the next Tab lands on its first item. The pinned actions follow the drawer. Above the breakpoint the
  toggle is not drawn, and its checkbox is out of the tab order too. With pinned actions the collapsed bar draws the
  toggle ahead of them, so the order on screen (brand, toggle, pinned actions) is the order Tab follows.
- **Parents without a page.** A `MenuItem` with `HasSubmenu` and no `Href` (or `Href="#"`) renders as the toggle itself,
  so a tap anywhere on it opens the submenu instead of following a dead link. An `OnClick` callback turns it back into a
  link.
- **Nested submenus.** Inside a dropdown, a nested submenu flies out beside its row. In the drawer and in vertical menus,
  every level opens in place, indented under its parent. Two levels read well in a dropdown, three rarely do; a mega
  submenu shows a large section better than a chain of flyouts.
- **Mega submenus.** Groups form a responsive column grid with the `SubmenuAside` on the right. In the drawer the groups
  become one list with small section labels, and descriptions and the aside are left out.
- **Icons.** `Icon` takes one of the built-in names (`home`, `blog`, `document`, `docs`, `documentation`, `user`,
  `profile`, `contact`, `about`, `info`, `settings`, `gear`, `dashboard`). Any other name renders no icon, rather than an
  empty slot; use `IconTemplate` for your own icon.
- **Active state.** Under static SSR the active state is computed once per request. In an interactive render mode
  `MenuItem` also follows later navigations.
- **Collapsible groups.** `MenuGroup` with `Collapsible` turns its label into the `summary` of a `details` element, so a
  click, Enter or Space opens and closes the items without script. `Expanded` is the starting state. A collapsible group
  needs a `Label`; without one it stays open.
- **Auto expand.** With `AutoExpandActive` a vertical menu keeps the submenu of the active item open, so the reader sees
  where the page sits. Because the open state comes from the active item and not from the toggle, the toggle cannot close
  that submenu while the item is active. Turn the parameter off for a menu where the reader should be able to close it.
- **Several menus.** Each menu and each parent item generates a unique id, so two menus on one page do not toggle each
  other. Give each menu its own `AriaLabel`.

### Security

`MenuItem`, the footer of `MenuGroup`, `MenuPanelLink`, `MenuImageCard`, `MenuCallToAction` and the brand link render an
`href` only for relative URLs and the `http`, `https`, `mailto` and `tel` schemes. With a `javascript:` URL from
CMS-driven menu data the element is rendered without an `href` (a `MenuGroup` footer is left out), so nothing can run.
`MenuImageCard` writes `ImageUrl` as given, so pass only addresses you trust.

## Styling

The menu reads the theme tokens (`--osirion-text-primary`, `--osirion-background-primary`, `--osirion-border-color`,
`--osirion-link-color` and others), so it follows the light and dark theme and the framework adapters. Class hooks:

- `osirion-menu-container`, `osirion-menu-horizontal` or `osirion-menu-vertical`, `osirion-menu-align-*` and
  `osirion-menu-sticky` on the container;
- `osirion-menu-item`, `osirion-menu-item-active`, `osirion-submenu`, `osirion-submenu-regular` and
  `osirion-submenu-mega` on items and submenus.

These variables override the defaults, on the menu or on any ancestor:

| Variable | Default |
| --- | --- |
| `--osirion-menu-height` | `4rem` |
| `--osirion-menu-max-width` | `none`; set a length to center the bar's content |
| `--osirion-menu-padding-x` | `1rem` |
| `--osirion-menu-logo-size` | `2.25rem` |
| `--osirion-menu-background`, `--osirion-menu-color`, `--osirion-menu-muted-color` | The theme background, text and secondary text |
| `--osirion-menu-border-color` | The theme border color |
| `--osirion-menu-accent` | The theme's link color (`--osirion-link-color`); used for the active item, focus rings and links |
| `--osirion-menu-hover-background`, `--osirion-menu-active-background` | Tints of the text and accent colors |
| `--osirion-menu-dropdown-background` | The menu background |
| `--osirion-menu-radius`, `--osirion-menu-panel-radius`, `--osirion-menu-mega-radius` | `0.5rem`, `0.75rem`, `1rem` |
| `--osirion-menu-shadow` | A soft two-layer shadow for panels and the drawer |
| `--osirion-menu-panel-background`, `--osirion-menu-panel-color` | The inverted surface tokens, for `MenuPanel` |

The drawer sets `--osirion-theme-toggle-padding` to `0` on its actions, so a `ThemeToggle` there lines up with the menu
items instead of keeping its own 10px padding.

## Related

- [ScrollToTop](/components/scroll-to-top)
- [EnhancedNavigation](/components/enhanced-navigation)
- [ThemeToggle](/components/theme-toggle), for the `Actions` slot
- [Navigation package](/docs/packages/navigation)
- [CMS navigation](/components/cms-navigation): menus, breadcrumbs and a table of contents built from your content
