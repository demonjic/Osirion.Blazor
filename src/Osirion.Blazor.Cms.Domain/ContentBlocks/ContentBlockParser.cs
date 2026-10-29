using System.Collections;
using Osirion.Blazor.Cms.Domain.Exceptions;
using Osirion.Blazor.Cms.Domain.ValueObjects;

namespace Osirion.Blazor.Cms.Domain.ContentBlocks;

/// <summary>
/// Reads and validates the <c>blocks</c> and <c>body</c> keys of a content item's front matter.
/// </summary>
/// <remarks>
/// <para>
/// Every problem of a file is collected and thrown together in one <see cref="ContentBlockException"/>, each naming the
/// file, the block's position and the key. The checks: <c>type</c> is required and lower case with hyphens; <c>id</c>
/// matches <c>[a-z0-9-]+</c> and is unique in the file; <c>variant</c>, <c>tone</c>, <c>columns</c>,
/// <c>aside_position</c> and <c>body</c> take only their listed values; links follow <see cref="ContentBlockLinks"/>;
/// required keys are present; a key a block type does not have is refused, so a misspelt key is not silently ignored.
/// </para>
/// <para>
/// A type the library does not define is read as a <see cref="CustomContentBlock"/>: its common keys are checked and
/// its other keys are kept for the site's renderer.
/// </para>
/// </remarks>
public static class ContentBlockParser
{
    private static readonly IReadOnlyDictionary<string, ContentBlockVariant> Variants = new Dictionary<string, ContentBlockVariant>(StringComparer.Ordinal)
    {
        ["default"] = ContentBlockVariant.Default,
        ["tinted"] = ContentBlockVariant.Tinted,
        ["dark"] = ContentBlockVariant.Dark,
    };

    private static readonly IReadOnlyDictionary<string, ContentBlockTone> Tones = new Dictionary<string, ContentBlockTone>(StringComparer.Ordinal)
    {
        ["neutral"] = ContentBlockTone.Neutral,
        ["success"] = ContentBlockTone.Success,
        ["warning"] = ContentBlockTone.Warning,
        ["danger"] = ContentBlockTone.Danger,
        ["info"] = ContentBlockTone.Info,
    };

    private static readonly IReadOnlyDictionary<string, int> Columns = new Dictionary<string, int>(StringComparer.Ordinal)
    {
        ["2"] = 2,
        ["3"] = 3,
        ["4"] = 4,
    };

    private static readonly IReadOnlyDictionary<string, ContentBlockAsidePosition> AsidePositions = new Dictionary<string, ContentBlockAsidePosition>(StringComparer.Ordinal)
    {
        ["end"] = ContentBlockAsidePosition.End,
        ["start"] = ContentBlockAsidePosition.Start,
    };

    private static readonly IReadOnlyDictionary<string, ContentBodyPlacement> BodyPlacements = new Dictionary<string, ContentBodyPlacement>(StringComparer.Ordinal)
    {
        ["after"] = ContentBodyPlacement.AfterBlocks,
    };

    private static readonly string[] HeroAsideTypes = [ContentBlockTypes.Record, ContentBlockTypes.Image];

    private static readonly string[] SplitAsideTypes =
        [ContentBlockTypes.Record, ContentBlockTypes.Checklist, ContentBlockTypes.Image, ContentBlockTypes.Metrics, ContentBlockTypes.Markdown];

    /// <summary>Reads the blocks of <paramref name="frontMatter"/>.</summary>
    /// <param name="frontMatter">The item's front matter, or <see langword="null"/> when it has none.</param>
    /// <param name="sourcePath">The item's path, named in every problem.</param>
    /// <returns>The blocks, or <see cref="ContentBlockSet.Empty"/> when the front matter has no <c>blocks</c>.</returns>
    /// <exception cref="ContentBlockException">The blocks break a rule; the message lists every problem.</exception>
    public static ContentBlockSet Parse(FrontMatter? frontMatter, string sourcePath) =>
        Parse(frontMatter?.Blocks, frontMatter?.Body, sourcePath);

    /// <summary>Reads blocks from the untyped YAML of the <c>blocks</c> key and the text of the <c>body</c> key.</summary>
    /// <param name="blocks">The <c>blocks</c> node as YamlDotNet reads it into <see cref="object"/>.</param>
    /// <param name="body">The <c>body</c> value; only <c>after</c> is allowed.</param>
    /// <param name="sourcePath">The item's path, named in every problem.</param>
    /// <returns>The blocks, or <see cref="ContentBlockSet.Empty"/> when <paramref name="blocks"/> is null.</returns>
    /// <exception cref="ContentBlockException">The blocks break a rule; the message lists every problem.</exception>
    public static ContentBlockSet Parse(object? blocks, string? body, string sourcePath)
    {
        if (blocks is null)
        {
            return ContentBlockSet.Empty;
        }

        var errors = new List<ContentBlockError>();
        var placement = ReadBodyPlacement(body, sourcePath, errors);

        if (blocks is not IList list || blocks is string)
        {
            errors.Add(new ContentBlockError(sourcePath, null, "blocks", "must be a list of blocks, each a map with a type"));
            throw new ContentBlockException(sourcePath, errors);
        }

        var parsed = new List<ContentBlock>(list.Count);
        for (var index = 0; index < list.Count; index++)
        {
            var scope = new ContentBlockFieldsScope(sourcePath, index, string.Empty, errors);
            if (ContentBlockFields.FromNode(list[index], scope) is not { } fields)
            {
                errors.Add(new ContentBlockError(sourcePath, index, string.Empty, "must be a map of keys and values with a type"));
                continue;
            }

            if (ReadBlock(fields, allowedTypes: null) is { } block)
            {
                parsed.Add(block);
            }
        }

        RejectDuplicateIds(parsed, sourcePath, errors);

        if (errors.Count > 0)
        {
            throw new ContentBlockException(sourcePath, errors);
        }

        return new ContentBlockSet(parsed, placement);
    }

    private static ContentBodyPlacement ReadBodyPlacement(string? body, string sourcePath, List<ContentBlockError> errors)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return ContentBodyPlacement.Omitted;
        }

        if (BodyPlacements.TryGetValue(body, out var placement))
        {
            return placement;
        }

        errors.Add(new ContentBlockError(sourcePath, null, "body", $"'{body}' is not one of: {string.Join(", ", BodyPlacements.Keys)}"));
        return ContentBodyPlacement.Omitted;
    }

    private static void RejectDuplicateIds(List<ContentBlock> blocks, string sourcePath, List<ContentBlockError> errors)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var block in blocks.Where(block => block.Id is not null))
        {
            if (!seen.Add(block.Id!))
            {
                errors.Add(new ContentBlockError(sourcePath, block.Index, "id", $"'{block.Id}' is already the id of an earlier block"));
            }
        }
    }

    // Reads one block; allowedTypes limits the types of a nested block (an aside), null allows every type.
    private static ContentBlock? ReadBlock(ContentBlockFields fields, string[]? allowedTypes)
    {
        var type = fields.RequiredText("type");
        if (type is null)
        {
            return null;
        }

        if (!IsTypeName(type))
        {
            fields.Error("type", $"'{type}' must be lower case letters and digits, words joined by hyphens");
            return null;
        }

        if (allowedTypes is not null && !allowedTypes.Contains(type, StringComparer.Ordinal))
        {
            fields.Error("type", $"'{type}' cannot be used here; use one of: {string.Join(", ", allowedTypes)}");
            return null;
        }

        var block = ReadTypeKeys(fields, type);
        var common = block with
        {
            Index = fields.BlockIndex,
            Id = ReadId(fields),
            Variant = fields.Choice("variant", Variants, ContentBlockVariant.Default),
            Eyebrow = fields.Text("eyebrow"),
            Title = fields.Text("title"),
            Description = fields.Text("description"),
        };

        if (common is not CustomContentBlock)
        {
            fields.RejectUnreadKeys($"a {type} block");
        }

        return common;
    }

    private static ContentBlock ReadTypeKeys(ContentBlockFields fields, string type) => type switch
    {
        ContentBlockTypes.Hero => ReadHero(fields),
        ContentBlockTypes.Section => new SectionBlock { Type = type, Markdown = fields.Text("markdown") },
        ContentBlockTypes.Split => ReadSplit(fields),
        ContentBlockTypes.Steps => new StepsBlock { Type = type, Items = ReadItems(fields, ReadStep) },
        ContentBlockTypes.Cards => ReadCards(fields),
        ContentBlockTypes.Checklist => new ChecklistBlock { Type = type, Items = ReadItems(fields, ReadChecklistItem) },
        ContentBlockTypes.Metrics => new MetricsBlock { Type = type, Items = ReadItems(fields, ReadMetric) },
        ContentBlockTypes.Faq => new FaqBlock { Type = type, Items = ReadItems(fields, ReadQuestion) },
        ContentBlockTypes.Cta => new CtaBlock
        {
            Type = type,
            PrimaryAction = ReadAction(fields, "primary_action"),
            SecondaryAction = ReadAction(fields, "secondary_action"),
        },
        ContentBlockTypes.Record => ReadRecord(fields),
        ContentBlockTypes.Image => ReadImageBlock(fields),
        ContentBlockTypes.Markdown => new MarkdownBlock { Type = type, Markdown = fields.RequiredText("markdown") ?? string.Empty },
        ContentBlockTypes.Component => ReadComponent(fields),
        _ => ReadCustom(fields, type),
    };

    private static HeroBlock ReadHero(ContentBlockFields fields) => new()
    {
        Type = ContentBlockTypes.Hero,
        Subtitle = fields.Text("subtitle"),
        Highlights = fields.TextList("highlights"),
        PrimaryAction = ReadAction(fields, "primary_action"),
        SecondaryAction = ReadAction(fields, "secondary_action"),
        Image = ReadImage(fields, "image"),
        Aside = ReadAside(fields, HeroAsideTypes),
    };

    private static SplitBlock ReadSplit(ContentBlockFields fields) => new()
    {
        Type = ContentBlockTypes.Split,
        Markdown = fields.Text("markdown"),
        Aside = ReadAside(fields, SplitAsideTypes),
        AsidePosition = fields.Choice("aside_position", AsidePositions, ContentBlockAsidePosition.End),
    };

    private static CardsBlock ReadCards(ContentBlockFields fields) => new()
    {
        Type = ContentBlockTypes.Cards,
        Columns = fields.Choice("columns", Columns, CardsBlock.DefaultColumns),
        Items = ReadItems(fields, ReadCard),
    };

    private static RecordBlock ReadRecord(ContentBlockFields fields) => new()
    {
        Type = ContentBlockTypes.Record,
        Status = fields.Text("status"),
        Tone = fields.Choice("tone", Tones, ContentBlockTone.Neutral),
        Rows = ReadList(fields, "rows", required: false, ReadRow),
        Caption = fields.Text("caption"),
    };

    private static ImageBlock ReadImageBlock(ContentBlockFields fields) => new()
    {
        Type = ContentBlockTypes.Image,
        Src = fields.ImageSource("src") ?? string.Empty,
        Alt = ReadAlt(fields) ?? string.Empty,
        Caption = fields.Text("caption"),
    };

    private static ComponentBlock ReadComponent(ContentBlockFields fields) => new()
    {
        Type = ContentBlockTypes.Component,
        Name = fields.RequiredText("name") ?? string.Empty,
        Parameters = ReadParameters(fields),
    };

    // The common keys are read after the type's keys, so they are left out of Values.
    private static CustomContentBlock ReadCustom(ContentBlockFields fields, string type)
    {
        string[] commonKeys = ["type", "id", "variant", "eyebrow", "title", "description"];
        var values = fields.Unread().Where(pair => !commonKeys.Contains(pair.Key, StringComparer.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        return new CustomContentBlock { Type = type, Values = values };
    }

    private static IReadOnlyList<T> ReadItems<T>(ContentBlockFields fields, Func<ContentBlockFields, T?> readItem)
        where T : class =>
        ReadList(fields, "items", required: true, readItem);

    private static IReadOnlyList<T> ReadList<T>(ContentBlockFields fields, string key, bool required, Func<ContentBlockFields, T?> readItem)
        where T : class
    {
        var items = new List<T>();
        foreach (var itemFields in fields.MapList(key, required))
        {
            if (readItem(itemFields) is { } item)
            {
                items.Add(item);
            }
        }

        return items;
    }

    private static StepsBlockItem? ReadStep(ContentBlockFields fields)
    {
        var title = fields.RequiredText("title");
        var description = fields.Text("description");
        fields.RejectUnreadKeys("a step");
        return title is null ? null : new StepsBlockItem(title, description);
    }

    private static CardsBlockItem? ReadCard(ContentBlockFields fields)
    {
        var title = fields.RequiredText("title");
        var card = new CardsBlockItem
        {
            Title = title ?? string.Empty,
            Description = fields.Text("description"),
            Icon = fields.Text("icon"),
            Url = fields.Link("url"),
            LinkText = fields.Text("link_text"),
            Items = fields.TextList("items"),
        };
        fields.RejectUnreadKeys("a card");
        return title is null ? null : card;
    }

    private static ChecklistBlockItem? ReadChecklistItem(ContentBlockFields fields)
    {
        var text = fields.RequiredText("text");
        var description = fields.Text("description");
        fields.RejectUnreadKeys("a checklist item");
        return text is null ? null : new ChecklistBlockItem(text, description);
    }

    private static MetricsBlockItem? ReadMetric(ContentBlockFields fields)
    {
        var value = fields.RequiredText("value");
        var label = fields.Text("label");
        var description = fields.Text("description");
        fields.RejectUnreadKeys("a metric");
        return value is null ? null : new MetricsBlockItem(value, label, description);
    }

    private static FaqBlockItem? ReadQuestion(ContentBlockFields fields)
    {
        var question = fields.RequiredText("question");
        var answer = fields.RequiredText("answer");
        fields.RejectUnreadKeys("a question");
        return question is null || answer is null ? null : new FaqBlockItem(question, answer);
    }

    private static RecordBlockRow? ReadRow(ContentBlockFields fields)
    {
        var label = fields.RequiredText("label");
        var value = fields.RequiredText("value");
        fields.RejectUnreadKeys("a record row");
        return label is null || value is null ? null : new RecordBlockRow(label, value);
    }

    private static ContentBlockAction? ReadAction(ContentBlockFields fields, string key)
    {
        if (fields.Map(key) is not { } action)
        {
            return null;
        }

        var text = action.RequiredText("text");
        var url = action.Link("url", required: true);
        action.RejectUnreadKeys("an action");
        return text is null || url is null ? null : new ContentBlockAction(text, url);
    }

    private static ContentBlockImage? ReadImage(ContentBlockFields fields, string key)
    {
        if (fields.Map(key) is not { } image)
        {
            return null;
        }

        var src = image.ImageSource("src");
        var alt = ReadAlt(image);
        image.RejectUnreadKeys("an image");
        return src is null || alt is null ? null : new ContentBlockImage(src, alt);
    }

    // An empty alt is allowed (a decorative image); a missing one is not, so it is never forgotten by accident.
    private static string? ReadAlt(ContentBlockFields fields)
    {
        if (!fields.Has("alt"))
        {
            fields.Text("alt");
            fields.Error("alt", "is required; write alt: \"\" for a decorative image");
            return null;
        }

        return fields.Text("alt") ?? string.Empty;
    }

    private static ContentBlock? ReadAside(ContentBlockFields fields, string[] allowedTypes)
    {
        if (fields.Map("aside") is not { } aside)
        {
            return null;
        }

        if (!aside.Has("type"))
        {
            aside.Error("type", $"is required; use one of: {string.Join(", ", allowedTypes)}");
            return null;
        }

        return ReadBlock(aside, allowedTypes);
    }

    private static IReadOnlyDictionary<string, string> ReadParameters(ContentBlockFields fields)
    {
        if (fields.Map("parameters") is not { } parameters)
        {
            return new Dictionary<string, string>();
        }

        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var key in parameters.Unread().Keys)
        {
            if (key.Length == 0)
            {
                parameters.Error(key, "a parameter needs a name");
                continue;
            }

            values[key] = parameters.Text(key) ?? string.Empty;
        }

        return values;
    }

    private static string? ReadId(ContentBlockFields fields)
    {
        var id = fields.Text("id");
        if (id is null)
        {
            return null;
        }

        if (id.Length > 0 && id.All(character => character is (>= 'a' and <= 'z') or (>= '0' and <= '9') or '-'))
        {
            return id;
        }

        fields.Error("id", $"'{id}' must be lower case letters, digits and hyphens");
        return null;
    }

    private static bool IsTypeName(string type)
    {
        var words = type.Split('-');
        return words.All(word => word.Length > 0 && word.All(character => character is (>= 'a' and <= 'z') or (>= '0' and <= '9')));
    }
}
