using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Osirion.Blazor.Components;

public abstract partial class OsirionComponentBase
{
    // Attribute names a caller plausibly means as HTML: global attributes and the element attributes components forward
    // to their root element. A name in this set is never reported, even when it resembles a parameter.
    private static readonly FrozenSet<string> KnownHtmlAttributes = new[]
    {
        "accesskey", "anchor", "autocapitalize", "autocorrect", "autofocus", "class", "contenteditable", "dir",
        "draggable", "enterkeyhint", "exportparts", "hidden", "id", "inert", "inputmode", "is", "itemid", "itemprop",
        "itemref", "itemscope", "itemtype", "lang", "nonce", "part", "popover", "role", "slot", "spellcheck", "style",
        "tabindex", "title", "translate", "writingsuggestions", "xmlns",
        "accept", "action", "autocomplete", "checked", "cols", "dirname", "disabled", "enctype", "for", "form",
        "formaction", "formenctype", "formmethod", "formnovalidate", "formtarget", "label", "list", "max", "maxlength",
        "method", "min", "minlength", "multiple", "name", "novalidate", "pattern", "placeholder", "popovertarget",
        "popovertargetaction", "readonly", "required", "rows", "selected", "size", "step", "type", "value", "wrap",
        "download", "href", "hreflang", "ping", "referrerpolicy", "rel", "target",
        "allow", "allowfullscreen", "alt", "autoplay", "blocking", "controls", "crossorigin", "decoding", "fetchpriority",
        "height", "integrity", "loading", "loop", "media", "muted", "playsinline", "poster", "preload", "sandbox",
        "sizes", "src", "srcdoc", "srclang", "srcset", "width",
        "abbr", "cite", "colspan", "content", "datetime", "headers", "open", "reversed", "rowspan", "scope", "span",
        "start", "viewbox",
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    private const int MaximumSimilarityDistance = 2;

    // A shorter name matches too much by prefix or by edit distance ("a" is a prefix of every parameter starting with A).
    private const int MinimumComparableLength = 3;

    private static readonly ConcurrentDictionary<Type, string[]> ParameterNamesByType = new();

    private static readonly ConcurrentDictionary<(Type ComponentType, string AttributeName), bool> CheckedAttributes = new();

    [Inject]
    private IServiceProvider ComponentServices { get; set; } = default!;

    private bool _developmentLoggerResolved;
    private ILogger? _developmentLogger;

    /// <summary>
    /// Warns in Development when a captured attribute looks like a misspelled parameter, because Blazor passes it through
    /// as an HTML attribute and the parameter the caller meant silently keeps its default.
    /// </summary>
    private void WarnAboutAttributesThatResembleParameters()
    {
        if (Attributes is not { Count: > 0 } attributes || GetDevelopmentLogger() is not { } logger)
        {
            return;
        }

        var componentType = GetType();
        foreach (var attributeName in attributes.Keys)
        {
            // Each component type and name is examined once, which also makes the warning appear only once.
            if (!CheckedAttributes.TryAdd((componentType, attributeName), true) || !CouldBeParameterName(attributeName))
            {
                continue;
            }

            var parameterName = FindResemblingParameter(componentType, attributeName);
            if (parameterName is not null)
            {
                LogAttributeResemblesParameter(logger, componentType.Name, attributeName, parameterName);
            }
        }
    }

    /// <summary>
    /// Gets a logger named after the component when the app runs in Development, otherwise <see langword="null"/>, for
    /// warnings about misuse that only the developer can fix.
    /// </summary>
    /// <remarks>
    /// Resolved once per instance and only through optional lookups, so it costs nothing outside Development and works
    /// in hosts that register no <see cref="IHostEnvironment"/> (WebAssembly, tests).
    /// </remarks>
    private protected ILogger? GetDevelopmentLogger()
    {
        if (!_developmentLoggerResolved)
        {
            _developmentLoggerResolved = true;
            if (ComponentServices?.GetService<IHostEnvironment>() is { } environment && environment.IsDevelopment())
            {
                _developmentLogger = ComponentServices.GetService<ILoggerFactory>()?.CreateLogger(GetType());
            }
        }

        return _developmentLogger;
    }

    private static bool CouldBeParameterName(string attributeName) =>
        attributeName.Length > 0
        && !attributeName.Contains('-')
        && !attributeName.StartsWith('@')
        && !attributeName.StartsWith("on", StringComparison.OrdinalIgnoreCase)
        && !KnownHtmlAttributes.Contains(attributeName);

    private static string? FindResemblingParameter(Type componentType, string attributeName)
    {
        string? bestName = null;
        var bestScore = int.MaxValue;
        foreach (var parameterName in ParameterNamesByType.GetOrAdd(componentType, ReadParameterNames))
        {
            var score = ScoreResemblance(attributeName, parameterName);
            if (score < bestScore)
            {
                bestScore = score;
                bestName = parameterName;
            }
        }

        return bestName;
    }

    // Lower is closer; int.MaxValue means no resemblance. A prefix match ranks before an edit-distance match because a
    // truncated name ("Html" for "HtmlContent") is the commoner mistake; among prefixes the shorter parameter wins.
    private static int ScoreResemblance(string attributeName, string parameterName)
    {
        if (string.Equals(attributeName, parameterName, StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        if (attributeName.Length < MinimumComparableLength || parameterName.Length < MinimumComparableLength)
        {
            return int.MaxValue;
        }

        if (parameterName.StartsWith(attributeName, StringComparison.OrdinalIgnoreCase)
            || attributeName.StartsWith(parameterName, StringComparison.OrdinalIgnoreCase))
        {
            return 1 + Math.Abs(parameterName.Length - attributeName.Length);
        }

        var distance = LevenshteinDistance(attributeName, parameterName);
        return distance <= MaximumSimilarityDistance ? 1_000 + distance : int.MaxValue;
    }

    private static string[] ReadParameterNames(Type componentType) =>
        componentType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.GetCustomAttribute<ParameterAttribute>() is { CaptureUnmatchedValues: false }
                && property.GetCustomAttribute<ObsoleteAttribute>() is null)
            .Select(property => property.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static int LevenshteinDistance(string first, string second)
    {
        var previous = new int[second.Length + 1];
        var current = new int[second.Length + 1];
        for (var j = 0; j <= second.Length; j++)
        {
            previous[j] = j;
        }

        for (var i = 1; i <= first.Length; i++)
        {
            current[0] = i;
            for (var j = 1; j <= second.Length; j++)
            {
                var cost = char.ToUpperInvariant(first[i - 1]) == char.ToUpperInvariant(second[j - 1]) ? 0 : 1;
                current[j] = Math.Min(Math.Min(current[j - 1] + 1, previous[j] + 1), previous[j - 1] + cost);
            }

            (previous, current) = (current, previous);
        }

        return previous[second.Length];
    }

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "{Component} has no parameter '{Attribute}'; did you mean '{Parameter}'? The value was passed through as an HTML attribute.")]
    private static partial void LogAttributeResemblesParameter(ILogger logger, string component, string attribute, string parameter);
}
