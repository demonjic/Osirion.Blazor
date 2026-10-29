using System.Text.Json;
using System.Text.Json.Serialization;

namespace Osirion.Blazor.Cms.Web.Components.Seo;

/// <summary>
/// Maps a <see cref="SchemaType"/> to its schema.org builder and serializes the result as JSON-LD.
/// </summary>
internal static class SeoSchemas
{
    private static readonly JsonSerializerOptions JsonLdOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly Dictionary<SchemaType, Func<SeoSchemaContext, object?>> Builders = new()
    {
        [SchemaType.Article] = context => ContentSchemas.Article(context, "Article"),
        [SchemaType.BlogPosting] = context => ContentSchemas.Article(context, "BlogPosting"),
        [SchemaType.TechArticle] = context => ContentSchemas.Article(context, "TechArticle"),
        [SchemaType.WebPage] = context => ContentSchemas.WebPage(context, "WebPage"),
        [SchemaType.ContactPage] = context => ContentSchemas.WebPage(context, "ContactPage"),
        [SchemaType.AboutPage] = ContentSchemas.AboutPage,
        [SchemaType.SoftwareApplication] = ContentSchemas.SoftwareApplication,
        [SchemaType.ProfilePage] = ContentSchemas.ProfilePage,
        [SchemaType.Product] = ContentSchemas.Product,
        [SchemaType.Organization] = ContentSchemas.Organization,
        [SchemaType.Person] = ContentSchemas.Person,
        [SchemaType.LocalBusiness] = ContentSchemas.LocalBusiness,
        [SchemaType.Event] = ContentSchemas.Event,
        [SchemaType.Recipe] = ContentSchemas.Recipe,

        // A FAQPage needs real questions and answers. The only source is the generated FAQ data, which is opt-in,
        // so an explicit FAQ schema type adds nothing by itself and an empty FAQPage is never emitted.
        [SchemaType.FAQ] = _ => null,
        [SchemaType.HowTo] = ContentSchemas.HowTo,
        [SchemaType.VideoObject] = ContentSchemas.Video,
        [SchemaType.Course] = ContentSchemas.Course,
        [SchemaType.Review] = ContentSchemas.Review,
        [SchemaType.BreadcrumbList] = NavigationSchemas.Breadcrumbs,
        [SchemaType.WebSite] = ContentSchemas.WebSite,
        [SchemaType.CollectionPage] = NavigationSchemas.CollectionPage
    };

    /// <summary>
    /// Builds the schema for <paramref name="schemaType"/>, or <see langword="null"/> when the type produces none.
    /// A value without a dedicated builder gets the default schema.
    /// </summary>
    internal static object? Create(SchemaType schemaType, SeoSchemaContext context) =>
        Builders.GetValueOrDefault(schemaType, ContentSchemas.Default)(context);

    /// <summary>
    /// Serializes a page's schema; with the site's own structured data on (<see cref="SeoSchemaContext.LinkToSite"/>),
    /// its publisher and web site are first replaced by references to the site's nodes.
    /// </summary>
    internal static string Serialize(object schema, SeoSchemaContext context) =>
        Serialize(context.LinkToSite ? SiteSchemas.Link(schema, context) : schema);

    /// <summary>Serializes a schema as indented JSON-LD, leaving out null properties.</summary>
    internal static string Serialize(object schema) => JsonSerializer.Serialize(schema, JsonLdOptions);
}
