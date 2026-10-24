using System.Text;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Osirion.Blazor.Core.Configuration;
using Osirion.Blazor.Core.Extensions;
using Osirion.Blazor.Core.Services;

namespace Osirion.Blazor.Components;

/// <summary>
/// Component for rendering HTML content with optional syntax highlighting
/// </summary>
public partial class OsirionHtmlRenderer
{
    /// <summary>Gets or sets the HTML to render. Renders nothing when empty or white space.</summary>
    [Parameter]
    public string? HtmlContent { get; set; }

    /// <summary>
    /// Gets or sets whether Prism highlights the code blocks. Prism loads only when the HTML has a code block (a
    /// <c>pre</c> element whose first child is a <c>code</c> element). When <see langword="false"/>, line numbers
    /// and copy buttons are not added either.
    /// </summary>
    [Parameter]
    public bool EnableSyntaxHighlighting { get; set; } = true;

    /// <summary>
    /// Gets or sets whether code blocks show line numbers (Prism's line-numbers plugin). Needs
    /// <see cref="EnableSyntaxHighlighting"/>. Each number is a button that highlights its line.
    /// </summary>
    [Parameter]
    public bool ShowLineNumbers { get; set; } = false;

    /// <summary>
    /// Gets or sets whether each code block is wrapped with a header that shows its language and a Copy button.
    /// Needs <see cref="EnableSyntaxHighlighting"/>.
    /// </summary>
    [Parameter]
    public bool EnableCopyButton { get; set; } = false;

    /// <summary>
    /// Gets or sets whether line highlighting is enabled. The value is written to the page as
    /// <c>data-enable-line-highlighting</c>, but the bundled line highlighting script does not read it: with
    /// <see cref="ShowLineNumbers"/> on, line numbers can be selected whatever this says.
    /// </summary>
    [Parameter]
    public bool EnableLineHighlighting { get; set; } = false;

    /// <summary>
    /// Gets or sets whether the built-in high-contrast dark theme styles the code. When <see langword="false"/>,
    /// Prism's Okaidia theme is loaded instead.
    /// </summary>
    [Parameter]
    public bool UseAccessibleTheme { get; set; } = true;

    /// <summary>
    /// Gets or sets CSS classes for specific languages. Obsolete and ignored: the component never read it. Style
    /// code blocks with CSS selectors on the <c>language-*</c> classes instead.
    /// </summary>
    /// <remarks>
    /// This parameter has never been read by the component. Style code blocks with CSS selectors on the
    /// <c>language-*</c> classes instead.
    /// </remarks>
    [Obsolete("LanguageClasses is not used by the component. Style code blocks with CSS selectors on the language-* classes instead.")]
    [Parameter]
    public Dictionary<string, string>? LanguageClasses { get; set; }

    /// <summary>Gets or sets whether the HTML is sanitized before it is rendered.</summary>
    /// <remarks>
    /// Sanitization uses the registered <see cref="IHtmlContentSanitizer"/> (see <c>AddOsirionHtmlSanitizer</c>),
    /// or the default allow-list when none is registered. Set to <see langword="false"/> only for markup the
    /// application itself produced; never for content, user input or anything read from a repository.
    /// </remarks>
    [Parameter]
    public bool SanitizeHtml { get; set; } = true;

    /// <summary>
    /// Gets or sets a sanitizer to use instead of the registered <see cref="IHtmlContentSanitizer"/>. Used only
    /// while <see cref="SanitizeHtml"/> is <see langword="true"/>.
    /// </summary>
    [Parameter]
    public Func<string, string>? HtmlSanitizer { get; set; }

    /// <summary>
    /// Gets or sets the base address Prism is loaded from, for hosts that serve it themselves (for example
    /// <c>/lib/prism/1.29.0/</c>). When empty, <see cref="PrismOptions.BaseUrl"/> applies (set with
    /// <c>AddOsirionPrism</c>), and without that the pinned cdnjs release is used and every file is verified with its
    /// Subresource Integrity hash. Files from a custom address load without an integrity check.
    /// </summary>
    /// <remarks>
    /// Files from a custom base address are loaded without an integrity check, because the library cannot know
    /// their content. Expected layout: <c>prism.min.js</c>, <c>components/prism-{language}.min.js</c>,
    /// <c>themes/prism-okaidia.min.css</c> and <c>plugins/line-numbers/...</c>, as in the Prism release.
    /// </remarks>
    [Parameter]
    public string? PrismBaseUrl { get; set; }

    /// <summary>
    /// Gets or sets whether fenced <c>demo</c> blocks whose text is a registered demo key (see
    /// <c>AddOsirionContentDemos</c>) render the live demo instead of the code block. Only blocks at the top level
    /// of the content are replaced; a block inside a list or quote stays a code block.
    /// </summary>
    [Parameter]
    public bool EnableDemos { get; set; } = true;

    /// <summary>
    /// Gets or sets whether code blocks and tables take a place in the tab order (<c>tabindex="0"</c>), so a
    /// keyboard user can scroll them sideways (WCAG 2.1.1). An element that already has a <c>tabindex</c> keeps it.
    /// </summary>
    [Parameter]
    public bool FocusableScrollBlocks { get; set; } = true;

    [Inject]
    private IServiceProvider ServiceProvider { get; set; } = default!;

    /// <summary>
    /// Whether the content has code blocks
    /// </summary>
    private bool HasCodeBlocks { get; set; }

    /// <summary>
    /// Detected programming languages in code blocks
    /// </summary>
    private HashSet<string> DetectedLanguages { get; set; } = new();

    /// <summary>
    /// Processed HTML with enhanced code blocks
    /// </summary>
    private string ProcessedHtml { get; set; } = string.Empty;

    /// <summary>
    /// The content split around embedded demos, or <see langword="null"/> when it has none.
    /// </summary>
    private IReadOnlyList<HtmlSegment>? Segments { get; set; }

    // Marks where a demo was taken out of the content. The random part cannot occur in content, and it is added after
    // sanitization, so content cannot forge a marker.
    private readonly string _demoMarker = "<!--osirion-demo-" + Guid.NewGuid().ToString("N") + "-";

    /// <summary>
    /// The base address Prism loads from: the <see cref="PrismBaseUrl"/> parameter, else the site's
    /// <see cref="PrismOptions.BaseUrl"/>; empty for the pinned cdnjs release.
    /// </summary>
    private string? _prismBaseUrl;

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        _prismBaseUrl = string.IsNullOrWhiteSpace(PrismBaseUrl)
            ? ServiceProvider.GetService<IOptions<PrismOptions>>()?.Value.BaseUrl
            : PrismBaseUrl;
        ProcessHtmlContent();
    }

    /// <summary>
    /// Matches a code block: a pre element whose first child is a code element.
    /// </summary>
    private static readonly Regex CodeBlockPattern = new(
        @"<pre[^>]*>\s*<code[^>]*>[\s\S]*?</code>\s*</pre>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex LanguageClassPattern = new(
        @"class\s*=\s*['""][^'""]*language-(\w+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Matches the opening pre tag at the start of a code block, attribute by attribute, so that markup held in a
    /// quoted attribute value is never mistaken for the end of the tag.
    /// </summary>
    private static readonly Regex OpeningPreTagPattern = new(
        @"^<pre((?:\s+[^\s""'<>/=]+(?:\s*=\s*(?:""[^""]*""|'[^']*'|[^\s""'=<>`]+))?)*)\s*>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex AttributePattern = new(
        @"(\s+)([^\s""'<>/=]+)(?:(\s*=\s*)(""[^""]*""|'[^']*'|[^\s""'=<>`]+))?", RegexOptions.Compiled);

    private const string LineNumbersClass = "line-numbers";

    /// <summary>
    /// Process the HTML content to detect and enhance code blocks
    /// </summary>
    private void ProcessHtmlContent()
    {
        if (string.IsNullOrWhiteSpace(HtmlContent))
        {
            ProcessedHtml = string.Empty;
            HasCodeBlocks = false;
            DetectedLanguages.Clear();
            return;
        }

        var content = HtmlContent;

        // Sanitize before the code blocks are wrapped: the wrapper adds the library's own copy button handler.
        if (SanitizeHtml)
        {
            content = SanitizeHtmlContent(content);
        }

        var demos = ExtractDemos(ref content);
        HasCodeBlocks = CodeBlockPattern.IsMatch(content) || demos.Any(demo => demo.Source is not null);
        DetectedLanguages.Clear();
        if (demos.Any(demo => demo.Source is not null))
        {
            DetectedLanguages.Add("cshtml");
        }

        ProcessedHtml = !HasCodeBlocks || !EnableSyntaxHighlighting ? content : EnhanceCodeBlocks(content);
        if (FocusableScrollBlocks)
        {
            ProcessedHtml = MakeScrollBlocksFocusable(ProcessedHtml);
        }

        Segments = demos.Count == 0 ? null : SplitAroundDemos(ProcessedHtml, demos);
    }

    private string EnhanceCodeBlocks(string content)
    {

        // One pass over the content: every block is rewritten where it stands, so identical blocks are each
        // handled once and a wrapped block is never searched again.
        var codeBlockIndex = 0;
        return CodeBlockPattern.Replace(content, match =>
        {
            var codeBlock = match.Value;

            var languageMatch = LanguageClassPattern.Match(codeBlock);
            var language = MapLanguageAlias(languageMatch.Success ? languageMatch.Groups[1].Value : "plaintext");

            if (!string.IsNullOrWhiteSpace(language) && language != "plaintext")
            {
                DetectedLanguages.Add(language);
            }

            // The block keeps its own language class, which Prism reads from the code element or its pre
            // ancestor. Only the class list of the opening pre tag is touched, and only through a tag-aware
            // match: this runs after sanitization, and a pattern that stops at the first '>' can turn markup
            // held in a quoted attribute value into a live element.
            if (ShowLineNumbers)
            {
                codeBlock = AddLineNumbersClass(codeBlock);
            }

            return EnableCopyButton ? WrapCodeBlock(codeBlock, language, codeBlockIndex++) : codeBlock;
        });
    }

    /// <summary>
    /// Adds the Prism line-numbers class to the opening pre tag of a code block, keeping the classes it has.
    /// </summary>
    private static string AddLineNumbersClass(string codeBlock)
    {
        var tag = OpeningPreTagPattern.Match(codeBlock);
        if (!tag.Success)
        {
            return codeBlock;
        }

        var hasClassAttribute = false;
        var attributes = AttributePattern.Replace(tag.Groups[1].Value, attribute =>
        {
            if (hasClassAttribute || !attribute.Groups[2].Value.Equals("class", StringComparison.OrdinalIgnoreCase))
            {
                return attribute.Value;
            }

            hasClassAttribute = true;
            var rawValue = attribute.Groups[4].Value;
            var quote = rawValue.Length > 0 && rawValue[0] is '"' or '\'' ? rawValue[0] : '"';
            var classes = rawValue.Trim('"', '\'');

            if (classes.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Contains(LineNumbersClass, StringComparer.Ordinal))
            {
                return attribute.Value;
            }

            var combined = string.IsNullOrWhiteSpace(classes) ? LineNumbersClass : $"{classes} {LineNumbersClass}";
            return $"{attribute.Groups[1].Value}class{attribute.Groups[3].Value}{quote}{combined}{quote}";
        });

        if (!hasClassAttribute)
        {
            attributes += $" class=\"{LineNumbersClass}\"";
        }

        return string.Concat("<pre", attributes, ">", codeBlock.AsSpan(tag.Length));
    }

    /// <summary>
    /// Common language aliases and the Prism.js language names they stand for. HTML and XML are both Prism's markup.
    /// </summary>
    private static readonly Dictionary<string, string> PrismLanguageAliases = new(StringComparer.Ordinal)
    {
        ["cs"] = "csharp",
        ["js"] = "javascript",
        ["ts"] = "typescript",
        ["py"] = "python",
        ["rb"] = "ruby",
        ["yml"] = "yaml",
        ["sh"] = "bash",
        ["ps1"] = "powershell",
        ["razor"] = "cshtml",
        ["blazor"] = "cshtml",
        ["html"] = "markup",
        ["xml"] = "markup",
    };

    /// <summary>
    /// Display labels for Prism language names; a language without one is shown in upper case.
    /// </summary>
    private static readonly Dictionary<string, string> LanguageLabels = new(StringComparer.Ordinal)
    {
        ["csharp"] = "C#",
        ["javascript"] = "JavaScript",
        ["typescript"] = "TypeScript",
        ["python"] = "Python",
        ["yaml"] = "YAML",
        ["bash"] = "Bash",
        ["powershell"] = "PowerShell",
        ["cshtml"] = "Razor",
        ["html"] = "HTML",
        ["css"] = "CSS",
        ["sql"] = "SQL",
        ["xml"] = "XML",
        ["json"] = "JSON",
    };

    /// <summary>
    /// Map common language aliases to Prism.js language names
    /// </summary>
    private static string MapLanguageAlias(string language) =>
        PrismLanguageAliases.TryGetValue(language, out var prismLanguage) ? prismLanguage : language;

    /// <summary>
    /// Check if a language component needs to be loaded
    /// </summary>
    private bool NeedsLanguageComponent(string language)
    {
        // These languages are included in Prism core
        var coreLanguages = new HashSet<string>
        {
            "markup", "html", "xml", "svg", "mathml",
            "css", "clike", "javascript", "js"
        };

        return !coreLanguages.Contains(language);
    }

    /// <summary>
    /// Replaces each top-level fenced <c>demo</c> block whose key is registered with a marker, and returns the demos in
    /// order. Content without such a block is returned unchanged, without being parsed.
    /// </summary>
    private List<ContentDemo> ExtractDemos(ref string content)
    {
        var demos = new List<ContentDemo>();
        if (!EnableDemos || !content.Contains("language-demo", StringComparison.OrdinalIgnoreCase)
            || ServiceProvider.GetService<IContentDemoRegistry>() is not { } registry)
        {
            return demos;
        }

        var parser = new HtmlParser();
        var context = parser.ParseDocument(string.Empty).Body!;
        var rebuilt = new StringBuilder(content.Length);
        foreach (var node in parser.ParseFragment(content, context))
        {
            if (node is IElement { LocalName: "pre" } pre
                && pre.Children.Length == 1
                && pre.Children[0] is IElement { LocalName: "code" } code
                && code.ClassList.Contains("language-demo")
                && registry.TryGet(code.TextContent.Trim(), out var demo))
            {
                rebuilt.Append(_demoMarker).Append(demos.Count).Append("-->");
                demos.Add(demo);
            }
            else
            {
                rebuilt.Append(node.ToHtml());
            }
        }

        if (demos.Count > 0)
        {
            content = rebuilt.ToString();
        }

        return demos;
    }

    /// <summary>
    /// Gives every pre and table element without a <c>tabindex</c> the value 0. The change is made on the parsed
    /// fragment, so text that only looks like a tag (in an attribute value, say) is never touched. Content without such
    /// an element is returned unchanged, without being parsed.
    /// </summary>
    private static string MakeScrollBlocksFocusable(string html)
    {
        if (!html.Contains("<pre", StringComparison.OrdinalIgnoreCase) && !html.Contains("<table", StringComparison.OrdinalIgnoreCase))
        {
            return html;
        }

        var parser = new HtmlParser();
        var context = parser.ParseDocument(string.Empty).Body!;
        var nodes = parser.ParseFragment(html, context);
        var changed = false;
        foreach (var element in nodes.OfType<IElement>().SelectMany(node => node.QuerySelectorAll("pre, table").Prepend(node)))
        {
            if (element.LocalName is "pre" or "table" && !element.HasAttribute("tabindex"))
            {
                element.SetAttribute("tabindex", "0");
                changed = true;
            }
        }

        return changed ? string.Concat(nodes.Select(node => node.ToHtml())) : html;
    }

    private List<HtmlSegment> SplitAroundDemos(string html, List<ContentDemo> demos)
    {
        var segments = new List<HtmlSegment>();
        var position = 0;
        foreach (Match marker in Regex.Matches(html, Regex.Escape(_demoMarker) + "([0-9]+)-->"))
        {
            if (marker.Index > position)
            {
                segments.Add(new HtmlSegment(html[position..marker.Index], null));
            }

            segments.Add(new HtmlSegment(null, demos[int.Parse(marker.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture)]));
            position = marker.Index + marker.Length;
        }

        if (position < html.Length)
        {
            segments.Add(new HtmlSegment(html[position..], null));
        }

        return segments;
    }

    private sealed record HtmlSegment(string? Html, ContentDemo? Demo);

    /// <summary>
    /// Sanitize HTML content to prevent XSS attacks
    /// </summary>
    private string SanitizeHtmlContent(string html)
    {
        if (HtmlSanitizer is not null)
        {
            return HtmlSanitizer(html);
        }

        return ServiceProvider.GetHtmlContentSanitizer().Sanitize(html);
    }

    /// <summary>
    /// Wrap a code block with additional markup for features
    /// </summary>
    private string WrapCodeBlock(string codeBlock, string language, int index) =>
        CodeBlockMarkup.Wrap(codeBlock, language, GetLanguageLabel(language), GetButtonClass());

    /// <summary>
    /// Get a display label for a language
    /// </summary>
    private static string GetLanguageLabel(string language) =>
        LanguageLabels.TryGetValue(language, out var label) ? label : language.ToUpper();

    /// <summary>
    /// Get the CSS class for the HTML renderer
    /// </summary>
    private string GetHtmlRendererClass()
    {
        var classes = new List<string> { "osirion-html-renderer osirion-content" };

        // The theme's rules in osirion-html-renderer.css follow this class. Without highlighting Prism never gives the
        // pre its dark background, so the theme's light code text would sit on the page's own background.
        if (UseAccessibleTheme && HasCodeBlocks && EnableSyntaxHighlighting)
        {
            classes.Add("osirion-wcag-theme");
        }

        if (!string.IsNullOrWhiteSpace(Class))
        {
            classes.Add(Class);
        }

        return string.Join(" ", classes);
    }

    /// <summary>
    /// Gets the address of a Prism file: the pinned cdnjs release, or the custom base address when one is set.
    /// </summary>
    private string PrismUrl(string relativePath) => string.IsNullOrWhiteSpace(_prismBaseUrl)
        ? PrismCdnAssets.BaseUrl + relativePath
        : _prismBaseUrl.TrimEnd('/') + "/" + relativePath;

    /// <summary>
    /// Gets the Subresource Integrity hash of a pinned Prism file, or <see langword="null"/> when the file comes
    /// from a custom base address (the host owns that source) or is not part of the pinned release.
    /// </summary>
    private string? PrismIntegrity(string relativePath) =>
        string.IsNullOrWhiteSpace(_prismBaseUrl) && PrismCdnAssets.TryGetIntegrity(relativePath, out var integrity)
            ? integrity
            : null;

    private string? PrismCrossOrigin(string relativePath) => PrismIntegrity(relativePath) is null ? null : "anonymous";

    /// <summary>
    /// Lists the Prism scripts to load, in execution order: the core first, then the language components found in
    /// the content, then the line-numbers plugin.
    /// </summary>
    private List<Dictionary<string, object?>> GetPrismScripts()
    {
        var usesPinnedRelease = string.IsNullOrWhiteSpace(_prismBaseUrl);
        var relativePaths = new List<string> { "prism.min.js" };

        // Without a custom source, a language that is not part of the pinned release is skipped instead of
        // requested: it has no integrity hash and the request could only fail.
        relativePaths.AddRange(DetectedLanguages
            .Where(NeedsLanguageComponent)
            .Order(StringComparer.Ordinal)
            .Select(language => $"components/prism-{language}.min.js")
            .Where(path => !usesPinnedRelease || PrismCdnAssets.TryGetIntegrity(path, out _)));

        if (ShowLineNumbers)
        {
            relativePaths.Add("plugins/line-numbers/prism-line-numbers.min.js");
        }

        return relativePaths
            .Select(path => new Dictionary<string, object?>
            {
                ["src"] = PrismUrl(path),
                ["integrity"] = PrismIntegrity(path),
                ["required"] = path == "prism.min.js"
            })
            .ToList();
    }
}
