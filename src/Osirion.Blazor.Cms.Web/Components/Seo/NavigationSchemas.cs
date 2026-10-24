using Osirion.Blazor.Cms.Domain.Entities;

namespace Osirion.Blazor.Cms.Web.Components.Seo;

/// <summary>
/// Builds the schema.org objects that describe where a page sits in the site and what it lists: the breadcrumb
/// trail, a collection page and the posts it lists, and the generated FAQ page.
/// </summary>
internal static class NavigationSchemas
{
    private const string SchemaContext = "https://schema.org";

    /// <summary>
    /// The breadcrumb trail: Home, then the custom breadcrumbs when given, otherwise one item per segment of the
    /// content URL (the last one named with the content title), otherwise the content alone, otherwise on a list page
    /// the list itself, as in the <c>breadcrumb</c> of <see cref="CollectionPage"/>.
    /// </summary>
    internal static object Breadcrumbs(SeoSchemaContext context)
    {
        List<Dictionary<string, object?>> items = [ListItem(1, context.BreadcrumbHomeText, context.BaseUrl)];

        if (context.CustomBreadcrumbs?.Any() == true)
        {
            items.AddRange(CustomBreadcrumbItems(context));
        }
        else if (!string.IsNullOrWhiteSpace(context.Content?.Url))
        {
            items.AddRange(PathBreadcrumbs(context, context.Content));
        }
        else if (context.Content != null)
        {
            items.Add(ListItem(2, context.Content.Title, context.CurrentUrl));
        }
        else if (context.ContentItems?.Any() == true)
        {
            items.Add(CollectionListItem(context));
        }

        return new Dictionary<string, object?>
        {
            ["@context"] = SchemaContext,
            ["@type"] = "BreadcrumbList",
            ["itemListElement"] = items.ToArray()
        };
    }

    /// <summary>
    /// A collection page with its items as an item list, a breadcrumb trail and the site as publisher. The trail is
    /// Home and the custom breadcrumbs when given, otherwise Home and the list, the same trail <see cref="Breadcrumbs"/>
    /// writes for a list page. A property without a value is left out, in the page and in each item.
    /// </summary>
    internal static object CollectionPage(SeoSchemaContext context)
    {
        var itemListElements = (context.ContentItems ?? [])
            .Select((item, index) => WithoutEmptyValues(new Dictionary<string, object?>
            {
                ["@type"] = "ListItem",
                ["position"] = index + 1,
                ["url"] = AbsoluteItemUrl(item, context),
                ["name"] = item.Title,
                ["description"] = item.Description,
                ["datePublished"] = ContentUrls.FormatUtc(item.PublishDate),
                ["image"] = context.NormalizeImageUrl(item.FeaturedImageUrl)
            }))
            .ToList();

        return WithoutEmptyValues(new Dictionary<string, object?>
        {
            ["@context"] = SchemaContext,
            ["@type"] = "CollectionPage",
            ["name"] = context.CollectionTitle ?? context.MetaTitle,
            ["description"] = context.CollectionDescription ?? context.MetaDescription,
            ["url"] = context.CurrentUrl,
            ["inLanguage"] = context.LanguageCode,
            ["mainEntity"] = new Dictionary<string, object?>
            {
                ["@type"] = "ItemList",
                ["numberOfItems"] = context.ContentItems?.Count ?? 0,
                ["itemListElement"] = itemListElements
            },
            ["breadcrumb"] = new Dictionary<string, object?>
            {
                ["@type"] = "BreadcrumbList",
                ["itemListElement"] = (Dictionary<string, object?>[])
                [
                    ListItem(1, context.BreadcrumbHomeText, context.BaseUrl),
                    .. context.CustomBreadcrumbs?.Any() == true ? CustomBreadcrumbItems(context) : [CollectionListItem(context)]
                ]
            },
            ["publisher"] = ContentSchemas.PublisherWithLogo(context)
        });
    }

    /// <summary>
    /// The absolute URL of a listed item. Item URLs are stored with or without a leading slash, so the join
    /// supplies exactly one.
    /// </summary>
    private static string AbsoluteItemUrl(ContentItem item, SeoSchemaContext context) =>
        $"{context.BaseUrl.TrimEnd('/')}/{item.Url.TrimStart('/')}";

    /// <summary>
    /// An article of the given <paramref name="type"/> (<c>Article</c>, <c>BlogPosting</c> or <c>TechArticle</c>) for
    /// one item of a collection page. A property without a value is left out: an item without tags has no
    /// <c>keywords</c>, one without an author no <c>author</c>.
    /// </summary>
    internal static object CollectionItemPosting(ContentItem item, SeoSchemaContext context, string type)
    {
        var itemUrl = AbsoluteItemUrl(item, context);
        return WithoutEmptyValues(new Dictionary<string, object?>
        {
            ["@context"] = SchemaContext,
            ["@type"] = type,
            ["headline"] = item.Title,
            ["description"] = item.Description,
            ["image"] = context.NormalizeImageUrl(item.FeaturedImageUrl) ?? context.OgImage,
            ["datePublished"] = ContentUrls.FormatUtc(item.PublishDate),
            ["dateModified"] = ContentUrls.FormatUtc(item.LastModified ?? item.PublishDate),
            ["author"] = string.IsNullOrWhiteSpace(item.Author) ? null : ContentSchemas.Author(item.Author, context.AuthorUrl),
            ["publisher"] = new Dictionary<string, object?>
            {
                ["@type"] = "Organization",
                ["name"] = context.SiteName,
                ["logo"] = new Dictionary<string, object?>
                {
                    ["@type"] = "ImageObject",
                    ["url"] = context.PublisherLogoUrl
                }
            },
            ["url"] = itemUrl,
            ["mainEntityOfPage"] = new Dictionary<string, object?>
            {
                ["@type"] = "WebPage",
                ["@id"] = itemUrl
            },
            ["keywords"] = JoinTags(item.Tags),
            ["articleSection"] = item.Categories?.FirstOrDefault(),
            ["inLanguage"] = item.Locale ?? context.LanguageCode,
            ["timeRequired"] = ReadingDuration(item.ReadTimeMinutes)
        });
    }

    /// <summary>A FAQ page with one question per entry of <paramref name="answers"/> (question to answer text).</summary>
    internal static object FaqPage(IReadOnlyDictionary<string, string> answers) => new Dictionary<string, object?>
    {
        ["@context"] = SchemaContext,
        ["@type"] = "FAQPage",
        ["mainEntity"] = answers.Select(answer => new Dictionary<string, object?>
        {
            ["@type"] = "Question",
            ["name"] = answer.Key,
            ["acceptedAnswer"] = new Dictionary<string, object?>
            {
                ["@type"] = "Answer",
                ["text"] = answer.Value
            }
        }).ToArray()
    };

    /// <summary>The custom breadcrumbs as list items, numbered from 2 after Home.</summary>
    private static IEnumerable<Dictionary<string, object?>> CustomBreadcrumbItems(SeoSchemaContext context) =>
        (context.CustomBreadcrumbs ?? []).Select((crumb, index) => ListItem(index + 2, crumb.Name, crumb.Url));

    /// <summary>The list page itself as the second breadcrumb entry.</summary>
    private static Dictionary<string, object?> CollectionListItem(SeoSchemaContext context) =>
        ListItem(2, context.CollectionTitle ?? "Collection", context.CurrentUrl);

    /// <summary>
    /// Leaves out the properties without a value (null or blank text). The serializer does not skip null dictionary
    /// values, so a schema built as a dictionary would otherwise say <c>"keywords": null</c>.
    /// </summary>
    private static Dictionary<string, object?> WithoutEmptyValues(Dictionary<string, object?> schema)
    {
        foreach (var (name, value) in schema.ToList())
        {
            if (value is null || (value is string text && string.IsNullOrWhiteSpace(text)))
            {
                schema.Remove(name);
            }
        }

        return schema;
    }

    private static string? JoinTags(IReadOnlyList<string>? tags) => tags?.Any() == true ? string.Join(", ", tags) : null;

    private static string? ReadingDuration(int minutes) => minutes > 0 ? $"PT{minutes}M" : null;

    private static IEnumerable<Dictionary<string, object?>> PathBreadcrumbs(SeoSchemaContext context, ContentItem content)
    {
        var position = 2;
        foreach (var segment in new Osirion.Blazor.Components.BreadcrumbPath(content.Url).Segments)
        {
            // The last item is this page: its title keeps the punctuation and casing a slug loses. Earlier segments
            // are directories, named from the slug.
            yield return segment.IsLast
                ? ListItem(position++, content.Title, context.CurrentUrl)
                : ListItem(position++, SeoText.FormatBreadcrumbSegment(segment.Name), $"{context.BaseUrl}/{segment.Path}");
        }
    }

    private static Dictionary<string, object?> ListItem(int position, string? name, string? item) => new()
    {
        ["@type"] = "ListItem",
        ["position"] = position,
        ["name"] = name,
        ["item"] = item
    };
}
