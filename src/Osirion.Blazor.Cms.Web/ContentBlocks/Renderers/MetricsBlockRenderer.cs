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

        BlockSection.Render(builder, Model, Context, content =>
        {
            content.OpenComponent<OsirionMetricGrid>(0);
            content.AddComponentParameter(1, nameof(OsirionMetricGrid.Metrics), metrics);
            content.AddComponentParameter(2, nameof(OsirionMetricGrid.Columns), columns);
            if (!string.IsNullOrWhiteSpace(Model.Title))
            {
                content.AddComponentParameter(3, nameof(OsirionMetricGrid.AriaLabel), Context.PlainText(Model.Title));
            }

            content.CloseComponent();
        });
    }

    private static string? NullIfEmpty(string text) => text.Length == 0 ? null : text;
}
