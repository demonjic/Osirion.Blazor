using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Osirion.Blazor.Cms.Web.Components;

/// <summary>
/// TableOfContents component for extracting and displaying headings from HTML content
/// </summary>
public partial class TableOfContents(NavigationManager navigationManager)
{
    // The content is untrusted HTML, so every pattern has a timeout. Singleline lets a heading's text run over lines.
    [GeneratedRegex(@"<h([1-6])(\s[^>]*)?>(.*?)</h\1\s*>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex HeadingPattern();

    // The id may follow other attributes and be quoted or bare; "data-id" must not count.
    [GeneratedRegex(@"(?<![\w-])id\s*=\s*(?:""([^""]*)""|'([^']*)'|([^\s""'>]+))", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex IdAttributePattern();

    [GeneratedRegex("<[^>]*>", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex HtmlTagPattern();

    [GeneratedRegex(@"[^a-z0-9\s-]", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex NonIdCharacterPattern();

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex WhitespacePattern();

    [Inject]
    private ILogger<TableOfContents> Logger { get; set; } = default!;

    /// <summary>
    /// Gets or sets the HTML to read the headings from, usually an item's rendered content. Nothing is listed when
    /// it is empty.
    /// </summary>
    [Parameter]
    public string? Content { get; set; }

    /// <summary>Gets or sets the lowest heading level included, so the page's <c>h1</c> is left out by default.</summary>
    [Parameter]
    public int MinLevel { get; set; } = 2;

    /// <summary>Gets or sets the highest heading level included.</summary>
    [Parameter]
    public int MaxLevel { get; set; } = 6;

    /// <summary>
    /// Gets or sets the nesting limit. <c>1</c> lists only top-level headings; the list renders at most two levels,
    /// so a value above <c>2</c> changes nothing.
    /// </summary>
    [Parameter]
    public int MaxDepth { get; set; } = 3;

    /// <summary>Gets or sets the text shown when no heading matches.</summary>
    [Parameter]
    public string EmptyText { get; set; } = "No headings found.";

    /// <summary>
    /// Gets or sets whether the link to the section being read is marked as it scrolls by: it gets the
    /// <c>osirion-toc-link-active</c> class and <c>aria-current="location"</c>. A script does it, so without script
    /// the contents work as plain links.
    /// </summary>
    [Parameter]
    public bool TrackActiveHeading { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the entries are numbered, <c>1.</c> for a top-level heading and <c>1.2</c> for one under
    /// it. When on, the root element gets the <c>osirion-toc-numbered</c> class that the numbering rules hang on;
    /// <see langword="false"/> renders a plain list.
    /// </summary>
    [Parameter]
    public bool ShowNumbers { get; set; } = true;

    /// <summary>
    /// Gets the extracted headings
    /// </summary>
    private IReadOnlyList<HeadingItem> Headings { get; set; } = Array.Empty<HeadingItem>();

    private string CurrentUrl { get; set; } = "/";

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        if (!string.IsNullOrWhiteSpace(Content))
        {
            try
            {
                Headings = ExtractHeadings(Content);
            }
            catch (RegexMatchTimeoutException ex)
            {
                Logger.LogWarning(ex, "The headings of the content could not be read within the match timeout");
                Headings = Array.Empty<HeadingItem>();
            }
        }
        else
        {
            Headings = Array.Empty<HeadingItem>();
        }

        var uri = new Uri(navigationManager.Uri);
        CurrentUrl = uri.AbsolutePath;
    }

    /// <summary>
    /// Extracts headings from HTML content
    /// </summary>
    private IReadOnlyList<HeadingItem> ExtractHeadings(string htmlContent)
    {
        var headings = new List<HeadingItem>();

        foreach (Match match in HeadingPattern().Matches(htmlContent))
        {
            var level = int.Parse(match.Groups[1].Value);

            if (level < MinLevel || level > MaxLevel)
                continue;

            var id = ReadId(match.Groups[2].Value);
            var text = StripHtmlTags(match.Groups[3].Value);

            // Generate ID if not present
            if (string.IsNullOrWhiteSpace(id))
            {
                id = GenerateId(text);
            }

            headings.Add(new HeadingItem
            {
                Level = level,
                Id = id,
                Text = text,
                Children = new List<HeadingItem>()
            });
        }

        return BuildHierarchy(headings);
    }

    private static string ReadId(string attributes)
    {
        var match = IdAttributePattern().Match(attributes);
        if (!match.Success)
        {
            return string.Empty;
        }

        return match.Groups[1].Success ? match.Groups[1].Value
            : match.Groups[2].Success ? match.Groups[2].Value
            : match.Groups[3].Value;
    }

    /// <summary>
    /// Builds a hierarchical structure from flat headings list
    /// </summary>
    private IReadOnlyList<HeadingItem> BuildHierarchy(List<HeadingItem> flatHeadings)
    {
        var result = new List<HeadingItem>();
        var stack = new Stack<HeadingItem>();

        foreach (var heading in flatHeadings)
        {
            // Remove items from stack that are at same or deeper level
            while (stack.Count > 0 && stack.Peek().Level >= heading.Level)
            {
                stack.Pop();
            }

            if (stack.Count == 0)
            {
                // Top level heading
                result.Add(heading);
            }
            else if (stack.Count < MaxDepth)
            {
                // Add as child to parent
                stack.Peek().Children.Add(heading);
            }

            stack.Push(heading);
        }

        return result;
    }

    /// <summary>
    /// Strips HTML tags from text
    /// </summary>
    private static string StripHtmlTags(string input)
    {
        return HtmlTagPattern().Replace(input, string.Empty).Trim();
    }

    /// <summary>
    /// Generates a URL-friendly ID from text
    /// </summary>
    private static string GenerateId(string text)
    {
        // Convert to lowercase and replace non-alphanumeric characters with hyphens
        var id = NonIdCharacterPattern().Replace(text.ToLowerInvariant(), "");
        id = WhitespacePattern().Replace(id, "-");
        id = id.Trim('-');

        return string.IsNullOrWhiteSpace(id) ? "heading" : id;
    }

    /// <summary>
    /// Gets the CSS class for the table of contents
    /// </summary>
    private string GetTableOfContentsClass()
    {
        var numbered = ShowNumbers ? " osirion-toc-numbered" : string.Empty;
        return $"osirion-table-of-contents{numbered} {Class}".Trim();
    }
}

/// <summary>
/// Represents a heading item in the table of contents
/// </summary>
public class HeadingItem
{
    /// <summary>Gets or sets the Level value.</summary>
    public int Level { get; set; }
    /// <summary>Gets or sets the Id value.</summary>
    public string Id { get; set; } = string.Empty;
    /// <summary>Gets or sets the Text value.</summary>
    public string Text { get; set; } = string.Empty;
    /// <summary>Gets or sets the Children value.</summary>
    public List<HeadingItem> Children { get; set; } = new();
}
