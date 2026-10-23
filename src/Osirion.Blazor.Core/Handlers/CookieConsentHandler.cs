using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Osirion.Blazor.Core.Utils;
using System.Text.Json;

namespace Osirion.Blazor.Core.Handlers;

/// <summary>
/// Handles cookie consent form submissions for SSR
/// </summary>
public static class CookieConsentHandler
{
    private const string ConsentCookieName = "osirion_cookie_consent";
    private const string ConsentCookieVersion = "1.0";
    private const string CustomizeQueryKey = "customize-cookies";
    private const int DefaultExpiryDays = 365;
    private const int MaxExpiryDays = 3650;

    /// <summary>
    /// Processes cookie consent form submission
    /// </summary>
    /// <remarks>
    /// The request must be a form post with a valid antiforgery token; otherwise the handler returns
    /// 400 without a body and records nothing. The posted <c>returnUrl</c> is followed only when it
    /// is a local path (see <see cref="LocalUrl.IsLocalPath(string?)"/>); any other value redirects
    /// to <c>/</c>. A <c>customize-cookies</c> query key left on the return URL is removed. The cookie lives for
    /// the posted <c>expiryDays</c> (1 to 3650), or 365 days when it is missing or out of range.
    /// </remarks>
    public static async Task<IResult> HandleConsentAsync(HttpContext context)
    {
        if (!await FormPostSecurity.IsAntiforgeryTokenValidAsync(context) || !context.Request.HasFormContentType)
        {
            return Results.BadRequest();
        }

        var form = await context.Request.ReadFormAsync(context.RequestAborted);
        var consentValue = form["consent"].FirstOrDefault();
        var returnUrl = GetReturnPath(form["returnUrl"].FirstOrDefault());

        var consentData = new CookieConsentData
        {
            Version = ConsentCookieVersion,
            ConsentDate = DateTime.UtcNow,
            ConsentType = consentValue ?? "declined"
        };

        // Handle different consent types
        switch (consentValue)
        {
            case "accepted":
                consentData.Categories = new Dictionary<string, bool>
                {
                    ["necessary"] = true,
                    ["analytics"] = true,
                    ["marketing"] = true,
                    ["preferences"] = true
                };
                break;

            case "declined":
                consentData.Categories = new Dictionary<string, bool>
                {
                    ["necessary"] = true,
                    ["analytics"] = false,
                    ["marketing"] = false,
                    ["preferences"] = false
                };
                break;

            case "customize":
                // Redirect to the same URL with customize parameter
                return Results.Redirect(QueryHelpers.AddQueryString(returnUrl, CustomizeQueryKey, "true"));

            case "save-preferences":
                // Process custom preferences
                consentData.Categories = new Dictionary<string, bool>
                {
                    ["necessary"] = true // Always true
                };

                foreach (var key in form.Keys.Where(k => k.StartsWith("category_")))
                {
                    var categoryId = key.Replace("category_", "");
                    consentData.Categories[categoryId] = form[key] == "true";
                }
                break;

            default:
                return Results.BadRequest("Invalid consent value");
        }

        // Set the consent cookie
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(GetExpiryDays(form["expiryDays"].FirstOrDefault())),
            Path = "/"
        };

        var consentJson = JsonSerializer.Serialize(consentData);
        context.Response.Cookies.Append(ConsentCookieName, consentJson, cookieOptions);

        // Redirect back to the original page
        return Results.Redirect(returnUrl);
    }

    /// <summary>
    /// Reads the lifetime the banner posted (its ConsentExpiryDays). The visitor can only change the lifetime of their
    /// own cookie, so a bounded value is accepted and anything else falls back to the default.
    /// </summary>
    private static int GetExpiryDays(string? posted) =>
        int.TryParse(posted, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var days)
            ? GetExpiryDays(days)
            : DefaultExpiryDays;

    /// <summary>
    /// Gets the lifetime of the consent cookie for a requested <paramref name="days"/>: 1 to 3650 days are kept, anything
    /// else becomes 365. The banner applies it too when consent is recorded in the browser.
    /// </summary>
    internal static int GetExpiryDays(int days) => days is >= 1 and <= MaxExpiryDays ? days : DefaultExpiryDays;

    private static string GetReturnPath(string? returnUrl) =>
        RemoveCustomizeFlag(FormPostSecurity.GetReturnPath(returnUrl));

    private static string RemoveCustomizeFlag(string localPath)
    {
        var fragmentStart = localPath.IndexOf('#');
        var pathAndQuery = fragmentStart < 0 ? localPath : localPath[..fragmentStart];
        var fragment = fragmentStart < 0 ? string.Empty : localPath[fragmentStart..];

        var queryStart = pathAndQuery.IndexOf('?');
        if (queryStart < 0)
        {
            return localPath;
        }

        var parameters = QueryHelpers.ParseQuery(pathAndQuery[queryStart..]);
        if (!parameters.Remove(CustomizeQueryKey))
        {
            return localPath;
        }

        return QueryHelpers.AddQueryString(pathAndQuery[..queryStart], parameters) + fragment;
    }

    /// <summary>
    /// Gets the current cookie consent status
    /// </summary>
    public static CookieConsentData? GetConsentData(HttpContext context)
    {
        var consentCookie = context.Request.Cookies[ConsentCookieName];
        if (string.IsNullOrWhiteSpace(consentCookie))
            return null;

        try
        {
            return JsonSerializer.Deserialize<CookieConsentData>(consentCookie);
        }
        catch
        {
            // Invalid cookie data
            return null;
        }
    }

    /// <summary>
    /// Checks if a specific cookie category is consented to
    /// </summary>
    public static bool IsCategoryConsented(HttpContext context, string category)
    {
        var consentData = GetConsentData(context);
        if (consentData is null)
            return false;

        return consentData.Categories is { } categories && categories.TryGetValue(category, out var consented) && consented;
    }

    /// <summary>
    /// Clears the consent cookie
    /// </summary>
    public static void ClearConsent(HttpContext context)
    {
        context.Response.Cookies.Delete(ConsentCookieName);
    }
}

/// <summary>
/// Cookie consent data model
/// </summary>
public class CookieConsentData
{
    /// <summary>Gets or sets the consent data version.</summary>
    public string Version { get; set; } = string.Empty;
    /// <summary>Gets or sets when consent was recorded.</summary>
    public DateTime ConsentDate { get; set; }
    /// <summary>Gets or sets the consent type.</summary>
    public string ConsentType { get; set; } = string.Empty;
    /// <summary>Gets or sets the consent state for each category.</summary>
    public Dictionary<string, bool> Categories { get; set; } = new();
}
