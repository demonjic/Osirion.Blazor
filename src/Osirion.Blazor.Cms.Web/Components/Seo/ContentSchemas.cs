namespace Osirion.Blazor.Cms.Web.Components.Seo;

/// <summary>
/// Builds the schema.org objects that describe a single content item. Each builder returns a dictionary whose key
/// order is the property order of the serialized JSON-LD.
/// </summary>
internal static class ContentSchemas
{
    private const string SchemaContext = "https://schema.org";

    internal static object Article(SeoSchemaContext context, string type)
    {
        var content = context.Content!;
        var schema = new Dictionary<string, object?>
        {
            ["@context"] = SchemaContext,
            ["@type"] = type,
            ["headline"] = content.Title,
            ["description"] = content.Description,
            ["image"] = context.NormalizeImageUrl(content.FeaturedImageUrl),
            ["author"] = Author(content.Author, context.AuthorUrl),
            ["publisher"] = new Dictionary<string, object?>
            {
                ["@type"] = "Organization",
                ["name"] = context.SiteName,
                // No width or height: the logo's real size is not known here, and an invented one is wrong data.
                ["logo"] = new Dictionary<string, object?>
                {
                    ["@type"] = "ImageObject",
                    ["url"] = context.PublisherLogoUrl
                }
            },
            ["datePublished"] = ContentUrls.FormatUtc(content.PublishDate),
            ["dateModified"] = ContentUrls.FormatUtc(content.LastModified ?? content.PublishDate),
            ["mainEntityOfPage"] = new Dictionary<string, object?>
            {
                ["@type"] = "WebPage",
                ["@id"] = context.CurrentUrl
            },
            ["articleSection"] = content.Categories.FirstOrDefault(),
            ["wordCount"] = content.Content?.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length,
            ["inLanguage"] = context.LanguageCode,
            ["url"] = context.CurrentUrl,
            ["timeRequired"] = $"PT{content.ReadTimeMinutes}M"
        };

        if (content.Tags.Any())
        {
            schema["keywords"] = string.Join(", ", content.Tags);
        }

        return schema;
    }

    internal static Dictionary<string, object?> WebPage(SeoSchemaContext context, string type)
    {
        var content = context.Content!;
        return new Dictionary<string, object?>
        {
            ["@context"] = SchemaContext,
            ["@type"] = type,
            ["name"] = content.Title,
            ["description"] = content.Description,
            ["url"] = context.CurrentUrl,
            ["datePublished"] = ContentUrls.FormatUtc(content.PublishDate),
            ["dateModified"] = content.LastModified is { } lastModified ? ContentUrls.FormatUtc(lastModified) : null,
            ["author"] = Person(content.Author),
            ["publisher"] = Organization(context.SiteName)
        };
    }

    internal static object AboutPage(SeoSchemaContext context)
    {
        var schema = WebPage(context, "AboutPage");
        schema["mainEntity"] = new Dictionary<string, object?>
        {
            ["@type"] = "Organization",
            ["name"] = context.SiteName,
            ["url"] = context.BaseUrl,
            ["logo"] = context.NormalizeImageUrl(context.SiteLogoUrl)
        };

        return schema;
    }

    // Search engines show software rich results only with a price and a rating or review, which a content page does
    // not carry; front matter json_ld can supply the full schema when those exist.
    internal static object SoftwareApplication(SeoSchemaContext context)
    {
        var content = context.Content!;
        var schema = new Dictionary<string, object?>
        {
            ["@context"] = SchemaContext,
            ["@type"] = "SoftwareApplication",
            ["name"] = content.Title,
            ["description"] = content.Description,
            ["url"] = context.CurrentUrl,
            ["image"] = context.NormalizeImageUrl(content.FeaturedImageUrl),
            ["applicationCategory"] = context.ApplicationCategory,
            ["inLanguage"] = context.LanguageCode,
            ["dateModified"] = ContentUrls.FormatUtc(content.LastModified ?? content.PublishDate),
            ["publisher"] = Organization(context.SiteName)
        };

        if (content.Tags.Any())
        {
            schema["keywords"] = string.Join(", ", content.Tags);
        }

        return schema;
    }

    // An author's page: the page is the ProfilePage and the person it describes is its main entity. Only tags that
    // are absolute links are profiles elsewhere (sameAs).
    internal static object ProfilePage(SeoSchemaContext context)
    {
        var content = context.Content!;
        var profiles = content.Tags
            .Where(tag => Uri.TryCreate(tag, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp))
            .ToArray();

        return new Dictionary<string, object?>
        {
            ["@context"] = SchemaContext,
            ["@type"] = "ProfilePage",
            ["url"] = context.CurrentUrl,
            ["dateCreated"] = ContentUrls.FormatUtc(content.PublishDate),
            ["dateModified"] = content.LastModified is { } lastModified ? ContentUrls.FormatUtc(lastModified) : null,
            ["inLanguage"] = context.LanguageCode,
            ["mainEntity"] = new Dictionary<string, object?>
            {
                ["@type"] = "Person",
                ["name"] = string.IsNullOrWhiteSpace(content.Author) ? content.Title : content.Author,
                ["description"] = content.Description,
                ["image"] = context.NormalizeImageUrl(content.FeaturedImageUrl),
                ["sameAs"] = profiles.Length > 0 ? profiles : null
            }
        };
    }

    internal static object Product(SeoSchemaContext context)
    {
        var content = context.Content!;
        return new Dictionary<string, object?>
        {
            ["@context"] = SchemaContext,
            ["@type"] = "Product",
            ["name"] = content.Title,
            ["description"] = content.Description,
            ["image"] = context.NormalizeImageUrl(content.FeaturedImageUrl),
            ["url"] = context.CurrentUrl,
            ["brand"] = new Dictionary<string, object?>
            {
                ["@type"] = "Brand",
                ["name"] = context.SiteName
            },
            ["offers"] = new Dictionary<string, object?>
            {
                ["@type"] = "Offer",
                ["url"] = context.CurrentUrl,
                ["priceCurrency"] = "USD",
                ["availability"] = "https://schema.org/InStock"
            }
        };
    }

    internal static object Organization(SeoSchemaContext context)
    {
        var content = context.Content!;
        var schema = new Dictionary<string, object?>
        {
            ["@context"] = SchemaContext,
            ["@type"] = "Organization",
            ["name"] = content.Title ?? context.SiteName,
            ["description"] = content.Description,
            ["url"] = context.CurrentUrl,
            ["logo"] = context.NormalizeImageUrl(content.FeaturedImageUrl) ?? $"{context.BaseUrl}/logo.png",
            ["image"] = context.NormalizeImageUrl(content.FeaturedImageUrl)
        };

        AddSameAs(schema, content.Tags);
        return schema;
    }

    internal static object Person(SeoSchemaContext context)
    {
        var content = context.Content!;
        var schema = new Dictionary<string, object?>
        {
            ["@context"] = SchemaContext,
            ["@type"] = "Person",
            ["name"] = content.Title ?? content.Author,
            ["description"] = content.Description,
            ["url"] = context.CurrentUrl,
            ["image"] = context.NormalizeImageUrl(content.FeaturedImageUrl)
        };

        AddSameAs(schema, content.Tags);
        return schema;
    }

    internal static object LocalBusiness(SeoSchemaContext context)
    {
        var content = context.Content!;
        return new Dictionary<string, object?>
        {
            ["@context"] = SchemaContext,
            ["@type"] = "LocalBusiness",
            ["name"] = content.Title,
            ["description"] = content.Description,
            ["image"] = context.NormalizeImageUrl(content.FeaturedImageUrl),
            ["url"] = context.CurrentUrl,
            ["address"] = EmptyPostalAddress(),
            ["geo"] = new Dictionary<string, object?>
            {
                ["@type"] = "GeoCoordinates",
                ["latitude"] = "",
                ["longitude"] = ""
            },
            ["openingHoursSpecification"] = new[]
            {
                new Dictionary<string, object?>
                {
                    ["@type"] = "OpeningHoursSpecification",
                    ["dayOfWeek"] = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" },
                    ["opens"] = "09:00",
                    ["closes"] = "17:00"
                }
            }
        };
    }

    internal static object Event(SeoSchemaContext context)
    {
        var content = context.Content!;
        return new Dictionary<string, object?>
        {
            ["@context"] = SchemaContext,
            ["@type"] = "Event",
            ["name"] = content.Title,
            ["description"] = content.Description,
            ["image"] = context.NormalizeImageUrl(content.FeaturedImageUrl),
            ["url"] = context.CurrentUrl,
            ["startDate"] = ContentUrls.FormatUtc(content.PublishDate),
            ["eventStatus"] = "https://schema.org/EventScheduled",
            ["eventAttendanceMode"] = "https://schema.org/OfflineEventAttendanceMode",
            ["location"] = new Dictionary<string, object?>
            {
                ["@type"] = "Place",
                ["name"] = context.SiteName,
                ["address"] = EmptyPostalAddress()
            },
            ["organizer"] = Organization(context.SiteName, context.BaseUrl)
        };
    }

    internal static object Recipe(SeoSchemaContext context)
    {
        var content = context.Content!;
        var schema = new Dictionary<string, object?>
        {
            ["@context"] = SchemaContext,
            ["@type"] = "Recipe",
            ["name"] = content.Title,
            ["description"] = content.Description,
            ["image"] = context.NormalizeImageUrl(content.FeaturedImageUrl),
            ["author"] = Person(content.Author),
            ["datePublished"] = ContentUrls.FormatUtc(content.PublishDate),
            ["prepTime"] = "PT30M",
            ["cookTime"] = "PT1H",
            ["totalTime"] = "PT1H30M",
            ["recipeYield"] = "4 servings",
            ["recipeCategory"] = content.Categories.FirstOrDefault(),
            ["recipeCuisine"] = "",
            ["recipeIngredient"] = Array.Empty<string>(),
            ["recipeInstructions"] = Array.Empty<object>(),
            ["aggregateRating"] = FiveStarRating()
        };

        if (content.Tags.Any())
        {
            schema["keywords"] = string.Join(", ", content.Tags);
        }

        return schema;
    }

    internal static object HowTo(SeoSchemaContext context)
    {
        var content = context.Content!;
        return new Dictionary<string, object?>
        {
            ["@context"] = SchemaContext,
            ["@type"] = "HowTo",
            ["name"] = content.Title,
            ["description"] = content.Description,
            ["image"] = context.NormalizeImageUrl(content.FeaturedImageUrl),
            ["totalTime"] = "PT1H",
            ["estimatedCost"] = new Dictionary<string, object?>
            {
                ["@type"] = "MonetaryAmount",
                ["currency"] = "USD",
                ["value"] = "0"
            },
            ["tool"] = Array.Empty<object>(),
            ["supply"] = Array.Empty<object>(),
            ["step"] = Array.Empty<object>()
        };
    }

    internal static object Video(SeoSchemaContext context)
    {
        var content = context.Content!;
        return new Dictionary<string, object?>
        {
            ["@context"] = SchemaContext,
            ["@type"] = "VideoObject",
            ["name"] = content.Title,
            ["description"] = content.Description,
            ["thumbnailUrl"] = context.NormalizeImageUrl(content.FeaturedImageUrl),
            ["uploadDate"] = ContentUrls.FormatUtc(content.PublishDate),
            ["contentUrl"] = context.CurrentUrl,
            ["embedUrl"] = context.CurrentUrl,
            ["duration"] = "PT1M",
            ["author"] = Person(content.Author),
            ["publisher"] = new Dictionary<string, object?>
            {
                ["@type"] = "Organization",
                ["name"] = context.SiteName,
                ["logo"] = new Dictionary<string, object?>
                {
                    ["@type"] = "ImageObject",
                    ["url"] = $"{context.BaseUrl}/logo.png"
                }
            }
        };
    }

    internal static object Course(SeoSchemaContext context)
    {
        var content = context.Content!;
        return new Dictionary<string, object?>
        {
            ["@context"] = SchemaContext,
            ["@type"] = "Course",
            ["name"] = content.Title,
            ["description"] = content.Description,
            ["provider"] = Organization(context.SiteName, context.BaseUrl),
            ["image"] = context.NormalizeImageUrl(content.FeaturedImageUrl),
            ["aggregateRating"] = FiveStarRating(),
            ["offers"] = new Dictionary<string, object?>
            {
                ["@type"] = "Offer",
                ["category"] = "Paid",
                ["priceCurrency"] = "USD",
                ["price"] = "0"
            }
        };
    }

    internal static object Review(SeoSchemaContext context)
    {
        var content = context.Content!;
        return new Dictionary<string, object?>
        {
            ["@context"] = SchemaContext,
            ["@type"] = "Review",
            ["itemReviewed"] = new Dictionary<string, object?>
            {
                ["@type"] = "Thing",
                ["name"] = content.Title
            },
            ["author"] = Person(content.Author),
            ["datePublished"] = ContentUrls.FormatUtc(content.PublishDate),
            ["reviewBody"] = content.Description,
            ["reviewRating"] = new Dictionary<string, object?>
            {
                ["@type"] = "Rating",
                ["ratingValue"] = "5",
                ["bestRating"] = "5",
                ["worstRating"] = "1"
            }
        };
    }

    internal static object WebSite(SeoSchemaContext context) => new Dictionary<string, object?>
    {
        ["@context"] = SchemaContext,
        ["@type"] = "WebSite",
        ["name"] = context.SiteName,
        ["url"] = context.BaseUrl,
        ["description"] = context.Content?.Description,
        ["inLanguage"] = context.LanguageCode,
        ["potentialAction"] = new Dictionary<string, object?>
        {
            ["@type"] = "SearchAction",
            ["target"] = new Dictionary<string, object?>
            {
                ["@type"] = "EntryPoint",
                ["urlTemplate"] = $"{context.BaseUrl}/search?q={{search_term_string}}"
            },
            ["query-input"] = "required name=search_term_string"
        },
        ["publisher"] = PublisherWithLogo(context)
    };

    /// <summary>The schema for a type without a dedicated builder: the front matter type, or <c>Thing</c>.</summary>
    internal static object Default(SeoSchemaContext context) => new Dictionary<string, object?>
    {
        ["@context"] = SchemaContext,
        ["@type"] = SeoText.FirstNonBlank(context.Content?.Metadata?.SeoProperties?.Type) ?? "Thing",
        ["name"] = context.Content?.Title,
        ["description"] = context.Content?.Description,
        ["url"] = context.CurrentUrl,
        ["inLanguage"] = context.LanguageCode
    };

    /// <summary>The site as publisher, with its URL and logo.</summary>
    internal static Dictionary<string, object?> PublisherWithLogo(SeoSchemaContext context) => new()
    {
        ["@type"] = "Organization",
        ["name"] = context.SiteName,
        ["url"] = context.BaseUrl,
        ["logo"] = new Dictionary<string, object?>
        {
            ["@type"] = "ImageObject",
            ["url"] = context.PublisherLogoUrl
        }
    };

    private static void AddSameAs(Dictionary<string, object?> schema, IReadOnlyList<string> tags)
    {
        if (tags.Any())
        {
            schema["sameAs"] = tags.ToArray();
        }
    }

    private static Dictionary<string, object?> Person(string name) => new()
    {
        ["@type"] = "Person",
        ["name"] = name
    };

    // A url only when the site names a page for the author: Google asks for "a link to a web page that uniquely
    // identifies the author", and an address guessed from the name answers 404.
    internal static Dictionary<string, object?> Author(string name, string? url)
    {
        var author = Person(name);
        if (url is not null)
        {
            author["url"] = url;
        }

        return author;
    }

    private static Dictionary<string, object?> Organization(string name) => new()
    {
        ["@type"] = "Organization",
        ["name"] = name
    };

    private static Dictionary<string, object?> Organization(string name, string url) => new()
    {
        ["@type"] = "Organization",
        ["name"] = name,
        ["url"] = url
    };

    private static Dictionary<string, object?> FiveStarRating() => new()
    {
        ["@type"] = "AggregateRating",
        ["ratingValue"] = "5",
        ["ratingCount"] = "1"
    };

    private static Dictionary<string, object?> EmptyPostalAddress() => new()
    {
        ["@type"] = "PostalAddress",
        ["streetAddress"] = "",
        ["addressLocality"] = "",
        ["addressRegion"] = "",
        ["postalCode"] = "",
        ["addressCountry"] = ""
    };
}
