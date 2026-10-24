using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.Logging;
using Osirion.Blazor.Core.Models;
using Osirion.Blazor.Core.Utils;

namespace Osirion.Blazor.Components;

/// <summary>
/// A contact form for sites published as static files, where no ASP.NET server receives a form post. Its script posts
/// the message as JSON to an endpoint the site provides, such as a Cloudflare Worker, with a Cloudflare Turnstile token
/// and a honeypot field, and shows the endpoint's answer in a status region.
/// </summary>
/// <remarks>
/// The endpoint takes JSON, so the form cannot work without script: it renders with the <c>hidden</c> attribute and
/// its script reveals it. <see cref="NoScriptContent"/> tells visitors without script what else they can do.
/// </remarks>
public partial class OsirionStaticContactForm : OsirionComponentBase
{
    // The limits the reference endpoint checks, so the browser refuses what the endpoint would refuse. That endpoint
    // does not read the company field, which gets a limit of its own.
    internal const int NameMaxLength = 100;
    internal const int EmailMaxLength = 254;
    internal const int CompanyMaxLength = 200;
    internal const int SubjectMaxLength = 200;
    internal const int MessageMinLength = 10;
    internal const int MessageMaxLength = 5000;

    // Generated once per instance so the ids stay the same on every render.
    private readonly string _idPrefix = $"osirion-static-contact-form-{Guid.NewGuid():N}";

    /// <summary>
    /// Gets or sets the address the message is posted to as JSON: a path on the site, such as <c>/api/contact</c>, or an
    /// <c>https</c> URL. Any other value (empty, <c>http:</c>, <c>javascript:</c>, <c>data:</c>, <c>mailto:</c> or a
    /// protocol-relative <c>//host</c>) renders nothing and, in Development, logs a warning. Required.
    /// </summary>
    [Parameter, EditorRequired]
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the site key of the Cloudflare Turnstile widget. When it is set, the script loads Turnstile from
    /// <c>challenges.cloudflare.com</c>, renders the widget in the form with the action <c>contact</c> and sends its
    /// token. Without it nothing is loaded from Cloudflare and the token is sent empty, so leave it out only when the
    /// endpoint does not check Turnstile: one that does, such as the outline on the component's documentation page,
    /// answers <c>fields</c> to an empty token.
    /// </summary>
    [Parameter]
    public string? TurnstileSiteKey { get; set; }

    /// <summary>
    /// Gets or sets whether an optional company field is shown. Its value is sent as <c>company</c>.
    /// </summary>
    [Parameter]
    public bool ShowCompanyField { get; set; }

    /// <summary>
    /// Gets or sets whether an optional subject field is shown, between the email (or company) and the message. Its
    /// value is sent as <c>subject</c>, at most 200 characters; the field is not sent when it is hidden, so an endpoint
    /// that does not read a subject gets the same message as before.
    /// </summary>
    [Parameter]
    public bool ShowSubjectField { get; set; }

    /// <summary>
    /// Gets or sets the heading (<c>h2</c>) above the form. Empty by default, which renders nothing.
    /// </summary>
    [Parameter]
    public string FormTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the text under the heading. Plain text; it is encoded. Empty by default, which renders nothing.
    /// </summary>
    [Parameter]
    public string FormDescription { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the placeholder of the name field. Null by default, which writes no placeholder.
    /// </summary>
    [Parameter]
    public string? NamePlaceholder { get; set; }

    /// <summary>
    /// Gets or sets the placeholder of the email field. Null by default, which writes no placeholder.
    /// </summary>
    [Parameter]
    public string? EmailPlaceholder { get; set; }

    /// <summary>
    /// Gets or sets the placeholder of the subject field. Null by default, which writes no placeholder.
    /// </summary>
    [Parameter]
    public string? SubjectPlaceholder { get; set; }

    /// <summary>
    /// Gets or sets the placeholder of the message field. Null by default, which writes no placeholder.
    /// </summary>
    [Parameter]
    public string? MessagePlaceholder { get; set; }

    /// <summary>
    /// Gets or sets whether the contact information sits beside the form (<see langword="true"/>, the default) or below
    /// it. It only matters when the contact information is shown. Below 768px it is below the form either way.
    /// </summary>
    [Parameter]
    public bool SideBySideLayout { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the contact information section is shown when <see cref="ContactInfo"/> is set.
    /// </summary>
    [Parameter]
    public bool ShowContactInformation { get; set; } = true;

    /// <summary>
    /// Gets or sets the title of the contact information section, as on <c>OsirionContactForm</c>.
    /// </summary>
    [Parameter]
    public string ContactInfoTitle { get; set; } = "Contact information";

    /// <summary>
    /// Gets or sets the details shown in an <c>OsirionContactInfoSection</c> beside or below the form: address, phone,
    /// email, website, a message and extra rows (<see cref="ContactInformation.AdditionalItems"/>). When it is
    /// <see langword="null"/>, which is the default, no section is rendered.
    /// </summary>
    [Parameter]
    public ContactInformation? ContactInfo { get; set; }

    /// <summary>
    /// Gets or sets whether a <c>*</c> follows the label of each required field: name, email and message. The marker is
    /// for the eye only; assistive technology announces the fields' <c>required</c> attribute instead.
    /// </summary>
    [Parameter]
    public bool ShowRequiredIndicators { get; set; } = true;

    /// <summary>
    /// Gets or sets the label of the name field.
    /// </summary>
    [Parameter]
    public string NameLabel { get; set; } = "Name";

    /// <summary>
    /// Gets or sets the label of the email field.
    /// </summary>
    [Parameter]
    public string EmailLabel { get; set; } = "Email";

    /// <summary>
    /// Gets or sets the label of the company field, shown with <see cref="ShowCompanyField"/>.
    /// </summary>
    [Parameter]
    public string CompanyLabel { get; set; } = "Company";

    /// <summary>
    /// Gets or sets the label of the subject field, shown with <see cref="ShowSubjectField"/>.
    /// </summary>
    [Parameter]
    public string SubjectLabel { get; set; } = "Subject";

    /// <summary>
    /// Gets or sets the label of the message field.
    /// </summary>
    [Parameter]
    public string MessageLabel { get; set; } = "Message";

    /// <summary>
    /// Gets or sets the text of the submit button.
    /// </summary>
    [Parameter]
    public string SubmitButtonText { get; set; } = "Send message";

    /// <summary>
    /// Gets or sets the text of the submit button while the message is sent; the button is disabled meanwhile.
    /// </summary>
    [Parameter]
    public string SubmittingText { get; set; } = "Sending...";

    /// <summary>
    /// Gets or sets the message shown when the endpoint answers <c>{"ok": true}</c>. The fields are cleared.
    /// </summary>
    [Parameter]
    public string SuccessMessage { get; set; } = "Thank you. Your message was sent.";

    /// <summary>
    /// Gets or sets the message shown when the endpoint answers <c>{"ok": false, "error": "fields"}</c>: a field is
    /// missing, too long or malformed.
    /// </summary>
    [Parameter]
    public string FieldsErrorMessage { get; set; } =
        $"Check the fields: your name, a valid email address and a message of at least {MessageMinLength} characters.";

    /// <summary>
    /// Gets or sets the message shown when the endpoint answers <c>{"ok": false, "error": "check"}</c>, or when the
    /// Turnstile widget fails to load or reports an error.
    /// </summary>
    [Parameter]
    public string CheckErrorMessage { get; set; } = "The spam check did not pass. Please try again.";

    /// <summary>
    /// Gets or sets the message shown when the visitor sends before the Turnstile check has finished. Nothing is sent.
    /// </summary>
    [Parameter]
    public string CheckPendingMessage { get; set; } = "Wait for the spam check to finish, then send again.";

    /// <summary>
    /// Gets or sets the message shown for any other answer: <c>{"ok": false, "error": "failed"}</c>, an unknown error,
    /// a response that is not JSON, or no response at all.
    /// </summary>
    [Parameter]
    public string ErrorMessage { get; set; } = "The message could not be sent. Please try again later.";

    /// <summary>
    /// Gets or sets a short note under the form, such as where the message goes and who checks it for spam. Plain text;
    /// it is encoded. Nothing is rendered when it is empty.
    /// </summary>
    [Parameter]
    public string? PrivacyNote { get; set; }

    /// <summary>
    /// Gets or sets what visitors without script see instead of the form, rendered inside <c>noscript</c>, for example
    /// an email link. Nothing is rendered when it is not set.
    /// </summary>
    [Parameter]
    public RenderFragment? NoScriptContent { get; set; }

    private bool HasSafeEndpoint { get; set; }

    private bool HasContactInformationPanel => ShowContactInformation && ContactInfo is not null;

    private string? SiteKey => string.IsNullOrWhiteSpace(TurnstileSiteKey) ? null : TurnstileSiteKey.Trim();

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        HasSafeEndpoint = IsSafeEndpoint(Endpoint);
        if (!HasSafeEndpoint && GetDevelopmentLogger() is { } logger)
        {
            LogUnsafeEndpoint(logger, Endpoint);
        }
    }

    // The message carries a name and an email address, so it goes to the site itself or over https. SafeLink also rules
    // out script schemes and protocol-relative addresses, which would send it to another host.
    private static bool IsSafeEndpoint(string? endpoint) =>
        SafeLink.IsSafe(endpoint)
        && (LocalUrl.IsLocalPath(endpoint)
            || (Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps));

    private string FieldId(string fieldName) => $"{_idPrefix}-{fieldName}";

    private string GetContainerClass() => CombineCssClasses(
        HasContactInformationPanel
            ? $"osirion-static-contact-form {(SideBySideLayout ? "osirion-static-contact-form-side-by-side" : "osirion-static-contact-form-stacked")}"
            : "osirion-static-contact-form");

    // Built here rather than in the markup, where the compiler would give the noscript the CSS isolation attribute: no
    // scoped rule styles it, and the attribute would only show up in every exported page.
    private void RenderNoScript(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "noscript");
        builder.AddContent(1, NoScriptContent);
        builder.CloseElement();
    }

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "OsirionStaticContactForm renders nothing: its Endpoint '{Endpoint}' is not a path on the site or an https URL.")]
    private static partial void LogUnsafeEndpoint(ILogger logger, string endpoint);
}
