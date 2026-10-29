using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Osirion.Blazor.Cms.Domain.ContentBlocks;
using Osirion.Blazor.Components;

namespace Osirion.Blazor.Cms.Web.ContentBlocks.Renderers;

/// <summary>
/// The band the section-like blocks share: an <see cref="OsirionBaseSection"/> with the block's id, its title as the
/// section heading and its description as inline Markdown, around the block's own content.
/// </summary>
/// <remarks>
/// The title and description go in through <c>TitleContent</c> and <c>DescriptionContent</c>, never through
/// <c>Title</c> and <c>Description</c>, which the section writes as raw markup.
/// </remarks>
internal static class BlockSection
{
    /// <summary>Renders the band with <paramref name="content"/> inside.</summary>
    public static void Render(RenderTreeBuilder builder, ContentBlock block, ContentBlockContext context, RenderFragment? content)
    {
        builder.OpenComponent<OsirionBaseSection>(0);
        builder.AddComponentParameter(1, nameof(OsirionBaseSection.Id), block.Id);
        builder.AddComponentParameter(2, nameof(OsirionBaseSection.Class), $"osirion-content-block osirion-content-block-{block.Type}");
        if (!string.IsNullOrWhiteSpace(block.Title))
        {
            builder.AddComponentParameter(3, nameof(OsirionBaseSection.TitleContent), Heading(block, context));
        }

        if (!string.IsNullOrWhiteSpace(block.Description))
        {
            builder.AddComponentParameter(4, nameof(OsirionBaseSection.DescriptionContent), Description(block, context));
        }

        if (content is not null)
        {
            builder.AddComponentParameter(5, nameof(OsirionBaseSection.ChildContent), content);
        }

        builder.CloseComponent();
    }

    private static RenderFragment Heading(ContentBlock block, ContentBlockContext context) => builder =>
    {
        builder.OpenElement(0, "h2");
        builder.AddAttribute(1, "class", "osirion-section-title");
        builder.AddContent(2, context.PlainText(block.Title));
        builder.CloseElement();
    };

    private static RenderFragment Description(ContentBlock block, ContentBlockContext context) => builder =>
    {
        builder.OpenElement(0, "p");
        builder.AddAttribute(1, "class", "osirion-section-description");
        builder.AddContent(2, context.InlineMarkdown(block.Description));
        builder.CloseElement();
    };
}
