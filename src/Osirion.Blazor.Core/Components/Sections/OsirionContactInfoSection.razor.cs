using Microsoft.AspNetCore.Components;
using Osirion.Blazor.Core.Extensions;
using Osirion.Blazor.Core.Models;
using Osirion.Blazor.Core.Utils;

namespace Osirion.Blazor.Components;

/// <summary>
/// A dedicated contact information section component that displays contact details
/// such as address, phone, email, and website with consistent styling and accessibility.
/// </summary>
public partial class OsirionContactInfoSection(NavigationManager navigationManager) : OsirionComponentBase
{
    /// <summary>
    /// The heading, rendered as an <c>h2</c>; nothing is rendered when it is empty. Simple markup is allowed and
    /// passes through the registered HTML sanitizer.
    /// </summary>
    [Parameter]
    public string Title { get; set; } = "Contact information";

    /// <summary>
    /// The details to show; only the fields that are filled in are rendered. Required.
    /// </summary>
    [Parameter, EditorRequired]
    public ContactInformation? ContactInfo { get; set; }

    /// <summary>
    /// The label of the address.
    /// </summary>
    [Parameter]
    public string AddressLabel { get; set; } = "Address:";

    /// <summary>
    /// The label of the phone numbers.
    /// </summary>
    [Parameter]
    public string PhoneLabel { get; set; } = "Phone:";

    /// <summary>
    /// The label of the email addresses.
    /// </summary>
    [Parameter]
    public string EmailLabel { get; set; } = "Email:";

    /// <summary>
    /// The label of the websites.
    /// </summary>
    [Parameter]
    public string WebsiteLabel { get; set; } = "Website:";

    private IReadOnlyList<WebsiteLink> _websiteLinks = [];

    [Inject]
    private IServiceProvider ServiceProvider { get; set; } = default!;

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        _websiteLinks = Websites()
            .Select(CreateWebsiteLink)
            .OfType<WebsiteLink>()
            .ToList();
    }

    #region Helper Methods

    private IReadOnlyList<ContactInfoItem> AdditionalItems =>
        ContactInfo?.AdditionalItems is { Count: > 0 } items
            ? items.Where(item => item is not null && !string.IsNullOrWhiteSpace(item.Text)).ToList()
            : [];

    private static bool HasSafeHref(ContactInfoItem item) => SafeLink.IsSafe(item.Href);

    // Only an absolute web address leaves the site, so only it opens in a new tab.
    private static bool IsWebAddress(ContactInfoItem item) =>
        Uri.TryCreate(item.Href, UriKind.Absolute, out var uri) && IsWebAddress(uri);

    private static string? ItemTarget(ContactInfoItem item) => IsWebAddress(item) ? "_blank" : null;

    private static string? ItemRel(ContactInfoItem item) => IsWebAddress(item) ? "noopener noreferrer" : null;

    private static string? IconPath(ContactInfoIcon icon) => icon switch
    {
        ContactInfoIcon.Address => "M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7zm0 9.5c-1.38 0-2.5-1.12-2.5-2.5s1.12-2.5 2.5-2.5 2.5 1.12 2.5 2.5-1.12 2.5-2.5 2.5z",
        ContactInfoIcon.Phone => "M6.62 10.79c1.44 2.83 3.76 5.14 6.59 6.59l2.2-2.2c.27-.27.67-.36 1.02-.24 1.12.37 2.33.57 3.57.57.55 0 1 .45 1 1V20c0 .55-.45 1-1 1-9.39 0-17-7.61-17-17 0-.55.45-1 1-1h3.5c.55 0 1 .45 1 1 0 1.25.2 2.45.57 3.57.11.35.03.74-.25 1.02l-2.2 2.2z",
        ContactInfoIcon.Email => "M20 4H4c-1.1 0-1.99.9-1.99 2L2 18c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 4l-8 5-8-5V6l8 5 8-5v2z",
        ContactInfoIcon.Website => "M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-1 17.93c-3.94-.49-7-3.85-7-7.93 0-.62.08-1.21.21-1.79L9 15v1c0 1.1.9 2 2 2v1.93zm6.9-2.54c-.26-.81-1-1.39-1.9-1.39h-1v-3c0-.55-.45-1-1-1H8v-2h2c.55 0 1-.45 1-1V7h2c1.1 0 2-.9 2-2v-.41c2.93 1.19 5 4.06 5 7.41 0 2.08-.8 3.97-2.1 5.39z",
        ContactInfoIcon.Calendar => "M17 12h-5v5h5v-5zM16 1v2H8V1H6v2H5c-1.11 0-1.99.9-1.99 2L3 19c0 1.1.89 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2h-1V1h-2zm3 18H5V8h14v11z",
        ContactInfoIcon.Clock => "M11.99 2C6.47 2 2 6.48 2 12s4.47 10 9.99 10C17.52 22 22 17.52 22 12S17.52 2 11.99 2zM12 20c-4.42 0-8-3.58-8-8s3.58-8 8-8 8 3.58 8 8-3.58 8-8 8zm.5-13H11v6l5.25 3.15.75-1.23-4.5-2.67z",
        ContactInfoIcon.Link => "M3.9 12c0-1.71 1.39-3.1 3.1-3.1h4V7H7c-2.76 0-5 2.24-5 5s2.24 5 5 5h4v-1.9H7c-1.71 0-3.1-1.39-3.1-3.1zM8 13h8v-2H8v2zm9-6h-4v1.9h4c1.71 0 3.1 1.39 3.1 3.1s-1.39 3.1-3.1 3.1h-4V17h4c2.76 0 5-2.24 5-5s-2.24-5-5-5z",
        _ => null
    };

    /// <summary>
    /// A website entry that passed validation: only http and https addresses with a host become links.
    /// </summary>
    private sealed record WebsiteLink(string Href, string Target, string DisplayText);

    /// <summary>
    /// Writes text that may carry intentional markup (a paragraph or line break) through the registered sanitizer.
    /// </summary>
    private MarkupString Sanitized(string? html) => string.IsNullOrWhiteSpace(html)
        ? default
        : new MarkupString(ServiceProvider.GetHtmlContentSanitizer().Sanitize(html));

    private WebsiteLink? CreateWebsiteLink(string entry)
    {
        var href = entry;
        if (!Uri.TryCreate(entry, UriKind.Absolute, out var uri) || !IsWebAddress(uri))
        {
            // A value such as "example.org" or "example.org:8080" has no scheme. A value that already names another
            // scheme (javascript:, data:) does not parse as a host and is dropped.
            if (entry.Contains("//", StringComparison.Ordinal)
                || !Uri.TryCreate("https://" + entry, UriKind.Absolute, out uri)
                || !IsWebAddress(uri))
            {
                return null;
            }

            href = uri.AbsoluteUri;
        }

        if (IsCurrentSite(uri))
        {
            return new WebsiteLink(uri.PathAndQuery + uri.Fragment, "_self", uri.Authority);
        }

        return new WebsiteLink(href, "_blank", uri.Authority);
    }

    private static bool IsWebAddress(Uri uri) =>
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) && !string.IsNullOrEmpty(uri.Host);

    /// <summary>
    /// Compares the host, not a string prefix: "https://site.com.evil.net" starts with "https://site.com" but is another site.
    /// </summary>
    private bool IsCurrentSite(Uri uri) =>
        Uri.TryCreate(navigationManager.BaseUri, UriKind.Absolute, out var baseUri)
        && string.Equals(uri.Host, baseUri.Host, StringComparison.OrdinalIgnoreCase);

    private string GetContactInfoSectionClass()
    {
        var classes = new List<string> { "osirion-contact-info-section" };

        return CombineCssClasses(string.Join(" ", classes));
    }

    private string[] Phones()
    {
        if (string.IsNullOrWhiteSpace(ContactInfo?.Phone))
        {
            return Array.Empty<string>();
        }

        return ContactInfo.Phone.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private string[] Emails()
    {
        if (string.IsNullOrWhiteSpace(ContactInfo?.Email))
        {
            return Array.Empty<string>();
        }
        return ContactInfo.Email.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private string[] Websites()
    {
        if (string.IsNullOrWhiteSpace(ContactInfo?.Website))
        {
            return Array.Empty<string>();
        }
        return ContactInfo.Website.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    #endregion
}
