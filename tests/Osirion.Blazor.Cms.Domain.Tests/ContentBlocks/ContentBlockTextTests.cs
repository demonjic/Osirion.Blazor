using Osirion.Blazor.Cms.Domain.ContentBlocks;
using Osirion.Blazor.Cms.Domain.Entities;
using Shouldly;

namespace Osirion.Blazor.Cms.Domain.Tests.ContentBlocks;

/// <summary>
/// <see cref="ContentBlockText.Map"/> changes the prose of blocks and leaves identifiers, choices and addresses alone.
/// </summary>
public class ContentBlockTextTests
{
    private static string Upper(string text) => text.ToUpperInvariant();

    [Fact]
    public void Map_ChangesProseAndKeepsEverythingElse()
    {
        var hero = new HeroBlock
        {
            Type = "hero",
            Id = "top",
            Variant = ContentBlockVariant.Dark,
            Eyebrow = "e",
            Title = "t",
            Description = "d",
            Subtitle = "s",
            Highlights = ["h"],
            PrimaryAction = new ContentBlockAction("go", "/go"),
            Image = new ContentBlockImage("/a.png", "alt"),
            Aside = new RecordBlock { Type = "record", Status = "st", Tone = ContentBlockTone.Danger, Rows = [new RecordBlockRow("l", "v")], Caption = "c" },
        };
        var cards = new CardsBlock
        {
            Type = "cards",
            Columns = 4,
            Items = [new CardsBlockItem { Title = "t", Description = "d", Icon = "icon", Url = "/u", LinkText = "lt", Items = ["i"] }],
        };
        var component = new ComponentBlock { Type = "component", Name = "tour", Parameters = new Dictionary<string, string> { ["a"] = "b" } };
        var set = new ContentBlockSet(
            [
                hero,
                cards,
                new StepsBlock { Type = "steps", Items = [new StepsBlockItem("t", "d")] },
                new ChecklistBlock { Type = "checklist", Items = [new ChecklistBlockItem("t", "d")] },
                new MetricsBlock { Type = "metrics", Items = [new MetricsBlockItem("v", "l", "d")] },
                new FaqBlock { Type = "faq", Items = [new FaqBlockItem("q", "a")] },
                new CtaBlock { Type = "cta", SecondaryAction = new ContentBlockAction("go", "https://example.com") },
                new ImageBlock { Type = "image", Src = "/i.png", Alt = "alt", Caption = "c" },
                new MarkdownBlock { Type = "markdown", Markdown = "m" },
                new SectionBlock { Type = "section", Markdown = "m" },
                new SplitBlock { Type = "split", Markdown = "m", Aside = new MarkdownBlock { Type = "markdown", Markdown = "x" } },
                component,
            ],
            ContentBodyPlacement.AfterBlocks);

        var mapped = ContentBlockText.Map(set, Upper);

        mapped.Body.ShouldBe(ContentBodyPlacement.AfterBlocks);
        var mappedHero = mapped.Blocks[0].ShouldBeOfType<HeroBlock>();
        (mappedHero.Type, mappedHero.Id, mappedHero.Variant).ShouldBe(("hero", "top", ContentBlockVariant.Dark));
        (mappedHero.Eyebrow, mappedHero.Title, mappedHero.Description, mappedHero.Subtitle).ShouldBe(("E", "T", "D", "S"));
        mappedHero.Highlights.ShouldBe(["H"]);
        mappedHero.PrimaryAction.ShouldBe(new ContentBlockAction("GO", "/go"));
        mappedHero.Image.ShouldBe(new ContentBlockImage("/a.png", "ALT"));
        var record = mappedHero.Aside.ShouldBeOfType<RecordBlock>();
        (record.Status, record.Tone, record.Caption).ShouldBe(("ST", ContentBlockTone.Danger, "C"));
        record.Rows.ShouldBe([new RecordBlockRow("L", "V")]);
        var card = mapped.Blocks[1].ShouldBeOfType<CardsBlock>().Items.ShouldHaveSingleItem();
        (card.Title, card.Description, card.Icon, card.Url, card.LinkText).ShouldBe(("T", "D", "icon", "/u", "LT"));
        card.Items.ShouldBe(["I"]);
        mapped.Blocks[1].ShouldBeOfType<CardsBlock>().Columns.ShouldBe(4);
        mapped.Blocks[2].ShouldBeOfType<StepsBlock>().Items.ShouldBe([new StepsBlockItem("T", "D")]);
        mapped.Blocks[3].ShouldBeOfType<ChecklistBlock>().Items.ShouldBe([new ChecklistBlockItem("T", "D")]);
        mapped.Blocks[4].ShouldBeOfType<MetricsBlock>().Items.ShouldBe([new MetricsBlockItem("V", "L", "D")]);
        mapped.Blocks[5].ShouldBeOfType<FaqBlock>().Items.ShouldBe([new FaqBlockItem("Q", "A")]);
        mapped.Blocks[6].ShouldBeOfType<CtaBlock>().SecondaryAction.ShouldBe(new ContentBlockAction("GO", "https://example.com"));
        var image = mapped.Blocks[7].ShouldBeOfType<ImageBlock>();
        (image.Src, image.Alt, image.Caption).ShouldBe(("/i.png", "ALT", "C"));
        mapped.Blocks[8].ShouldBeOfType<MarkdownBlock>().Markdown.ShouldBe("M");
        mapped.Blocks[9].ShouldBeOfType<SectionBlock>().Markdown.ShouldBe("M");
        mapped.Blocks[10].ShouldBeOfType<SplitBlock>().Aside.ShouldBeOfType<MarkdownBlock>().Markdown.ShouldBe("X");
        var mappedComponent = mapped.Blocks[11].ShouldBeOfType<ComponentBlock>();
        mappedComponent.Name.ShouldBe("tour");
        mappedComponent.Parameters["a"].ShouldBe("b");
        set.Blocks[0].ShouldBeSameAs(hero);
        hero.Title.ShouldBe("t");
    }

    [Fact]
    public void ContentItem_CloneAndSetBlocks_CarryTheBlocks()
    {
        var item = ContentItem.Create("1", "Home", string.Empty, "en/home.md", "test");
        var set = new ContentBlockSet([new MarkdownBlock { Type = "markdown", Markdown = "x" }], ContentBodyPlacement.AfterBlocks);

        item.HasBlocks.ShouldBeFalse();
        item.SetBlocks(set);
        var clone = item.Clone();

        clone.HasBlocks.ShouldBeTrue();
        clone.Blocks.ShouldBe(set.Blocks);
        clone.BodyPlacement.ShouldBe(ContentBodyPlacement.AfterBlocks);
    }
}
