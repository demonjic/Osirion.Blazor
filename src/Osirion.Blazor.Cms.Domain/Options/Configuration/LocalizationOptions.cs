namespace Osirion.Blazor.Cms.Domain.Options.Configuration;

/// <summary>
/// Configuration options for localization
/// </summary>
public class LocalizationOptions
{
    private const string DefaultSupportedCulture = "en-US";

    private List<string> _supportedCultures = new();
    private bool _supportedCulturesAssigned;

    /// <summary>
    /// Gets or sets the supported cultures. Empty until configured, so a configured list (for example
    /// <c>["en-US", "sr-Latn-RS"]</c> in appsettings) is exactly the list, not appended to a default. Read the list to
    /// use with <see cref="GetEffectiveSupportedCultures"/>.
    /// </summary>
    public List<string> SupportedCultures
    {
        get => _supportedCultures;
        set
        {
            _supportedCultures = value ?? new List<string>();
            _supportedCulturesAssigned = true;
        }
    }

    /// <summary>
    /// Gets the cultures to use: <see cref="SupportedCultures"/>, or <c>en-US</c> when none was configured. A list
    /// assigned as empty stays empty.
    /// </summary>
    /// <returns>The cultures to use.</returns>
    public List<string> GetEffectiveSupportedCultures() =>
        _supportedCulturesAssigned || _supportedCultures.Count > 0
            ? _supportedCultures
            : new List<string> { DefaultSupportedCulture };

    /// <summary>
    /// Gets or sets the default culture
    /// </summary>
    public string DefaultCulture { get; set; } = DefaultSupportedCulture;

    /// <summary>
    /// Gets or sets whether to enable culture fallback
    /// </summary>
    public bool EnableCultureFallback { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to show the culture selector
    /// </summary>
    public bool ShowCultureSelector { get; set; } = true;

    /// <summary>
    /// Gets or sets the culture display format (e.g., "NativeName", "EnglishName", "Name")
    /// </summary>
    public string CultureDisplayFormat { get; set; } = "NativeName";

    /// <summary>
    /// Gets or sets whether to require culture prefix in URLs
    /// </summary>
    public bool RequireCulturePrefix { get; set; } = false;

    /// <summary>
    /// Adds supported cultures to the effective list, so with nothing configured they are added to <c>en-US</c>
    /// </summary>
    /// <param name="cultures">The cultures to add</param>
    public void AddSupportedCultures(params string[] cultures)
    {
        StartFromEffectiveCultures();
        foreach (var culture in cultures)
        {
            if (!SupportedCultures.Contains(culture))
            {
                SupportedCultures.Add(culture);
            }
        }
    }

    /// <summary>
    /// Sets the default culture
    /// </summary>
    /// <param name="culture">The default culture</param>
    public void SetDefaultCulture(string culture)
    {
        DefaultCulture = culture;
        StartFromEffectiveCultures();

        // Ensure the default culture is in the supported cultures list
        if (!SupportedCultures.Contains(culture))
        {
            SupportedCultures.Add(culture);
        }
    }

    // The code-based methods keep their old result: added to the en-US default when nothing was configured.
    private void StartFromEffectiveCultures()
    {
        if (!_supportedCulturesAssigned && _supportedCultures.Count == 0)
        {
            _supportedCultures.Add(DefaultSupportedCulture);
        }
    }
}
