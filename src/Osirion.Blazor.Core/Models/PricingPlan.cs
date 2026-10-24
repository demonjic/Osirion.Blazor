namespace Osirion.Blazor.Core.Models;

/// <summary>
/// A plan in <see cref="Osirion.Blazor.Components.OsirionPricingTable"/>.
/// </summary>
public sealed record PricingPlan
{
    /// <summary>Gets the plan's name, for example <c>Team</c>.</summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the price as shown, with its currency, for example <c>$29</c> or <c>Free</c>. Empty by default: a plan
    /// without a price, such as one quoted on request, shows no amount and no <see cref="Period"/>.
    /// </summary>
    public string Price { get; init; } = string.Empty;

    /// <summary>Gets what the price covers, for example <c>per month</c>.</summary>
    public string? Period { get; init; }

    /// <summary>Gets a one-line description of who the plan is for.</summary>
    public string? Description { get; init; }

    /// <summary>Gets the plan's features, listed under the price.</summary>
    public IReadOnlyList<string> Features { get; init; } = [];

    /// <summary>Gets the call-to-action label, for example <c>Start free trial</c>.</summary>
    public string? CallToActionText { get; init; }

    /// <summary>Gets where the call to action goes: an http(s) URL or a path on the site.</summary>
    public string? CallToActionUrl { get; init; }

    /// <summary>
    /// Gets whether this is the recommended plan, which is set apart and gets the primary button while the other plans
    /// get the secondary one. When no plan in the table is recommended, every plan gets the primary button.
    /// </summary>
    public bool Highlighted { get; init; }

    /// <summary>Gets a short label on the plan, for example <c>Most popular</c>.</summary>
    public string? Badge { get; init; }
}
