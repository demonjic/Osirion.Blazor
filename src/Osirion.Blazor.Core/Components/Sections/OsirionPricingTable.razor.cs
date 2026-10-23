using Microsoft.AspNetCore.Components;
using Osirion.Blazor.Core.Models;

namespace Osirion.Blazor.Components;

/// <summary>
/// Pricing plans side by side: name, price, description, features and a call to action, with the recommended plan
/// set apart. The buttons take the classes of the configured CSS framework. It renders without script.
/// </summary>
public partial class OsirionPricingTable
{
    /// <summary>
    /// The plans, in the order they are shown. Required; with no plans nothing is rendered.
    /// </summary>
    [Parameter, EditorRequired]
    public IReadOnlyList<PricingPlan> Plans { get; set; } = [];

    /// <summary>
    /// The level of the heading that holds each plan's name, <c>1</c> to <c>6</c> (<c>h1</c> to <c>h6</c>), so the
    /// plans can be reached with a screen reader's heading navigation. Pick the level under the section's own heading,
    /// such as <c>3</c> under an <c>h2</c>. A value outside the range is moved to the nearest level, and the name keeps
    /// its look at every level. Null, the default, writes the name as a paragraph.
    /// </summary>
    [Parameter]
    public int? PlanHeadingLevel { get; set; }

    private int? NameHeadingLevel => PlanHeadingLevel is int level ? Math.Clamp(level, 1, 6) : null;

    private string GetPricingTableClass() =>
        string.IsNullOrWhiteSpace(Class) ? "osirion-pricing-table" : $"osirion-pricing-table {Class}";

    private static string GetPlanClass(PricingPlan plan) =>
        plan.Highlighted ? "osirion-pricing-plan osirion-pricing-plan-highlighted" : "osirion-pricing-plan";
}
