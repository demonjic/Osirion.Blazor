using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Osirion.Blazor.Cms.Domain.ContentBlocks;
using Osirion.Blazor.Cms.Web.ContentBlocks;
using Osirion.Blazor.Core.Extensions;
using Osirion.Blazor.Core.Localization;

namespace Osirion.Blazor.Cms.Web.Components;

/// <summary>
/// Renders content blocks, in order, each with the renderer registered for its type: a page written as front matter
/// (<c>blocks:</c>) instead of a Razor page. <c>ContentRenderer</c> and <c>LandingPage</c> render an item's blocks with
/// it; use it directly to place blocks in a page of your own.
/// </summary>
/// <remarks>
/// <para>
/// The renderers come from the <see cref="ContentBlockRegistry"/> registered by <c>AddOsirionContentBlocks</c>, or the
/// library's renderers when none is registered. Text is formatted and sanitized by the renderers; nothing from the
/// content is written as raw HTML. Site-relative links are localized with <c>IOsirionLocalePaths</c> when it is
/// registered and left as written otherwise.
/// </para>
/// <para>
/// A block that cannot render (an unknown type without a fallback renderer, an unknown component name) is logged as an
/// error and renders nothing; in Development it renders a notice that names the file, the block and the reason.
/// </para>
/// </remarks>
public partial class OsirionContentBlocks
{
    /// <summary>Gets or sets the blocks to render, usually <c>ContentItem.Blocks</c>.</summary>
    [Parameter]
    public IReadOnlyList<ContentBlock> Blocks { get; set; } = [];

    /// <summary>
    /// Gets or sets the path of the content item the blocks come from (<c>ContentItem.Path</c>), named in logs and
    /// notices.
    /// </summary>
    [Parameter]
    public string? SourcePath { get; set; }

    /// <summary>
    /// Gets or sets whether the page's first <c>faq</c> block writes FAQPage structured data. Turn it off when the
    /// page writes its own FAQPage, so the page has one.
    /// </summary>
    [Parameter]
    public bool IncludeFaqStructuredData { get; set; } = true;

    [Inject]
    private IServiceProvider ServiceProvider { get; set; } = default!;

    [Inject]
    private ILogger<OsirionContentBlocks> Logger { get; set; } = default!;

    private ContentBlockContext? _context;

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        var localePaths = ServiceProvider.GetService<IOsirionLocalePaths>();
        _context = new ContentBlockContext(new ContentBlockContextSettings(
            ServiceProvider.GetService<ContentBlockRegistry>() ?? ContentBlockRegistry.Default,
            ServiceProvider.GetHtmlContentSanitizer(),
            localePaths is null ? static path => path : localePaths.LocalizeForCurrentCulture,
            Logger,
            ServiceProvider.GetService<IHostEnvironment>()?.IsDevelopment() == true,
            SourcePath ?? string.Empty,
            IncludeFaqStructuredData ? Blocks.OfType<FaqBlock>().FirstOrDefault() : null));
    }
}
