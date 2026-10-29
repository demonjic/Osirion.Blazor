using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Components;
using Osirion.Blazor.Core.Models;

namespace Osirion.Blazor.Components;

/// <summary>
/// Frequently asked questions as an <see cref="OsirionAccordion"/>, with FAQPage structured data
/// (<see href="https://schema.org/FAQPage">schema.org</see>) that describes the questions and answers to search
/// engines and other readers of the page. Google no longer shows FAQ rich results (since May 2026), so the data does
/// not change how the page looks in Google results.
/// </summary>
public partial class OsirionFaq
{
    /// <summary>
    /// Gets or sets the questions and answers, in the order they are shown. Answers are plain text: markup in an
    /// answer is shown as text, and line breaks are kept. Renders nothing when empty.
    /// </summary>
    [Parameter]
    public IReadOnlyList<FaqItem> Items { get; set; } = [];

    /// <summary>
    /// Gets or sets whether FAQPage structured data (JSON-LD) is written after the questions. Turn it off when the
    /// page already describes these questions in its own structured data, and use one FAQPage per page.
    /// </summary>
    [Parameter]
    public bool IncludeStructuredData { get; set; } = true;

    /// <summary>
    /// Gets or sets how each answer renders, in place of the plain-text paragraph, for answers with formatting such as
    /// rendered Markdown. The structured data still describes the answer with <see cref="FaqItem.Answer"/>, so give it
    /// the answer's plain text. Markup the template writes is not sanitized here; sanitize content before passing it.
    /// </summary>
    [Parameter]
    public RenderFragment<FaqItem>? AnswerTemplate { get; set; }

    /// <summary>
    /// Gets or sets whether several answers can be open at once. When <see langword="false"/>, opening one answer
    /// closes the others, in browsers that support exclusive <c>details</c> elements.
    /// </summary>
    [Parameter]
    public bool AllowMultiple { get; set; } = true;

    private string GetContainerClass() =>
        string.IsNullOrWhiteSpace(Class) ? "osirion-faq" : $"osirion-faq {Class}";

    /// <summary>
    /// Builds the FAQPage JSON-LD, or returns <see langword="null"/> when a question or answer holds text that cannot be
    /// written as JSON (half of a surrogate pair); the questions still render, only the structured data is left out.
    /// </summary>
    private string? BuildStructuredData()
    {
        var questions = new JsonArray();
        foreach (var item in Items)
        {
            questions.Add(new JsonObject
            {
                ["@type"] = "Question",
                ["name"] = item.Question,
                ["acceptedAnswer"] = new JsonObject
                {
                    ["@type"] = "Answer",
                    ["text"] = item.Answer,
                },
            });
        }

        var root = new JsonObject
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "FAQPage",
            ["mainEntity"] = questions,
        };

        try
        {
            return root.ToJsonString();
        }
        catch (ArgumentException)
        {
            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }
}
