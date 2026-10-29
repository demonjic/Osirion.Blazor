using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Osirion.Blazor.Cms.Domain.ContentBlocks;
using Osirion.Blazor.Components;

namespace Osirion.Blazor.Cms.Web.ContentBlocks.Renderers;

/// <summary>
/// The band the section-like blocks share: an <see cref="OsirionBaseSection"/> with the block's id, band style
/// (<c>variant</c>) and eyebrow, its title as the section heading and its description as inline Markdown, around the
/// block's own content.
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
        builder.AddComponentParameter(3, nameof(OsirionBaseSection.Variant), Variant(block.Variant));
        if (!string.IsNullOrWhiteSpace(block.Eyebrow))
        {
            builder.AddComponentParameter(4, nameof(OsirionBaseSection.Eyebrow), context.PlainText(block.Eyebrow));
        }

        if (!string.IsNullOrWhiteSpace(block.Title))
        {
            builder.AddComponentParameter(5, nameof(OsirionBaseSection.TitleContent), Heading(block, context));
        }

        if (!string.IsNullOrWhiteSpace(block.Description))
        {
            builder.AddComponentParameter(6, nameof(OsirionBaseSection.DescriptionContent), Description(block, context));
        }

        if (content is not null)
        {
            builder.AddComponentParameter(7, nameof(OsirionBaseSection.ChildContent), content);
        }

        builder.CloseComponent();
    }

    /// <summary>Maps a block's <c>variant</c> to the band style of the section components.</summary>
    private static SectionVariant Variant(ContentBlockVariant variant) => variant switch
    {
        ContentBlockVariant.Tinted => SectionVariant.Tinted,
        ContentBlockVariant.Dark => SectionVariant.Dark,
        _ => SectionVariant.Default,
    };

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
