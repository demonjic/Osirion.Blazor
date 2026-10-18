using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Interfaces;

namespace Osirion.Blazor.Examples.Shared.Pages;

/// <summary>The changelog at /changelog, rendered from the repository's CHANGELOG.md.</summary>
public partial class Changelog(IMarkdownProcessor markdownProcessor)
{
    /// <summary>The name of the embedded copy of CHANGELOG.md (see the project file).</summary>
    public const string ResourceName = "CHANGELOG.md";

    private ContentItem? _page;

    /// <inheritdoc/>
    protected override async Task OnInitializedAsync()
    {
        await using var stream = typeof(Changelog).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"The embedded {ResourceName} is missing.");
        using var reader = new StreamReader(stream);
        var (_, markdown) = markdownProcessor.ExtractFrontMatterAndContent(await reader.ReadToEndAsync());
        var html = await markdownProcessor.RenderToHtmlAsync(markdown, sanitizeHtml: true);

        var page = ContentItem.Create("changelog", "Changelog", html, ResourceName, "site");
        page.SetUrl("changelog");
        page.SetDescription("Every change to the Osirion.Blazor packages, release by release: what was added, changed and fixed.");
        // The front matter of CHANGELOG.md is not read, so the page sets its link preview card here (tools/site-media).
        page.SetFeaturedImage("https://media.getosirion.com/og/changelog.jpg");
        _page = page;
    }
}
