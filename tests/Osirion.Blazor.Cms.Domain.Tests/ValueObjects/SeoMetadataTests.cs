using Osirion.Blazor.Cms.Domain.ValueObjects;

namespace Osirion.Blazor.Cms.Domain.Tests.ValueObjects;

public class SeoMetadataTests
{
    [Fact]
    public void Create_WithRequiredValues_ReturnsValidInstance()
    {
        // Arrange
        string metaTitle = "Test Title";
        string metaDescription = "Test Description";

        // Act
        var seo = SeoMetadata.Create(metaTitle, metaDescription);

        // Assert
        Assert.NotNull(seo);
        Assert.Equal(metaTitle, seo.OgTitle);
        Assert.Equal(metaDescription, seo.OgDescription);
        Assert.Equal(metaTitle, seo.TwitterTitle);
        Assert.Equal(metaDescription, seo.TwitterDescription);
        Assert.Equal(string.Empty, seo.Robots);
        Assert.Equal(string.Empty, seo.OgType);
        Assert.Equal("summary_large_image", seo.TwitterCard);
    }

    // Fails before the fix: a new instance said "index, follow" and "article", so the renderer never reached
    // its own robots value or its computed og:type for an item with front matter.
    [Fact]
    public void NewInstance_LeavesRobotsAndOgTypeEmptyForTheRendererToDecide()
    {
        var seo = new SeoMetadata();

        Assert.Equal(string.Empty, seo.Robots);
        Assert.Equal(string.Empty, seo.OgType);
    }

    // Fails before the fix: the schema type said "Article" whether or not the front matter named one, so the renderer
    // published every item with front matter as an Article with og:type "article".
    [Fact]
    public void NewInstanceAndCreate_LeaveTheSchemaTypeEmptyForTheRendererToDecide()
    {
        Assert.Equal(string.Empty, new SeoMetadata().Type);
        Assert.Equal(string.Empty, SeoMetadata.Create("t", "d").Type);
    }

    [Fact]
    public void Create_WithAllValues_ReturnsInstanceWithAllPropertiesSet()
    {
        // Arrange
        string metaTitle = "Test Title";
        string metaDescription = "Test Description";
        string canonicalUrl = "https://example.com/test";
        string robots = "noindex, follow";
        string ogTitle = "OG Title";
        string ogDescription = "OG Description";
        string ogImageUrl = "https://example.com/image.jpg";
        string ogType = "website";
        string jsonLd = "{\"@type\":\"WebPage\"}";
        string schemaType = "WebPage";
        string twitterCard = "summary";
        string twitterTitle = "Twitter Title";
        string twitterDescription = "Twitter Description";
        string twitterImageUrl = "https://example.com/twitter-image.jpg";

        // Act
        var seo = SeoMetadata.Create(
            metaTitle,
            metaDescription,
            canonicalUrl,
            robots,
            ogTitle,
            ogDescription,
            ogImageUrl,
            ogType,
            jsonLd,
            schemaType,
            twitterCard,
            twitterTitle,
            twitterDescription,
            twitterImageUrl);

        // Assert
        Assert.NotNull(seo);
        Assert.Equal(robots, seo.Robots);
        Assert.Equal(ogTitle, seo.OgTitle);
        Assert.Equal(ogDescription, seo.OgDescription);
        Assert.Equal(ogImageUrl, seo.OgImageUrl);
        Assert.Equal(ogType, seo.OgType);
        Assert.Equal(jsonLd, seo.JsonLd);
        Assert.Equal(twitterCard, seo.TwitterCard);
        Assert.Equal(twitterTitle, seo.TwitterTitle);
        Assert.Equal(twitterDescription, seo.TwitterDescription);
        Assert.Equal(twitterImageUrl, seo.TwitterImageUrl);
    }

    [Fact]
    public void WithMetaTitle_SetsNewTitle_ReturnsNewInstance()
    {
        // Arrange
        var original = SeoMetadata.Create("Original Title", "Description");
        string newTitle = "New Title";

        // Act
        var modified = original.WithMetaTitle(newTitle);

        // Assert
        Assert.NotSame(original, modified);
    }

    [Fact]
    public void WithMetaDescription_SetsNewDescription_ReturnsNewInstance()
    {
        // Arrange
        var original = SeoMetadata.Create("Title", "Original Description");
        string newDescription = "New Description";

        // Act
        var modified = original.WithMetaDescription(newDescription);

        // Assert
        Assert.NotSame(original, modified);
    }

    [Fact]
    public void WithCanonicalUrl_SetsUrl_ReturnsNewInstance()
    {
        // Arrange
        var original = SeoMetadata.Create("Title", "Description");
        string url = "https://example.com/canonical";

        // Act
        var modified = original.WithCanonicalUrl(url);

        // Assert
        Assert.NotSame(original, modified);
    }

    [Fact]
    public void WithRobots_SetsValue_ReturnsNewInstance()
    {
        // Arrange
        var original = SeoMetadata.Create("Title", "Description");
        string robots = "noindex, nofollow";

        // Act
        var modified = original.WithRobots(robots);

        // Assert
        Assert.NotSame(original, modified);
        Assert.Equal(robots, modified.Robots);
    }

    [Fact]
    public void WithOpenGraph_SetsValues_ReturnsNewInstance()
    {
        // Arrange
        var original = SeoMetadata.Create("Title", "Description");
        string title = "OG Title";
        string description = "OG Description";
        string imageUrl = "https://example.com/og-image.jpg";
        string type = "website";

        // Act
        var modified = original.WithOpenGraph(title, description, imageUrl, type);

        // Assert
        Assert.NotSame(original, modified);
        Assert.Equal(title, modified.OgTitle);
        Assert.Equal(description, modified.OgDescription);
        Assert.Equal(imageUrl, modified.OgImageUrl);
        Assert.Equal(type, modified.OgType);
    }

    [Fact]
    public void WithTwitterCard_SetsValues_ReturnsNewInstance()
    {
        // Arrange
        var original = SeoMetadata.Create("Title", "Description");
        string title = "Twitter Title";
        string description = "Twitter Description";
        string imageUrl = "https://example.com/twitter-image.jpg";
        string cardType = "summary";

        // Act
        var modified = original.WithTwitterCard(title, description, imageUrl, cardType);

        // Assert
        Assert.NotSame(original, modified);
        Assert.Equal(title, modified.TwitterTitle);
        Assert.Equal(description, modified.TwitterDescription);
        Assert.Equal(imageUrl, modified.TwitterImageUrl);
        Assert.Equal(cardType, modified.TwitterCard);
    }

    [Fact]
    public void WithJsonLd_SetsValues_ReturnsNewInstance()
    {
        // Arrange
        var original = SeoMetadata.Create("Title", "Description");
        string jsonLd = "{\"@type\":\"WebPage\"}";
        string schemaType = "WebPage";

        // Act
        var modified = original.WithJsonLd(jsonLd, schemaType);

        // Assert
        Assert.NotSame(original, modified);
        Assert.Equal(jsonLd, modified.JsonLd);
    }

    [Fact]
    public void Clone_CreatesDeepCopy_WithSameValues()
    {
        // Arrange
        var original = SeoMetadata.Create(
            "Title",
            "Description",
            "https://example.com/canonical",
            "index, follow",
            "OG Title",
            "OG Description",
            "https://example.com/og-image.jpg",
            "website",
            "{\"@type\":\"WebPage\"}",
            "WebPage",
            "summary",
            "Twitter Title",
            "Twitter Description",
            "https://example.com/twitter-image.jpg");

        // Act
        var clone = original.Clone();

        // Assert
        Assert.NotSame(original, clone);
        Assert.Equal(original.Robots, clone.Robots);
        Assert.Equal(original.OgTitle, clone.OgTitle);
        Assert.Equal(original.OgDescription, clone.OgDescription);
        Assert.Equal(original.OgImageUrl, clone.OgImageUrl);
        Assert.Equal(original.OgType, clone.OgType);
        Assert.Equal(original.JsonLd, clone.JsonLd);
        Assert.Equal(original.TwitterCard, clone.TwitterCard);
        Assert.Equal(original.TwitterTitle, clone.TwitterTitle);
        Assert.Equal(original.TwitterDescription, clone.TwitterDescription);
        Assert.Equal(original.TwitterImageUrl, clone.TwitterImageUrl);
    }

    [Fact]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        // Arrange
        var seo1 = SeoMetadata.Create("Title", "Description");
        var seo2 = SeoMetadata.Create("Title", "Description");

        // Act & Assert
        Assert.Equal(seo1, seo2);
        Assert.True(seo1 == seo2);
        Assert.False(seo1 != seo2);
    }

    [Fact]
    public void Equals_WithDifferentValues_ReturnsFalse()
    {
        // Arrange
        var seo1 = SeoMetadata.Create("Title 1", "Description 1");
        var seo2 = SeoMetadata.Create("Title 2", "Description 2");

        // Act & Assert
        Assert.NotEqual(seo1, seo2);
        Assert.False(seo1 == seo2);
        Assert.True(seo1 != seo2);
    }

    [Fact]
    public void GetHashCode_WithSameValues_ReturnsSameHash()
    {
        // Arrange
        var seo1 = SeoMetadata.Create("Title", "Description");
        var seo2 = SeoMetadata.Create("Title", "Description");

        // Act & Assert
        Assert.Equal(seo1.GetHashCode(), seo2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithDifferentValues_ReturnsDifferentHash()
    {
        // Arrange
        var seo1 = SeoMetadata.Create("Title 1", "Description 1");
        var seo2 = SeoMetadata.Create("Title 2", "Description 2");

        // Act & Assert
        Assert.NotEqual(seo1.GetHashCode(), seo2.GetHashCode());
    }
}