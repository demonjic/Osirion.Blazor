using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Osirion.Blazor.Cms.Domain.ContentBlocks;
using Osirion.Blazor.Components;

namespace Osirion.Blazor.Cms.Web.ContentBlocks.Renderers;

/// <summary>
/// Renders a <see cref="CardsBlock"/> as <see cref="OsirionFeatureCard"/>s in an <see cref="OsirionCardGrid"/>, in a
/// section. A card's <c>icon</c> is the icon the site registered under that name with
/// <see cref="ContentBlockOptions.AddIcon"/>, or none; its <c>items</c> are listed on the card. A card with a
/// <c>url</c> shows its read more link, with <c>link_text</c> as its text.
/// </summary>
internal sealed class CardsBlockRenderer : ContentBlockRenderer<CardsBlock>
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var cards = Model;
        RenderFragment grid = gridBuilder =>
        {
            foreach (var card in cards.Items)
            {
                RenderCard(gridBuilder, card);
            }
        };

        BlockSection.Render(builder, cards, Context, content =>
        {
            content.OpenComponent<OsirionCardGrid>(0);
            content.AddComponentParameter(1, nameof(OsirionCardGrid.Columns), cards.Columns);
            content.AddComponentParameter(2, nameof(OsirionCardGrid.ChildContent), grid);
            content.CloseComponent();
        });
    }

    private void RenderCard(RenderTreeBuilder builder, CardsBlockItem card)
    {
        builder.OpenComponent<OsirionFeatureCard>(0);
        builder.SetKey(card);
        builder.AddComponentParameter(1, nameof(OsirionFeatureCard.Title), Context.PlainText(card.Title));
        builder.AddComponentParameter(2, nameof(OsirionFeatureCard.Description), Context.PlainText(card.Description));
        if (Context.Icon(card.Icon) is { } icon)
        {
            builder.AddComponentParameter(3, nameof(OsirionFeatureCard.Icon), icon);
        }

        if (card.Items.Count > 0)
        {
            builder.AddComponentParameter(4, nameof(OsirionFeatureCard.Items), card.Items.Select(Context.PlainText).ToList());
        }

        if (Context.Link(card.Url) is { } url)
        {
            builder.AddComponentParameter(5, nameof(OsirionFeatureCard.Url), url);
            builder.AddComponentParameter(6, nameof(OsirionFeatureCard.ShowReadMoreLink), true);
            if (!string.IsNullOrWhiteSpace(card.LinkText))
            {
                builder.AddComponentParameter(7, nameof(OsirionFeatureCard.ReadMoreText), Context.PlainText(card.LinkText));
            }
        }

        builder.CloseComponent();
    }
}
