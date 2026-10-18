using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Osirion.Blazor.Core.Handlers;

namespace Osirion.Blazor.Components;

/// <summary>
/// Cookie consent banner component for GDPR compliance (SSR-compatible)
/// </summary>
public partial class OsirionCookieConsent
{
    private const string ConsentCookieName = "osirion_cookie_consent";
    private const string ConsentCookieVersion = "1.0";

    /// <summary>
    /// Gets or sets the banner heading (<c>h3</c>), which is also the accessible name of the banner region.
    /// </summary>
    [Parameter]
    public string Title { get; set; } = "Cookie Consent";

    /// <summary>
    /// Gets or sets the banner text, rendered as encoded text.
    /// </summary>
    [Parameter]
    public string Message { get; set; } = "This site would like to use optional cookies in addition to the ones it needs to work. Nothing optional is set until you choose.";

    /// <summary>
    /// Gets or sets the text of the button that accepts every category.
    /// </summary>
    [Parameter]
    public string AcceptButtonText { get; set; } = "Accept All";

    /// <summary>
    /// Gets or sets the text of the button that declines every optional category.
    /// </summary>
    [Parameter]
    public string DeclineButtonText { get; set; } = "Decline";

    /// <summary>
    /// Gets or sets the text of the button that opens the category choices.
    /// </summary>
    [Parameter]
    public string CustomizeButtonText { get; set; } = "Customize";

    /// <summary>
    /// Gets or sets the text of the button that saves the ticked categories.
    /// </summary>
    [Parameter]
    public string SavePreferencesButtonText { get; set; } = "Save Preferences";

    /// <summary>
    /// Gets or sets whether the decline button is shown.
    /// </summary>
    [Parameter]
    public bool ShowDeclineButton { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the customize button is shown.
    /// </summary>
    [Parameter]
    public bool ShowCustomizeButton { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the category checkboxes are shown once the visitor chose to customize.
    /// When it is <see langword="false"/>, the customize button reloads the page without any choices.
    /// </summary>
    [Parameter]
    public bool ShowCustomizationPanel { get; set; } = true;

    /// <summary>
    /// Gets or sets the heading (<c>h4</c>) of the category panel.
    /// </summary>
    [Parameter]
    public string CustomizePanelTitle { get; set; } = "Cookie Preferences";

    /// <summary>
    /// Gets or sets the address of your privacy or cookie policy, shown as a link after the message. Null or empty, the
    /// default, shows no link.
    /// </summary>
    [Parameter]
    public string? PolicyLink { get; set; }

    /// <summary>
    /// Gets or sets the text of the policy link.
    /// </summary>
    [Parameter]
    public string PolicyLinkText { get; set; } = "Learn more";

    /// <summary>
    /// Gets or sets the markup shown beside the text. Null, the default, shows no icon.
    /// </summary>
    [Parameter]
    public RenderFragment? Icon { get; set; }

    /// <summary>
    /// Gets or sets the categories shown in the panel. Defaults to <c>necessary</c> (required), <c>analytics</c>,
    /// <c>marketing</c> and <c>preferences</c>. Accept All records only those four IDs, so a custom ID is recorded
    /// only when the visitor ticks it in the panel.
    /// </summary>
    [Parameter]
    public IReadOnlyList<CookieCategory> Categories { get; set; } = GetDefaultCategories();

    /// <summary>
    /// Gets or sets where the form posts. Defaults to <c>/api/cookie-consent</c>, the pattern <c>MapOsirionCookieConsent</c>
    /// maps; change it only together with that call.
    /// </summary>
    [Parameter]
    public string ConsentEndpoint { get; set; } = "/api/cookie-consent";

    /// <summary>
    /// Gets or sets the banner position, added as the class <c>osirion-cookie-consent-{Position}</c>. The stylesheet defines
    /// <c>bottom</c> (the default) and <c>top</c>.
    /// </summary>
    [Parameter]
    public string Position { get; set; } = "bottom";

    /// <summary>
    /// Gets or sets the consent expiry in days. The value is posted with the form and applied to the consent cookie by
    /// the consent endpoint, which accepts 1 to 3650 days and uses 365 for anything else.
    /// </summary>
    [Parameter]
    public int ConsentExpiryDays { get; set; } = 365;

    /// <summary>
    /// Gets or sets whether the banner should be shown
    /// </summary>
    private bool ShowBanner { get; set; }

    private CookieConsentData? _savedConsent;

    // A reopened panel shows the visitor's saved choice for each category; a first visit shows the defaults.
    private bool IsChecked(CookieCategory category)
    {
        if (category.IsRequired)
        {
            return true;
        }

        return _savedConsent is not null && _savedConsent.Categories.TryGetValue(category.Id, out var consented)
            ? consented
            : category.IsEnabled;
    }

    /// <summary>
    /// Gets or sets whether the customization panel is shown
    /// </summary>
    private bool IsCustomizing { get; set; }

    [Inject]
    private IServiceProvider ServiceProvider { get; set; } = default!;

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();
        CheckConsentStatus();
    }

    /// <summary>
    /// Checks if consent has already been given
    /// </summary>
    /// <remarks>
    /// The banner reads the consent cookie from the request, so it exists only while an HTTP request is being rendered
    /// (static server rendering and prerendering). Without an HTTP context, for example in WebAssembly where no
    /// <see cref="IHttpContextAccessor"/> is registered or in an interactive server circuit, it stays hidden.
    /// </remarks>
    private void CheckConsentStatus()
    {
        var httpContext = ServiceProvider.GetService<IHttpContextAccessor>()?.HttpContext;
        if (httpContext is not null)
        {
            var consentCookie = httpContext.Request.Cookies[ConsentCookieName];
            IsCustomizing = httpContext.Request.Query.ContainsKey("customize-cookies");

            // A link with ?customize-cookies (a site's "cookie settings" link) reopens the panel after an answer, so
            // a visitor can change or withdraw consent as easily as they gave it.
            ShowBanner = string.IsNullOrWhiteSpace(consentCookie) || (IsCustomizing && ShowCustomizationPanel);
            _savedConsent = CookieConsentHandler.GetConsentData(httpContext);
        }
    }

    /// <summary>
    /// Gets the current path and query for the return URL field
    /// </summary>
    /// <remarks>
    /// The consent endpoint only redirects to local paths, so the absolute URL is never posted.
    /// </remarks>
    private string GetCurrentUrl()
    {
        return new Uri(Navigation.Uri, UriKind.Absolute).PathAndQuery;
    }

    /// <summary>
    /// Gets the CSS class for the cookie consent
    /// </summary>
    private string GetCookieConsentClass()
    {
        var classes = new List<string> { "osirion-cookie-consent" };

        classes.Add($"osirion-cookie-consent-{Position}");
        //classes.Add($"osirion-cookie-consent-theme-{Theme}");

        if (IsCustomizing)
        {
            classes.Add("osirion-cookie-consent-customizing");
        }

        if (!string.IsNullOrWhiteSpace(Class))
        {
            classes.Add(Class);
        }

        return string.Join(" ", classes);
    }

    /// <summary>
    /// Gets default cookie categories
    /// </summary>
    private static IReadOnlyList<CookieCategory> GetDefaultCategories()
    {
        return new[]
        {
            new CookieCategory
            {
                Id = "necessary",
                Name = "Necessary",
                Description = "These cookies are essential for the website to function properly.",
                IsRequired = true,
                IsEnabled = true
            },
            new CookieCategory
            {
                Id = "analytics",
                Name = "Analytics",
                Description = "These cookies help us understand how visitors interact with our website.",
                IsRequired = false,
                IsEnabled = false
            },
            new CookieCategory
            {
                Id = "marketing",
                Name = "Marketing",
                Description = "These cookies are used to show you relevant ads based on your interests.",
                IsRequired = false,
                IsEnabled = false
            },
            new CookieCategory
            {
                Id = "preferences",
                Name = "Preferences",
                Description = "These cookies remember your settings and preferences.",
                IsRequired = false,
                IsEnabled = false
            }
        };
    }
}

/// <summary>
/// Represents a cookie category
/// </summary>
public class CookieCategory
{
    /// <summary>
    /// Gets or sets the category ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this category is required
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets whether this category is enabled
    /// </summary>
    public bool IsEnabled { get; set; }
}
