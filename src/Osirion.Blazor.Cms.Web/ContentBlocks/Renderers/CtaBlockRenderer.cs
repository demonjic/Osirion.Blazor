using Microsoft.AspNetCore.Components.Rendering;
using Osirion.Blazor.Cms.Domain.ContentBlocks;
using Osirion.Blazor.Components;

namespace Osirion.Blazor.Cms.Web.ContentBlocks.Renderers;

/// <summary>
/// Renders a <see cref="CtaBlock"/> as an <see cref="OsirionCallToActionSection"/>. The section takes its title and
/// description as text, so inline Markdown in them is shown as plain text.
/// </summary>
internal sealed class CtaBlockRenderer : ContentBlockRenderer<CtaBlock>
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var cta = Model;
        builder.OpenComponent<OsirionCallToActionSection>(0);
        if (cta.Id is not null)
        {
            builder.AddComponentParameter(1, "id", cta.Id);
        }

        builder.AddComponentParameter(2, nameof(OsirionCallToActionSection.Title), Context.PlainText(cta.Title));
        builder.AddComponentParameter(3, nameof(OsirionCallToActionSection.Description), Context.PlainText(cta.Description));
        if (cta.PrimaryAction is { } primary && Context.Link(primary.Url) is { } primaryUrl)
        {
            builder.AddComponentParameter(4, nameof(OsirionCallToActionSection.PrimaryButtonText), Context.PlainText(primary.Text));
            builder.AddComponentParameter(5, nameof(OsirionCallToActionSection.PrimaryButtonUrl), primaryUrl);
        }

        if (cta.SecondaryAction is { } secondary && Context.Link(secondary.Url) is { } secondaryUrl)
        {
            builder.AddComponentParameter(6, nameof(OsirionCallToActionSection.SecondaryButtonText), Context.PlainText(secondary.Text));
            builder.AddComponentParameter(7, nameof(OsirionCallToActionSection.SecondaryButtonUrl), secondaryUrl);
        }

        builder.CloseComponent();
    }
}
