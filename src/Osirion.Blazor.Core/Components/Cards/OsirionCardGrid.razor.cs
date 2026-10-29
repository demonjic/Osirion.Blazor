using Microsoft.AspNetCore.Components;

namespace Osirion.Blazor.Components;

/// <summary>
/// Lays cards out in equal columns that collapse on narrow screens: <see cref="Columns"/> side by side from 992px,
/// at most two from 640px, and one below. It renders a plain <c>div</c> grid without script.
/// </summary>
public partial class OsirionCardGrid
{
    private const int MinColumns = 1;

    // Four is the most cards that stay readable side by side in the widest Osirion container (1320px).
    private const int MaxColumns = 4;

    /// <summary>
    /// The cards, or any other blocks, one per grid cell.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// The number of columns on wide screens, <c>1</c> to <c>4</c>; it defaults to <c>3</c>. A value outside the range
    /// is moved to the nearest one.
    /// </summary>
    [Parameter]
    public int Columns { get; set; } = 3;

    private string GetGridClass() =>
        CombineCssClasses($"osirion-card-grid osirion-card-grid-columns-{Math.Clamp(Columns, MinColumns, MaxColumns)}");
}
