using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Exceptions;
using Osirion.Blazor.Cms.Domain.ValueObjects;
using Shouldly;

namespace Osirion.Blazor.Cms.Domain.Tests.Entities;

public class ContentItemDerivedLocaleTests
{
    private static readonly DateTime Modified = new(2026, 2, 3, 4, 5, 6, DateTimeKind.Utc);

    private static ContentItem Source()
    {
        var item = ContentItem.Create("id-sr", "Наслов", "<p>Текст</p>", "sr/post.md", "provider");
        item.SetContent("<p>Текст</p>", "Текст");
        item.SetProviderSpecificId("sr/post.md");
        item.SetLocale("sr");
        item.SetContentId("post");
        item.Metadata = new FrontMatter { Title = "Наслов", Lang = "sr" };
        item.SetLastModifiedDate(Modified);
        return item;
    }

    [Fact]
    public void FileItem_IsNotDerived()
    {
        var item = Source();

        item.IsDerived.ShouldBeFalse();
        item.DerivedFromLocale.ShouldBeNull();
    }

    [Fact]
    public void DeriveForLocale_TakesTheNewIdentityAndKeepsTheLocalizationId()
    {
        var derived = Source().DeriveForLocale("id-latn", "sr-Latn", "sr-Latn/post.md");

        derived.Id.ShouldBe("id-latn");
        derived.Locale.ShouldBe("sr-Latn");
        derived.Path.ShouldBe("sr-Latn/post.md");
        derived.ContentId.ShouldBe("post");
        derived.Metadata!.Lang.ShouldBe("sr-Latn");
        derived.LastModified.ShouldBe(Modified);
    }

    [Fact]
    public void DeriveForLocale_MarksTheItemAndLeavesItWithoutAFile()
    {
        var derived = Source().DeriveForLocale("id-latn", "sr-Latn", "sr-Latn/post.md");

        derived.IsDerived.ShouldBeTrue();
        derived.DerivedFromLocale.ShouldBe("sr");
        derived.ProviderSpecificId.ShouldBeNull();
        derived.OriginalMarkdown.ShouldBeNull();
    }

    [Fact]
    public void DeriveForLocale_LeavesTheSourceUnchanged()
    {
        var source = Source();

        source.DeriveForLocale("id-latn", "sr-Latn", "sr-Latn/post.md").Metadata!.Title = "Naslov";

        source.Locale.ShouldBe("sr");
        source.Metadata!.Title.ShouldBe("Наслов");
        source.Metadata.Lang.ShouldBe("sr");
        source.ProviderSpecificId.ShouldBe("sr/post.md");
    }

    [Theory]
    [InlineData("", "sr-Latn", "sr-Latn/post.md")]
    [InlineData("id", " ", "sr-Latn/post.md")]
    [InlineData("id", "sr-Latn", "")]
    public void DeriveForLocale_MissingValue_Throws(string id, string locale, string path)
    {
        Should.Throw<ContentValidationException>(() => Source().DeriveForLocale(id, locale, path));
    }

    [Fact]
    public void Clone_KeepsTheDerivedMark()
    {
        var derived = Source().DeriveForLocale("id-latn", "sr-Latn", "sr-Latn/post.md");

        derived.Clone().DerivedFromLocale.ShouldBe("sr");
    }
}
