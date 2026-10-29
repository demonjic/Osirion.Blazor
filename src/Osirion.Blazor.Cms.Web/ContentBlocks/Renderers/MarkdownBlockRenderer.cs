using Microsoft.AspNetCore.Components.Rendering;
using Osirion.Blazor.Cms.Domain.ContentBlocks;

namespace Osirion.Blazor.Cms.Web.ContentBlocks.Renderers;

/// <summary>
/// Renders a <see cref="MarkdownBlock"/> as a plain section: an optional heading and lead paragraph, then the
/// sanitized Markdown.
/// </summary>
internal sealed class MarkdownBlockRenderer : ContentBlockRenderer<MarkdownBlock>
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var block = Model;
        builder.OpenElement(0, "section");
        builder.AddAttribute(1, "id", block.Id);
        builder.AddAttribute(2, "class", "osirion-content-block osirion-content-block-markdown");
        if (!string.IsNullOrWhiteSpace(block.Title))
        {
            builder.OpenElement(3, "h2");
            builder.AddContent(4, Context.PlainText(block.Title));
            builder.CloseElement();
        }

        if (!string.IsNullOrWhiteSpace(block.Description))
        {
            builder.OpenElement(5, "p");
            builder.AddAttribute(6, "class", "osirion-content-block-description");
            builder.AddContent(7, Context.InlineMarkdown(block.Description));
            builder.CloseElement();
        }

        builder.AddContent(8, SectionBlockRenderer.Copy(Context, block.Markdown));
        builder.CloseElement();
    }
}
