using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Osirion.Blazor.Cms.Admin.Features.ContentEditor.Components.Shared;
using Osirion.Blazor.Cms.Admin.Tests.TestDoubles;
using Osirion.Blazor.Cms.Domain.ValueObjects;
using Shouldly;

namespace Osirion.Blazor.Cms.Admin.Tests.Features.ContentEditor.Components.Shared;

public class BasicMetadataFormTests : AdminComponentTestContext
{
    private readonly List<FrontMatter> _changes = [];

    public BasicMetadataFormTests()
    {
        RenderInteractively();
    }

    private static FrontMatter CreateMetadata()
    {
        var metadata = FrontMatter.Create("Hello", "World", new DateTime(2024, 1, 15));
        metadata.Categories = ["news"];
        metadata.Tags = ["a"];
        return metadata;
    }

    private IRenderedComponent<BasicMetadataForm> RenderForm(FrontMatter metadata) =>
        Render<BasicMetadataForm>(parameters => parameters
            .Add(p => p.Metadata, metadata)
            .Add(p => p.MetadataChanged, (FrontMatter value) => _changes.Add(value)));

    [Fact]
    public void BasicMetadataForm_StaticRender_ShowsFieldsCountersAndTags()
    {
        SetRendererInfo(new RendererInfo("Static", false));

        var cut = Render<BasicMetadataForm>(parameters => parameters.Add(p => p.Metadata, CreateMetadata()));

        cut.Find("#title").GetAttribute("value").ShouldBe("Hello");
        cut.Find("#date").GetAttribute("value").ShouldBe("2024-01-15T00:00:00");
        var counters = cut.FindAll(".char-counter");
        counters[0].TextContent.ShouldContain("5 / 100");
        counters[0].ClassList.ShouldNotContain("warning");
        counters[1].ClassList.ShouldNotContain("warning");
        cut.FindAll(".tag-chip").Select(chip => chip.TextContent.Trim()).ShouldBe(["news", "a"]);
        cut.FindAll("img.featured-image-preview").ShouldBeEmpty();
    }

    [Fact]
    public void BasicMetadataForm_LongTitleAndDescription_MarkCountersAsWarning()
    {
        var metadata = FrontMatter.Create(new string('t', 81), new string('d', 251), new DateTime(2024, 1, 15));

        var cut = Render<BasicMetadataForm>(parameters => parameters.Add(p => p.Metadata, metadata));

        cut.FindAll(".char-counter").ShouldAllBe(counter => counter.ClassList.Contains("warning"));
    }

    [Fact]
    public void BasicMetadataForm_NullMetadata_FallsBackToNewPost()
    {
        var cut = Render<BasicMetadataForm>(parameters => parameters.Add(p => p.Metadata, null!));

        cut.Find("#title").GetAttribute("value").ShouldBe("New Post");
    }

    [Fact]
    public void BasicMetadataForm_UnparsableDate_ShowsAValidDate()
    {
        var metadata = CreateMetadata();
        metadata.Date = "not a date";

        var cut = Render<BasicMetadataForm>(parameters => parameters.Add(p => p.Metadata, metadata));

        DateTime.TryParse(cut.Find("#date").GetAttribute("value"), out _).ShouldBeTrue();
    }

    [Fact]
    public void BasicMetadataForm_FeaturedImageThatFailsToLoad_ShowsError()
    {
        var metadata = CreateMetadata();
        metadata.FeaturedImage = "https://example.com/missing.png";
        var cut = Render<BasicMetadataForm>(parameters => parameters.Add(p => p.Metadata, metadata));
        cut.Find("img.featured-image-preview").GetAttribute("src").ShouldBe("https://example.com/missing.png");
        cut.Markup.ShouldNotContain("Failed to load image");

        cut.Find("img.featured-image-preview").TriggerEvent("onerror", new Microsoft.AspNetCore.Components.Web.ErrorEventArgs());

        cut.Markup.ShouldContain("Failed to load image");
    }

    [Fact]
    public void BasicMetadataForm_EditingTextFields_RaisesChangedWithNewValues()
    {
        var cut = RenderForm(CreateMetadata());

        cut.Find("#title").Input("New title");
        cut.Find("#description").Input("New description");
        cut.Find("#author").Input("Jane");
        cut.Find("#featuredImage").Input("https://example.com/f.png");
        cut.Find("#isFeatured").Change(true);
        cut.Find("#isPublished").Change(false);

        var last = _changes[^1];
        last.Title.ShouldBe("New title");
        last.Description.ShouldBe("New description");
        last.Author.ShouldBe("Jane");
        last.FeaturedImage.ShouldBe("https://example.com/f.png");
        last.IsFeatured.ShouldBeTrue();
        last.Published.ShouldBeFalse();
    }

    // Failed before the fix: the field takes a date and a time, and the front matter kept only the day.
    [Fact]
    public void BasicMetadataForm_ChangingDate_RaisesChangedWithNewDateAndTime()
    {
        var cut = RenderForm(CreateMetadata());

        cut.Find("#date").Change("2025-03-04T10:30:00");

        _changes.ShouldNotBeEmpty();
        _changes[^1].Date.ShouldBe("2025-03-04T10:30:00");
    }

    // A moment with an offset shows in UTC, as the front matter reads a moment without one, and the label says so.
    [Fact]
    public void BasicMetadataForm_DateWithAnOffset_ShowsTheMomentInUtc()
    {
        var metadata = CreateMetadata();
        metadata.Date = "2026-10-29T18:30:00+01:00";

        var cut = Render<BasicMetadataForm>(parameters => parameters.Add(p => p.Metadata, metadata));

        cut.Find("#date").GetAttribute("value").ShouldBe("2026-10-29T17:30:00");
        cut.Find("label[for=date]").TextContent.ShouldBe("Publish Date (UTC)");
    }

    [Fact]
    public void BasicMetadataForm_EditingWithoutListener_UpdatesBoundValue()
    {
        var metadata = CreateMetadata();
        var cut = Render<BasicMetadataForm>(parameters => parameters.Add(p => p.Metadata, metadata));

        cut.Find("#title").Input("local");

        metadata.Title.ShouldBe("local");
    }

    [Fact]
    public void BasicMetadataForm_AddingCategoryAndTag_RaisesChangedWithLists()
    {
        var cut = RenderForm(CreateMetadata());
        var inputs = cut.FindAll("input.tag-input");

        inputs[0].Input("tech");
        inputs[0].KeyDown(new KeyboardEventArgs { Key = "Enter" });
        _changes[^1].Categories.ShouldBe(["news", "tech"]);

        cut.FindAll("input.tag-input")[1].Input("b");
        cut.FindAll("input.tag-input")[1].KeyDown(new KeyboardEventArgs { Key = "Enter" });

        _changes[^1].Tags.ShouldBe(["a", "b"]);
    }
}
