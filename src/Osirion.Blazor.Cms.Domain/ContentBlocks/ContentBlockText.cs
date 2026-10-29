namespace Osirion.Blazor.Cms.Domain.ContentBlocks;

/// <summary>
/// Changes the reader-facing text of blocks, for example to transliterate a derived locale's copy, and leaves every
/// value that is not prose as written.
/// </summary>
/// <remarks>
/// Changed: titles, descriptions, eyebrows, subtitles, highlights, item texts, labels, values, captions, questions,
/// answers, statuses, link and action texts, image alternatives and <c>markdown</c>. Left as written: <c>type</c>,
/// <c>id</c>, <c>variant</c>, <c>tone</c>, <c>columns</c>, <c>icon</c>, <c>name</c>, every <c>url</c> and <c>src</c>,
/// a component's <c>parameters</c>, and the values of a <see cref="CustomContentBlock"/>, whose meaning only the
/// site's renderer knows. Inside a value, only the prose changes, as <c>HtmlTransliteration</c> treats rendered HTML:
/// code spans, code blocks, raw HTML, autolinks and link destinations stay as written.
/// </remarks>
public static class ContentBlockText
{
    /// <summary>Returns <paramref name="blocks"/> with every text value passed through <paramref name="map"/>.</summary>
    /// <param name="blocks">The blocks to copy.</param>
    /// <param name="map">The change applied to each text value.</param>
    /// <returns>A new set; <paramref name="blocks"/> is not changed.</returns>
    public static ContentBlockSet Map(ContentBlockSet blocks, Func<string, string> map)
    {
        ArgumentNullException.ThrowIfNull(blocks);
        ArgumentNullException.ThrowIfNull(map);

        // Every value is plain text or Markdown, so only its prose changes: code, raw HTML and link destinations stay.
        string Prose(string text) => MarkdownProse.Map(text, map);
        return blocks with { Blocks = blocks.Blocks.Select(block => MapBlock(block, Prose)).ToList() };
    }

    private static ContentBlock MapBlock(ContentBlock block, Func<string, string> map)
    {
        var mapped = block with
        {
            Eyebrow = Optional(block.Eyebrow, map),
            Title = Optional(block.Title, map),
            Description = Optional(block.Description, map),
        };

        return mapped switch
        {
            HeroBlock hero => hero with
            {
                Subtitle = Optional(hero.Subtitle, map),
                Highlights = hero.Highlights.Select(map).ToList(),
                PrimaryAction = Action(hero.PrimaryAction, map),
                SecondaryAction = Action(hero.SecondaryAction, map),
                Image = hero.Image is { } image ? image with { Alt = map(image.Alt) } : null,
                Aside = hero.Aside is { } aside ? MapBlock(aside, map) : null,
            },
            SectionBlock section => section with { Markdown = Optional(section.Markdown, map) },
            SplitBlock split => split with
            {
                Markdown = Optional(split.Markdown, map),
                Aside = split.Aside is { } aside ? MapBlock(aside, map) : null,
            },
            StepsBlock steps => steps with
            {
                Items = steps.Items.Select(item => new StepsBlockItem(map(item.Title), Optional(item.Description, map))).ToList(),
            },
            CardsBlock cards => cards with
            {
                Items = cards.Items.Select(item => item with
                {
                    Title = map(item.Title),
                    Description = Optional(item.Description, map),
                    LinkText = Optional(item.LinkText, map),
                    Items = item.Items.Select(map).ToList(),
                }).ToList(),
            },
            ChecklistBlock checklist => checklist with
            {
                Items = checklist.Items.Select(item => new ChecklistBlockItem(map(item.Text), Optional(item.Description, map))).ToList(),
            },
            MetricsBlock metrics => metrics with
            {
                Items = metrics.Items
                    .Select(item => new MetricsBlockItem(map(item.Value), Optional(item.Label, map), Optional(item.Description, map)))
                    .ToList(),
            },
            FaqBlock faq => faq with
            {
                Items = faq.Items.Select(item => new FaqBlockItem(map(item.Question), map(item.Answer))).ToList(),
            },
            CtaBlock cta => cta with
            {
                PrimaryAction = Action(cta.PrimaryAction, map),
                SecondaryAction = Action(cta.SecondaryAction, map),
            },
            RecordBlock record => record with
            {
                Status = Optional(record.Status, map),
                Rows = record.Rows.Select(row => new RecordBlockRow(map(row.Label), map(row.Value))).ToList(),
                Caption = Optional(record.Caption, map),
            },
            ImageBlock image => image with { Alt = map(image.Alt), Caption = Optional(image.Caption, map) },
            MarkdownBlock markdown => markdown with { Markdown = map(markdown.Markdown) },
            _ => mapped,
        };
    }

    private static ContentBlockAction? Action(ContentBlockAction? action, Func<string, string> map) =>
        action is null ? null : action with { Text = map(action.Text) };

    private static string? Optional(string? value, Func<string, string> map) =>
        value is null ? null : map(value);
}
