using System.Text.Json;
using Bunit;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Models;
using Osirion.Blazor.Core.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Components.Content;

public class OsirionFaqTests : OsirionComponentTestBase
{
    private static readonly FaqItem[] Items =
    [
        new("Does it need JavaScript?", "No. Every component renders with static server-side rendering."),
        new("Which frameworks are supported?", "Bootstrap, MudBlazor, Fluent UI and Radzen."),
    ];

    [Fact]
    public void OsirionFaq_Items_RenderAQuestionAndAnswerEach()
    {
        var cut = RenderStatic<OsirionFaq>(parameters => parameters.Add(p => p.Items, Items));

        var details = cut.FindAll("details.osirion-accordion-item");
        details.Count.ShouldBe(2);
        details[0].QuerySelector("summary")!.TextContent.Trim().ShouldBe("Does it need JavaScript?");
        details[1].QuerySelector(".osirion-faq-answer")!.TextContent.ShouldBe("Bootstrap, MudBlazor, Fluent UI and Radzen.");
    }

    [Fact]
    public void OsirionFaq_AnswerTemplate_RendersEachAnswerAndKeepsTheTextInTheStructuredData()
    {
        var cut = RenderStatic<OsirionFaq>(parameters => parameters
            .Add(p => p.Items, Items)
            .Add(p => p.AnswerTemplate, item => builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "rich-answer");
                builder.AddContent(2, item.Answer.ToUpperInvariant());
                builder.CloseElement();
            }));

        cut.FindAll("p.osirion-faq-answer").ShouldBeEmpty();
        cut.FindAll("div.rich-answer").Select(answer => answer.TextContent).ShouldBe(Items.Select(item => item.Answer.ToUpperInvariant()));
        cut.Find("script[type='application/ld+json']").TextContent.ShouldContain(Items[0].Answer);
    }

    [Fact]
    public void OsirionFaq_Default_WritesFaqPageStructuredData()
    {
        var cut = RenderStatic<OsirionFaq>(parameters => parameters.Add(p => p.Items, Items));

        using var json = JsonDocument.Parse(cut.Find("script[type='application/ld+json']").TextContent);
        var root = json.RootElement;
        root.GetProperty("@context").GetString().ShouldBe("https://schema.org");
        root.GetProperty("@type").GetString().ShouldBe("FAQPage");
        var questions = root.GetProperty("mainEntity").EnumerateArray().ToList();
        questions.Count.ShouldBe(2);
        questions[0].GetProperty("@type").GetString().ShouldBe("Question");
        questions[0].GetProperty("name").GetString().ShouldBe("Does it need JavaScript?");
        questions[0].GetProperty("acceptedAnswer").GetProperty("@type").GetString().ShouldBe("Answer");
        questions[0].GetProperty("acceptedAnswer").GetProperty("text").GetString().ShouldBe(Items[0].Answer);
    }

    // A question or answer from content must not be able to end the script element and inject markup.
    [Fact]
    public void OsirionFaq_TextWithScriptTags_StaysInsideTheJson()
    {
        FaqItem[] hostile = [new("</script><script>alert(1)</script>", "<!-- & \" ' >")];

        var cut = RenderStatic<OsirionFaq>(parameters => parameters.Add(p => p.Items, hostile));

        cut.FindAll("script").Count.ShouldBe(1);
        var scriptText = cut.Find("script[type='application/ld+json']").TextContent;
        scriptText.ShouldNotContain("</script");
        scriptText.ShouldNotContain("<!--");
        using var json = JsonDocument.Parse(scriptText);
        json.RootElement.GetProperty("mainEntity")[0].GetProperty("name").GetString().ShouldBe(hostile[0].Question);
    }

    [Fact]
    public void OsirionFaq_StructuredDataOff_WritesNoScript()
    {
        var cut = RenderStatic<OsirionFaq>(parameters => parameters
            .Add(p => p.Items, Items)
            .Add(p => p.IncludeStructuredData, false));

        cut.FindAll("script").ShouldBeEmpty();
        cut.FindAll("details").Count.ShouldBe(2);
    }

    [Fact]
    public void OsirionFaq_NoItems_RendersNothing()
    {
        var cut = RenderStatic<OsirionFaq>(parameters => parameters.Add(p => p.Items, Array.Empty<FaqItem>()));

        cut.Markup.Trim().ShouldBeEmpty();
    }
}
