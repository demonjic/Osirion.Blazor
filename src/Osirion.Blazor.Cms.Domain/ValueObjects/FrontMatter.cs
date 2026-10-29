using Osirion.Blazor.Cms.Domain.Common;
using System.Globalization;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Osirion.Blazor.Cms.Domain.ValueObjects;

/// <summary>
/// Represents the frontmatter metadata for a blog post or page
/// </summary>
public class FrontMatter : ValueObject
{
    /// <summary>
    /// Gets the Id
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets the order
    /// </summary>
    public int? Order { get; set; }

    /// <summary>
    /// The layout template to use from the _layouts directory.
    /// Set to null or "none" to bypass layouts (useful for RSS, JSON feeds).
    /// </summary>
    public string? Layout { get; set; }

    /// <summary>
    /// Gets the title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The address this content is served at, instead of the one built from its folder and slug. A leading or
    /// trailing slash is optional, the locale folder is not added, and the value is used as written: placeholders
    /// such as <c>:year</c> are not expanded. Example: "/custom-url/". A folder's <c>_index.md</c> uses it for the
    /// folder's address.
    /// </summary>
    public string? Permalink { get; set; }

    /// <summary>
    /// Gets the description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets the author
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Gets the date in ISO format (yyyy-MM-dd). Empty when the file does not say, so the repository dates the item
    /// from its source (a file's creation time) instead of the day it was loaded.
    /// </summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// Gets the last modified date in ISO format (yyyy-MM-dd)
    /// </summary>
    public string? LastModified { get; set; }

    /// <summary>
    /// Gets the URL for the featured image
    /// </summary>
    public string? FeaturedImage { get; set; }

    /// <summary>
    /// Gets the categories
    /// </summary>
    public List<string> Categories { get; set; } = new List<string>();

    /// <summary>
    /// Gets the tags
    /// </summary>
    public List<string> Tags { get; set; } = new List<string>();

    /// <summary>
    /// Gets whether the post is featured
    /// </summary>
    public bool IsFeatured { get; set; }

    /// <summary>
    /// Gets whether the post is published. Front matter without a <c>published</c> key is published, as in Jekyll
    /// and Hugo; write <c>published: false</c> to keep a draft hidden.
    /// </summary>
    public bool Published { get; set; } = true;

    /// <summary>
    /// Gets the URL slug
    /// </summary>
    public string? Slug { get; set; }

    /// <summary>
    /// Gets or sets the series the item belongs to, such as a multi-part tutorial (<c>series</c>). Items with the same
    /// series name are listed together in <see cref="SeriesOrder"/>.
    /// </summary>
    public string? Series { get; set; }

    /// <summary>
    /// Gets or sets the item's position in its <see cref="Series"/>, starting at 1 (<c>series_order</c>). Items without
    /// one follow the numbered parts, oldest first.
    /// </summary>
    public int? SeriesOrder { get; set; }

    /// <summary>
    /// Gets or sets earlier URLs of this item (<c>redirect_from</c>), such as <c>/docs/components/core/content/tabs</c>.
    /// The redirect middleware answers requests for them with a permanent redirect to the item's current URL.
    /// </summary>
    public List<string> RedirectFrom { get; set; } = new List<string>();

    /// <summary>
    /// Gets the language. Empty when the file does not say, so the repository gives the item the locale of its
    /// folder, or the provider's default locale.
    /// </summary>
    public string Lang { get; set; } = string.Empty;

    /// <summary>
    /// Gets custom fields
    /// </summary>
    public Dictionary<string, object> CustomFields { get; set; } = new Dictionary<string, object>();

    /// <summary>Gets or sets SEO metadata.</summary>
    public SeoMetadata SeoProperties { get; set; } = new SeoMetadata();

    /// <summary>
    /// Gets or sets the page's content blocks (<c>blocks</c>) as YamlDotNet reads them: a list of maps, untyped, so a
    /// block YamlDotNet could not map to a class still loads and <c>ContentBlockParser</c> reports what is wrong with
    /// it, naming the file and the block. <see langword="null"/> when the front matter has no <c>blocks</c>.
    /// </summary>
    [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
    public object? Blocks { get; set; }

    /// <summary>
    /// Gets or sets where the Markdown body renders on a page with <see cref="Blocks"/> (<c>body</c>): <c>after</c>
    /// renders it after the blocks; without the key only the blocks render.
    /// </summary>
    [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
    public string? Body { get; set; }

    /// <summary>
    /// Gets or sets the <c>url</c> key, another name for <see cref="Permalink"/>, which directory metadata writes. The
    /// content repositories copy it into <see cref="Permalink"/> when that is not set and then clear it.
    /// </summary>
    [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets the <c>locale</c> key, another name for <see cref="Lang"/>. The content repositories copy it into
    /// <see cref="Lang"/> when that is not set and then clear it.
    /// </summary>
    [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
    public string? Locale { get; set; }

    /// <summary>
    /// Gets or sets the <c>language</c> key, another name for <see cref="Lang"/>, read after <see cref="Locale"/>.
    /// </summary>
    [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
    public string? Language { get; set; }

    /// <summary>
    /// Gets or sets the <c>localization_id</c> key, another name for <see cref="Id"/>, the id translations share. The
    /// content repositories copy it into <see cref="Id"/> when that is not set and then clear it.
    /// </summary>
    [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
    public string? LocalizationId { get; set; }

    /// <summary>
    /// Gets or sets the <c>content_id</c> key, another name for <see cref="Id"/>, read after
    /// <see cref="LocalizationId"/>.
    /// </summary>
    [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
    public string? ContentId { get; set; }

    // Private constructor to enforce creation through factory method
    /// <summary>Performs the FrontMatter operation.</summary>
    public FrontMatter() { }

    /// <summary>
    /// Factory method to create a new FrontMatter instance
    /// </summary>
    public static FrontMatter Create(string title)
    {
        var frontMatter = new FrontMatter
        {
            Title = title,
            Date = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        };

        return frontMatter;
    }

    /// <summary>
    /// Factory method to create a new FrontMatter instance
    /// </summary>
    public static FrontMatter Create(string title, string description, DateTime dateCreated)
    {
        var frontMatter = new FrontMatter
        {
            Title = title,
            Description = description,
            Date = dateCreated.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        };

        return frontMatter;
    }

    /// <summary>
    /// Factory method to create a new FrontMatter instance
    /// </summary>
    public static FrontMatter Create(
        string id,
        string title,
        string? description = null,
        string? author = null,
        DateTime? date = null,
        DateTime? lastModified = null,
        string? featuredImage = null,
        IEnumerable<string>? categories = null,
        IEnumerable<string>? tags = null,
        bool isFeatured = false,
        bool published = true,
        string? layout = null,
        string? slug = null,
        IDictionary<string, object>? customFields = null)
    {
        var frontMatter = new FrontMatter
        {
            Title = title,
            Description = description ?? string.Empty,
            Author = author ?? string.Empty,
            Date = (date ?? DateTime.Now).ToString("yyyy-MM-dd"),
            LastModified = lastModified?.ToString("yyyy-MM-dd"),
            FeaturedImage = featuredImage,
            IsFeatured = isFeatured,
            Published = published,
            Layout = layout,
            Slug = slug
        };

        // Set categories if provided
        if (categories is not null)
        {
            frontMatter = frontMatter.WithCategories(categories);
        }

        // Set tags if provided
        if (tags is not null)
        {
            frontMatter = frontMatter.WithTags(tags);
        }

        // Set custom fields if provided
        if (customFields is not null)
        {
            frontMatter = frontMatter.WithCustomFields(customFields);
        }

        return frontMatter;
    }

    /// <summary>Returns a copy with a new title.</summary>
    /// <param name="title">The new title.</param>
    public FrontMatter WithTitle(string title)
    {
        var clone = Clone();
        clone.Title = title;
        return clone;
    }

    /// <summary>Returns a copy with a new description.</summary>
    /// <param name="description">The new description.</param>
    public FrontMatter WithDescription(string description)
    {
        var clone = Clone();
        clone.Description = description;
        return clone;
    }

    /// <summary>Returns a copy with a new author.</summary>
    /// <param name="author">The new author.</param>
    public FrontMatter WithAuthor(string author)
    {
        var clone = Clone();
        clone.Author = author;
        return clone;
    }

    /// <summary>Returns a copy with a new date.</summary>
    /// <param name="date">The new date.</param>
    public FrontMatter WithDate(DateTime date)
    {
        var clone = Clone();
        clone.Date = date.ToString("yyyy-MM-dd");
        return clone;
    }

    /// <summary>Returns a copy with a new last-modified date.</summary>
    /// <param name="lastModified">The last-modified date.</param>
    public FrontMatter WithLastModified(DateTime? lastModified)
    {
        var clone = Clone();
        clone.LastModified = lastModified?.ToString("yyyy-MM-dd");
        return clone;
    }

    /// <summary>Returns a copy with a new featured image.</summary>
    /// <param name="featuredImage">The image URL.</param>
    public FrontMatter WithFeaturedImage(string? featuredImage)
    {
        var clone = Clone();
        clone.FeaturedImage = featuredImage;
        return clone;
    }

    /// <summary>Returns a copy with new categories.</summary>
    /// <param name="categories">The categories.</param>
    public FrontMatter WithCategories(IEnumerable<string> categories)
    {
        var clone = Clone();
        clone.Categories = categories is not null
            ? new List<string>(categories)
            : new List<string>();
        return clone;
    }

    /// <summary>Returns a copy with new tags.</summary>
    /// <param name="tags">The tags.</param>
    public FrontMatter WithTags(IEnumerable<string> tags)
    {
        var clone = Clone();
        clone.Tags = tags is not null
            ? new List<string>(tags)
            : new List<string>();
        return clone;
    }

    /// <summary>Returns a copy with a new featured state.</summary>
    /// <param name="isFeatured">Whether the item is featured.</param>
    public FrontMatter WithFeatured(bool isFeatured)
    {
        var clone = Clone();
        clone.IsFeatured = isFeatured;
        return clone;
    }

    /// <summary>Returns a copy with a new published state.</summary>
    /// <param name="published">Whether the item is published.</param>
    public FrontMatter WithPublished(bool published)
    {
        var clone = Clone();
        clone.Published = published;
        return clone;
    }

    /// <summary>Returns a copy with a new layout.</summary>
    /// <param name="layout">The layout name.</param>
    public FrontMatter WithLayout(string? layout)
    {
        var clone = Clone();
        clone.Layout = layout;
        return clone;
    }

    /// <summary>Returns a copy with a new slug.</summary>
    /// <param name="slug">The slug.</param>
    public FrontMatter WithSlug(string? slug)
    {
        var clone = Clone();
        clone.Slug = slug;
        return clone;
    }

    /// <summary>Returns a copy with new custom fields.</summary>
    /// <param name="customFields">The custom fields.</param>
    public FrontMatter WithCustomFields(IDictionary<string, object> customFields)
    {
        if (customFields is null)
            throw new ArgumentNullException(nameof(customFields));

        var clone = Clone();

        // Create a new dictionary with the custom fields
        var newCustomFields = new Dictionary<string, object>();
        foreach (var kvp in customFields)
        {
            newCustomFields[kvp.Key] = kvp.Value;
        }

        clone.CustomFields = newCustomFields;
        return clone;
    }

    /// <summary>Returns a copy with one custom field added or replaced.</summary>
    /// <param name="key">The custom field key.</param>
    /// <param name="value">The custom field value.</param>
    public FrontMatter WithCustomField(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty", nameof(key));

        var clone = Clone();

        // Create a new dictionary with the existing custom fields plus the new one
        var newCustomFields = new Dictionary<string, object>(CustomFields);
        newCustomFields[key] = value;
        clone.CustomFields = newCustomFields;

        return clone;
    }

    /// <summary>
    /// Creates a YAML representation of the frontmatter
    /// </summary>
    public string ToYaml()
    {
        // Configure serializer for clean Jekyll-compatible output
        var serializer = new SerializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            //.ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .Build();

        // Serialize just the frontmatter
        string yamlContent = serializer.Serialize(this);

        // Build the complete Jekyll file with proper structure
        var jekyllFile = new StringBuilder();
        jekyllFile.AppendLine("---");
        jekyllFile.Append(yamlContent); // YAML serializer adds its own final newline
        jekyllFile.AppendLine("---");
        jekyllFile.AppendLine(); // Empty line between frontmatter and content

        return jekyllFile.ToString();
    }

    /// <summary>
    /// Creates a deep clone of this FrontMatter
    /// </summary>
    public FrontMatter Clone()
    {
        return new FrontMatter
        {
            Id = Id,
            Order = Order,
            Title = Title,
            Permalink = Permalink,
            Description = Description,
            Author = Author,
            Date = Date,
            LastModified = LastModified,
            FeaturedImage = FeaturedImage,
            IsFeatured = IsFeatured,
            Published = Published,
            Layout = Layout,
            Slug = Slug,
            Lang = Lang,
            Series = Series,
            SeriesOrder = SeriesOrder,
            RedirectFrom = new List<string>(RedirectFrom),
            Categories = new List<string>(Categories),
            Tags = new List<string>(Tags),
            CustomFields = new Dictionary<string, object>(CustomFields),
            SeoProperties = SeoProperties?.Clone() ?? new SeoMetadata(),
            // The raw YAML of the blocks is only read, never changed, so the clone shares it
            Blocks = Blocks,
            Url = Url,
            Locale = Locale,
            Language = Language,
            LocalizationId = LocalizationId,
            ContentId = ContentId,
            Body = Body,
        };
    }

    /// <summary>
    /// Readers for the keys <see cref="FromDictionary"/> maps onto properties. Each entry lists its aliases in
    /// priority order: the first alias present wins, even when its value cannot be parsed.
    /// </summary>
    private static readonly (string[] Keys, Func<FrontMatter, string, FrontMatter> Apply)[] KeyReaders =
    [
        (["description"], (frontMatter, value) => frontMatter.WithDescription(value)),
        (["author"], (frontMatter, value) => frontMatter.WithAuthor(value)),
        (["date"], ApplyDate),
        (["lastModified", "last_modified"], ApplyLastModified),
        (["featuredImage", "featured_image"], (frontMatter, value) => frontMatter.WithFeaturedImage(value)),
        (["featured", "isFeatured"], (frontMatter, value) => frontMatter.WithFeatured(bool.TryParse(value, out var featured) && featured)),
        (["published"], (frontMatter, value) => frontMatter.WithPublished(!bool.TryParse(value, out var published) || published)),
        (["layout"], (frontMatter, value) => frontMatter.WithLayout(value)),
        (["slug"], (frontMatter, value) => frontMatter.WithSlug(value)),
        (["categories", "category"], (frontMatter, value) => frontMatter.WithCategories(ParseListValue(value))),
        (["tags", "tag"], (frontMatter, value) => frontMatter.WithTags(ParseListValue(value))),
        (["series"], ApplySeries),
        (["series_order"], ApplySeriesOrder),
        (["redirect_from"], ApplyRedirectFrom),
    ];

    /// <summary>
    /// Static method to create FrontMatter from a dictionary of values
    /// </summary>
    public static FrontMatter FromDictionary(IDictionary<string, string> values)
    {
        if (values is null)
            throw new ArgumentNullException(nameof(values));

        // Not Create: a page whose keys have no date has none, and is not dated the day it is read
        var frontMatter = new FrontMatter { Title = values.TryGetValue("title", out var title) ? title : string.Empty };

        foreach (var (keys, apply) in KeyReaders)
        {
            if (TryGetFirstValue(values, keys, out var value))
                frontMatter = apply(frontMatter, value);
        }

        var customFields = ReadCustomFields(values);
        if (customFields.Count > 0)
            frontMatter = frontMatter.WithCustomFields(customFields);

        return frontMatter;
    }

    private static bool TryGetFirstValue(IDictionary<string, string> values, string[] keys, out string value)
    {
        foreach (var key in keys)
        {
            if (values.TryGetValue(key, out value!))
                return true;
        }

        value = string.Empty;
        return false;
    }

    private static FrontMatter ApplyDate(FrontMatter frontMatter, string value) =>
        DateTime.TryParse(value, out var date) ? frontMatter.WithDate(date) : frontMatter;

    private static FrontMatter ApplyLastModified(FrontMatter frontMatter, string value) =>
        DateTime.TryParse(value, out var lastModified) ? frontMatter.WithLastModified(lastModified) : frontMatter;

    private static FrontMatter ApplySeries(FrontMatter frontMatter, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return frontMatter;

        var clone = frontMatter.Clone();
        clone.Series = value;
        return clone;
    }

    private static FrontMatter ApplySeriesOrder(FrontMatter frontMatter, string value)
    {
        if (!int.TryParse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var seriesOrder))
            return frontMatter;

        var clone = frontMatter.Clone();
        clone.SeriesOrder = seriesOrder;
        return clone;
    }

    private static FrontMatter ApplyRedirectFrom(FrontMatter frontMatter, string value)
    {
        var clone = frontMatter.Clone();
        clone.RedirectFrom = ParseListValue(value);
        return clone;
    }

    private static Dictionary<string, object> ReadCustomFields(IDictionary<string, string> values)
    {
        var customFields = new Dictionary<string, object>();
        foreach (var kvp in values)
        {
            if (!IsStandardKey(kvp.Key))
                customFields[kvp.Key] = ParseCustomFieldValue(kvp.Value);
        }

        return customFields;
    }

    private static object ParseCustomFieldValue(string value)
    {
        if (bool.TryParse(value, out var boolValue))
            return boolValue;

        if (int.TryParse(value, out var intValue))
            return intValue;

        if (double.TryParse(value, out var doubleValue))
            return doubleValue;

        return value;
    }

    /// <summary>
    /// Provides equality comparison components for the value object
    /// </summary>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Title;
        yield return Description;
        yield return Author;
        yield return Date;
        yield return LastModified ?? string.Empty;
        yield return FeaturedImage ?? string.Empty;
        yield return IsFeatured;
        yield return Published;
        yield return Layout ?? string.Empty;
        yield return Slug ?? string.Empty;
        yield return Series ?? string.Empty;
        yield return SeriesOrder ?? 0;

        foreach (var redirect in RedirectFrom)
        {
            yield return "redirect:" + redirect;
        }

        // Include collections in equality check
        foreach (var category in Categories)
        {
            yield return category;
        }

        foreach (var tag in Tags)
        {
            yield return tag;
        }

        foreach (var field in CustomFields)
        {
            yield return $"{field.Key}:{field.Value}";
        }
    }

    /// <summary>
    /// Helper method to parse list values from string
    /// </summary>
    private static List<string> ParseListValue(string value)
    {
        var result = new List<string>();

        if (string.IsNullOrWhiteSpace(value))
            return result;

        // Handle YAML array format [item1, item2]
        if (value.StartsWith("[") && value.EndsWith("]"))
        {
            value = value.Substring(1, value.Length - 2);
        }

        // Split by comma or semicolon and process each item
        foreach (var item in value.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmedItem = StripQuotes(item.Trim());

            if (!string.IsNullOrWhiteSpace(trimmedItem))
            {
                result.Add(trimmedItem);
            }
        }

        return result;
    }

    private static string StripQuotes(string item)
    {
        // A lone quote is both the first and the last character, so it needs two characters to be a quoted value.
        if (item.Length < 2)
        {
            return item;
        }

        if ((item.StartsWith("\"") && item.EndsWith("\"")) ||
            (item.StartsWith("'") && item.EndsWith("'")))
        {
            return item.Substring(1, item.Length - 2);
        }

        return item;
    }

    /// <summary>
    /// Checks if a key is a standard frontmatter key
    /// </summary>
    private static bool IsStandardKey(string key)
    {
        var standardKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "title", "description", "author", "date", "lastmodified", "last_modified",
            "featuredimage", "featured_image", "featured", "isFeatured", "published",
            "layout", "slug", "categories", "category", "tags", "tag", "series", "series_order", "redirect_from"
        };

        return standardKeys.Contains(key);
    }
}
