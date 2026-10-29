using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Osirion.Blazor.Cms.Domain.ContentBlocks;
using Osirion.Blazor.Components;

namespace Osirion.Blazor.Cms.Web.ContentBlocks.Renderers;

/// <summary>Renders a <see cref="SectionBlock"/> as an <see cref="OsirionBaseSection"/> around its Markdown.</summary>
internal sealed class SectionBlockRenderer : ContentBlockRenderer<SectionBlock>
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var section = Model;
        BlockSection.Render(builder, section, Context, string.IsNullOrWhiteSpace(section.Markdown) ? null : Copy(Context, section.Markdown));
    }

    /// <summary>
    /// Renders block Markdown the way page bodies render: through <see cref="OsirionHtmlRenderer"/>, which
    /// sanitizes it again and highlights code.
    /// </summary>
    internal static RenderFragment Copy(ContentBlockContext context, string markdown) => builder =>
    {
        builder.OpenComponent<OsirionHtmlRenderer>(0);
        builder.AddComponentParameter(1, nameof(OsirionHtmlRenderer.HtmlContent), context.Markdown(markdown).Value);
        builder.CloseComponent();
    };
}
