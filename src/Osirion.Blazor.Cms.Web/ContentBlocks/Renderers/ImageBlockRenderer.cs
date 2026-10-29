using Microsoft.AspNetCore.Components.Rendering;
using Osirion.Blazor.Cms.Domain.ContentBlocks;

namespace Osirion.Blazor.Cms.Web.ContentBlocks.Renderers;

/// <summary>Renders an <see cref="ImageBlock"/> as a figure with the image and its caption.</summary>
internal sealed class ImageBlockRenderer : ContentBlockRenderer<ImageBlock>
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var image = Model;
        if (!ContentBlockLinks.IsAllowedImageSource(image.Src))
        {
            builder.AddContent(0, Context.Problem(image, $"'{image.Src}' is not an allowed image address"));
            return;
        }

        builder.OpenElement(1, "figure");
        builder.AddAttribute(2, "id", image.Id);
        builder.AddAttribute(3, "class", "osirion-content-block osirion-content-block-image");
        builder.OpenElement(4, "img");
        builder.AddAttribute(5, "src", image.Src);
        builder.AddAttribute(6, "alt", Context.PlainText(image.Alt));
        builder.AddAttribute(7, "loading", "lazy");
        builder.AddAttribute(8, "decoding", "async");
        builder.CloseElement();
        if (!string.IsNullOrWhiteSpace(image.Caption))
        {
            builder.OpenElement(9, "figcaption");
            builder.AddContent(10, Context.InlineMarkdown(image.Caption));
            builder.CloseElement();
        }

        builder.CloseElement();
    }
}
