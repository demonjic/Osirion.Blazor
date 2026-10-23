namespace Osirion.Blazor.Analytics;

/// <summary>
/// Render-time information passed to a provider when it builds its tracker markup.
/// </summary>
public sealed class AnalyticsScriptContext
{
    /// <summary>Gets the context used when no consent gate is configured.</summary>
    public static AnalyticsScriptContext Default { get; } = new();

    /// <summary>
    /// Gets whether the consent gate is on. When it is, the markup only runs after the visitor consented: on the server
    /// it is built only then, and in the browser (<c>ConsentEvaluation.Browser</c>) its scripts are held until then. Providers
    /// with a consent API (Google Consent Mode) declare it explicitly.
    /// </summary>
    public bool ConsentGateActive { get; init; }
}
