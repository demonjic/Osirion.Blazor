using System.Net;
using System.Text;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Osirion.Blazor.Components;

namespace Osirion.Blazor.Core.Utils;

/// <summary>
/// Holds scripts back until the visitor consents in the browser (<see cref="ConsentEvaluation.Browser"/>). A held
/// script is written with <c>type="text/plain"</c>, so the browser does not run it, and the consent script
/// (<c>osirion-consent.js</c>, also loaded by <c>OsirionCookieConsent</c>) runs it once the consent cookie includes its
/// category.
/// </summary>
public static class ConsentScripts
{
    /// <summary>The consent script module, relative to the document's base address.</summary>
    internal const string ModulePath = "./_content/Osirion.Blazor.Core/js/osirion-consent.js";

    // The contract with osirion-consent.js: the category a held script waits for, and its own type, restored when it runs.
    private const string CategoryAttribute = "data-osirion-consent";
    private const string TypeAttribute = "data-osirion-type";

    private const string ModuleTag = "<script type=\"module\" src=\"" + ModulePath + "\"></script>";

    /// <summary>
    /// Returns the script elements of <paramref name="markup"/> held back until the visitor's consent includes
    /// <paramref name="category"/>, followed by the module script that runs them.
    /// </summary>
    /// <remarks>
    /// The markup is parsed as a browser with script enabled would parse it, so text that only looks like a script (in a
    /// comment, an attribute or a <c>noscript</c> element) is never taken for one. Each script keeps its text and every
    /// attribute except <c>type</c>, which moves to <c>data-osirion-type</c> and is restored when the script runs.
    /// Everything that is not a script, such as a <c>noscript</c> pixel or a preconnect <c>link</c>, is left out,
    /// because it would reach the vendor before the visitor chose. Returns an empty string when the markup has no script.
    /// </remarks>
    /// <param name="markup">The tracker markup, as a provider builds it.</param>
    /// <param name="category">The consent category the scripts wait for, such as <c>analytics</c>.</param>
    /// <returns>The held scripts and the consent module, or an empty string.</returns>
    public static string HoldUntilConsent(string markup, string category)
    {
        ArgumentNullException.ThrowIfNull(markup);
        ArgumentException.ThrowIfNullOrWhiteSpace(category);

        var scripts = FindScripts(markup);
        if (scripts.Count == 0)
        {
            return string.Empty;
        }

        var held = new StringBuilder();
        foreach (var script in scripts)
        {
            AppendHeld(held, script, category);
            held.Append('\n');
        }

        return held.Append(ModuleTag).ToString();
    }

    private static List<IElement> FindScripts(string markup)
    {
        if (!markup.Contains("<script", StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        var parser = new HtmlParser(new HtmlParserOptions { IsScripting = true });
        var nodes = parser.ParseFragment(markup, parser.ParseDocument(string.Empty).Body!);
        return nodes.OfType<IElement>()
            .SelectMany(element => element.QuerySelectorAll("script").Prepend(element))
            .Where(element => element.LocalName == "script")
            .ToList();
    }

    private static void AppendHeld(StringBuilder held, IElement script, string category)
    {
        held.Append("<script type=\"text/plain\" ").Append(CategoryAttribute).Append("=\"").Append(Encode(category)).Append('"');
        if (script.GetAttribute("type") is { } type)
        {
            held.Append(' ').Append(TypeAttribute).Append("=\"").Append(Encode(type)).Append('"');
        }

        foreach (var attribute in script.Attributes)
        {
            if (attribute.Name is "type" or CategoryAttribute or TypeAttribute)
            {
                continue;
            }

            held.Append(' ').Append(attribute.Name);
            if (attribute.Value.Length > 0)
            {
                held.Append("=\"").Append(Encode(attribute.Value)).Append('"');
            }
        }

        // The text is written back as the parser read it: script content is not decoded, so it reads back the same.
        held.Append('>').Append(script.TextContent).Append("</script>");
    }

    private static string Encode(string value) => WebUtility.HtmlEncode(value);
}
