using System.Text.RegularExpressions;
using Shouldly;

namespace Osirion.Blazor.Navigation.Tests.Components;

/// <summary>Checks the collapsed bar rules of Menu.razor.css: the toggle, the drawer and the pinned actions.</summary>
public partial class MenuCollapsedBarStylesheetTests
{
    public static TheoryData<string, string> Breakpoints => new()
    {
        { "@media(max-width:768px){", ".osirion-menu-collapsible" },
        { "@media(max-width:1199.98px){", ":is(.osirion-menu-collapse-large,.osirion-menu-collapse-extra-large)" },
        { "@media(max-width:1439.98px){", ".osirion-menu-collapse-extra-large" },
    };

    // Fails before the fix: the visually hidden checkbox stayed in the tab order above the breakpoint, where the toggle
    // it stands for is not drawn, so a keyboard user met a focus stop with nothing on screen.
    [Fact]
    public void Toggler_AboveTheBreakpoint_IsNotAFocusStop()
    {
        // Act
        var css = Stylesheet();

        // Assert
        Regex.Match(css, @"(?<![\w-])\.osirion-navbar-toggler\{(?<body>[^}]*)\}").Groups["body"].Value
            .ShouldStartWith("display:none;");
    }

    [Theory]
    [MemberData(nameof(Breakpoints))]
    public void Toggler_BelowTheBreakpoint_IsAFocusStop(string media, string prefix)
    {
        // Act
        var block = MediaBlock(media);

        // Assert
        block.ShouldContain($"{prefix}.osirion-navbar-toggler{{display:block;}}");
    }

    // Fails before the fix: the drawer opened from a checkbox placed before the nav (":checked ~ .osirion-menu-nav"),
    // which put the toggle ahead of the brand in keyboard order. The checkbox now sits in the nav, after the brand.
    [Theory]
    [MemberData(nameof(Breakpoints))]
    public void Drawer_OpensFromTheCheckboxBesideIt(string media, string prefix)
    {
        // Act
        var block = MediaBlock(media);

        // Assert
        block.ShouldContain($"{prefix}.osirion-navbar-toggler:checked~.osirion-menu{{display:flex;");
        block.ShouldContain($"{prefix}.osirion-navbar-toggler:checked~.osirion-menu-backdrop{{display:block;}}");
        block.ShouldNotContain("~.osirion-menu-nav");
    }

    // Fails before the fix: there were no pinned actions; everything after the items moved into the drawer.
    [Theory]
    [MemberData(nameof(Breakpoints))]
    public void PinnedActions_StayInTheBarBesideTheToggle(string media, string prefix)
    {
        // Act
        var block = MediaBlock(media);

        // Assert
        block.ShouldContain($"{prefix}.osirion-menu-pinned-actions{{margin-inline-start:auto;");
        block.ShouldContain($"{prefix}.osirion-nav-branding{{flex-shrink:1;");
    }

    // Fails before the fix: the theme toggle padded itself by 10px in the drawer, so it sat inset from the menu items.
    [Theory]
    [MemberData(nameof(Breakpoints))]
    public void DrawerActions_TurnOffTheThemeTogglePadding(string media, string prefix)
    {
        // Act
        var block = MediaBlock(media);

        // Assert
        Regex.Match(block, Regex.Escape(prefix + ".osirion-menu-actions{") + "(?<body>[^}]*)\\}").Groups["body"].Value
            .ShouldContain("--osirion-theme-toggle-padding:0;");
    }

    private static string MediaBlock(string media)
    {
        var css = Stylesheet();
        var start = css.IndexOf(media, StringComparison.Ordinal);
        start.ShouldBeGreaterThanOrEqualTo(0);
        var end = css.IndexOf("@media", start + media.Length, StringComparison.Ordinal);
        return end < 0 ? css[start..] : css[start..end];
    }

    private static string Stylesheet() => Squash(File.ReadAllText(MenuBrandLogoTests.StylesheetPath("Menu.razor.css")));

    private static string Squash(string css) => WhitespaceRun().Replace(CssComment().Replace(css, string.Empty), string.Empty);

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex CssComment();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRun();
}
