using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Osirion.Blazor.Cms.Domain.ContentBlocks;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Models;

namespace Osirion.Blazor.Cms.Web.ContentBlocks.Renderers;

/// <summary>
/// Renders a <see cref="FaqBlock"/> as an <see cref="OsirionFaq"/> in a section. Answers are block Markdown; the
/// FAQPage structured data carries their plain text and is written by the page's first FAQ block only.
/// </summary>
internal sealed class FaqBlockRenderer : ContentBlockRenderer<FaqBlock>
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var answers = new Dictionary<FaqItem, MarkupString>();
        var items = new List<FaqItem>(Model.Items.Count);
        foreach (var question in Model.Items)
        {
            var item = new FaqItem(Context.PlainText(question.Question), Context.PlainMarkdownText(question.Answer));
            items.Add(item);
            answers.TryAdd(item, Context.Markdown(question.Answer));
        }

        BlockSection.Render(builder, Model, Context, content =>
        {
            content.OpenComponent<OsirionFaq>(0);
            content.AddComponentParameter(1, nameof(OsirionFaq.Items), items);
            content.AddComponentParameter(2, nameof(OsirionFaq.IncludeStructuredData), Context.WritesFaqStructuredData(Model));
            content.AddComponentParameter(3, nameof(OsirionFaq.AnswerTemplate), (RenderFragment<FaqItem>)(item => answer =>
            {
                answer.OpenElement(0, "div");
                answer.AddAttribute(1, "class", "osirion-faq-answer");
                answer.AddContent(2, answers[item]);
                answer.CloseElement();
            }));
            content.CloseComponent();
        });
    }
}
