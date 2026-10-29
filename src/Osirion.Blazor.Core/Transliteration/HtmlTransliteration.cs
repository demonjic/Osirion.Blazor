using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Osirion.Blazor.Core.Localization;

namespace Osirion.Blazor.Core.Transliteration;

/// <summary>
/// Writes HTML of one language version in the version derived from it by transliteration: its text is transliterated
/// and its links to the site's pages in the source culture move to the derived culture. Everything else is left as it
/// is. Derived content and a derived site version's copyright line use it.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Text nodes and the <c>alt</c>, <c>title</c>, <c>aria-label</c> and <c>placeholder</c> attributes are
/// transliterated.</item>
/// <item>Nothing inside <c>code</c>, <c>pre</c>, <c>kbd</c>, <c>samp</c>, <c>script</c>, <c>style</c> or an element with
/// <c>translate="no"</c> changes, and neither does a link's text that is its address (an autolink).</item>
/// <item>An <c>href</c> or <c>src</c> moves by the rule of <see cref="DerivedLocaleLinks.Move"/>: only a link to a page
/// of the site in the source culture moves; absolute, protocol-relative, <c>mailto:</c>, <c>tel:</c> and fragment links,
/// relative paths and links to files stay.</item>
/// </list>
/// HTML without a change is returned as it came; changed HTML is serialized from the parsed fragment.
/// </remarks>
public sealed class HtmlTransliteration
{
    private static readonly HashSet<string> UntouchedElements =
        new(["code", "pre", "kbd", "samp", "script", "style"], StringComparer.OrdinalIgnoreCase);

    private static readonly string[] TextAttributes = ["alt", "title", "aria-label", "placeholder"];

    private static readonly string[] AddressAttributes = ["href", "src"];

    private readonly ITextTransliterator _transliterator;
    private readonly string _fromCulture;
    private readonly string _toCulture;
    private readonly IOsirionLocalePaths? _localePaths;
    private bool _changed;

    private HtmlTransliteration(ITextTransliterator transliterator, string fromCulture, string toCulture, IOsirionLocalePaths? localePaths)
    {
        _transliterator = transliterator;
        _fromCulture = fromCulture;
        _toCulture = toCulture;
        _localePaths = localePaths;
    }

    /// <summary>Writes <paramref name="html"/> in the derived culture.</summary>
    /// <param name="html">The source version's HTML, a fragment such as an item's rendered content.</param>
    /// <param name="transliterator">Writes the text.</param>
    /// <param name="fromCulture">The source culture, for example <c>sr</c>.</param>
    /// <param name="toCulture">The derived culture, for example <c>sr-Latn</c>.</param>
    /// <param name="localePaths">The site's culture prefixes, when <c>Osirion:Localization</c> is registered.</param>
    /// <returns>The derived HTML; the same instance when nothing changes.</returns>
    public static string Transform(
        string html, ITextTransliterator transliterator, string fromCulture, string toCulture, IOsirionLocalePaths? localePaths = null)
    {
        ArgumentNullException.ThrowIfNull(transliterator);

        if (string.IsNullOrEmpty(html))
        {
            return html;
        }

        var parser = new HtmlParser();
        var context = parser.ParseDocument(string.Empty).Body!;
        var nodes = parser.ParseFragment(html, context);

        var transform = new HtmlTransliteration(transliterator, fromCulture, toCulture, localePaths);
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
            RewriteAttribute(element, name, address => DerivedLocaleLinks.Move(address, _fromCulture, _toCulture, _localePaths));
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
}
