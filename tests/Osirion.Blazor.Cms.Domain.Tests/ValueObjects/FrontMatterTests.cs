using Osirion.Blazor.Cms.Domain.ValueObjects;

namespace Osirion.Blazor.Cms.Domain.Tests.ValueObjects;

public class FrontMatterTests
{
    [Fact]
    public void Create_WithTitle_SetsTitleAndDefaults()
    {
        var frontMatter = FrontMatter.Create("Hello");

        Assert.Equal("Hello", frontMatter.Title);
        Assert.True(frontMatter.Published);
        Assert.Equal(string.Empty, frontMatter.Lang);
        Assert.NotNull(frontMatter.SeoProperties);
    }

    [Fact]
    public void NewInstance_HasNoDate()
    {
        // Fails before the fix: the date defaulted to the day the instance was made, so a parsed page with no date key
        // looked as if it were published on the day it was loaded.
        Assert.Equal(string.Empty, new FrontMatter().Date);
    }

    [Fact]
    public void Create_WithTitle_DatesNewContentToday()
    {
        var frontMatter = FrontMatter.Create("Hello");

        Assert.Equal(DateTime.Now.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture), frontMatter.Date);
    }

    // Failed before the fix: the time of day was dropped, so posts of one day had no order.
    [Fact]
    public void Create_WithTitleDescriptionAndDate_WritesTheIsoMoment()
    {
        var created = new DateTime(2024, 3, 4, 5, 6, 7, DateTimeKind.Utc);

        var frontMatter = FrontMatter.Create("Title", "Description", created);

        Assert.Equal("Title", frontMatter.Title);
        Assert.Equal("Description", frontMatter.Description);
        Assert.Equal("2024-03-04T05:06:07Z", frontMatter.Date);
    }

    [Fact]
    public void Create_WithTitleDescriptionAndDay_WritesTheIsoDate()
    {
        var frontMatter = FrontMatter.Create("Title", "Description", new DateTime(2024, 3, 4));

        Assert.Equal("2024-03-04", frontMatter.Date);
    }

    [Fact]
    public void Create_WithAllValues_SetsEveryProperty()
    {
        var frontMatter = FrontMatter.Create(
            id: "id-1",
            title: "Title",
            description: "Description",
            author: "Author",
            date: new DateTime(2024, 1, 2),
            lastModified: new DateTime(2024, 1, 3),
            featuredImage: "/img.png",
            categories: ["c1", "c2"],
            tags: ["t1"],
            isFeatured: true,
            published: false,
            layout: "post",
            slug: "title",
            customFields: new Dictionary<string, object> { ["k"] = 1 });

        Assert.Equal("Title", frontMatter.Title);
        Assert.Equal("Description", frontMatter.Description);
        Assert.Equal("Author", frontMatter.Author);
        Assert.Equal("2024-01-02", frontMatter.Date);
        Assert.Equal("2024-01-03", frontMatter.LastModified);
        Assert.Equal("/img.png", frontMatter.FeaturedImage);
        Assert.Equal(["c1", "c2"], frontMatter.Categories);
        Assert.Equal(["t1"], frontMatter.Tags);
        Assert.True(frontMatter.IsFeatured);
        Assert.False(frontMatter.Published);
        Assert.Equal("post", frontMatter.Layout);
        Assert.Equal("title", frontMatter.Slug);
        Assert.Equal(1, frontMatter.CustomFields["k"]);
    }

    [Fact]
    public void Create_WithOnlyIdAndTitle_LeavesOptionalValuesEmpty()
    {
        var frontMatter = FrontMatter.Create("id-1", "Title");

        Assert.Equal(string.Empty, frontMatter.Description);
        Assert.Equal(string.Empty, frontMatter.Author);
        Assert.Null(frontMatter.LastModified);
        Assert.Null(frontMatter.FeaturedImage);
        Assert.Empty(frontMatter.Categories);
        Assert.Empty(frontMatter.Tags);
        Assert.Empty(frontMatter.CustomFields);
        Assert.False(frontMatter.IsFeatured);
        Assert.True(frontMatter.Published);
        Assert.Equal(10, frontMatter.Date.Length);
    }

    [Fact]
    public void WithMethods_ReturnUpdatedCopiesAndLeaveTheOriginalUnchanged()
    {
        var original = FrontMatter.Create("Original");

        var updated = original
            .WithTitle("New")
            .WithDescription("Desc")
            .WithAuthor("Ana")
            .WithDate(new DateTime(2020, 5, 6))
            .WithLastModified(new DateTime(2020, 5, 7))
            .WithFeaturedImage("/f.png")
            .WithCategories(["c"])
            .WithTags(["t"])
            .WithFeatured(true)
            .WithPublished(false)
            .WithLayout("page")
            .WithSlug("new");

        Assert.Equal("Original", original.Title);
        Assert.Empty(original.Tags);
        Assert.Equal("New", updated.Title);
        Assert.Equal("Desc", updated.Description);
        Assert.Equal("Ana", updated.Author);
        Assert.Equal("2020-05-06", updated.Date);
        Assert.Equal("2020-05-07", updated.LastModified);
        Assert.Equal("/f.png", updated.FeaturedImage);
        Assert.Equal(["c"], updated.Categories);
        Assert.Equal(["t"], updated.Tags);
        Assert.True(updated.IsFeatured);
        Assert.False(updated.Published);
        Assert.Equal("page", updated.Layout);
        Assert.Equal("new", updated.Slug);
    }

    // Failed before the fix: the admin's date and time field kept only the day.
    [Fact]
    public void WithDateAndWithLastModified_MomentsWithATime_KeepTheTime()
    {
        var frontMatter = FrontMatter.Create("T")
            .WithDate(new DateTime(2026, 10, 29, 17, 30, 0, DateTimeKind.Utc))
            .WithLastModified(new DateTime(2026, 10, 30, 8, 15, 0, DateTimeKind.Unspecified));

        Assert.Equal("2026-10-29T17:30:00Z", frontMatter.Date);
        Assert.Equal("2026-10-30T08:15:00", frontMatter.LastModified);
    }

    [Fact]
    public void Create_WithAllValuesAndMoments_KeepsTheirTimes()
    {
        var frontMatter = FrontMatter.Create(
            id: "id-1",
            title: "Title",
            date: new DateTime(2024, 1, 2, 9, 0, 0, DateTimeKind.Utc),
            lastModified: new DateTime(2024, 1, 3, 10, 0, 0, DateTimeKind.Utc));

        Assert.Equal("2024-01-02T09:00:00Z", frontMatter.Date);
        Assert.Equal("2024-01-03T10:00:00Z", frontMatter.LastModified);
    }

    [Fact]
    public void WithLastModified_Null_ClearsTheDate()
    {
        var frontMatter = FrontMatter.Create("T").WithLastModified(new DateTime(2020, 1, 1));

        var cleared = frontMatter.WithLastModified(null);

        Assert.Null(cleared.LastModified);
    }

    [Fact]
    public void WithCategoriesAndTags_Null_ReturnEmptyLists()
    {
        var frontMatter = FrontMatter.Create("T").WithCategories(["a"]).WithTags(["b"]);

        var cleared = frontMatter.WithCategories(null!).WithTags(null!);

        Assert.Empty(cleared.Categories);
        Assert.Empty(cleared.Tags);
    }

    [Fact]
    public void WithCustomFields_Null_ThrowsArgumentNullException()
    {
        var frontMatter = FrontMatter.Create("T");

        var exception = Assert.Throws<ArgumentNullException>(() => frontMatter.WithCustomFields(null!));

        Assert.Equal("customFields", exception.ParamName);
    }

    [Fact]
    public void WithCustomFields_ReplacesAllFieldsWithACopy()
    {
        var source = new Dictionary<string, object> { ["a"] = 1 };
        var frontMatter = FrontMatter.Create("T").WithCustomField("old", "x");

        var updated = frontMatter.WithCustomFields(source);
        source["b"] = 2;

        Assert.Single(updated.CustomFields);
        Assert.Equal(1, updated.CustomFields["a"]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void WithCustomField_BlankKey_ThrowsArgumentException(string? key)
    {
        var frontMatter = FrontMatter.Create("T");

        var exception = Assert.Throws<ArgumentException>(() => frontMatter.WithCustomField(key!, "v"));

        Assert.Equal("key", exception.ParamName);
    }

    [Fact]
    public void WithCustomField_AddsOrReplacesWithoutChangingTheOriginal()
    {
        var original = FrontMatter.Create("T").WithCustomField("a", 1);

        var updated = original.WithCustomField("a", 2).WithCustomField("b", 3);

        Assert.Equal(1, original.CustomFields["a"]);
        Assert.Equal(2, updated.CustomFields["a"]);
        Assert.Equal(3, updated.CustomFields["b"]);
    }

    [Fact]
    public void Clone_CopiesEveryValueIntoIndependentCollections()
    {
        var original = FrontMatter.Create("T").WithTags(["t"]).WithCategories(["c"]).WithCustomField("k", "v");
        original.Id = "id";
        original.Order = 4;
        original.Permalink = "/p/";
        original.Lang = "sr";
        original.SeoProperties = SeoMetadata.Create("Seo", "Desc");

        var clone = original.Clone();
        original.Tags.Add("other");

        Assert.Equal("id", clone.Id);
        Assert.Equal(4, clone.Order);
        Assert.Equal("/p/", clone.Permalink);
        Assert.Equal("sr", clone.Lang);
        Assert.Equal(["t"], clone.Tags);
        Assert.Equal(["c"], clone.Categories);
        Assert.Equal("v", clone.CustomFields["k"]);
        Assert.NotSame(original.SeoProperties, clone.SeoProperties);
        Assert.Equal(original.SeoProperties, clone.SeoProperties);
    }

    [Fact]
    public void Clone_WithoutSeoProperties_CreatesEmptySeoMetadata()
    {
        var original = FrontMatter.Create("T");
        original.SeoProperties = null!;

        var clone = original.Clone();

        Assert.NotNull(clone.SeoProperties);
    }

    [Fact]
    public void Equals_SameValues_AreEqual()
    {
        var first = FrontMatter.Create("T").WithDate(new DateTime(2020, 1, 1)).WithTags(["a"]).WithCategories(["c"]).WithCustomField("k", 1);
        var second = FrontMatter.Create("T").WithDate(new DateTime(2020, 1, 1)).WithTags(["a"]).WithCategories(["c"]).WithCustomField("k", 1);

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void Equals_DifferentTags_AreNotEqual()
    {
        var first = FrontMatter.Create("T").WithDate(new DateTime(2020, 1, 1)).WithTags(["a"]);
        var second = FrontMatter.Create("T").WithDate(new DateTime(2020, 1, 1)).WithTags(["b"]);

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void ToYaml_WrapsSerializedValuesInFrontMatterFences()
    {
        var frontMatter = FrontMatter.Create("My Post").WithAuthor("Ana").WithTags(["x"]);

        var yaml = frontMatter.ToYaml();

        Assert.StartsWith("---" + Environment.NewLine, yaml);
        Assert.EndsWith("---" + Environment.NewLine + Environment.NewLine, yaml);
        Assert.Contains("title: My Post", yaml);
        Assert.Contains("author: Ana", yaml);
        Assert.Contains("is_featured: false", yaml);
        Assert.Contains("- x", yaml);
    }
}
