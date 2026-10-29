using Microsoft.AspNetCore.Components.Rendering;
using Osirion.Blazor.Cms.Domain.ContentBlocks;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Models;

namespace Osirion.Blazor.Cms.Web.ContentBlocks.Renderers;

/// <summary>Renders a <see cref="StepsBlock"/> as <see cref="OsirionSteps"/> in a section.</summary>
internal sealed class StepsBlockRenderer : ContentBlockRenderer<StepsBlock>
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var steps = Model.Items
            .Select(item => new StepItem(Context.PlainText(item.Title), NullIfEmpty(Context.PlainText(item.Description))))
            .ToList();

        BlockSection.Render(builder, Model, Context, content =>
        {
            content.OpenComponent<OsirionSteps>(0);
            content.AddComponentParameter(1, nameof(OsirionSteps.Items), steps);
            content.CloseComponent();
        });
    }

    private static string? NullIfEmpty(string text) => text.Length == 0 ? null : text;
}
