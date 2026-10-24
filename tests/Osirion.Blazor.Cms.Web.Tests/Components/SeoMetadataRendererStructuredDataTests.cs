using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.ValueObjects;
using Osirion.Blazor.Cms.Web.Components;
using Osirion.Blazor.Cms.Web.Options;
using Osirion.Blazor.Cms.Web.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.Components;

/// <summary>
/// Pins the exact JSON-LD the renderer writes for every schema type, collection pages and the generated FAQ page.
/// </summary>
public class SeoMetadataRendererStructuredDataTests : CmsWebComponentTestBase
{
    public static TheoryData<SchemaType, string> PrimarySchemas => new()
    {
        { SchemaType.Article, ArticleJson },
        { SchemaType.WebPage, WebPageJson },
        { SchemaType.Organization, OrganizationJson },
        { SchemaType.Person, PersonJson },
        { SchemaType.VideoObject, VideoObjectJson },
        { SchemaType.BreadcrumbList, BreadcrumbListJson },
        { SchemaType.WebSite, WebSiteJson },
        { SchemaType.CollectionPage, ContentCollectionPageJson },
        { (SchemaType)99, DefaultJson },
    };

    [Theory]
    [MemberData(nameof(PrimarySchemas))]
    public void SeoMetadataRenderer_SingleSchemaType_RendersItsJsonLdFirst(SchemaType schemaType, string expectedJson)
    {
        var schemas = RenderJsonLd(p => p
            .Add(c => c.Content, SeoTestContent.Article())
            .Add(c => c.SchemaTypes, [schemaType]));

        schemas[0].ShouldBe(Normalize(expectedJson));
    }

    public static TheoryData<SchemaType, string> SchemasWithPlaceholderValues => new()
    {
        { SchemaType.Product, ProductJson },
        { SchemaType.LocalBusiness, LocalBusinessJson },
        { SchemaType.Event, EventJson },
        { SchemaType.Recipe, RecipeJson },
        { SchemaType.HowTo, HowToJson },
        { SchemaType.Course, CourseJson },
        { SchemaType.Review, ReviewJson },
    };

    // Known defect, pinned as current behavior: these builders publish hard-coded values the content never stated (a 5-star rating, recipe times and yield, opening hours, an in-stock offer without a price, a paid course at price 0, a fixed event status and how-to cost). Update this expectation when it is fixed.
    [Theory]
    [MemberData(nameof(SchemasWithPlaceholderValues))]
    public void SeoMetadataRenderer_SchemaTypeWithPlaceholderValues_PublishesValuesNotInTheContent(SchemaType schemaType, string expectedJson)
    {
        var schemas = RenderJsonLd(p => p
            .Add(c => c.Content, SeoTestContent.Article())
            .Add(c => c.SchemaTypes, [schemaType]));

        schemas[0].ShouldBe(Normalize(expectedJson));
    }

    // Known defect, pinned as current behavior: the BlogPosting schema type is published as "@type": "Article". Update this expectation when it is fixed.
    [Fact]
    public void SeoMetadataRenderer_BlogPostingSchemaType_PublishesTheBlogPostingType()
    {
        var schemas = RenderJsonLd(p => p
            .Add(c => c.Content, SeoTestContent.Article())
            .Add(c => c.SchemaTypes, [SchemaType.BlogPosting]));

        schemas[0].ShouldContain("\"@type\": \"BlogPosting\"");
    }

    [Fact]
    public void SeoMetadataRenderer_SingleSchemaType_AddsTheBreadcrumbListAfterIt()
    {
        var schemas = RenderJsonLd(p => p
            .Add(c => c.Content, SeoTestContent.Article())
            .Add(c => c.SchemaTypes, [SchemaType.Article]));

        schemas.Count.ShouldBe(2);
        schemas[1].ShouldBe(Normalize(BreadcrumbListJson));
    }

    [Fact]
    public void SeoMetadataRenderer_GenerateMultipleSchemasOff_RendersOnlyTheRequestedSchema()
    {
        var schemas = RenderJsonLd(p => p
            .Add(c => c.Content, SeoTestContent.Article())
            .Add(c => c.SchemaTypes, [SchemaType.Article])
            .Add(c => c.GenerateMultipleSchemas, false));

        schemas.ShouldBe([Normalize(ArticleJson)]);
    }

    [Fact]
    public void SeoMetadataRenderer_SeveralSchemaTypes_RendersEachWithoutAddingBreadcrumbs()
    {
        var schemas = RenderJsonLd(p => p
            .Add(c => c.Content, SeoTestContent.Article())
            .Add(c => c.SchemaTypes, [SchemaType.Article, SchemaType.WebSite, SchemaType.FAQ]));

        schemas.ShouldBe([Normalize(ArticleJson), Normalize(WebSiteJson)]);
    }

    [Fact]
    public void SeoMetadataRenderer_FaqSchemaTypeAlone_RendersOnlyTheBreadcrumbList()
    {
        var schemas = RenderJsonLd(p => p
            .Add(c => c.Content, SeoTestContent.Article())
            .Add(c => c.SchemaTypes, [SchemaType.FAQ]));

        schemas.ShouldBe([Normalize(BreadcrumbListJson)]);
    }

    [Fact]
    public void SeoMetadataRenderer_NoSchemaTypes_UsesTheFrontMatterType()
    {
        var item = SeoTestContent.Article();
        item.Metadata = new FrontMatter { SeoProperties = new SeoMetadata { Type = "Recipe" } };

        var schemas = RenderJsonLd(p => p.Add(c => c.Content, item));

        schemas[0].ShouldContain("\"@type\": \"Recipe\"");
        schemas[1].ShouldBe(Normalize(BreadcrumbListJson));
    }

    [Fact]
    public void SeoMetadataRenderer_FrontMatterTypeLocalBusiness_DoesNotWriteTheTypeAsTelephone()
    {
        var item = SeoTestContent.Article();
        item.Metadata = new FrontMatter { SeoProperties = new SeoMetadata { Type = "LocalBusiness" } };

        var schemas = RenderJsonLd(p => p.Add(c => c.Content, item));

        schemas[0].ShouldContain("\"@type\": \"LocalBusiness\"");
        schemas[0].ShouldNotContain("\"telephone\"");
    }

    [Theory]
    [InlineData("Unknown")]
    [InlineData("")]
    public void SeoMetadataRenderer_FrontMatterTypeIsNotASchemaType_FallsBackToWebPage(string frontMatterType)
    {
        var item = SeoTestContent.Article();
        item.Metadata = new FrontMatter { SeoProperties = new SeoMetadata { Type = frontMatterType } };

        var schemas = RenderJsonLd(p => p.Add(c => c.Content, item));

        schemas.ShouldBe([Normalize(WebPageJson), Normalize(BreadcrumbListJson)]);
    }

    [Fact]
    public void SeoMetadataRenderer_EmptySchemaTypes_FallsBackToWebPage()
    {
        var schemas = RenderJsonLd(p => p
            .Add(c => c.Content, SeoTestContent.Article())
            .Add(c => c.SchemaTypes, []));

        schemas[0].ShouldBe(Normalize(WebPageJson));
    }

    [Fact]
    public void SeoMetadataRenderer_SchemaTypeWithoutBuilder_UsesTheFrontMatterTypeName()
    {
        var item = SeoTestContent.Article();
        item.Metadata = new FrontMatter { SeoProperties = new SeoMetadata { Type = "Gadget" } };

        var schemas = RenderJsonLd(p => p
            .Add(c => c.Content, item)
            .Add(c => c.SchemaTypes, [(SchemaType)99]));

        schemas[0].ShouldContain("\"@type\": \"Gadget\"");
        schemas[0].ShouldContain("\"name\": \"Guide to testing Blazor\"");
    }

    [Fact]
    public void SeoMetadataRenderer_ContentWithoutTagsOrImage_LeavesOutKeywordsAndSameAsAndWritesANullImage()
    {
        var item = SeoTestContent.Article(withTaxonomy: false);
        item.FeaturedImageUrl = null;

        var schemas = RenderJsonLd(p => p
            .Add(c => c.Content, item)
            .Add(c => c.SchemaTypes, [SchemaType.Article, SchemaType.Organization, SchemaType.Person, SchemaType.Recipe]));

        schemas.Count.ShouldBe(4);
        schemas.ShouldAllBe(schema => !schema.Contains("\"keywords\"") && !schema.Contains("\"sameAs\""));

        // The null-ignore option does not apply to dictionary entries, so a missing image is written as null.
        schemas.ShouldAllBe(schema => schema.Contains("\"image\": null"));
        schemas[1].ShouldContain("\"logo\": \"http://localhost/logo.png\"");
    }

    [Fact]
    public void SeoMetadataRenderer_CustomBreadcrumbs_ReplaceThePathSegments()
    {
        var schemas = RenderJsonLd(p => p
            .Add(c => c.Content, SeoTestContent.Article())
            .Add(c => c.SchemaTypes, [SchemaType.BreadcrumbList])
            .Add(c => c.CustomBreadcrumbs, [("Docs", "http://localhost/docs"), ("Page", "http://localhost/docs/page")]));

        schemas.ShouldBe([Normalize(CustomBreadcrumbJson)]);
    }

    [Fact]
    public void SeoMetadataRenderer_ContentWithoutUrl_BreadcrumbsHomeAndThePage()
    {
        var item = SeoTestContent.Article(url: string.Empty);

        var schemas = RenderJsonLd(p => p
            .Add(c => c.Content, item)
            .Add(c => c.SchemaTypes, [SchemaType.BreadcrumbList]));

        schemas.ShouldBe([Normalize(NoUrlBreadcrumbJson)]);
    }

    [Fact]
    public void SeoMetadataRenderer_CollectionPageSchema_RendersTheCollectionThenOneArticlePerItem()
    {
        var schemas = RenderJsonLd(p => p
            .Add(c => c.ContentItems, [SeoTestContent.Article(), SeoTestContent.BarePost()])
            .Add(c => c.CollectionTitle, "All posts")
            .Add(c => c.CollectionDescription, "Every post on the site.")
            .Add(c => c.SchemaTypes, [SchemaType.CollectionPage]));

        schemas.ShouldBe([
            Normalize(CollectionPageJson),
            Normalize(FirstCollectionPostingJson),
            Normalize(SecondCollectionPostingJson)]);
    }

    [Fact]
    public void SeoMetadataRenderer_CollectionWithoutCollectionPageSchema_RendersOnlyTheArticles()
    {
        var schemas = RenderJsonLd(p => p
            .Add(c => c.ContentItems, [SeoTestContent.Article(), SeoTestContent.BarePost()])
            .Add(c => c.CollectionTitle, "All posts")
            .Add(c => c.CollectionDescription, "Every post on the site."));

        schemas.ShouldBe([Normalize(FirstCollectionPostingJson), Normalize(SecondCollectionPostingJson)]);
    }

    [Fact]
    public void SeoMetadataRenderer_ContentAndContentItems_RendersThePageSchemasThenThePostings()
    {
        var schemas = RenderJsonLd(p => p
            .Add(c => c.Content, SeoTestContent.Article())
            .Add(c => c.ContentItems, [SeoTestContent.BarePost()]));

        schemas.Count.ShouldBe(3);
        schemas[0].ShouldContain("\"@type\": \"WebPage\"");
        schemas[1].ShouldBe(Normalize(BreadcrumbListJson));
        schemas[2].ShouldContain("\"headline\": \"Second post\"");
    }

    // Known defect, pinned as current behavior: the generated FAQ question for "Guide to testing Blazor" is the ungrammatical "How to testing Blazor?". Update this expectation when it is fixed.
    [Fact]
    public void SeoMetadataRenderer_GeneratedFaq_AppendsTheFaqPage()
    {
        var schemas = RenderJsonLd(p => p
            .Add(c => c.Content, SeoTestContent.Article())
            .Add(c => c.EnableGeneratedFaq, true));

        schemas.Count.ShouldBe(3);
        schemas[2].ShouldBe(Normalize(FaqPageJson));
    }

    // Known defect, pinned as current behavior: the generated FAQ question for "Guide to testing Blazor" is the ungrammatical "How to testing Blazor?". Update this expectation when it is fixed.
    [Fact]
    public void SeoMetadataRenderer_ValidCustomJsonLd_SkipsGeneratedSchemasButKeepsTheGeneratedFaq()
    {
        var item = SeoTestContent.Article();
        item.Metadata = new FrontMatter { SeoProperties = new SeoMetadata { JsonLd = """{"@type":"Thing"}""" } };

        var schemas = RenderJsonLd(p => p
            .Add(c => c.Content, item)
            .Add(c => c.EnableGeneratedFaq, true));

        schemas.Count.ShouldBe(2);
        schemas[0].ShouldBe("{\n  \"@type\": \"Thing\"\n}");
        schemas[1].ShouldBe(Normalize(FaqPageJson));
    }

    [Fact]
    public void SeoMetadataRenderer_AuthorUrl_NamesTheAuthorsPageAbsolute()
    {
        var schemas = RenderJsonLd(p => p
            .Add(c => c.Content, SeoTestContent.Article())
            .Add(c => c.SchemaTypes, [SchemaType.Article])
            .Add(c => c.AuthorUrl, "/about"));

        schemas[0].ShouldContain(Normalize(AuthorWithAboutPage));
    }

    [Fact]
    public void SeoMetadataRenderer_SeoOptionsAuthorUrl_ReachesEveryPostingInACollection()
    {
        Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new SeoMetadataOptions { AuthorUrl = "/about" }));

        var schemas = RenderJsonLd(p => p
            .Add(c => c.ContentItems, [SeoTestContent.Article()])
            .Add(c => c.CollectionTitle, "All posts"));

        schemas.ShouldHaveSingleItem().ShouldContain(Normalize(AuthorWithAboutPage));
    }

    [Fact]
    public void SeoMetadataRenderer_SeoOptionsTurnMultipleSchemasOff_RendersOnlyTheRequestedSchema()
    {
        Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new SeoMetadataOptions { GenerateMultipleSchemas = false }));

        var schemas = RenderJsonLd(p => p
            .Add(c => c.Content, SeoTestContent.Article())
            .Add(c => c.SchemaTypes, [SchemaType.Article]));

        schemas.ShouldBe([Normalize(ArticleJson)]);
    }

    private const string AuthorWithAboutPage = """
          "author": {
            "@type": "Person",
            "name": "Jane Doe",
            "url": "http://localhost/about"
          }
        """;

    private List<string> RenderJsonLd(Action<ComponentParameterCollectionBuilder<SeoMetadataRenderer>> parameters)
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo(SeoTestContent.PageUrl);

        var cut = RenderStatic(parameters);

        return cut.FindAll("script[type='application/ld+json']")
            .Select(script => Normalize(script.TextContent.Trim()))
            .ToList();
    }

    private static string Normalize(string json) => json.ReplaceLineEndings("\n");

    private const string ArticleJson = """
        {
          "@context": "https://schema.org",
          "@type": "Article",
          "headline": "Guide to testing Blazor",
          "description": "A practical description of testing. It covers the basics.",
          "image": "http://localhost/images/cover.png",
          "author": {
            "@type": "Person",
            "name": "Jane Doe"
          },
          "publisher": {
            "@type": "Organization",
            "name": "localhost",
            "logo": {
              "@type": "ImageObject",
              "url": "http://localhost/logo.png"
            }
          },
          "datePublished": "2020-01-02T03:04:05Z",
          "dateModified": "2020-02-03T04:05:06Z",
          "mainEntityOfPage": {
            "@type": "WebPage",
            "@id": "http://localhost/blog/guides/my-post"
          },
          "articleSection": "Guides",
          "wordCount": 5,
          "inLanguage": "en",
          "url": "http://localhost/blog/guides/my-post",
          "timeRequired": "PT1M",
          "keywords": "blazor, testing"
        }
        """;

    private const string WebPageJson = """
        {
          "@context": "https://schema.org",
          "@type": "WebPage",
          "name": "Guide to testing Blazor",
          "description": "A practical description of testing. It covers the basics.",
          "url": "http://localhost/blog/guides/my-post",
          "datePublished": "2020-01-02T03:04:05Z",
          "dateModified": "2020-02-03T04:05:06Z",
          "author": {
            "@type": "Person",
            "name": "Jane Doe"
          },
          "publisher": {
            "@type": "Organization",
            "name": "localhost"
          }
        }
        """;

    private const string ProductJson = """
        {
          "@context": "https://schema.org",
          "@type": "Product",
          "name": "Guide to testing Blazor",
          "description": "A practical description of testing. It covers the basics.",
          "image": "http://localhost/images/cover.png",
          "url": "http://localhost/blog/guides/my-post",
          "brand": {
            "@type": "Brand",
            "name": "localhost"
          },
          "offers": {
            "@type": "Offer",
            "url": "http://localhost/blog/guides/my-post",
            "priceCurrency": "USD",
            "availability": "https://schema.org/InStock"
          }
        }
        """;

    private const string OrganizationJson = """
        {
          "@context": "https://schema.org",
          "@type": "Organization",
          "name": "Guide to testing Blazor",
          "description": "A practical description of testing. It covers the basics.",
          "url": "http://localhost/blog/guides/my-post",
          "logo": "http://localhost/images/cover.png",
          "image": "http://localhost/images/cover.png",
          "sameAs": [
            "blazor",
            "testing"
          ]
        }
        """;

    private const string PersonJson = """
        {
          "@context": "https://schema.org",
          "@type": "Person",
          "name": "Guide to testing Blazor",
          "description": "A practical description of testing. It covers the basics.",
          "url": "http://localhost/blog/guides/my-post",
          "image": "http://localhost/images/cover.png",
          "sameAs": [
            "blazor",
            "testing"
          ]
        }
        """;

    private const string LocalBusinessJson = """
        {
          "@context": "https://schema.org",
          "@type": "LocalBusiness",
          "name": "Guide to testing Blazor",
          "description": "A practical description of testing. It covers the basics.",
          "image": "http://localhost/images/cover.png",
          "url": "http://localhost/blog/guides/my-post",
          "address": {
            "@type": "PostalAddress",
            "streetAddress": "",
            "addressLocality": "",
            "addressRegion": "",
            "postalCode": "",
            "addressCountry": ""
          },
          "geo": {
            "@type": "GeoCoordinates",
            "latitude": "",
            "longitude": ""
          },
          "openingHoursSpecification": [
            {
              "@type": "OpeningHoursSpecification",
              "dayOfWeek": [
                "Monday",
                "Tuesday",
                "Wednesday",
                "Thursday",
                "Friday"
              ],
              "opens": "09:00",
              "closes": "17:00"
            }
          ]
        }
        """;

    private const string EventJson = """
        {
          "@context": "https://schema.org",
          "@type": "Event",
          "name": "Guide to testing Blazor",
          "description": "A practical description of testing. It covers the basics.",
          "image": "http://localhost/images/cover.png",
          "url": "http://localhost/blog/guides/my-post",
          "startDate": "2020-01-02T03:04:05Z",
          "eventStatus": "https://schema.org/EventScheduled",
          "eventAttendanceMode": "https://schema.org/OfflineEventAttendanceMode",
          "location": {
            "@type": "Place",
            "name": "localhost",
            "address": {
              "@type": "PostalAddress",
              "streetAddress": "",
              "addressLocality": "",
              "addressRegion": "",
              "postalCode": "",
              "addressCountry": ""
            }
          },
          "organizer": {
            "@type": "Organization",
            "name": "localhost",
            "url": "http://localhost"
          }
        }
        """;

    private const string RecipeJson = """
        {
          "@context": "https://schema.org",
          "@type": "Recipe",
          "name": "Guide to testing Blazor",
          "description": "A practical description of testing. It covers the basics.",
          "image": "http://localhost/images/cover.png",
          "author": {
            "@type": "Person",
            "name": "Jane Doe"
          },
          "datePublished": "2020-01-02T03:04:05Z",
          "prepTime": "PT30M",
          "cookTime": "PT1H",
          "totalTime": "PT1H30M",
          "recipeYield": "4 servings",
          "recipeCategory": "Guides",
          "recipeCuisine": "",
          "recipeIngredient": [],
          "recipeInstructions": [],
          "aggregateRating": {
            "@type": "AggregateRating",
            "ratingValue": "5",
            "ratingCount": "1"
          },
          "keywords": "blazor, testing"
        }
        """;

    private const string HowToJson = """
        {
          "@context": "https://schema.org",
          "@type": "HowTo",
          "name": "Guide to testing Blazor",
          "description": "A practical description of testing. It covers the basics.",
          "image": "http://localhost/images/cover.png",
          "totalTime": "PT1H",
          "estimatedCost": {
            "@type": "MonetaryAmount",
            "currency": "USD",
            "value": "0"
          },
          "tool": [],
          "supply": [],
          "step": []
        }
        """;

    private const string VideoObjectJson = """
        {
          "@context": "https://schema.org",
          "@type": "VideoObject",
          "name": "Guide to testing Blazor",
          "description": "A practical description of testing. It covers the basics.",
          "thumbnailUrl": "http://localhost/images/cover.png",
          "uploadDate": "2020-01-02T03:04:05Z",
          "contentUrl": "http://localhost/blog/guides/my-post",
          "embedUrl": "http://localhost/blog/guides/my-post",
          "duration": "PT1M",
          "author": {
            "@type": "Person",
            "name": "Jane Doe"
          },
          "publisher": {
            "@type": "Organization",
            "name": "localhost",
            "logo": {
              "@type": "ImageObject",
              "url": "http://localhost/logo.png"
            }
          }
        }
        """;

    private const string CourseJson = """
        {
          "@context": "https://schema.org",
          "@type": "Course",
          "name": "Guide to testing Blazor",
          "description": "A practical description of testing. It covers the basics.",
          "provider": {
            "@type": "Organization",
            "name": "localhost",
            "url": "http://localhost"
          },
          "image": "http://localhost/images/cover.png",
          "aggregateRating": {
            "@type": "AggregateRating",
            "ratingValue": "5",
            "ratingCount": "1"
          },
          "offers": {
            "@type": "Offer",
            "category": "Paid",
            "priceCurrency": "USD",
            "price": "0"
          }
        }
        """;

    private const string ReviewJson = """
        {
          "@context": "https://schema.org",
          "@type": "Review",
          "itemReviewed": {
            "@type": "Thing",
            "name": "Guide to testing Blazor"
          },
          "author": {
            "@type": "Person",
            "name": "Jane Doe"
          },
          "datePublished": "2020-01-02T03:04:05Z",
          "reviewBody": "A practical description of testing. It covers the basics.",
          "reviewRating": {
            "@type": "Rating",
            "ratingValue": "5",
            "bestRating": "5",
            "worstRating": "1"
          }
        }
        """;

    private const string BreadcrumbListJson = """
        {
          "@context": "https://schema.org",
          "@type": "BreadcrumbList",
          "itemListElement": [
            {
              "@type": "ListItem",
              "position": 1,
              "name": "Home",
              "item": "http://localhost"
            },
            {
              "@type": "ListItem",
              "position": 2,
              "name": "Blog",
              "item": "http://localhost/blog"
            },
            {
              "@type": "ListItem",
              "position": 3,
              "name": "Guides",
              "item": "http://localhost/blog/guides"
            },
            {
              "@type": "ListItem",
              "position": 4,
              "name": "Guide to testing Blazor",
              "item": "http://localhost/blog/guides/my-post"
            }
          ]
        }
        """;

    private const string WebSiteJson = """
        {
          "@context": "https://schema.org",
          "@type": "WebSite",
          "name": "localhost",
          "url": "http://localhost",
          "description": "A practical description of testing. It covers the basics.",
          "inLanguage": "en",
          "potentialAction": {
            "@type": "SearchAction",
            "target": {
              "@type": "EntryPoint",
              "urlTemplate": "http://localhost/search?q={search_term_string}"
            },
            "query-input": "required name=search_term_string"
          },
          "publisher": {
            "@type": "Organization",
            "name": "localhost",
            "url": "http://localhost",
            "logo": {
              "@type": "ImageObject",
              "url": "http://localhost/logo.png"
            }
          }
        }
        """;

    private const string ContentCollectionPageJson = """
        {
          "@context": "https://schema.org",
          "@type": "CollectionPage",
          "name": "Guide to testing Blazor | localhost",
          "description": "A practical description of testing. It covers the basics.",
          "url": "http://localhost/blog/guides/my-post",
          "inLanguage": "en",
          "mainEntity": {
            "@type": "ItemList",
            "numberOfItems": 0,
            "itemListElement": []
          },
          "breadcrumb": {
            "@type": "BreadcrumbList",
            "itemListElement": [
              {
                "@type": "ListItem",
                "position": 1,
                "name": "Home",
                "item": "http://localhost"
              },
              {
                "@type": "ListItem",
                "position": 2,
                "name": "Collection",
                "item": "http://localhost/blog/guides/my-post"
              }
            ]
          },
          "publisher": {
            "@type": "Organization",
            "name": "localhost",
            "url": "http://localhost",
            "logo": {
              "@type": "ImageObject",
              "url": "http://localhost/logo.png"
            }
          }
        }
        """;

    private const string DefaultJson = """
        {
          "@context": "https://schema.org",
          "@type": "Thing",
          "name": "Guide to testing Blazor",
          "description": "A practical description of testing. It covers the basics.",
          "url": "http://localhost/blog/guides/my-post",
          "inLanguage": "en"
        }
        """;

    private const string CollectionPageJson = """
        {
          "@context": "https://schema.org",
          "@type": "CollectionPage",
          "name": "All posts",
          "description": "Every post on the site.",
          "url": "http://localhost/blog/guides/my-post",
          "inLanguage": "en",
          "mainEntity": {
            "@type": "ItemList",
            "numberOfItems": 2,
            "itemListElement": [
              {
                "@type": "ListItem",
                "position": 1,
                "url": "http://localhost/blog/guides/my-post",
                "name": "Guide to testing Blazor",
                "description": "A practical description of testing. It covers the basics.",
                "datePublished": "2020-01-02T03:04:05Z",
                "image": "http://localhost/images/cover.png"
              },
              {
                "@type": "ListItem",
                "position": 2,
                "url": "http://localhost/blog/second",
                "name": "Second post",
                "description": "A practical description of testing. It covers the basics.",
                "datePublished": "2020-01-02T03:04:05Z"
              }
            ]
          },
          "breadcrumb": {
            "@type": "BreadcrumbList",
            "itemListElement": [
              {
                "@type": "ListItem",
                "position": 1,
                "name": "Home",
                "item": "http://localhost"
              },
              {
                "@type": "ListItem",
                "position": 2,
                "name": "All posts",
                "item": "http://localhost/blog/guides/my-post"
              }
            ]
          },
          "publisher": {
            "@type": "Organization",
            "name": "localhost",
            "url": "http://localhost",
            "logo": {
              "@type": "ImageObject",
              "url": "http://localhost/logo.png"
            }
          }
        }
        """;

    private const string FirstCollectionPostingJson = """
        {
          "@context": "https://schema.org",
          "@type": "Article",
          "headline": "Guide to testing Blazor",
          "description": "A practical description of testing. It covers the basics.",
          "image": "http://localhost/images/cover.png",
          "datePublished": "2020-01-02T03:04:05Z",
          "dateModified": "2020-02-03T04:05:06Z",
          "author": {
            "@type": "Person",
            "name": "Jane Doe"
          },
          "publisher": {
            "@type": "Organization",
            "name": "localhost",
            "logo": {
              "@type": "ImageObject",
              "url": "http://localhost/logo.png"
            }
          },
          "url": "http://localhost/blog/guides/my-post",
          "mainEntityOfPage": {
            "@type": "WebPage",
            "@id": "http://localhost/blog/guides/my-post"
          },
          "keywords": "blazor, testing",
          "articleSection": "Guides",
          "inLanguage": "en",
          "timeRequired": "PT1M"
        }
        """;

    private const string SecondCollectionPostingJson = """
        {
          "@context": "https://schema.org",
          "@type": "Article",
          "headline": "Second post",
          "description": "A practical description of testing. It covers the basics.",
          "image": "http://localhost/images/cover.png",
          "datePublished": "2020-01-02T03:04:05Z",
          "dateModified": "2020-01-02T03:04:05Z",
          "publisher": {
            "@type": "Organization",
            "name": "localhost",
            "logo": {
              "@type": "ImageObject",
              "url": "http://localhost/logo.png"
            }
          },
          "url": "http://localhost/blog/second",
          "mainEntityOfPage": {
            "@type": "WebPage",
            "@id": "http://localhost/blog/second"
          },
          "keywords": "blazor, testing",
          "articleSection": "Guides",
          "inLanguage": "en",
          "timeRequired": "PT1M"
        }
        """;

    private const string CustomBreadcrumbJson = """
        {
          "@context": "https://schema.org",
          "@type": "BreadcrumbList",
          "itemListElement": [
            {
              "@type": "ListItem",
              "position": 1,
              "name": "Home",
              "item": "http://localhost"
            },
            {
              "@type": "ListItem",
              "position": 2,
              "name": "Docs",
              "item": "http://localhost/docs"
            },
            {
              "@type": "ListItem",
              "position": 3,
              "name": "Page",
              "item": "http://localhost/docs/page"
            }
          ]
        }
        """;

    private const string NoUrlBreadcrumbJson = """
        {
          "@context": "https://schema.org",
          "@type": "BreadcrumbList",
          "itemListElement": [
            {
              "@type": "ListItem",
              "position": 1,
              "name": "Home",
              "item": "http://localhost"
            },
            {
              "@type": "ListItem",
              "position": 2,
              "name": "Guide to testing Blazor",
              "item": "http://localhost/blog/guides/my-post"
            }
          ]
        }
        """;

    private const string FaqPageJson = """
        {
          "@context": "https://schema.org",
          "@type": "FAQPage",
          "mainEntity": [
            {
              "@type": "Question",
              "name": "How to testing Blazor?",
              "acceptedAnswer": {
                "@type": "Answer",
                "text": "A practical description of testing. It covers the basics."
              }
            }
          ]
        }
        """;
}
