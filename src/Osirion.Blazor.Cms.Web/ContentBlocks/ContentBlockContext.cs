using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.Logging;
using Osirion.Blazor.Cms.Domain.ContentBlocks;

namespace Osirion.Blazor.Cms.Web.ContentBlocks;

/// <summary>
/// What a block renderer needs from the page: Markdown rendering, link localization, rendering a nested block, and
/// reporting a block it cannot render. One context serves one <c>OsirionContentBlocks</c> render.
/// </summary>
public sealed class ContentBlockContext
{
    private const string ProblemClass = "osirion-content-block-problem";

    private readonly ContentBlockRegistry _registry;
    private readonly ContentBlockMarkdown _markdown;
    private readonly Func<string, string> _localizeSitePath;
    private readonly ILogger _logger;
    private readonly bool _showProblems;
    private readonly FaqBlock? _faqWithStructuredData;
    private readonly HashSet<string> _loggedProblems = new(StringComparer.Ordinal);

    internal ContentBlockContext(ContentBlockContextSettings settings)
    {
        _registry = settings.Registry;
        _localizeSitePath = settings.LocalizeSitePath;
        _markdown = new ContentBlockMarkdown(settings.Sanitizer, settings.LocalizeSitePath);
        _logger = settings.Logger;
        _showProblems = settings.ShowProblems;
        _faqWithStructuredData = settings.FaqWithStructuredData;
        SourcePath = settings.SourcePath;
    }

    /// <summary>Gets the path of the content item the blocks come from, for messages.</summary>
    public string SourcePath { get; }

    /// <summary>Gets the registry the blocks are rendered from.</summary>
    internal ContentBlockRegistry Registry => _registry;

    /// <summary>Returns inline Markdown as plain text, for a component parameter that takes a string.</summary>
    /// <param name="text">The value as written; may be <see langword="null"/>.</param>
    public string PlainText(string? text) => ContentBlockMarkdown.ToPlainText(text);

    /// <summary>Returns inline Markdown as sanitized phrasing markup, with site-relative links localized.</summary>
    /// <param name="text">The value as written; may be <see langword="null"/>.</param>
    public MarkupString InlineMarkdown(string? text) => new(_markdown.ToInlineHtml(text));

    /// <summary>Returns block Markdown as sanitized HTML, with site-relative links localized.</summary>
    /// <param name="markdown">The value as written; may be <see langword="null"/>.</param>
    public MarkupString Markdown(string? markdown) => new(_markdown.ToBlockHtml(markdown));

    /// <summary>Returns block Markdown as plain text, for structured data.</summary>
    /// <param name="markdown">The value as written; may be <see langword="null"/>.</param>
    public string PlainMarkdownText(string? markdown) => ContentBlockMarkdown.ToPlainBlockText(markdown);

    /// <summary>
    /// Returns the destination to write in an <c>href</c>: a site-relative path localized for the current culture
    /// when <c>IOsirionLocalePaths</c> is registered, any other allowed link as written, and <see langword="null"/>
    /// for a link the content block rules do not allow.
    /// </summary>
    /// <param name="url">The link as written; may be <see langword="null"/>.</param>
    public string? Link(string? url)
    {
        if (!ContentBlockLinks.IsAllowedLink(url))
        {
            return null;
        }

        return ContentBlockLinks.IsSitePath(url!) ? _localizeSitePath(url!) : url;
    }

    /// <summary>
    /// Returns whether <paramref name="block"/> writes the page's FAQPage structured data: only the first <c>faq</c>
    /// block of the page does, so a page has one FAQPage, and none does when the page writes its own.
    /// </summary>
    /// <param name="block">The FAQ block.</param>
    public bool WritesFaqStructuredData(FaqBlock block) => ReferenceEquals(block, _faqWithStructuredData);

    /// <summary>
    /// Renders <paramref name="block"/> with its registered renderer, with the fallback renderer for a type that has
    /// none, or as a problem when neither exists.
    /// </summary>
    /// <param name="block">The block, at the top level or nested in another (an aside).</param>
    public RenderFragment RenderBlock(ContentBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);

        if (_registry.TryGetRenderer(block.Type, out var renderer))
        {
            return RenderComponent(renderer, block);
        }

        if (ContentBlockRegistry.AwaitsRenderer(block.Type))
        {
            return AwaitingRenderer(block);
        }

        if (block is CustomContentBlock && _registry.FallbackRenderer is { } fallback)
        {
            return RenderComponent(fallback, block);
        }

        return Problem(block, $"no renderer is registered for type '{block.Type}' and the site has no fallback renderer");
    }

    /// <summary>
    /// Reports a block that cannot be rendered: logs an error once, and renders a notice in Development and nothing
    /// otherwise. The notice is text only; nothing from the content is written as markup.
    /// </summary>
    /// <param name="block">The block.</param>
    /// <param name="reason">Why it cannot be rendered.</param>
    public RenderFragment Problem(ContentBlock block, string reason)
    {
        ArgumentNullException.ThrowIfNull(block);
        var message = Describe(block, reason);
        if (_loggedProblems.Add(message))
        {
            ContentBlockLog.BlockNotRendered(_logger, SourcePath, Location(block), reason);
        }

        return Notice(message);
    }

    /// <summary>
    /// Reports a block whose renderer the library does not have yet: logs a warning once naming the type, and renders
    /// a notice in Development and nothing otherwise.
    /// </summary>
    internal RenderFragment AwaitingRenderer(ContentBlock block)
    {
        var message = Describe(block, $"type '{block.Type}' has no renderer yet and renders nothing");
        if (_loggedProblems.Add(message))
        {
            ContentBlockLog.TypeAwaitingRenderer(_logger, SourcePath, Location(block), block.Type);
        }

        return Notice(message);
    }

    private RenderFragment RenderComponent(Type renderer, ContentBlock block) => builder =>
    {
        builder.OpenComponent(0, renderer);
        builder.AddComponentParameter(1, nameof(ContentBlockRenderer.Block), block);
        builder.AddComponentParameter(2, nameof(ContentBlockRenderer.Context), this);
        builder.SetKey(block);
        builder.CloseComponent();
    };

    private RenderFragment Notice(string message) => builder =>
    {
        if (!_showProblems)
        {
            return;
        }

        RenderNotice(builder, message);
    };

    private static void RenderNotice(RenderTreeBuilder builder, string message)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", ProblemClass);
        builder.AddAttribute(2, "role", "alert");
        builder.OpenElement(3, "p");
        builder.AddAttribute(4, "class", $"{ProblemClass}-title");
        builder.AddContent(5, "Content block problem (shown in Development only)");
        builder.CloseElement();
        builder.OpenElement(6, "p");
        builder.AddContent(7, message);
        builder.CloseElement();
        builder.CloseElement();
    }

    private string Describe(ContentBlock block, string reason) => $"{SourcePath}: {Location(block)}: {reason}";

    private static string Location(ContentBlock block) => $"blocks[{block.Index}] ({block.Type})";
}

/// <summary>What a <see cref="ContentBlockContext"/> is made from.</summary>
internal sealed record ContentBlockContextSettings(
    ContentBlockRegistry Registry,
    Osirion.Blazor.Core.Services.IHtmlContentSanitizer Sanitizer,
    Func<string, string> LocalizeSitePath,
    ILogger Logger,
    bool ShowProblems,
    string SourcePath,
    FaqBlock? FaqWithStructuredData);
