using Microsoft.AspNetCore.Components.Rendering;
using Osirion.Blazor.Cms.Domain.ContentBlocks;
using Osirion.Blazor.Components;

namespace Osirion.Blazor.Cms.Web.ContentBlocks.Renderers;

/// <summary>Renders a <see cref="CardsBlock"/> as a grid of <see cref="OsirionFeatureCard"/> in a section.</summary>
/// <remarks>
/// A card's <c>icon</c> and <c>items</c> are read but not rendered until <see cref="OsirionFeatureCard"/> has
/// parameters for them. A card with a <c>url</c> shows its read more link, with <c>link_text</c> as its text.
/// </remarks>
internal sealed class CardsBlockRenderer : ContentBlockRenderer<CardsBlock>
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var cards = Model;
        BlockSection.Render(builder, cards, Context, content =>
        {
            content.OpenElement(0, "ul");
            content.AddAttribute(1, "class", $"osirion-content-block-card-grid osirion-content-block-card-grid-{cards.Columns}");
            content.AddAttribute(2, "role", "list");
            foreach (var card in cards.Items)
            {
                content.OpenElement(3, "li");
                content.SetKey(card);
                content.OpenComponent<OsirionFeatureCard>(4);
                content.AddComponentParameter(5, nameof(OsirionFeatureCard.Title), Context.PlainText(card.Title));
                content.AddComponentParameter(6, nameof(OsirionFeatureCard.Description), Context.PlainText(card.Description));
                if (Context.Link(card.Url) is { } url)
                {
                    content.AddComponentParameter(7, nameof(OsirionFeatureCard.Url), url);
                    content.AddComponentParameter(8, nameof(OsirionFeatureCard.ShowReadMoreLink), true);
                    if (!string.IsNullOrWhiteSpace(card.LinkText))
                    {
                        content.AddComponentParameter(9, nameof(OsirionFeatureCard.ReadMoreText), Context.PlainText(card.LinkText));
                    }
                }

                content.CloseComponent();
                content.CloseElement();
            }

            content.CloseElement();
        });
    }
}
