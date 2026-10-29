using Bunit;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.ContentBlocks;

/// <summary>The markup each library renderer writes for its block type.</summary>
public class ContentBlockRendererTests : ContentBlockTestBase
{
    [Fact]
    public void Hero_RendersTheHeroSectionWithHeadingCopyButtonsAndImage()
    {
        var cut = RenderBlocks("""
            blocks:
              - type: hero
                id: top
                title: "Every *decision* recorded"
                subtitle: "Access and *attendance*"
                description: "Read the [guide](https://example.com/guide)."
                primary_action: { text: "Book a call", url: /contact }
                secondary_action: { text: "Tour", url: "https://example.com/tour" }
                image: { src: /images/plant.webp, alt: "The plant" }
            """);

        var hero = cut.Find("section.osirion-hero-section, section[id='top']");
        hero.GetAttribute("id").ShouldBe("top");
        cut.Find("h1.osirion-hero-title").TextContent.ShouldBe("Every decision recorded");
        cut.Find("p.osirion-hero-subtitle").InnerHtml.ShouldBe("Access and <em>attendance</em>");
        cut.Find("p.osirion-hero-summary a").GetAttribute("href").ShouldBe("https://example.com/guide");
        cut.Find("a.osirion-hero-button-primary").GetAttribute("href").ShouldBe("/contact");
        cut.Find("a.osirion-hero-button-primary").TextContent.Trim().ShouldBe("Book a call");
        cut.Find("a.osirion-hero-button-secondary").GetAttribute("href").ShouldBe("https://example.com/tour");
        var image = cut.Find("img.osirion-hero-featured-image");
        image.GetAttribute("src").ShouldBe("/images/plant.webp");
        image.GetAttribute("alt").ShouldBe("The plant");
        cut.FindAll("h1").Count.ShouldBe(1);
    }

    [Fact]
    public void Hero_ImageAside_TakesThePlaceOfTheImage()
    {
        var cut = RenderBlocks("blocks:\n  - { type: hero, title: T, image: { src: /a.png, alt: A }, aside: { type: image, src: /b.png, alt: B } }\n");

        cut.Find("img.osirion-hero-featured-image").GetAttribute("src").ShouldBe("/b.png");
    }

    [Fact]
    public void Section_RendersHeadingDescriptionAndMarkdownInABaseSection()
    {
        var cut = RenderBlocks("""
            blocks:
              - type: section
                id: why
                title: "Why *it* works"
                description: "Short **lead**."
                markdown: |
                  First paragraph.

                  - one
                  - two
            """);

        var section = cut.Find("section#why");
        section.ClassList.ShouldContain("osirion-content-block-section");
        section.QuerySelector("h2.osirion-section-title")!.TextContent.ShouldBe("Why it works");
        section.QuerySelector("p.osirion-section-description")!.InnerHtml.ShouldBe("Short <strong>lead</strong>.");
        section.QuerySelectorAll(".osirion-section-content li").Length.ShouldBe(2);
    }

    [Fact]
    public void Steps_RendersTheStepsInOrder()
    {
        var cut = RenderBlocks("blocks:\n  - type: steps\n    title: How\n    items:\n      - { title: Made, description: \"First *step*.\" }\n      - { title: Recorded }\n");

        var steps = cut.FindAll("ol.osirion-steps li.osirion-step");
        steps.Count.ShouldBe(2);
        steps[0].QuerySelector(".osirion-step-title")!.TextContent.ShouldBe("Made");
        steps[0].QuerySelector(".osirion-step-description")!.TextContent.ShouldBe("First step.");
        steps[1].QuerySelector(".osirion-step-description").ShouldBeNull();
    }

    [Fact]
    public void Cards_RendersAGridOfFeatureCardsWithTheirLinks()
    {
        var cut = RenderBlocks("""
            blocks:
              - type: cards
                columns: 2
                items:
                  - { title: Govern, description: "Rules *first*.", url: /platform#govern, link_text: "Read about it" }
                  - { title: Explain }
            """);

        cut.Find("ul.osirion-content-block-card-grid").ClassList.ShouldContain("osirion-content-block-card-grid-2");
        var cards = cut.FindAll("li .osirion-feature-card");
        cards.Count.ShouldBe(2);
        cards[0].QuerySelector(".osirion-feature-card-title")!.TextContent.Trim().ShouldBe("Govern");
        cards[0].QuerySelector(".osirion-feature-card-description")!.TextContent.ShouldBe("Rules first.");
        var readMore = cards[0].QuerySelector(".osirion-feature-card-content a")!;
        readMore.GetAttribute("href").ShouldBe("/platform#govern");
        readMore.TextContent.ShouldContain("Read about it");
        cards[1].QuerySelectorAll(".osirion-feature-card-content a").ShouldBeEmpty();
    }

    [Fact]
    public void Metrics_RendersAMetricGridWithAColumnPerFigure()
    {
        var cut = RenderBlocks("blocks:\n  - type: metrics\n    title: Results\n    items:\n      - { value: \"99%\", label: Uptime }\n      - { value: \"3x\", label: Faster, description: Than before }\n      - { value: \"0\", label: Incidents }\n");

        var grid = cut.Find("ul.osirion-metric-grid");
        grid.ClassList.ShouldContain("osirion-metric-grid-columns-3");
        grid.GetAttribute("aria-label").ShouldBe("Results");
        cut.FindAll("ul.osirion-metric-grid > li").Count.ShouldBe(3);
        cut.Markup.ShouldContain("99%");
        cut.Markup.ShouldContain("Than before");
    }

    [Fact]
    public void Faq_RendersMarkdownAnswersAndPlainTextStructuredData()
    {
        var cut = RenderBlocks("blocks:\n  - type: faq\n    title: Questions\n    items:\n      - { question: \"Why?\", answer: \"Because **it** works.\" }\n");

        cut.Find(".osirion-faq-answer").InnerHtml.ShouldContain("Because <strong>it</strong> works.");
        var script = cut.FindAll("script[type='application/ld+json']").ShouldHaveSingleItem();
        script.TextContent.ShouldContain("\"FAQPage\"");
        script.TextContent.ShouldContain("\"text\":\"Because it works.\"");
    }

    [Fact]
    public void Cta_RendersTheCallToActionSection()
    {
        var cut = RenderBlocks("blocks:\n  - { type: cta, id: start, title: Start now, description: \"No *card*.\", primary_action: { text: Go, url: /signup }, secondary_action: { text: Mail, url: \"mailto:sales@example.com\" } }\n");

        var section = cut.Find("section.osirion-call-to-action, section#start");
        section.GetAttribute("id").ShouldBe("start");
        cut.Find(".osirion-call-to-action-title").TextContent.ShouldBe("Start now");
        cut.Find(".osirion-call-to-action-description").TextContent.ShouldBe("No card.");
        cut.Find("a.osirion-call-to-action-button-primary").GetAttribute("href").ShouldBe("/signup");
        cut.Find("a.osirion-call-to-action-button-secondary").GetAttribute("href").ShouldBe("mailto:sales@example.com");
    }

    [Fact]
    public void Image_RendersAFigureWithItsCaption()
    {
        var cut = RenderBlocks("blocks:\n  - { type: image, id: shot, src: \"https://cdn.example.com/a.png\", alt: Dashboard, caption: \"Sample *data*\" }\n");

        var figure = cut.Find("figure#shot.osirion-content-block-image");
        var image = figure.QuerySelector("img")!;
        image.GetAttribute("src").ShouldBe("https://cdn.example.com/a.png");
        image.GetAttribute("alt").ShouldBe("Dashboard");
        image.GetAttribute("loading").ShouldBe("lazy");
        figure.QuerySelector("figcaption")!.InnerHtml.ShouldBe("Sample <em>data</em>");
    }

    [Fact]
    public void Markdown_RendersAPlainSection()
    {
        var cut = RenderBlocks("blocks:\n  - { type: markdown, id: notes, title: Notes, markdown: \"Some **bold** text.\" }\n");

        var section = cut.Find("section#notes.osirion-content-block-markdown");
        section.QuerySelector("h2")!.TextContent.ShouldBe("Notes");
        section.QuerySelector("strong")!.TextContent.ShouldBe("bold");
    }

    [Fact]
    public void Blocks_RenderInTheOrderOfTheFile()
    {
        var cut = RenderBlocks("blocks:\n  - { type: markdown, markdown: first }\n  - { type: cta, title: second }\n  - { type: markdown, markdown: third }\n");

        var text = cut.Find(".osirion-content-blocks").TextContent;
        text.IndexOf("first", StringComparison.Ordinal).ShouldBeLessThan(text.IndexOf("second", StringComparison.Ordinal));
        text.IndexOf("second", StringComparison.Ordinal).ShouldBeLessThan(text.IndexOf("third", StringComparison.Ordinal));
    }

    [Fact]
    public void NoBlocks_RendersNothing()
    {
        RenderStatic<Osirion.Blazor.Cms.Web.Components.OsirionContentBlocks>(p => p.Add(c => c.Blocks, [])).Markup.Trim().ShouldBeEmpty();
    }

    [Fact]
    public void InlineValue_StaysPhrasingText()
    {
        var cut = RenderBlocks("blocks:\n  - { type: section, title: T, description: \"2026. A year\" }\n");

        var description = cut.Find("p.osirion-section-description");
        description.InnerHtml.ShouldBe("2026. A year");
    }
}
