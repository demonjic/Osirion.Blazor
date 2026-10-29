using System.Text;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Osirion.Blazor.Cms.Domain.ContentBlocks;

/// <summary>
/// Changes the prose of a Markdown value and leaves its syntax alone, the way <c>HtmlTransliteration</c> changes the
/// text of rendered HTML: code spans and code blocks, raw HTML, autolinks, link and image destinations and the Markdown
/// markers are kept as written; link texts and image descriptions change with the rest of the text.
/// </summary>
internal static class MarkdownProse
{
    // The extensions page bodies and blocks render with, so the text found here is the text the reader sees.
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UsePreciseSourceLocation()
        .Build();

    /// <summary>Returns <paramref name="markdown"/> with each run of literal text passed through <paramref name="map"/>.</summary>
    public static string Map(string markdown, Func<string, string> map)
    {
        if (string.IsNullOrEmpty(markdown))
        {
            return markdown;
        }

        var spans = Markdown.Parse(markdown, Pipeline)
            .Descendants<LiteralInline>()
            .Where(literal => !IsAutolinkText(literal) && !literal.Span.IsEmpty && literal.Span.Start >= 0 && literal.Span.End < markdown.Length)
            .Select(literal => literal.Span)
            .OrderBy(span => span.Start)
            .ToList();

        var result = new StringBuilder(markdown.Length);
        var position = 0;
        foreach (var span in spans.Where(span => span.Start >= 0))
        {
            if (span.Start < position)
            {
                continue;
            }

            result.Append(markdown, position, span.Start - position);
            result.Append(map(markdown.Substring(span.Start, span.Length)));
            position = span.End + 1;
        }

        result.Append(markdown, position, markdown.Length - position);
        return result.ToString();
    }

    // A bare address the autolink extension turned into a link shows the address itself, which stays as written.
    private static bool IsAutolinkText(LiteralInline literal) =>
        literal.Parent is LinkInline { IsAutoLink: true };
}
