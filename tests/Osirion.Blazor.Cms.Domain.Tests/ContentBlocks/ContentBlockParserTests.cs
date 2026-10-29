using Osirion.Blazor.Cms.Domain.ContentBlocks;
using Osirion.Blazor.Cms.Domain.Exceptions;
using Osirion.Blazor.Cms.Domain.ValueObjects;
using Shouldly;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Osirion.Blazor.Cms.Domain.Tests.ContentBlocks;

/// <summary>
/// Reading the <c>blocks</c> front matter of the content blocks contract, from YAML deserialized the way
/// <c>MarkdownProcessor</c> deserializes front matter.
/// </summary>
public class ContentBlockParserTests
{
    private const string SourcePath = "en/home.md";

    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    private static ContentBlockSet Parse(string yaml) =>
        ContentBlockParser.Parse(Deserializer.Deserialize<FrontMatter>(yaml), SourcePath);

    private static ContentBlockException Fails(string yaml) =>
        Should.Throw<ContentBlockException>(() => Parse(yaml));

    private static T Single<T>(string yaml) where T : ContentBlock =>
        Parse(yaml).Blocks.ShouldHaveSingleItem().ShouldBeOfType<T>();

    [Fact]
    public void Parse_NoBlocks_ReturnsTheEmptySet()
    {
        Parse("title: Plain page\nbody: after\n").ShouldBeSameAs(ContentBlockSet.Empty);
        ContentBlockParser.Parse(null, SourcePath).ShouldBeSameAs(ContentBlockSet.Empty);
    }

    [Fact]
    public void Parse_TheContractExample_ReadsEveryKey()
    {
        var set = Parse("""
            title: "Decisions"
            blocks:
              - type: hero
                id: top
                variant: dark
                eyebrow: "System of record"
                title: "Every decision, recorded"
                description: "Lead *text*."
                subtitle: "Access and attendance."
                highlights: ["One", "Two"]
                primary_action: { text: "Book a call", url: "/contact" }
                secondary_action: { text: "Tour", url: "https://example.com/tour" }
                aside:
                  type: record
                  title: "Night shift"
                  status: "Blocked"
                  tone: danger
                  rows:
                    - { label: "Rule", value: "11 h rest" }
                  caption: "Sample data"
              - type: steps
                title: "How it moves"
                items:
                  - { title: "Made", description: "First." }
                  - { title: "Recorded" }
            """);

        set.Body.ShouldBe(ContentBodyPlacement.Omitted);
        set.Blocks.Count.ShouldBe(2);
        var hero = set.Blocks[0].ShouldBeOfType<HeroBlock>();
        hero.Type.ShouldBe("hero");
        hero.Index.ShouldBe(0);
        hero.Id.ShouldBe("top");
        hero.Variant.ShouldBe(ContentBlockVariant.Dark);
        hero.Eyebrow.ShouldBe("System of record");
        hero.Title.ShouldBe("Every decision, recorded");
        hero.Description.ShouldBe("Lead *text*.");
        hero.Subtitle.ShouldBe("Access and attendance.");
        hero.Highlights.ShouldBe(["One", "Two"]);
        hero.PrimaryAction.ShouldBe(new ContentBlockAction("Book a call", "/contact"));
        hero.SecondaryAction.ShouldBe(new ContentBlockAction("Tour", "https://example.com/tour"));
        var record = hero.Aside.ShouldBeOfType<RecordBlock>();
        record.Index.ShouldBe(0);
        record.Status.ShouldBe("Blocked");
        record.Tone.ShouldBe(ContentBlockTone.Danger);
        record.Rows.ShouldBe([new RecordBlockRow("Rule", "11 h rest")]);
        record.Caption.ShouldBe("Sample data");
        var steps = set.Blocks[1].ShouldBeOfType<StepsBlock>();
        steps.Index.ShouldBe(1);
        steps.Items.ShouldBe([new StepsBlockItem("Made", "First."), new StepsBlockItem("Recorded", null)]);
    }

    [Fact]
    public void Parse_BodyAfter_PlacesTheBodyAfterTheBlocks()
    {
        Parse("body: after\nblocks:\n  - { type: markdown, markdown: \"Hi\" }\n").Body.ShouldBe(ContentBodyPlacement.AfterBlocks);
    }

    [Fact]
    public void Parse_Section_ReadsItsMarkdown()
    {
        Single<SectionBlock>("blocks:\n  - type: section\n    title: Why\n    markdown: |\n      Para one.\n\n      - item\n")
            .Markdown.ShouldBe("Para one.\n\n- item\n");
    }

    [Fact]
    public void Parse_Split_ReadsItsAsideAndPosition()
    {
        var split = Single<SplitBlock>("""
            blocks:
              - type: split
                markdown: "Copy"
                aside_position: start
                aside: { type: checklist, items: [{ text: "One", description: "More" }] }
            """);

        split.Markdown.ShouldBe("Copy");
        split.AsidePosition.ShouldBe(ContentBlockAsidePosition.Start);
        split.Aside.ShouldBeOfType<ChecklistBlock>().Items.ShouldBe([new ChecklistBlockItem("One", "More")]);
    }

    [Fact]
    public void Parse_Split_DefaultsTheAsideToTheEnd()
    {
        Single<SplitBlock>("blocks:\n  - { type: split, markdown: Copy }\n").AsidePosition.ShouldBe(ContentBlockAsidePosition.End);
    }

    [Fact]
    public void Parse_Cards_ReadsColumnsAndEveryCardKey()
    {
        var cards = Single<CardsBlock>("""
            blocks:
              - type: cards
                columns: 4
                items:
                  - title: "Govern"
                    description: "Rules."
                    icon: shield
                    url: /platform#govern
                    link_text: "Read more"
                    items: [ "A", "B" ]
            """);

        cards.Columns.ShouldBe(4);
        var card = cards.Items.ShouldHaveSingleItem();
        card.Title.ShouldBe("Govern");
        card.Description.ShouldBe("Rules.");
        card.Icon.ShouldBe("shield");
        card.Url.ShouldBe("/platform#govern");
        card.LinkText.ShouldBe("Read more");
        card.Items.ShouldBe(["A", "B"]);
    }

    [Fact]
    public void Parse_Cards_DefaultsToThreeColumns()
    {
        Single<CardsBlock>("blocks:\n  - { type: cards, items: [{ title: A }] }\n").Columns.ShouldBe(3);
    }

    [Fact]
    public void Parse_MetricsFaqCtaImageMarkdown_ReadTheirKeys()
    {
        var set = Parse("""
            blocks:
              - type: metrics
                items: [{ value: "99%", label: "Uptime", description: "Last year" }]
              - type: faq
                items: [{ question: "Why?", answer: "Because **so**." }]
              - type: cta
                title: "Start"
                primary_action: { text: "Go", url: "mailto:sales@example.com" }
                secondary_action: { text: "Call", url: "tel:+381111" }
              - type: image
                src: /images/a.png
                alt: ""
                caption: "A *caption*"
              - type: markdown
                markdown: "Text"
            """);

        set.Blocks[0].ShouldBeOfType<MetricsBlock>().Items.ShouldBe([new MetricsBlockItem("99%", "Uptime", "Last year")]);
        set.Blocks[1].ShouldBeOfType<FaqBlock>().Items.ShouldBe([new FaqBlockItem("Why?", "Because **so**.")]);
        var cta = set.Blocks[2].ShouldBeOfType<CtaBlock>();
        cta.PrimaryAction!.Url.ShouldBe("mailto:sales@example.com");
        cta.SecondaryAction!.Url.ShouldBe("tel:+381111");
        var image = set.Blocks[3].ShouldBeOfType<ImageBlock>();
        image.Src.ShouldBe("/images/a.png");
        image.Alt.ShouldBe(string.Empty);
        image.Caption.ShouldBe("A *caption*");
        set.Blocks[4].ShouldBeOfType<MarkdownBlock>().Markdown.ShouldBe("Text");
    }

    [Fact]
    public void Parse_Record_DefaultsToNeutral()
    {
        Single<RecordBlock>("blocks:\n  - { type: record, title: R }\n").Tone.ShouldBe(ContentBlockTone.Neutral);
    }

    [Fact]
    public void Parse_HeroImage_ReadsSourceAndAlternative()
    {
        Single<HeroBlock>("blocks:\n  - { type: hero, title: T, image: { src: \"https://cdn.example.com/a.webp\", alt: Plant } }\n")
            .Image.ShouldBe(new ContentBlockImage("https://cdn.example.com/a.webp", "Plant"));
    }

    [Fact]
    public void Parse_Component_ReadsNameAndParametersAsText()
    {
        var component = Single<ComponentBlock>("blocks:\n  - type: component\n    name: product-tour\n    parameters: { start: 2, mode: compact }\n");

        component.Name.ShouldBe("product-tour");
        component.Parameters.ShouldBe(new Dictionary<string, string> { ["start"] = "2", ["mode"] = "compact" });
    }

    [Fact]
    public void Parse_UnknownType_KeepsItsKeysForTheSite()
    {
        var custom = Single<CustomContentBlock>("blocks:\n  - type: pricing-table\n    id: prices\n    title: Prices\n    plans: [ a, b ]\n");

        custom.Type.ShouldBe("pricing-table");
        custom.Id.ShouldBe("prices");
        custom.Title.ShouldBe("Prices");
        custom.Values.Keys.ShouldBe(["plans"]);
    }

    [Fact]
    public void Parse_InvalidBlocks_NamesTheFileTheBlockAndTheKey()
    {
        var exception = Fails("blocks:\n  - { type: hero, title: T }\n  - { type: cta, primary_action: { text: Go, url: \"javascript:alert(1)\" } }\n");

        var error = exception.Errors.ShouldHaveSingleItem();
        error.SourcePath.ShouldBe(SourcePath);
        error.BlockIndex.ShouldBe(1);
        error.Key.ShouldBe("primary_action.url");
        exception.SourcePath.ShouldBe(SourcePath);
        exception.Message.ShouldContain(SourcePath);
        exception.Message.ShouldContain("blocks[1].primary_action.url");
        error.ToString().ShouldStartWith("en/home.md: blocks[1].primary_action.url: 'javascript:alert(1)' is not an allowed link");
    }

    [Fact]
    public void Parse_SeveralProblems_ReportsEveryOne()
    {
        var exception = Fails("blocks:\n  - { title: No type }\n  - { type: steps }\n  - { type: cards, columns: 5, items: [{ title: A }] }\n");

        exception.Errors.Select(error => error.Location).ShouldBe(["blocks[0].type", "blocks[1].items", "blocks[2].columns"]);
    }

    [Theory]
    [InlineData("type: Hero", "type")]
    [InlineData("type: hero_x", "type")]
    [InlineData("type: hero\n    id: Top", "id")]
    [InlineData("type: hero\n    id: \"a b\"", "id")]
    [InlineData("type: hero\n    variant: light", "variant")]
    [InlineData("type: record\n    tone: error", "tone")]
    [InlineData("type: cards\n    columns: 1\n    items: [{ title: A }]", "columns")]
    [InlineData("type: split\n    aside_position: left", "aside_position")]
    [InlineData("type: hero\n    primary_acton: { text: A, url: /a }", "primary_acton")]
    [InlineData("type: steps\n    items: [{ description: x }]", "items[0].title")]
    [InlineData("type: steps\n    items: [{ title: A, extra: x }]", "items[0].extra")]
    [InlineData("type: faq\n    items: [{ question: Q }]", "items[0].answer")]
    [InlineData("type: metrics\n    items: [{ label: L }]", "items[0].value")]
    [InlineData("type: checklist\n    items: [{ description: d }]", "items[0].text")]
    [InlineData("type: record\n    rows: [{ label: L }]", "rows[0].value")]
    [InlineData("type: cta\n    primary_action: { text: Go }", "primary_action.url")]
    [InlineData("type: cta\n    primary_action: { url: /a }", "primary_action.text")]
    [InlineData("type: image\n    src: /a.png", "alt")]
    [InlineData("type: image\n    alt: A", "src")]
    [InlineData("type: markdown", "markdown")]
    [InlineData("type: component", "name")]
    [InlineData("type: component\n    name: x\n    parameters: { a: [1, 2] }", "parameters.a")]
    [InlineData("type: hero\n    aside: { type: steps, items: [{ title: A }] }", "aside.type")]
    [InlineData("type: split\n    aside: { type: hero }", "aside.type")]
    [InlineData("type: hero\n    aside: { title: A }", "aside.type")]
    [InlineData("type: hero\n    title: [a, b]", "title")]
    [InlineData("type: hero\n    highlights: one", "highlights")]
    [InlineData("type: steps\n    items: one", "items")]
    public void Parse_BrokenRule_ReportsTheKey(string block, string key)
    {
        var error = Fails($"blocks:\n  - {block}\n").Errors.ShouldHaveSingleItem();

        error.BlockIndex.ShouldBe(0);
        error.Key.ShouldBe(key);
    }

    [Fact]
    public void Parse_DuplicateId_ReportsTheLaterBlock()
    {
        var error = Fails("blocks:\n  - { type: markdown, id: a, markdown: x }\n  - { type: markdown, id: a, markdown: y }\n")
            .Errors.ShouldHaveSingleItem();

        error.BlockIndex.ShouldBe(1);
        error.Key.ShouldBe("id");
    }

    [Fact]
    public void Parse_BlocksNotAList_ReportsTheBlocksKey()
    {
        var error = Fails("blocks: hero\n").Errors.ShouldHaveSingleItem();

        error.BlockIndex.ShouldBeNull();
        error.Location.ShouldBe("blocks");
    }

    [Fact]
    public void Parse_BlockNotAMap_ReportsTheBlock()
    {
        Fails("blocks:\n  - hero\n").Errors.ShouldHaveSingleItem().Location.ShouldBe("blocks[0]");
    }

    [Fact]
    public void Parse_UnknownBodyValue_ReportsTheBodyKey()
    {
        Fails("body: before\nblocks:\n  - { type: markdown, markdown: x }\n").Errors.ShouldHaveSingleItem().Location.ShouldBe("body");
    }

    [Theory]
    [InlineData("/platform")]
    [InlineData("/platform#govern")]
    [InlineData("#pricing")]
    [InlineData("https://example.com/a?b=c")]
    [InlineData("HTTPS://example.com")]
    [InlineData("mailto:sales@example.com")]
    [InlineData("tel:+381111222")]
    public void Parse_AllowedLink_IsKept(string url)
    {
        Single<CtaBlock>($"blocks:\n  - {{ type: cta, primary_action: {{ text: Go, url: \"{url}\" }} }}\n")
            .PrimaryAction!.Url.ShouldBe(url);
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("JavaScript:alert(1)")]
    [InlineData("java\\tscript:alert(1)")]
    [InlineData("data:text/html,<b>x</b>")]
    [InlineData("vbscript:x")]
    [InlineData("//evil.example.com")]
    [InlineData("/\\\\evil.example.com")]
    [InlineData("http://example.com")]
    [InlineData("https://")]
    [InlineData("platform")]
    [InlineData("mailto:")]
    [InlineData("/a b")]
    [InlineData("#")]
    public void Parse_RefusedLink_IsAnError(string url)
    {
        var error = Fails($"blocks:\n  - {{ type: cta, primary_action: {{ text: Go, url: \"{url}\" }} }}\n").Errors.ShouldHaveSingleItem();

        error.Key.ShouldBe("primary_action.url");
        error.Message.ShouldContain("is not an allowed link");
    }

    [Theory]
    [InlineData("mailto:a@example.com")]
    [InlineData("tel:+1")]
    [InlineData("#top")]
    [InlineData("data:image/png;base64,AAAA")]
    [InlineData("javascript:alert(1)")]
    public void Parse_RefusedImageSource_IsAnError(string src)
    {
        var error = Fails($"blocks:\n  - {{ type: image, src: \"{src}\", alt: A }}\n").Errors.ShouldHaveSingleItem();

        error.Key.ShouldBe("src");
    }

    [Fact]
    public void Parse_CardUrl_FollowsTheLinkRules()
    {
        Fails("blocks:\n  - { type: cards, items: [{ title: A, url: \"//evil.example.com\" }] }\n")
            .Errors.ShouldHaveSingleItem().Key.ShouldBe("items[0].url");
    }
}
