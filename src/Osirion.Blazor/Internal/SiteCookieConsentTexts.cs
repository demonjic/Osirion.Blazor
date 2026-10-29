using Osirion.Blazor.Components;
using Osirion.Blazor.Options;

namespace Osirion.Blazor.Internal;

/// <summary>
/// Turns a version's <see cref="SiteCookieConsentLabels"/> into the <see cref="OsirionCookieConsent"/> parameters it
/// sets, so a text the version leaves out keeps the banner's default.
/// </summary>
internal static class SiteCookieConsentTexts
{
    public static Dictionary<string, object>? For(SiteCookieConsentLabels? labels)
    {
        if (labels is null)
        {
            return null;
        }

        var parameters = new Dictionary<string, object>(StringComparer.Ordinal);
        Add(parameters, nameof(OsirionCookieConsent.Title), labels.Title);
        Add(parameters, nameof(OsirionCookieConsent.Message), labels.Message);
        Add(parameters, nameof(OsirionCookieConsent.AcceptButtonText), labels.AcceptButton);
        Add(parameters, nameof(OsirionCookieConsent.DeclineButtonText), labels.DeclineButton);
        Add(parameters, nameof(OsirionCookieConsent.CustomizeButtonText), labels.CustomizeButton);
        Add(parameters, nameof(OsirionCookieConsent.SavePreferencesButtonText), labels.SavePreferencesButton);
        Add(parameters, nameof(OsirionCookieConsent.CustomizePanelTitle), labels.CustomizePanelTitle);
        Add(parameters, nameof(OsirionCookieConsent.PolicyLinkText), labels.PolicyLinkText);
        Add(parameters, nameof(OsirionCookieConsent.RequiredLabel), labels.Required);

        if (labels.Categories.Count > 0)
        {
            parameters[nameof(OsirionCookieConsent.Categories)] = Categories(labels.Categories);
        }

        return parameters.Count > 0 ? parameters : null;
    }

    // The default categories, with the version's names and descriptions, so the IDs the consent records stay the same.
    private static List<CookieCategory> Categories(Dictionary<string, SiteCookieCategoryLabels> labels)
    {
        var categories = OsirionCookieConsent.CreateDefaultCategories().ToList();
        foreach (var category in categories)
        {
            if (labels.TryGetValue(category.Id, out var text) && text is not null)
            {
                category.Name = string.IsNullOrWhiteSpace(text.Name) ? category.Name : text.Name;
                category.Description = string.IsNullOrWhiteSpace(text.Description) ? category.Description : text.Description;
            }
        }

        return categories;
    }

    private static void Add(Dictionary<string, object> parameters, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            parameters[name] = value;
        }
    }
}
