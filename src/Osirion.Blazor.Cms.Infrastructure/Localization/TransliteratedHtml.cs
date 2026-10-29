using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Osirion.Blazor.Core.Transliteration;
using Osirion.Blazor.Core.Utils;

namespace Osirion.Blazor.Cms.Infrastructure.Localization;

/// <summary>
/// Writes an item's rendered HTML in a derived locale: its text is transliterated and its links to the site's pages
/// in the source locale move to the derived locale. Everything else is left as it is.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Text nodes and the <c>alt</c>, <c>title</c>, <c>aria-label</c> and <c>placeholder</c> attributes are
/// transliterated.</item>
/// <item>Nothing inside <c>code</c>, <c>pre</c>, <c>kbd</c>, <c>samp</c>, <c>script</c>, <c>style</c> or an element with
/// <c>translate="no"</c> changes, and neither does a link's text that is its address (an autolink).</item>
/// <item>An <c>href</c> or <c>src</c> moves only when it is a path on the site (<see cref="LocalUrl.IsLocalPath"/>) whose
/// first segment is the source locale and whose last segment names no file; absolute, protocol-relative,
/// <c>mailto:</c>, <c>tel:</c> and fragment links, relative paths and links to files stay.</item>
/// </list>
/// HTML without a change is returned as it came; changed HTML is serialized from the parsed fragment.
/// </remarks>
internal sealed class TransliteratedHtml
{
    private static readonly HashSet<string> UntouchedElements =
        new(["code", "pre", "kbd", "samp", "script", "style"], StringComparer.OrdinalIgnoreCase);

    private static readonly string[] TextAttributes = ["alt", "title", "aria-label", "placeholder"];

    private static readonly string[] AddressAttributes = ["href", "src"];

    private readonly ITextTransliterator _transliterator;
    private readonly string _fromLocale;
    private readonly string _toLocale;
    private bool _changed;

    private TransliteratedHtml(ITextTransliterator transliterator, string fromLocale, string toLocale)
    {
        _transliterator = transliterator;
        _fromLocale = fromLocale;
        _toLocale = toLocale;
    }

    /// <summary>Writes <paramref name="html"/> in the derived locale.</summary>
    /// <param name="html">The source item's rendered HTML.</param>
    /// <param name="transliterator">Writes the text.</param>
    /// <param name="fromLocale">The source locale, the first segment of the links that move.</param>
    /// <param name="toLocale">The derived locale, the segment those links move under.</param>
    /// <returns>The derived HTML.</returns>
    internal static string Transform(string html, ITextTransliterator transliterator, string fromLocale, string toLocale)
    {
        if (string.IsNullOrEmpty(html))
        {
            return html;
        }

        var parser = new HtmlParser();
        var context = parser.ParseDocument(string.Empty).Body!;
        var nodes = parser.ParseFragment(html, context);

        var transform = new TransliteratedHtml(transliterator, fromLocale, toLocale);
        foreach (var node in nodes)
        {
            transform.Visit(node);
        }

        return transform._changed ? string.Concat(nodes.Select(node => node.ToHtml())) : html;
    }

    private void Visit(INode node)
    {
        if (node is IText text)
        {
            text.Data = Transliterate(text.Data);
            return;
        }

        if (node is not IElement element || IsUntouched(element))
        {
            return;
        }

        foreach (var name in TextAttributes)
        {
            RewriteAttribute(element, name, Transliterate);
        }

        foreach (var name in AddressAttributes)
        {
            RewriteAttribute(element, name, MoveToDerivedLocale);
        }

        if (IsAutolink(element))
        {
            return;
        }

        foreach (var child in element.ChildNodes)
        {
            Visit(child);
        }
    }

    private static bool IsUntouched(IElement element) =>
        UntouchedElements.Contains(element.LocalName)
        || string.Equals(element.GetAttribute("translate"), "no", StringComparison.OrdinalIgnoreCase);

    // The text of an autolink is its address, which must read the same as the address it opens. A renderer may write
    // the address percent-encoded and the text decoded, and a mailto: link's text without the scheme.
    private static bool IsAutolink(IElement element)
    {
        if (!element.LocalName.Equals("a", StringComparison.OrdinalIgnoreCase)
            || element.GetAttribute("href") is not { Length: > 0 } href)
        {
            return false;
        }

        var text = element.TextContent.Trim();
        return text == href
            || text == Uri.UnescapeDataString(href)
            || (href.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase) && text == href["mailto:".Length..]);
    }

    private void RewriteAttribute(IElement element, string name, Func<string, string> rewrite)
    {
        if (element.GetAttribute(name) is not { } value)
        {
            return;
        }

        var rewritten = rewrite(value);
        if (rewritten != value)
        {
            element.SetAttribute(name, rewritten);
            _changed = true;
        }
    }

    private string Transliterate(string text)
    {
        var transliterated = _transliterator.Transliterate(text);
        if (transliterated != text)
        {
            _changed = true;
        }

        return transliterated;
    }

    private string MoveToDerivedLocale(string address)
    {
        if (!LocalUrl.IsLocalPath(address))
        {
            return address;
        }

        var pathEnd = address.IndexOfAny(['?', '#']);
        var path = pathEnd < 0 ? address : address[..pathEnd];
        var firstSegmentEnd = path.IndexOf('/', 1);
        var firstSegment = firstSegmentEnd < 0 ? path[1..] : path[1..firstSegmentEnd];
        var lastSegment = path[(path.LastIndexOf('/') + 1)..];

        if (!firstSegment.Equals(_fromLocale, StringComparison.OrdinalIgnoreCase) || Path.HasExtension(lastSegment))
        {
            return address;
        }

        return "/" + _toLocale + address[(1 + firstSegment.Length)..];
    }
}
