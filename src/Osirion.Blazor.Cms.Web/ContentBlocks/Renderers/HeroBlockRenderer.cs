using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Osirion.Blazor.Cms.Domain.ContentBlocks;
using Osirion.Blazor.Components;

namespace Osirion.Blazor.Cms.Web.ContentBlocks.Renderers;

/// <summary>
/// Renders a <see cref="HeroBlock"/> as a <see cref="HeroSection"/>: the title is the page's <c>h1</c>, the subtitle
/// and description are inline Markdown, an image aside takes the place of <c>image</c>.
/// </summary>
/// <remarks>
/// <c>eyebrow</c>, <c>variant</c>, <c>highlights</c> and a <c>record</c> aside are read but not rendered until
/// <see cref="HeroSection"/> has parameters for them; a record aside is reported as a block without a renderer.
/// </remarks>
internal sealed class HeroBlockRenderer : ContentBlockRenderer<HeroBlock>
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var hero = Model;
        var image = hero.Aside is ImageBlock aside ? new ContentBlockImage(aside.Src, aside.Alt) : hero.Image;

        builder.OpenComponent<HeroSection>(0);
        if (hero.Id is not null)
        {
            builder.AddComponentParameter(1, "id", hero.Id);
        }

        builder.AddComponentParameter(2, nameof(HeroSection.Title), Context.PlainText(hero.Title));
        if (!string.IsNullOrWhiteSpace(hero.Subtitle))
        {
            builder.AddComponentParameter(3, nameof(HeroSection.SubtitleContent), Paragraph("osirion-hero-subtitle", hero.Subtitle));
        }

        if (!string.IsNullOrWhiteSpace(hero.Description))
        {
            builder.AddComponentParameter(4, nameof(HeroSection.SummaryContent), Paragraph("osirion-hero-summary", hero.Description));
        }

        if (hero.PrimaryAction is { } primary && Context.Link(primary.Url) is { } primaryUrl)
        {
            builder.AddComponentParameter(5, nameof(HeroSection.PrimaryButtonText), Context.PlainText(primary.Text));
            builder.AddComponentParameter(6, nameof(HeroSection.PrimaryButtonUrl), primaryUrl);
        }

        if (hero.SecondaryAction is { } secondary && Context.Link(secondary.Url) is { } secondaryUrl)
        {
            builder.AddComponentParameter(7, nameof(HeroSection.SecondaryButtonText), Context.PlainText(secondary.Text));
            builder.AddComponentParameter(8, nameof(HeroSection.SecondaryButtonUrl), secondaryUrl);
        }
        if (image is not null && ContentBlockLinks.IsAllowedImageSource(image.Src))
        {
            builder.AddComponentParameter(9, nameof(HeroSection.ImageUrl), image.Src);
            builder.AddComponentParameter(10, nameof(HeroSection.ImageAlt), image.Alt);
        }

        builder.CloseComponent();

        if (hero.Aside is { } other and not ImageBlock)
        {
            builder.AddContent(11, Context.AwaitingRenderer(other));
        }
    }

    private RenderFragment Paragraph(string cssClass, string text) => builder =>
    {
        builder.OpenElement(0, "p");
        builder.AddAttribute(1, "class", cssClass);
        builder.AddContent(2, Context.InlineMarkdown(text));
        builder.CloseElement();
    };
}
