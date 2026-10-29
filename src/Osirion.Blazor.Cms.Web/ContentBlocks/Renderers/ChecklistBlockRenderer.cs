using Microsoft.AspNetCore.Components.Rendering;
using Osirion.Blazor.Cms.Domain.ContentBlocks;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Models;

namespace Osirion.Blazor.Cms.Web.ContentBlocks.Renderers;

/// <summary>
/// Renders a <see cref="ChecklistBlock"/> as an <see cref="OsirionChecklist"/>: in a section with the block's heading,
/// or, as an aside, on its own with the title as the checklist's heading.
/// </summary>
internal sealed class ChecklistBlockRenderer : ContentBlockRenderer<ChecklistBlock>
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var items = Model.Items
            .Select(item => new ChecklistItem(Context.PlainText(item.Text), NullIfEmpty(Context.PlainText(item.Description))))
            .ToList();

        if (IsAside)
        {
            builder.OpenComponent<OsirionChecklist>(0);
            builder.AddComponentParameter(1, nameof(OsirionChecklist.Title), NullIfEmpty(Context.PlainText(Model.Title)));
            builder.AddComponentParameter(2, nameof(OsirionChecklist.Items), items);
            builder.CloseComponent();
            return;
        }

        BlockSection.Render(builder, Model, Context, content =>
        {
            content.OpenComponent<OsirionChecklist>(0);
            content.AddComponentParameter(1, nameof(OsirionChecklist.Items), items);
            content.CloseComponent();
        });
    }

    private static string? NullIfEmpty(string text) => text.Length == 0 ? null : text;
}
