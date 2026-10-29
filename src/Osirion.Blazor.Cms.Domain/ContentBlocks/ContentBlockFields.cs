using System.Collections;
using System.Globalization;

namespace Osirion.Blazor.Cms.Domain.ContentBlocks;

/// <summary>
/// Reads the keys of one YAML map of a block (the block itself, an item, an action, an aside) and records every
/// problem with the file, the block and the key, so the parser reports all problems of a file at once.
/// </summary>
/// <remarks>
/// YamlDotNet reads an untyped node as a map (<see cref="IDictionary"/>), a list (<see cref="IList"/>), a
/// <see cref="string"/> or <see langword="null"/>. Other scalars (a number or a boolean set in code) are read as their
/// invariant text. Each key read is remembered, so <see cref="RejectUnreadKeys"/> can report the keys a block does not
/// have, which are usually misspellings.
/// </remarks>
internal sealed class ContentBlockFields
{
    private readonly Dictionary<string, object?> _values;
    private readonly HashSet<string> _read = new(StringComparer.Ordinal);
    private readonly List<ContentBlockError> _errors;
    private readonly string _sourcePath;
    private readonly string _prefix;

    private ContentBlockFields(Dictionary<string, object?> values, ContentBlockFieldsScope scope)
    {
        _values = values;
        _errors = scope.Errors;
        _sourcePath = scope.SourcePath;
        BlockIndex = scope.BlockIndex;
        _prefix = scope.Prefix;
    }

    /// <summary>Gets the position of the block these keys belong to.</summary>
    public int BlockIndex { get; }

    /// <summary>Reads <paramref name="node"/> as a map, or returns <see langword="null"/> when it is not one.</summary>
    public static ContentBlockFields? FromNode(object? node, ContentBlockFieldsScope scope)
    {
        if (node is not IDictionary map)
        {
            return null;
        }

        var values = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (DictionaryEntry entry in map)
        {
            values[Convert.ToString(entry.Key, CultureInfo.InvariantCulture) ?? string.Empty] = entry.Value;
        }

        return new ContentBlockFields(values, scope);
    }

    /// <summary>Records a problem with <paramref name="key"/>, a key of this map.</summary>
    public void Error(string key, string message) =>
        _errors.Add(new ContentBlockError(_sourcePath, BlockIndex, _prefix + key, message));

    /// <summary>Returns whether the map has <paramref name="key"/>, without reading it.</summary>
    public bool Has(string key) => _values.ContainsKey(key);

    /// <summary>Reads a text value, or <see langword="null"/> when the key is missing or empty.</summary>
    public string? Text(string key)
    {
        var value = Read(key);
        if (value is null)
        {
            return null;
        }

        if (AsScalar(value) is { } text)
        {
            return text;
        }

        Error(key, "must be text, not a list or a map");
        return null;
    }

    /// <summary>Reads a text value that must be present and not blank.</summary>
    public string? RequiredText(string key)
    {
        var text = Text(key);
        if (string.IsNullOrWhiteSpace(text))
        {
            if (!HasErrorFor(key))
            {
                Error(key, "is required");
            }

            return null;
        }

        return text;
    }

    /// <summary>Reads a list of text values; a missing key gives an empty list.</summary>
    public IReadOnlyList<string> TextList(string key)
    {
        var value = Read(key);
        if (value is null)
        {
            return [];
        }

        if (value is not IList list || value is string)
        {
            Error(key, "must be a list of text values");
            return [];
        }

        var texts = new List<string>(list.Count);
        for (var position = 0; position < list.Count; position++)
        {
            if (AsScalar(list[position]) is { } text && !string.IsNullOrWhiteSpace(text))
            {
                texts.Add(text);
            }
            else
            {
                Error($"{key}[{position}]", "must be text that is not empty");
            }
        }

        return texts;
    }

    /// <summary>Reads a nested map, or <see langword="null"/> when the key is missing.</summary>
    public ContentBlockFields? Map(string key)
    {
        var value = Read(key);
        if (value is null)
        {
            return null;
        }

        var fields = FromNode(value, Scope($"{key}."));
        if (fields is null)
        {
            Error(key, "must be a map of keys and values");
        }

        return fields;
    }

    /// <summary>
    /// Reads a list of maps, such as <c>items</c>. When <paramref name="required"/> is set, a missing or empty list is
    /// a problem.
    /// </summary>
    public IReadOnlyList<ContentBlockFields> MapList(string key, bool required)
    {
        var value = Read(key);
        if (value is null || (value is IList { Count: 0 } && value is not string))
        {
            if (required)
            {
                Error(key, "needs at least one item");
            }

            return [];
        }

        if (value is not IList list || value is string)
        {
            Error(key, "must be a list");
            return [];
        }

        var maps = new List<ContentBlockFields>(list.Count);
        for (var position = 0; position < list.Count; position++)
        {
            var itemKey = $"{key}[{position}]";
            if (FromNode(list[position], Scope($"{itemKey}.")) is { } fields)
            {
                maps.Add(fields);
            }
            else
            {
                Error(itemKey, "must be a map of keys and values");
            }
        }

        return maps;
    }

    /// <summary>
    /// Reads a value that must be one of <paramref name="choices"/>; a missing key gives
    /// <paramref name="defaultValue"/>.
    /// </summary>
    public T Choice<T>(string key, IReadOnlyDictionary<string, T> choices, T defaultValue)
    {
        var text = Text(key);
        if (text is null)
        {
            return defaultValue;
        }

        if (choices.TryGetValue(text, out var choice))
        {
            return choice;
        }

        Error(key, $"'{text}' is not one of: {string.Join(", ", choices.Keys)}");
        return defaultValue;
    }

    /// <summary>Reads a link that must follow <see cref="ContentBlockLinks.IsAllowedLink"/>.</summary>
    public string? Link(string key, bool required = false)
    {
        var text = required ? RequiredText(key) : Text(key);
        if (text is null)
        {
            return null;
        }

        if (ContentBlockLinks.IsAllowedLink(text))
        {
            return text;
        }

        Error(key, $"'{text}' is not an allowed link; use {ContentBlockLinks.AllowedLinkForms}");
        return null;
    }

    /// <summary>Reads an image address that must follow <see cref="ContentBlockLinks.IsAllowedImageSource"/>.</summary>
    public string? ImageSource(string key)
    {
        var text = RequiredText(key);
        if (text is null)
        {
            return null;
        }

        if (ContentBlockLinks.IsAllowedImageSource(text))
        {
            return text;
        }

        Error(key, $"'{text}' is not an allowed image address; use {ContentBlockLinks.AllowedImageForms}");
        return null;
    }

    /// <summary>Reads the keys that were not read, unchanged, for a block type the library does not define.</summary>
    public IReadOnlyDictionary<string, object?> Unread() =>
        _values.Where(pair => !_read.Contains(pair.Key)).ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);

    /// <summary>Reports every key that was not read as a key the map does not have.</summary>
    /// <param name="owner">What the map is, for the message, for example <c>a hero block</c>.</param>
    public void RejectUnreadKeys(string owner)
    {
        foreach (var key in _values.Keys.Where(key => !_read.Contains(key)))
        {
            Error(key, $"is not a key of {owner}");
        }
    }

    /// <summary>Creates the scope of a map nested in this one, under <paramref name="prefix"/>.</summary>
    public ContentBlockFieldsScope Scope(string prefix) =>
        new(_sourcePath, BlockIndex, _prefix + prefix, _errors);

    private object? Read(string key)
    {
        _read.Add(key);
        return _values.GetValueOrDefault(key);
    }

    private bool HasErrorFor(string key)
    {
        var location = _prefix + key;
        return _errors.Any(error => error.BlockIndex == BlockIndex && error.Key == location);
    }

    private static string? AsScalar(object? value) => value switch
    {
        null => null,
        string text => text,
        IDictionary or IList => null,
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString(),
    };
}

/// <summary>Where a map of a block is: the file, the block, the key path before its keys, and the error list.</summary>
/// <param name="SourcePath">The content item's path.</param>
/// <param name="BlockIndex">The block's position in <c>blocks</c>.</param>
/// <param name="Prefix">The key path before the map's keys, for example <c>items[1].</c>.</param>
/// <param name="Errors">The list the problems are added to.</param>
internal sealed record ContentBlockFieldsScope(string SourcePath, int BlockIndex, string Prefix, List<ContentBlockError> Errors);
