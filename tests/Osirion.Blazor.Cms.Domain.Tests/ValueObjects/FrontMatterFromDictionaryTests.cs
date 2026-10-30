using System.Collections;
using System.Globalization;
using Osirion.Blazor.Cms.Domain.ValueObjects;
using YamlDotNet.Serialization;

namespace Osirion.Blazor.Cms.Domain.Tests.ValueObjects;

public class FrontMatterFromDictionaryTests
{
    private static FrontMatter Parse(params (string Key, string Value)[] pairs) =>
        FrontMatter.FromDictionary(pairs.ToDictionary(pair => pair.Key, pair => pair.Value));

    // Reads the YAML that ToYaml writes the way a front matter reader hands it to FromDictionary: scalars as text,
    // sequences as "[a, b]"; nested mappings (custom_fields, seo_properties) and empty values have no flat form.
    private static FrontMatter RoundTrip(FrontMatter original)
    {
        var yaml = original.ToYaml().Split("---")[1];
        var parsed = new DeserializerBuilder().Build().Deserialize<Dictionary<string, object?>>(yaml);
        var values = new Dictionary<string, string>();
        foreach (var (key, value) in parsed)
        {
            if (value is string text)
                values[key] = text;
            else if (value is IList items)
                values[key] = "[" + string.Join(", ", items.Cast<object>()) + "]";
        }

        return FrontMatter.FromDictionary(values);
    }

    private static FrontMatter FullFrontMatter() => new()
    {
        Id = "post-1",
        Order = 3,
        Title = "Post",
        Permalink = "/blog/post/",
        Description = "Desc",
        Author = "Ana",
        Date = "2024-02-03",
        LastModified = "2024-02-04",
        FeaturedImage = "/f.png",
        Categories = ["one", "two"],
        Tags = ["t1", "t2"],
        IsFeatured = true,
        Published = false,
        Layout = "post",
        Slug = "post",
        Lang = "sr",
    };

    [Fact]
    public void FromDictionary_NullValues_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => FrontMatter.FromDictionary(null!));

        Assert.Equal("values", exception.ParamName);
    }

    [Fact]
    public void FromDictionary_EmptyDictionary_ReturnsDefaults()
    {
        var frontMatter = Parse();

        Assert.Equal(string.Empty, frontMatter.Title);
        Assert.Equal(string.Empty, frontMatter.Description);
        Assert.Equal(string.Empty, frontMatter.Author);
        Assert.Null(frontMatter.LastModified);
        Assert.Null(frontMatter.FeaturedImage);
        Assert.False(frontMatter.IsFeatured);
        Assert.True(frontMatter.Published);
        Assert.Null(frontMatter.Layout);
        Assert.Null(frontMatter.Slug);
        Assert.Empty(frontMatter.Categories);
        Assert.Empty(frontMatter.Tags);
        Assert.Empty(frontMatter.CustomFields);
        Assert.Equal(string.Empty, frontMatter.Id);
        Assert.Null(frontMatter.Order);
        Assert.Null(frontMatter.Permalink);
        Assert.Equal(string.Empty, frontMatter.Lang);
        Assert.Equal(string.Empty, frontMatter.Date);
    }

    [Fact]
    public void FromDictionary_TextKeys_SetsMatchingProperties()
    {
        var frontMatter = Parse(
            ("title", "Hello"),
            ("description", "A greeting"),
            ("author", "Ana"),
            ("layout", "post"),
            ("slug", "hello"));

        Assert.Equal("Hello", frontMatter.Title);
        Assert.Equal("A greeting", frontMatter.Description);
        Assert.Equal("Ana", frontMatter.Author);
        Assert.Equal("post", frontMatter.Layout);
        Assert.Equal("hello", frontMatter.Slug);
        Assert.Empty(frontMatter.CustomFields);
    }

    [Fact]
    public void FromDictionary_ValidDate_StoresIsoDate()
    {
        var frontMatter = Parse(("date", "2024-05-17"));

        Assert.Equal("2024-05-17", frontMatter.Date);
    }

    // Failed before the fix: the time was dropped, and the offset moved the moment to the server's time zone first.
    [Theory]
    [InlineData("2024-05-17T13:45:00", "2024-05-17T13:45:00Z")]
    [InlineData("2024-05-17T13:45:00Z", "2024-05-17T13:45:00Z")]
    [InlineData("2024-05-18T00:30:00+02:00", "2024-05-17T22:30:00Z")]
    public void FromDictionary_DateWithATime_StoresTheMomentInUtc(string value, string expected)
    {
        var frontMatter = Parse(("date", value), ("last_modified", value));

        Assert.Equal(expected, frontMatter.Date);
        Assert.Equal(expected, frontMatter.LastModified);
    }

    // Failed before the fix: the date was read in the server's culture, so en-GB read 02/03/2024 as the 2nd of March,
    // where a file read by a content repository is the 3rd of February.
    [Fact]
    public void FromDictionary_UnderAnotherCulture_ReadsTheDateAsTheRepositoriesDo()
    {
        var culture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-GB");
        try
        {
            var frontMatter = Parse(("date", "02/03/2024"));

            Assert.Equal("2024-02-03", frontMatter.Date);
        }
        finally
        {
            CultureInfo.CurrentCulture = culture;
        }
    }

    [Fact]
    public void FromDictionary_InvalidDate_KeepsTheDefaultDate()
    {
        var frontMatter = Parse(("date", "not a date"));

        Assert.Equal(string.Empty, frontMatter.Date);
    }

    [Theory]
    [InlineData("lastModified")]
    [InlineData("last_modified")]
    public void FromDictionary_LastModifiedAlias_StoresIsoDate(string key)
    {
        var frontMatter = Parse((key, "2023-01-02"));

        Assert.Equal("2023-01-02", frontMatter.LastModified);
    }

    [Fact]
    public void FromDictionary_BothLastModifiedAliases_PrefersCamelCase()
    {
        var frontMatter = Parse(("lastModified", "2023-01-02"), ("last_modified", "2022-12-31"));

        Assert.Equal("2023-01-02", frontMatter.LastModified);
    }

    [Fact]
    public void FromDictionary_InvalidCamelCaseLastModified_DoesNotFallBackToSnakeCase()
    {
        var frontMatter = Parse(("lastModified", "soon"), ("last_modified", "2022-12-31"));

        Assert.Null(frontMatter.LastModified);
    }

    [Fact]
    public void FromDictionary_InvalidLastModified_LeavesLastModifiedEmpty()
    {
        var frontMatter = Parse(("last_modified", "never"));

        Assert.Null(frontMatter.LastModified);
    }

    [Theory]
    [InlineData("featuredImage")]
    [InlineData("featured_image")]
    public void FromDictionary_FeaturedImageAlias_SetsFeaturedImage(string key)
    {
        var frontMatter = Parse((key, "/img/cover.png"));

        Assert.Equal("/img/cover.png", frontMatter.FeaturedImage);
    }

    [Fact]
    public void FromDictionary_BothFeaturedImageAliases_PrefersCamelCase()
    {
        var frontMatter = Parse(("featuredImage", "/a.png"), ("featured_image", "/b.png"));

        Assert.Equal("/a.png", frontMatter.FeaturedImage);
    }

    [Theory]
    [InlineData("featured", "true", true)]
    [InlineData("featured", "True", true)]
    [InlineData("featured", "false", false)]
    [InlineData("featured", "yes", false)]
    [InlineData("isFeatured", "true", true)]
    [InlineData("isFeatured", "1", false)]
    public void FromDictionary_FeaturedAlias_ParsesBoolean(string key, string value, bool expected)
    {
        var frontMatter = Parse((key, value));

        Assert.Equal(expected, frontMatter.IsFeatured);
    }

    [Fact]
    public void FromDictionary_BothFeaturedAliases_PrefersFeatured()
    {
        var frontMatter = Parse(("featured", "false"), ("isFeatured", "true"));

        Assert.False(frontMatter.IsFeatured);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("FALSE", false)]
    [InlineData("false", false)]
    [InlineData("draft", true)]
    [InlineData("", true)]
    public void FromDictionary_Published_TreatsOnlyAParsedFalseAsUnpublished(string value, bool expected)
    {
        var frontMatter = Parse(("published", value));

        Assert.Equal(expected, frontMatter.Published);
    }

    [Theory]
    [InlineData("categories")]
    [InlineData("category")]
    public void FromDictionary_CategoryAlias_ParsesList(string key)
    {
        var frontMatter = Parse((key, "news, tech"));

        Assert.Equal(["news", "tech"], frontMatter.Categories);
    }

    [Fact]
    public void FromDictionary_BothCategoryAliases_PrefersPlural()
    {
        var frontMatter = Parse(("categories", "a"), ("category", "b"));

        Assert.Equal(["a"], frontMatter.Categories);
    }

    [Theory]
    [InlineData("tags")]
    [InlineData("tag")]
    public void FromDictionary_TagAlias_ParsesList(string key)
    {
        var frontMatter = Parse((key, "[blazor; dotnet]"));

        Assert.Equal(["blazor", "dotnet"], frontMatter.Tags);
    }

    [Fact]
    public void FromDictionary_BothTagAliases_PrefersPlural()
    {
        var frontMatter = Parse(("tags", "x"), ("tag", "y"));

        Assert.Equal(["x"], frontMatter.Tags);
    }

    [Theory]
    [InlineData("", new string[0])]
    [InlineData("   ", new string[0])]
    [InlineData("[]", new string[0])]
    [InlineData("single", new[] { "single" })]
    [InlineData("[a, b]", new[] { "a", "b" })]
    [InlineData("[a, b", new[] { "[a", "b" })]
    [InlineData("a, b]", new[] { "a", "b]" })]
    [InlineData("\"quoted\", 'single'", new[] { "quoted", "single" })]
    [InlineData("\"half, half'", new[] { "\"half", "half'" })]
    [InlineData("'mixed\"", new[] { "'mixed\"" })]
    [InlineData("a,,b;;c", new[] { "a", "b", "c" })]
    [InlineData("a,  ,b", new[] { "a", "b" })]
    [InlineData("'', \"\", x", new[] { "x" })]
    [InlineData("' padded '", new[] { " padded " })]
    public void FromDictionary_ListValue_ParsesItems(string value, string[] expected)
    {
        var frontMatter = Parse(("tags", value));

        Assert.Equal(expected, frontMatter.Tags);
    }

    // Threw ArgumentOutOfRangeException before the fix, so one stray quote made the whole file unreadable.
    [Theory]
    [InlineData("a, \"", new[] { "a", "\"" })]
    [InlineData("'", new[] { "'" })]
    public void FromDictionary_TagsWithALoneQuote_KeepTheQuoteAsAnItem(string value, string[] expected)
    {
        var frontMatter = Parse(("tags", value));

        Assert.Equal(expected, frontMatter.Tags);
    }

    [Fact]
    public void FromDictionary_UnknownKeys_BecomeTypedCustomFields()
    {
        var frontMatter = Parse(
            ("draft", "true"),
            ("weight", "42"),
            ("ratio", "1e3"),
            ("subtitle", "hello"));

        Assert.Equal(4, frontMatter.CustomFields.Count);
        Assert.Equal(true, frontMatter.CustomFields["draft"]);
        Assert.Equal(42, frontMatter.CustomFields["weight"]);
        Assert.Equal(1000d, frontMatter.CustomFields["ratio"]);
        Assert.Equal("hello", frontMatter.CustomFields["subtitle"]);
    }

    [Fact]
    public void FromDictionary_PropertiesWithoutAKey_AreStoredAsCustomFields()
    {
        var frontMatter = Parse(("id", "abc"), ("order", "3"), ("permalink", "/x/"), ("lang", "sr"));

        Assert.Equal(string.Empty, frontMatter.Id);
        Assert.Null(frontMatter.Order);
        Assert.Null(frontMatter.Permalink);
        Assert.Equal(string.Empty, frontMatter.Lang);
        Assert.Equal("abc", frontMatter.CustomFields["id"]);
        Assert.Equal(3, frontMatter.CustomFields["order"]);
        Assert.Equal("/x/", frontMatter.CustomFields["permalink"]);
        Assert.Equal("sr", frontMatter.CustomFields["lang"]);
    }

    [Theory]
    [InlineData("Title")]
    [InlineData("DESCRIPTION")]
    [InlineData("lastmodified")]
    [InlineData("featuredimage")]
    [InlineData("IsFeatured")]
    [InlineData("Tags")]
    public void FromDictionary_StandardKeyInOtherCase_IsNeitherReadNorStoredAsCustomField(string key)
    {
        var frontMatter = Parse((key, "2024-01-01"));

        Assert.Empty(frontMatter.CustomFields);
        Assert.Equal(string.Empty, frontMatter.Title);
        Assert.Equal(string.Empty, frontMatter.Description);
        Assert.Null(frontMatter.LastModified);
        Assert.Null(frontMatter.FeaturedImage);
        Assert.False(frontMatter.IsFeatured);
        Assert.Empty(frontMatter.Tags);
    }

    [Fact]
    public void FromDictionary_CaseInsensitiveDictionary_ReadsKeysInAnyCase()
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["TITLE"] = "Upper",
            ["Featured_Image"] = "/c.png",
            ["PUBLISHED"] = "false",
        };

        var frontMatter = FrontMatter.FromDictionary(values);

        Assert.Equal("Upper", frontMatter.Title);
        Assert.Equal("/c.png", frontMatter.FeaturedImage);
        Assert.False(frontMatter.Published);
        Assert.Empty(frontMatter.CustomFields);
    }

    [Fact]
    public void FromDictionary_AllKeys_ProducesTheSameValueAsTheFluentApi()
    {
        var frontMatter = Parse(
            ("title", "Post"),
            ("description", "Desc"),
            ("author", "Ana"),
            ("date", "2024-02-03"),
            ("last_modified", "2024-02-04"),
            ("featured_image", "/f.png"),
            ("isFeatured", "true"),
            ("published", "false"),
            ("layout", "post"),
            ("slug", "post"),
            ("category", "one;two"),
            ("tag", "'t1', \"t2\""),
            ("rating", "5"));

        var expected = FrontMatter.Create("Post")
            .WithDescription("Desc")
            .WithAuthor("Ana")
            .WithDate(new DateTime(2024, 2, 3))
            .WithLastModified(new DateTime(2024, 2, 4))
            .WithFeaturedImage("/f.png")
            .WithFeatured(true)
            .WithPublished(false)
            .WithLayout("post")
            .WithSlug("post")
            .WithCategories(["one", "two"])
            .WithTags(["t1", "t2"])
            .WithCustomField("rating", 5);

        Assert.Equal(expected, frontMatter);
        Assert.Equal(expected.ToYaml(), frontMatter.ToYaml());
    }

    [Fact]
    public void FromDictionary_ParsedToYaml_KeepsTheFieldsBothSidesSupport()
    {
        var original = FullFrontMatter();

        var roundTripped = RoundTrip(original);

        Assert.Equal(original.Title, roundTripped.Title);
        Assert.Equal(original.Description, roundTripped.Description);
        Assert.Equal(original.Author, roundTripped.Author);
        Assert.Equal(original.Date, roundTripped.Date);
        Assert.Equal(original.LastModified, roundTripped.LastModified);
        Assert.Equal(original.FeaturedImage, roundTripped.FeaturedImage);
        Assert.Equal(original.Published, roundTripped.Published);
        Assert.Equal(original.Layout, roundTripped.Layout);
        Assert.Equal(original.Slug, roundTripped.Slug);
        Assert.Equal(original.Categories, roundTripped.Categories);
        Assert.Equal(original.Tags, roundTripped.Tags);
    }

    // Known defect, pinned as current behavior: FromDictionary does not read id, order, permalink, lang or is_featured as ToYaml writes them, so they are lost or end up as custom fields. Update this expectation when it is fixed.
    [Fact]
    public void FromDictionary_ParsedToYaml_LosesIdOrderPermalinkLangAndFeatured()
    {
        var original = FullFrontMatter();

        var roundTripped = RoundTrip(original);

        Assert.Equal(string.Empty, roundTripped.Id);
        Assert.Null(roundTripped.Order);
        Assert.Null(roundTripped.Permalink);
        Assert.Equal(string.Empty, roundTripped.Lang);
        Assert.False(roundTripped.IsFeatured);
        Assert.Equal("post-1", roundTripped.CustomFields["id"]);
        Assert.Equal(3, roundTripped.CustomFields["order"]);
        Assert.Equal("/blog/post/", roundTripped.CustomFields["permalink"]);
        Assert.Equal("sr", roundTripped.CustomFields["lang"]);
        Assert.Equal(true, roundTripped.CustomFields["is_featured"]);
    }

    // Known defect, pinned as current behavior: with a case-sensitive dictionary a standard key in another case is neither read nor kept as a custom field, so its value is dropped. Update this expectation when it is fixed.
    [Fact]
    public void FromDictionary_CaseSensitiveDictionaryWithCapitalizedKeys_DropsTheirValues()
    {
        var frontMatter = Parse(("Title", "Upper"), ("Featured_Image", "/c.png"), ("PUBLISHED", "false"));

        Assert.Equal(string.Empty, frontMatter.Title);
        Assert.Null(frontMatter.FeaturedImage);
        Assert.True(frontMatter.Published);
        Assert.Empty(frontMatter.CustomFields);
    }
}
