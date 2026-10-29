using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Osirion.Blazor.Cms.Domain.ContentBlocks;
using Osirion.Blazor.Components;

namespace Osirion.Blazor.Cms.Web.ContentBlocks.Renderers;

/// <summary>
/// Renders a <see cref="SplitBlock"/> as an <see cref="OsirionSplitSection"/> in a section: the Markdown copy on one
/// side and the aside block, rendered by its own renderer, on the side <c>aside_position</c> names.
/// </summary>
internal sealed class SplitBlockRenderer : ContentBlockRenderer<SplitBlock>
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var split = Model;
        BlockSection.Render(builder, split, Context, content =>
        {
            content.OpenComponent<OsirionSplitSection>(0);
            if (!string.IsNullOrWhiteSpace(split.Markdown))
            {
                content.AddComponentParameter(1, nameof(OsirionSplitSection.Content), SectionBlockRenderer.Copy(Context, split.Markdown));
            }

            if (split.Aside is { } aside)
            {
                content.AddComponentParameter(2, nameof(OsirionSplitSection.Aside), Context.RenderAside(aside));
            }

            content.AddComponentParameter(3, nameof(OsirionSplitSection.AsidePosition), Position(split.AsidePosition));
            content.CloseComponent();
        });
    }

    private static SplitAsidePosition Position(ContentBlockAsidePosition position) =>
        position == ContentBlockAsidePosition.Start ? SplitAsidePosition.Start : SplitAsidePosition.End;
}
