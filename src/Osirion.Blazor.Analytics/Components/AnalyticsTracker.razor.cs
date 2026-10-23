using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Osirion.Blazor.Analytics.Options;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Handlers;
using Osirion.Blazor.Core.Utils;

namespace Osirion.Blazor.Analytics.Components;

/// <summary>
/// Base component for analytics trackers
/// </summary>
public abstract class AnalyticsTracker : OsirionComponentBase
{
    /// <summary>
    /// Gets the analytics provider
    /// </summary>
    protected abstract IAnalyticsProvider? Provider { get; }

    /// <summary>
    /// Gets or sets the site ID for this render, replacing the configured <c>SiteId</c>. <c>ClarityTracker</c> (the project
    /// ID), <c>MatomoTracker</c> (the site ID) and <c>UmamiTracker</c> (the website ID, a GUID) use it. <c>GA4Tracker</c>,
    /// <c>YandexMetricaTracker</c> and <c>PlausibleTracker</c> ignore it: they take their own ID parameter or none.
    /// </summary>
    [Parameter]
    public string? SiteId { get; set; }

    /// <summary>
    /// Gets or sets whether the tracker renders, replacing the configured <c>Enabled</c> for this render. <c>false</c> turns
    /// the tracker off. <c>true</c> does not skip the other requirements: a valid ID, a registered service and, with the
    /// consent gate on, the visitor consent.
    /// </summary>
    [Parameter]
    public bool? Enabled { get; set; }

    [CascadingParameter]
    private HttpContext? CascadedHttpContext { get; set; }

    [Inject]
    private IServiceProvider TrackerServices { get; set; } = default!;

    /// <summary>Gets the registered <typeparamref name="TService"/>, or <see langword="null"/> when it is not registered.</summary>
    protected TService? GetOptionalService<TService>() where TService : class => TrackerServices?.GetService<TService>();

    /// <summary>
    /// Gets whether the provider can render with this tracker's parameter overrides applied.
    /// </summary>
    protected virtual bool CanRenderProvider => Enabled != false && (Provider?.ShouldRender ?? false);

    /// <summary>
    /// Builds the tracker markup with this tracker's parameter overrides applied.
    /// </summary>
    protected virtual string BuildScript(AnalyticsScriptContext context) =>
        Provider?.GetScript(context) ?? string.Empty;

    /// <summary>
    /// Gets whether the tracker should emit its markup: the provider can render and, when the
    /// consent gate is on, the visitor consented to analytics. With the gate evaluated in the browser the
    /// markup is emitted for every visitor, held until the browser finds the consent.
    /// </summary>
    protected bool ShouldRenderTracker => CanRenderProvider && IsConsentSatisfied();

    /// <summary>
    /// Gets the markup to emit. Every value in it is encoded by the provider. With the consent gate evaluated in the
    /// browser, its scripts are held (<c>type="text/plain"</c>) for the consent category and followed by the consent
    /// script that runs them.
    /// </summary>
    protected MarkupString TrackerMarkup => new(BuildTrackerMarkup());

    /// <summary>
    /// Determines whether the component should render
    /// </summary>
    protected override bool ShouldRender() => ShouldRenderTracker;

    private string BuildTrackerMarkup()
    {
        var consent = GetConsentOptions();
        var markup = BuildScript(CreateScriptContext(consent));
        return IsHeldForTheBrowser(consent) ? ConsentScripts.HoldUntilConsent(markup, consent.Category) : markup;
    }

    // In both modes the scripts run only after consent, so providers write their consented variant.
    private static AnalyticsScriptContext CreateScriptContext(AnalyticsConsentOptions consent) => consent.RequireConsent
        ? new AnalyticsScriptContext { ConsentGateActive = true }
        : AnalyticsScriptContext.Default;

    private static bool IsHeldForTheBrowser(AnalyticsConsentOptions consent) =>
        consent.RequireConsent && consent.Evaluation == ConsentEvaluation.Browser;

    private bool IsConsentSatisfied()
    {
        var consent = GetConsentOptions();
        if (!consent.RequireConsent)
        {
            return true;
        }

        // The page is the same for every visitor, so the browser decides; a blank category can never be consented to.
        if (IsHeldForTheBrowser(consent))
        {
            return !string.IsNullOrWhiteSpace(consent.Category);
        }

        // Fail closed: without a request there is no cookie to read.
        var httpContext = CascadedHttpContext ?? TrackerServices?.GetService<IHttpContextAccessor>()?.HttpContext;
        return httpContext is not null
            && CookieConsentHandler.IsCategoryConsented(httpContext, consent.Category);
    }

    private AnalyticsConsentOptions GetConsentOptions() =>
        TrackerServices?.GetService<IOptions<AnalyticsConsentOptions>>()?.Value ?? new AnalyticsConsentOptions();
}
