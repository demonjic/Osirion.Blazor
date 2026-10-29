# OsirionPricingTable and OsirionFeatureComparison

Purpose
`OsirionPricingTable` shows plans side by side: name, price, period, description, features and a call to action,
with the recommended plan set apart. `OsirionFeatureComparison` is the detailed table that usually follows it,
with check marks, crosses and values per plan. Both render without script; the buttons take the classes of the
configured CSS framework.

Key parameters
- OsirionPricingTable: `Plans` (`IReadOnlyList<PricingPlan>`, in `Osirion.Blazor.Core.Models`),
  `PlanHeadingLevel` (`int?`, default null: each plan's name is a paragraph; 1 to 6 makes it an `h1` to `h6` with
  the same look, out-of-range values move to the nearest level), `Class`
- `PricingPlan`: `Name` (required), `Price` (empty by default; a blank price leaves out the amount and the period,
  for plans quoted on request), `Period`, `Description`, `Features`, `CallToActionText`, `CallToActionUrl`,
  `Highlighted` (primary button and accent border; other plans get the secondary button, and with no plan
  highlighted every plan gets the primary button), `Badge`
- OsirionFeatureComparison: `PlanNames`, `Rows` (`FeatureComparisonRow(Feature, Values)`), `Caption`
  (default "Compare plans"), `ShowCaption`, `FeatureColumnText`, `IncludedText`, `NotIncludedText`
- `ComparisonCell`: `true` or `false` (check or cross) or text, converted implicitly

Usage
```razor
<OsirionPricingTable Plans="plans" />
<OsirionFeatureComparison PlanNames="@(["Starter", "Team"])" Rows="rows" />

@code {
    private readonly PricingPlan[] plans =
    [
        new() { Name = "Starter", Price = "Free", Features = ["1 site"], CallToActionText = "Get started", CallToActionUrl = "/start" },
        new() { Name = "Team", Price = "$29", Period = "per month", Features = ["5 sites", "Analytics"],
                CallToActionText = "Start trial", CallToActionUrl = "/trial", Highlighted = true, Badge = "Most popular" },
    ];

    private readonly FeatureComparisonRow[] rows =
    [
        new("Static SSR", [true, true]),
        new("Single sign-on", [false, "Add-on"]),
    ];
}
```

Notes
- Plans are a list (`ul`), so screen readers announce how many there are. Each button's accessible name includes
  the plan ("Start trial: Team").
- Set `PlanHeadingLevel` to the level under the section's own heading (`3` under an `h2`) so screen reader users
  can move from plan to plan by heading. The badge then follows the heading in the markup, so it is read with its
  plan, and it is still drawn on the top border.
- Only http(s), `mailto:` and site-relative call-to-action URLs are rendered; a plan with another URL shows no button.
- Check marks and crosses are drawn in CSS and read as `IncludedText` and `NotIncludedText`. Rows shorter than the
  number of plans leave the remaining cells empty.
- The comparison scrolls sideways on narrow screens; the scroll region is focusable and named by the caption.
