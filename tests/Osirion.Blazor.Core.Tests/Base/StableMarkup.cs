using System.Text.RegularExpressions;

namespace Osirion.Blazor.Core.Tests.Base;

/// <summary>
/// The rendered markup without what changes from one build to the next (the scoped CSS attribute) and without the white
/// space between tags, so a guard test can pin a component's markup exactly.
/// </summary>
internal static partial class StableMarkup
{
    public static string Of(string markup)
    {
        var withoutScope = ScopeAttribute().Replace(markup, string.Empty);
        return WhiteSpaceBetweenTags().Replace(withoutScope, "><").Trim();
    }

    [GeneratedRegex(" b-[a-z0-9]{10}")]
    private static partial Regex ScopeAttribute();

    [GeneratedRegex(@">\s+<")]
    private static partial Regex WhiteSpaceBetweenTags();
}
