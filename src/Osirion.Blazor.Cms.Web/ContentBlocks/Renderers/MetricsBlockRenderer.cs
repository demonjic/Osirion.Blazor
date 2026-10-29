using Microsoft.AspNetCore.Components.Rendering;
using Osirion.Blazor.Cms.Domain.ContentBlocks;
using Osirion.Blazor.Components;

namespace Osirion.Blazor.Cms.Web.ContentBlocks.Renderers;

/// <summary>
/// Renders a <see cref="MetricsBlock"/> as an <see cref="OsirionMetricGrid"/> in a section, with as many columns as
/// figures, from two to four.
/// </summary>
internal sealed class MetricsBlockRenderer : ContentBlockRenderer<MetricsBlock>
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var metrics = Model.Items
            .Select(item => new OsirionMetricGrid.MetricGridItem(
                Context.PlainText(item.Value),
                NullIfEmpty(Context.PlainText(item.Label)),
                NullIfEmpty(Context.PlainText(item.Description))))
            .ToList();

        var columns = metrics.Count switch
        {
            <= 2 => MetricGridColumns.Two,
            3 => MetricGridColumns.Three,
            _ => MetricGridColumns.Four,
        };

        void Grid(RenderTreeBuilder grid)
        {
            grid.OpenComponent<OsirionMetricGrid>(0);
            grid.AddComponentParameter(1, nameof(OsirionMetricGrid.Metrics), metrics);
            grid.AddComponentParameter(2, nameof(OsirionMetricGrid.Columns), columns);
            if (!string.IsNullOrWhiteSpace(Model.Title))
            {
                grid.AddComponentParameter(3, nameof(OsirionMetricGrid.AriaLabel), Context.PlainText(Model.Title));
            }

            grid.CloseComponent();
        }

        // As an aside the grid stands alone, named by the title; at the top level it sits in a section.
        if (IsAside)
        {
            Grid(builder);
            return;
        }

        BlockSection.Render(builder, Model, Context, Grid);
    }

    private static string? NullIfEmpty(string text) => text.Length == 0 ? null : text;
}
