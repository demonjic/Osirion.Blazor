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

    // Fails before the fix: a card with a url but no image or icon drew its image placeholder in a link with no name.
    [Fact]
    public void Cards_WithoutIconsOrImages_EveryLinkHasANameAndEachCardLinksOnce()
    {
        var cut = RenderBlocks("blocks:\n  - { type: cards, items: [{ title: A, url: /a }, { title: B, url: /b, link_text: Open }, { title: C }] }\n");

        var links = cut.FindAll("a[href]");
        links.Select(link => link.GetAttribute("href")).ShouldBe(["/a", "/b"]);
        foreach (var link in links)
        {
            (link.TextContent.Trim().Length > 0 || !string.IsNullOrWhiteSpace(link.GetAttribute("aria-label"))).ShouldBeTrue(link.OuterHtml);
        }

        cut.FindAll(".osirion-feature-card-image-link").ShouldBeEmpty();
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

        cut.Find(".osirion-card-grid").ClassList.ShouldContain("osirion-card-grid-columns-2");
        var cards = cut.FindAll(".osirion-card-grid > .osirion-feature-card");
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
    public void Section_VariantAndEyebrow_StyleTheBand()
    {
        var cut = RenderBlocks("blocks:\n  - { type: steps, id: how, variant: dark, eyebrow: \"How *it* works\", title: Steps, items: [{ title: A }] }\n");

        var section = cut.Find("section#how");
        section.ClassList.ShouldContain("osirion-section-variant-dark");
        cut.Find(".osirion-section-eyebrow").TextContent.ShouldBe("How it works");
    }

    [Fact]
    public void Cards_ItemsAreListedOnTheCard()
    {
        var cut = RenderBlocks("blocks:\n  - { type: cards, items: [{ title: A, items: [\"One *x*\", Two] }] }\n");

        cut.FindAll(".osirion-feature-card .osirion-checklist-item-title").Select(item => item.TextContent).ShouldBe(["One x", "Two"]);
    }

    [Fact]
    public void Hero_EyebrowHighlightsAndRecordAside_RenderInTheHero()
    {
        var cut = RenderBlocks("""
            blocks:
              - type: hero
                eyebrow: "System *of* record"
                title: T
                highlights: ["Fast", "Clear"]
                image: { src: /a.png, alt: A }
                aside:
                  type: record
                  title: "Night shift"
                  status: Blocked
                  tone: danger
                  rows: [{ label: Rule, value: "11 h rest" }]
                  caption: Sample data
            """);

        cut.Find(".osirion-hero-eyebrow").TextContent.ShouldBe("System of record");
        cut.FindAll(".osirion-hero-highlights .osirion-checklist-item-title").Select(item => item.TextContent).ShouldBe(["Fast", "Clear"]);
        var record = cut.Find(".osirion-hero-aside figure.osirion-record-card");
        record.QuerySelector(".osirion-record-card-status-negative")!.TextContent.ShouldContain("Blocked");
        cut.FindAll("img.osirion-hero-featured-image").ShouldBeEmpty();
        Logs.Problems.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("neutral", "neutral")]
    [InlineData("info", "neutral")]
    [InlineData("success", "positive")]
    [InlineData("warning", "warning")]
    [InlineData("danger", "negative")]
    public void Record_ToneMapsToTheCardsStatusTone(string tone, string expected)
    {
        var cut = RenderBlocks($"blocks:\n  - {{ type: record, status: S, tone: {tone} }}\n");

        cut.Find($".osirion-record-card-status-{expected}").TextContent.ShouldContain("S");
    }

    [Fact]
    public void Record_RendersTitleRowsAndCaptionAsFootnote()
    {
        var cut = RenderBlocks("blocks:\n  - { type: record, id: sample, title: \"Order *7*\", rows: [{ label: Total, value: \"10 EUR\" }], caption: Sample data }\n");

        var card = cut.Find("figure#sample.osirion-record-card");
        card.QuerySelector(".osirion-record-card-title")!.TextContent.ShouldBe("Order 7");
        card.QuerySelector("dt")!.TextContent.ShouldBe("Total");
        card.QuerySelector("dd")!.TextContent.ShouldBe("10 EUR");
        card.QuerySelector("figcaption.osirion-record-card-footnote")!.TextContent.ShouldBe("Sample data");
    }

    [Fact]
    public void Checklist_RendersInASectionWithItsHeading()
    {
        var cut = RenderBlocks("blocks:\n  - { type: checklist, id: inc, title: Included, items: [{ text: \"One *x*\", description: More }, { text: Two }] }\n");

        var section = cut.Find("section#inc");
        section.QuerySelector("h2.osirion-section-title")!.TextContent.ShouldBe("Included");
        section.QuerySelectorAll(".osirion-checklist-title").ShouldBeEmpty();
        section.QuerySelectorAll(".osirion-checklist-item-title").Select(item => item.TextContent).ShouldBe(["One x", "Two"]);
        section.TextContent.ShouldContain("More");
    }

    [Theory]
    [InlineData("end", false)]
    [InlineData("start", true)]
    public void Split_RendersCopyAndAsideOnTheNamedSide(string position, bool start)
    {
        var cut = RenderBlocks($$"""
            blocks:
              - type: split
                title: Why
                markdown: "Copy **here**."
                aside_position: {{position}}
                aside: { type: checklist, title: Included, items: [{ text: One }] }
            """);

        var split = cut.Find(".osirion-split-section");
        split.ClassList.Contains("osirion-split-section-aside-start").ShouldBe(start);
        split.QuerySelector(".osirion-split-section-content strong")!.TextContent.ShouldBe("here");
        var aside = split.QuerySelector(".osirion-split-section-aside")!;
        aside.QuerySelector(".osirion-checklist-title")!.TextContent.ShouldBe("Included");
        aside.QuerySelectorAll("section").ShouldBeEmpty();
    }

    [Fact]
    public void Split_MetricsAside_StandsAloneWithoutASection()
    {
        var cut = RenderBlocks("blocks:\n  - { type: split, markdown: Copy, aside: { type: metrics, title: Results, items: [{ value: \"9\" }] } }\n");

        var aside = cut.Find(".osirion-split-section-aside");
        aside.QuerySelector("ul.osirion-metric-grid")!.GetAttribute("aria-label").ShouldBe("Results");
        aside.QuerySelectorAll("section").ShouldBeEmpty();
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
