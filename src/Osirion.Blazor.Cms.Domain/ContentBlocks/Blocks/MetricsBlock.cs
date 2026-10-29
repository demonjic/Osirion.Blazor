namespace Osirion.Blazor.Cms.Domain.ContentBlocks;

/// <summary>A row of figures (<c>type: metrics</c>).</summary>
public sealed record MetricsBlock : ContentBlock
{
    /// <summary>Gets the figures, in order (<c>items</c>); never empty on a parsed block.</summary>
    public IReadOnlyList<MetricsBlockItem> Items { get; init; } = [];
}

/// <summary>One figure of a <see cref="MetricsBlock"/>.</summary>
/// <param name="Value">The figure as shown, for example <c>99.9%</c> (<c>value</c>).</param>
/// <param name="Label">What it measures (<c>label</c>).</param>
/// <param name="Description">More about it (<c>description</c>).</param>
public sealed record MetricsBlockItem(string Value, string? Label, string? Description);
