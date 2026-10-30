using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Interfaces;
using Osirion.Blazor.Cms.Infrastructure.Markdown;
using Osirion.Blazor.Cms.Infrastructure.Tests.TestDoubles;
using Shouldly;

namespace Osirion.Blazor.Cms.Infrastructure.Tests.Repositories;

/// <summary>
/// A publication date with a time of day, read from a file and written back by the base content repository.
/// </summary>
public class BaseContentRepositoryDateTests
{
    private static IMarkdownProcessor CreateProcessor() =>
        new MarkdownProcessor(new DefaultMarkdownRenderer(), new MarkdownSanitizer(), new HtmlToMarkdownConverter());

    private static async Task<ContentItem> ReadAsync(ScriptedContentRepository repository, string frontMatter)
    {
        var item = new ContentItem { Path = "blog/post.md", Slug = "post" };
        await repository.ProcessAsync($"---\n{frontMatter}---\n\nBody.\n", item);
        return item;
    }

    [Fact]
    public async Task ProcessMarkdown_DateWithATimeAndOffset_DatesTheItemAtThatMomentInUtc()
    {
        var repository = new ScriptedContentRepository(CreateProcessor());

        var item = await ReadAsync(repository, "title: Post\ndate: \"2026-10-29T18:30:00+01:00\"\nlast_modified: 2026-10-30T09:15:00Z\n");

        item.DateCreated.ShouldBe(new DateTime(2026, 10, 29, 17, 30, 0));
        item.DateCreated.Kind.ShouldBe(DateTimeKind.Utc);
        item.LastModified.ShouldBe(new DateTime(2026, 10, 30, 9, 15, 0));
    }

    // Failed before the fix: saving wrote the day alone, so the time a file named was lost on the first save.
    [Fact]
    public async Task GenerateMarkdown_DateUnchanged_KeepsTheFilesTextForIt()
    {
        var processor = CreateProcessor();
        var repository = new ScriptedContentRepository(processor);
        var item = await ReadAsync(repository, "title: Post\ndate: \"2026-10-29T18:30:00+01:00\"\n");

        var markdown = await repository.GenerateAsync(item);

        processor.ExtractFrontMatterAndContent(markdown).FrontMatter.ShouldNotBeNull().Date.ShouldBe("2026-10-29T18:30:00+01:00");
    }

    [Fact]
    public async Task GenerateMarkdown_DateChanged_WritesTheNewMomentWithItsTime()
    {
        var processor = CreateProcessor();
        var repository = new ScriptedContentRepository(processor);
        var item = await ReadAsync(repository, "title: Post\ndate: \"2026-10-29T18:30:00+01:00\"\n");
        item.SetCreatedDate(new DateTime(2026, 10, 30, 8, 0, 0, DateTimeKind.Utc));

        var markdown = await repository.GenerateAsync(item);

        processor.ExtractFrontMatterAndContent(markdown).FrontMatter.ShouldNotBeNull().Date.ShouldBe("2026-10-30T08:00:00Z");
    }

    [Fact]
    public async Task GenerateMarkdown_ItemWithoutMetadataAndAMoment_WritesTheTime()
    {
        var repository = new ScriptedContentRepository();
        var item = new ContentItem { DateCreated = new DateTime(2026, 10, 29, 17, 30, 0, DateTimeKind.Utc) };

        var markdown = await repository.GenerateAsync(item);

        markdown.ShouldContain(Environment.NewLine + "date: 2026-10-29T17:30:00Z" + Environment.NewLine);
    }
}
