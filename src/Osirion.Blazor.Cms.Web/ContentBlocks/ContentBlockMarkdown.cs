using Markdig;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Osirion.Blazor.Cms.Domain.ContentBlocks;
using Osirion.Blazor.Core.Services;

namespace Osirion.Blazor.Cms.Web.ContentBlocks;

/// <summary>
/// Turns the text values of blocks into markup: block Markdown (<c>markdown</c>, answers) with the extensions page
/// bodies use, inline Markdown (descriptions, captions) as phrasing content only, and plain text for parameters that
/// take a string. Every HTML result goes through the site's <see cref="IHtmlContentSanitizer"/>, the sanitizer of
/// page bodies, and site-relative link destinations are localized on the way.
/// </summary>
internal sealed class ContentBlockMarkdown
{
    private static readonly MarkdownPipeline BlockPipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

    // Only paragraphs, so "2026. A year" or "- item" in a description stays text instead of becoming a list, and no
    // raw HTML: an inline value has no reason to carry markup.
    private static readonly MarkdownPipeline InlinePipeline = CreateInlinePipeline();

    private readonly IHtmlContentSanitizer _sanitizer;
    private readonly Func<string, string> _localizeSitePath;

    public ContentBlockMarkdown(IHtmlContentSanitizer sanitizer, Func<string, string> localizeSitePath)
    {
        _sanitizer = sanitizer;
        _localizeSitePath = localizeSitePath;
    }

    /// <summary>Renders block Markdown to sanitized HTML.</summary>
    public string ToBlockHtml(string? markdown) =>
        string.IsNullOrWhiteSpace(markdown) ? string.Empty : _sanitizer.Sanitize(Render(markdown, BlockPipeline, implicitParagraph: false));

    /// <summary>Renders inline Markdown to sanitized phrasing HTML, without a paragraph element around it.</summary>
    public string ToInlineHtml(string? text) =>
        string.IsNullOrWhiteSpace(text) ? string.Empty : _sanitizer.Sanitize(Render(text, InlinePipeline, implicitParagraph: true).Trim());

    /// <summary>Returns the text of inline Markdown without its markers, for a parameter that takes plain text.</summary>
    public static string ToPlainText(string? text) =>
        string.IsNullOrWhiteSpace(text) ? string.Empty : Markdown.ToPlainText(text, InlinePipeline).Trim();

    /// <summary>Returns the text of block Markdown without its markup, for structured data.</summary>
    public static string ToPlainBlockText(string? markdown) =>
        string.IsNullOrWhiteSpace(markdown) ? string.Empty : Markdown.ToPlainText(markdown, BlockPipeline).Trim();

    private string Render(string markdown, MarkdownPipeline pipeline, bool implicitParagraph)
    {
        var document = Markdown.Parse(markdown, pipeline);
        foreach (var link in document.Descendants<LinkInline>())
        {
            if (!link.IsImage && link.Url is { } url && ContentBlockLinks.IsSitePath(url))
            {
                link.Url = _localizeSitePath(url);
            }
        }

        using var writer = new StringWriter();
        var renderer = new HtmlRenderer(writer) { ImplicitParagraph = implicitParagraph };
        pipeline.Setup(renderer);
        renderer.Render(document);
        writer.Flush();
        return writer.ToString();
    }

    private static MarkdownPipeline CreateInlinePipeline()
    {
        var builder = new MarkdownPipelineBuilder().DisableHtml();
        builder.BlockParsers.RemoveAll(parser => parser is not ParagraphBlockParser);
        return builder.Build();
    }
}
