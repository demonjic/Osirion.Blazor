---
title: "Navigation in Osirion.Blazor: Menus, Scroll and Breadcrumbs"
author: "Dejan Demonjić"
date: "2025-04-14"
last_modified: 2026-10-18
description: "A tour of the Osirion.Blazor navigation components: a menu that works without JavaScript, mega menus, a back-to-top button, scroll handling and breadcrumbs."
tags: [Navigation, Menu, Breadcrumbs, Accessibility, SSR]
categories: [Documentation]
slug: "navigation-components"
is_featured: false
featured_image: 'https://media.getosirion.com/og/blog-navigation-components.jpg'
seo_properties:
  og_image_url: 'https://media.getosirion.com/og/blog-navigation-components.jpg'
---

# Navigation in Osirion.Blazor: Menus, Scroll and Breadcrumbs

A content site needs a few navigation pieces on every page: a main menu that collapses on phones, a way back to the top
of long articles, sensible scrolling between pages, and breadcrumbs. Osirion.Blazor covers each of them with a component
that renders on the server and works under static SSR. This article walks through them and shows where each one goes.

The menu and scroll components are in `Osirion.Blazor.Navigation` (namespace `Osirion.Blazor.Navigation.Components`).
The breadcrumbs are in `Osirion.Blazor.Core`.

## The main menu

`Menu` renders a `<nav>` with an optional brand link and whatever items you put inside it:

```razor
@using Osirion.Blazor.Navigation.Components

<Menu BrandText="My Site"
      BrandLogo="/logo.png"
      Alignment="MenuAlignment.Right"
      AriaLabel="Main navigation">
    <MenuItem Text="Home" Href="/" Icon="home" />
    <MenuItem Text="Blog" Href="/blog" Match="NavLinkMatch.Prefix" />
    <MenuItem Text="Guides" Href="/guides" HasSubmenu>
        <MenuItem Text="Getting started" Href="/guides/getting-started" />
        <MenuItem Text="Deployment" Href="/guides/deployment" />
    </MenuItem>
    <MenuDivider />
    <MenuItem Text="Contact" Href="/contact" Icon="contact" />
</Menu>
```

`NavLinkMatch` comes from `Microsoft.AspNetCore.Components.Routing`, which the Blazor template already imports.

![An Osirion menu with the Docs dropdown open](https://media.getosirion.com/shots/blog-navigation-components/menu-dark.webp){width=1600 height=498}

A few things this markup gets you without any script:

- **Mobile toggle.** With `CollapseOnMobile` (on by default), the menu collapses at 768px and below behind a toggle
  built on a visually hidden, focusable checkbox, so it opens from the keyboard. Set
  `CollapseBreakpoint="MenuCollapseBreakpoint.ExtraLarge"` to collapse below 1440px instead.
- **Submenus.** A `MenuItem` with `HasSubmenu` renders its child content as a submenu with its own toggle, again a
  focusable checkbox, so it opens from the keyboard and without script. The link itself still navigates to `Href`.
- **Active item.** Each item compares its `Href` with the current URL (`Match` is `NavLinkMatch.All` by default;
  `Prefix` matches on a path segment boundary) and marks the match with `aria-current="page"`. Query strings and
  fragments are ignored. When the page is interactive, the active item follows navigation. Set `IsActive` to force it,
  or `AutoDetectActive="false"` to switch detection off.
- **Safe links.** Items render no `href` for schemes other than relative, http, https, mailto and tel, so a
  `javascript:` URL in menu data from a CMS cannot run. `Target="_blank"` adds `rel="noopener noreferrer"`.

`Sticky` (on by default) keeps a horizontal menu at the top of the page; `Orientation="MenuOrientation.Vertical"` gives
a side menu. `Icon` accepts a few built-in names (`home`, `blog`, `docs`, `user`, `contact`, `about`, `settings`,
`dashboard` and some aliases); for anything else use `IconTemplate`. `BrandingTemplate` replaces the brand link
completely.

## Mega menus

For a wider dropdown, set `SubmenuVariant="SubmenuVariant.Mega"` and lay the content out in groups, with an optional
aside column:

```razor
<MenuItem Text="Product" Href="/product" HasSubmenu SubmenuVariant="SubmenuVariant.Mega">
    <ChildContent>
        <MenuGroup Label="Build"
                   Description="Components for content sites"
                   FooterText="All components"
                   FooterHref="/docs/components">
            <MenuItem Text="Content" Href="/docs/components/cms-web" Description="Lists, articles and search" />
            <MenuItem Text="Navigation" Href="/docs/components/navigation" Description="Menus and scrolling" />
        </MenuGroup>
    </ChildContent>
    <SubmenuAside>
        <MenuPanel Title="Start here">
            <MenuPanelLink Text="Quick start" Href="/docs" />
        </MenuPanel>
    </SubmenuAside>
</MenuItem>
```

`MenuImageCard` (an image link with a caption) and `MenuCallToAction` (a highlighted link with a description) also fit in
the aside. The panel colors come from `--osirion-menu-panel-background` and `--osirion-menu-panel-color`.

## Back to top

`ScrollToTop` renders a button that appears once the reader has scrolled past `VisibilityThreshold` pixels (300 by
default) and scrolls back to the top when pressed. Unlike the menu, it needs a small inline script, which it renders
itself; without JavaScript the button stays hidden.

With the meta-package, `AddOsirion` registers it with its defaults, and you render it once in the layout through the
provider:

```razor
@using Osirion.Blazor.Navigation.Components

<ScrollToTopProvider />
```

If you call `AddOsirion(builder.Configuration)`, configure it in `Osirion:Navigation:ScrollToTop` (`Position`,
`Behavior`, `VisibilityThreshold`, `Text`, `Title`, `CssClass`, `IconCssClass`, `Enabled`). You can also render
`<ScrollToTop />` directly and set the same values as parameters:

```razor
<ScrollToTop Position="Position.BottomLeft" Text="Top" />
```

`Position` and `ScrollBehavior` are in the `Osirion.Blazor.Components` namespace. `Title` is the button's accessible
name. The default icon is an arrow; `IconCssClass` puts an icon font class in its place.

## Scrolling between pages

Blazor's enhanced navigation patches the next page into the current document. `EnhancedNavigation`, placed after the
`blazor.web.js` script in `App.razor`, scrolls to the top after each of those navigations. The details are in
[Scroll position after enhanced navigation](/blog/enhanced-navigation-interceptor).

## Breadcrumbs

`OsirionBreadcrumbs` turns a path into a breadcrumb trail:

```razor
@using Osirion.Blazor.Components

<OsirionBreadcrumbs Path="/blog/navigation-components"
                    CurrentPageText="Navigation in Osirion.Blazor" />
```

It starts with a home link (`ShowHome`, `HomeText`, `HomeUrl`), links each path segment, and marks the last item, which
is not a link unless you set `LinkLastItem`, with `aria-current="page"`. By default a segment is shown with its hyphens
turned into spaces and each word capitalized ("navigation-components" becomes "Navigation Components"); pass
`SegmentFormatter` to name segments yourself, and `CurrentPageText` to use the page title for the last item. The CMS
`ArticlePage` sets `CurrentPageText` to the article title for you.

## Related

- [Navigation components](/docs/packages/navigation)
- [Breadcrumbs](/components/breadcrumbs)
- [Scroll position after enhanced navigation](/blog/enhanced-navigation-interceptor)
