---
id: 'pricing-table'
order: 4
layout: docs
title: OsirionPricingTable and OsirionFeatureComparison
permalink: /components/pricing-table
description: OsirionPricingTable shows plans side by side with a highlighted plan; OsirionFeatureComparison compares their features in an accessible table.
author: Dejan Demonjić
date: 2026-10-11
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-pricing-table.jpg'
categories:
- Components
- Core Components
- Sections
tags:
- blazor
- pricing
- comparison-table
- accessibility
- static-ssr
is_featured: true
published: true
slug: pricing-table
lang: en
redirect_from:
- /docs/components/core/sections/pricing-table
custom_fields:
  group: sections
seo_properties:
  title: 'OsirionPricingTable: pricing and plan comparison for Blazor'
  description: 'Pricing plans side by side and an accessible feature comparison table for Blazor, rendered without script under static SSR.'
  image: 'https://media.getosirion.com/og/components-pricing-table.jpg'
  canonical: 'https://getosirion.com/components/pricing-table'
  lang: en
  robots: index, follow
  og_title: 'OsirionPricingTable and OsirionFeatureComparison - Osirion.Blazor'
  og_description: 'Pricing plans side by side and an accessible feature comparison table, rendered without script.'
  og_image_url: 'https://media.getosirion.com/og/components-pricing-table.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionPricingTable and OsirionFeatureComparison - Osirion.Blazor'
  twitter_description: 'Pricing plans side by side and an accessible feature comparison table, rendered without script.'
  twitter_image_url: 'https://media.getosirion.com/og/components-pricing-table.jpg'
  meta_tags: {}
---

# OsirionPricingTable and OsirionFeatureComparison

`OsirionPricingTable` shows plans side by side, each with a price, a short description, its features and a call to
action, with the recommended plan set apart. `OsirionFeatureComparison` follows it with a table that compares the
plans feature by feature. Both render without script.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Sections/OsirionPricingTable.razor). Available from 4.0.

```demo
pricing-table
```

## Usage

The plans and rows are data types from `Osirion.Blazor.Core.Models`:

```razor
@using Osirion.Blazor.Components
@using Osirion.Blazor.Core.Models

<OsirionPricingTable Plans="@plans" />

<OsirionFeatureComparison PlanNames="@planNames" Rows="@rows" />

@code {
    private readonly PricingPlan[] plans =
    [
        new()
        {
            Name = "Starter",
            Price = "Free",
            Description = "For a personal site.",
            Features = ["One site", "Community support"],
            CallToActionText = "Start",
            CallToActionUrl = "/signup"
        },
        new()
        {
            Name = "Team",
            Price = "$29",
            Period = "per month",
            Description = "For teams publishing every week.",
            Features = ["Five sites", "Email support", "Scheduled posts"],
            CallToActionText = "Start a trial",
            CallToActionUrl = "/signup?plan=team",
            Highlighted = true,
            Badge = "Most popular"
        },
        new()
        {
            Name = "Enterprise",
            Price = "Custom",
            Description = "For organizations with many sites.",
            Features = ["Unlimited sites", "Support agreement"],
            CallToActionText = "Contact sales",
            CallToActionUrl = "mailto:sales@example.org"
        },
    ];

    private readonly string[] planNames = ["Starter", "Team", "Enterprise"];

    private readonly FeatureComparisonRow[] rows =
    [
        new("Sites", ["1", "5", "Unlimited"]),
        new("Scheduled posts", [false, true, true]),
        new("Single sign-on", [false, "Add-on", true]),
    ];
}
```

A cell is a `ComparisonCell`. A `bool` converts to a check mark (`true`) or a dash (`false`), and a `string` to text,
so rows can mix both. `ComparisonCell.Yes` and `ComparisonCell.No` are the same marks written out.

### Plans quoted on request

Leave `Price` out for a plan you quote on request: the card shows its name, description, features and button
without a price. `PlanHeadingLevel` makes each plan's name a heading, so screen reader users can move from plan to
plan by heading; pick the level under the section's own heading:

```razor
<h2>Plans</h2>

<OsirionPricingTable Plans="@plans" PlanHeadingLevel="3" />

@code {
    private readonly PricingPlan[] plans =
    [
        new()
        {
            Name = "Team",
            Price = "$29",
            Period = "per month",
            CallToActionText = "Start a trial",
            CallToActionUrl = "/signup?plan=team"
        },
        new()
        {
            Name = "Enterprise",
            Description = "Priced for your sites and support.",
            CallToActionText = "Ask for a quote",
            CallToActionUrl = "/contact"
        },
    ];
}
```

Neither plan is `Highlighted` here, so the cards are equal and both buttons are primary buttons. Mark one plan
`Highlighted` and it keeps the primary button while the others get the secondary one.

## Parameters

### OsirionPricingTable

<!-- osirion:parameters OsirionPricingTable -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Plans` | `IReadOnlyList<PricingPlan>` | `[]` | The plans, in the order they are shown. Required; with no plans nothing is rendered. |
| `PlanHeadingLevel` | `int?` | `null` | The level of the heading that holds each plan's name, `1` to `6` (`h1` to `h6`), so the plans can be reached with a screen reader's heading navigation. Pick the level under the section's own heading, such as `3` under an `h2`. A value outside the range is moved to the nearest level, and the name keeps its look at every level. Null, the default, writes the name as a paragraph. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

`PricingPlan` is a record:

| Property | Type | Description |
| --- | --- | --- |
| `Name` | `string` | The plan's name. Required. |
| `Price` | `string` | The price as shown, with its currency, such as `$29` or `Free`. Empty by default; a plan without a price, such as one quoted on request, shows neither the amount nor the `Period`. |
| `Period` | `string?` | What the price covers, such as `per month`. |
| `Description` | `string?` | One line on who the plan is for. |
| `Features` | `IReadOnlyList<string>` | Features listed under the price. Empty by default. |
| `CallToActionText` | `string?` | Label of the plan's button. |
| `CallToActionUrl` | `string?` | Where the button goes. |
| `Highlighted` | `bool` | Marks the recommended plan: a colored border and the primary button, while the other plans get the secondary button. When no plan is highlighted, every plan gets the primary button. |
| `Badge` | `string?` | A short label on the plan's top border, such as `Most popular`. |

### OsirionFeatureComparison

<!-- osirion:parameters OsirionFeatureComparison -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `PlanNames` | `IReadOnlyList<string>` | `[]` | The plan names, one column each. Required; with no plan names or no rows nothing is rendered. |
| `Rows` | `IReadOnlyList<FeatureComparisonRow>` | `[]` | The features, one row each, with a value per plan in the order of `PlanNames`. Missing values leave the cell empty. Required. |
| `Caption` | `string` | `"Compare plans"` | The table's caption, which also names the scrollable region. |
| `ShowCaption` | `bool` | `false` | Whether the caption is shown; otherwise only screen readers announce it. |
| `FeatureColumnText` | `string` | `"Feature"` | The header of the feature column, read by screen readers only. |
| `IncludedText` | `string` | `"Included"` | What screen readers say for a check mark. |
| `NotIncludedText` | `string` | `"Not included"` | What screen readers say for a dash. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

`FeatureComparisonRow(string Feature, IReadOnlyList<ComparisonCell> Values)` holds one value per plan, in the order
of `PlanNames`; missing values leave the cell empty.

Both components write unmatched attributes to their root element.

## Behavior

- Both render plain HTML and need no script.
- The pricing table is a `<ul>` with `role="list"`, so screen readers still announce it as a list of plans after
  its bullets are removed.
- Each plan's name is a paragraph unless `PlanHeadingLevel` is set; then it is an `h1` to `h6` with the same look,
  and a level outside 1 to 6 moves to the nearest one. The plan's badge then follows the heading in the markup, so
  a screen reader that jumps to the heading reads the badge with its plan; it is still drawn on the top border.
- A plan whose `Price` is empty or white space has no price line: neither the amount nor the period is rendered.
- A plan's button is rendered only when it has `CallToActionText` and a safe `CallToActionUrl`: a path starting
  with `/` (not `//` or `/\`), or an `http`, `https` or `mailto` address. Anything else, such as a `javascript:` URL or a relative path
  without the leading slash, leaves the button out.
- Each button's accessible name includes the plan, for example "Start a trial: Team", so a list of buttons with the
  same text stays distinguishable. The highlighted plan gets the CSS framework's primary button class, the others
  the secondary one. With no highlighted plan, every plan is equal and gets the primary class, so a table of plans
  quoted on request still offers a primary action.
- The comparison is a real `<table>` with a caption, column headers for the plans and row headers for the features.
  Check marks and dashes are drawn in CSS and hidden from screen readers, which read `IncludedText` or
  `NotIncludedText` instead.
- The table keeps a minimum width of 32rem and scrolls sideways inside its wrapper on narrow screens. The wrapper is
  a focusable region named by the caption, so keyboard users can scroll it.
- All text is HTML-encoded.

## Styling

The pricing table is a grid of columns at least 15rem wide that wraps on narrow screens. Classes:
`osirion-pricing-table`, `osirion-pricing-plan` (and `osirion-pricing-plan-highlighted`), `osirion-pricing-badge`,
`osirion-pricing-name`, `osirion-pricing-price` with `osirion-pricing-amount` and `osirion-pricing-period`,
`osirion-pricing-description`, `osirion-pricing-features` and `osirion-pricing-action`.

The comparison uses `osirion-feature-comparison` (the scrolling wrapper), `osirion-feature-comparison-table`,
`osirion-feature-comparison-caption`, `osirion-feature-comparison-yes` and `osirion-feature-comparison-no`.

Both take their colors from the theme tokens: `--osirion-action-primary` for the highlighted border, badge and check
marks, `--osirion-action-primary-text` for the badge text, `--osirion-border-color`, `--osirion-background-primary`,
`--osirion-text-primary`, `--osirion-text-secondary` and `--osirion-text-tertiary` for the dashes.

## Related

- [Steps](/components/steps)
- [Base section](/components/base-section)
- [Core section components](/components/sections)
