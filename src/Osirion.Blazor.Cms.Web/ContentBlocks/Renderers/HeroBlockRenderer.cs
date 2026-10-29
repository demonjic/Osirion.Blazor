using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Osirion.Blazor.Cms.Domain.ContentBlocks;
using Osirion.Blazor.Components;

namespace Osirion.Blazor.Cms.Web.ContentBlocks.Renderers;

/// <summary>
/// Renders a <see cref="HeroBlock"/> as a <see cref="HeroSection"/>: the title is the page's <c>h1</c>, the subtitle
/// and description are inline Markdown, the eyebrow and highlights are plain text. An <c>image</c> aside takes the place
/// of <c>image</c>; any other aside (a <c>record</c>) renders in <see cref="HeroSection.Aside"/>, beside the copy.
/// </summary>
/// <remarks>
/// <see cref="HeroSection"/> has no band style, so a hero's <c>variant</c> is read and not used.
/// </remarks>
internal sealed class HeroBlockRenderer : ContentBlockRenderer<HeroBlock>
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var hero = Model;
        var image = hero.Aside is ImageBlock imageAside ? new ContentBlockImage(imageAside.Src, imageAside.Alt) : hero.Image;

        builder.OpenComponent<HeroSection>(0);
        if (hero.Id is not null)
        {
            builder.AddComponentParameter(1, "id", hero.Id);
        }

        builder.AddComponentParameter(2, nameof(HeroSection.Title), Context.PlainText(hero.Title));
        if (!string.IsNullOrWhiteSpace(hero.Eyebrow))
        {
            builder.AddComponentParameter(3, nameof(HeroSection.Eyebrow), Context.PlainText(hero.Eyebrow));
        }

        if (hero.Highlights.Count > 0)
        {
            builder.AddComponentParameter(4, nameof(HeroSection.Highlights), hero.Highlights.Select(Context.PlainText).ToList());
        }

        if (!string.IsNullOrWhiteSpace(hero.Subtitle))
        {
            builder.AddComponentParameter(5, nameof(HeroSection.SubtitleContent), Paragraph("osirion-hero-subtitle", hero.Subtitle));
        }

        if (!string.IsNullOrWhiteSpace(hero.Description))
        {
            builder.AddComponentParameter(6, nameof(HeroSection.SummaryContent), Paragraph("osirion-hero-summary", hero.Description));
        }

        if (hero.PrimaryAction is { } primary && Context.Link(primary.Url) is { } primaryUrl)
        {
            builder.AddComponentParameter(7, nameof(HeroSection.PrimaryButtonText), Context.PlainText(primary.Text));
            builder.AddComponentParameter(8, nameof(HeroSection.PrimaryButtonUrl), primaryUrl);
        }

        if (hero.SecondaryAction is { } secondary && Context.Link(secondary.Url) is { } secondaryUrl)
        {
            builder.AddComponentParameter(9, nameof(HeroSection.SecondaryButtonText), Context.PlainText(secondary.Text));
            builder.AddComponentParameter(10, nameof(HeroSection.SecondaryButtonUrl), secondaryUrl);
        }

        if (image is not null && ContentBlockLinks.IsAllowedImageSource(image.Src))
        {
            builder.AddComponentParameter(11, nameof(HeroSection.ImageUrl), image.Src);
            builder.AddComponentParameter(12, nameof(HeroSection.ImageAlt), image.Alt);
        }

        if (hero.Aside is { } aside and not ImageBlock)
        {
            builder.AddComponentParameter(13, nameof(HeroSection.Aside), Context.RenderAside(aside));
        }

        builder.CloseComponent();
    }

    private RenderFragment Paragraph(string cssClass, string text) => builder =>
    {
        builder.OpenElement(0, "p");
        builder.AddAttribute(1, "class", cssClass);
        builder.AddContent(2, Context.InlineMarkdown(text));
        builder.CloseElement();
    };
}
