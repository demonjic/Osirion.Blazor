namespace Osirion.Blazor.Options;

/// <summary>Where <see cref="OsirionSiteOptions.LanguagePicker"/> shows the language picker.</summary>
[Flags]
public enum SiteLanguagePicker
{
    /// <summary>No language picker, the default.</summary>
    None = 0,

    /// <summary>A compact disclosure among the menu's actions, after the theme toggle.</summary>
    Header = 1,

    /// <summary>A row of links below the footer's bottom row.</summary>
    Footer = 2,

    /// <summary>Both in the header and in the footer.</summary>
    Both = Header | Footer,
}
