using Bunit;
using Microsoft.AspNetCore.Components;
using Osirion.Blazor.Navigation.Components;
using Shouldly;

namespace Osirion.Blazor.Navigation.Tests.Components;

/// <summary><see cref="MenuItem.ToggleSubmenuLabel"/>: the submenu toggle's name in the page's language.</summary>
public class MenuItemToggleLabelTests : BunitContext
{
    public MenuItemToggleLabelTests()
    {
        SetRendererInfo(new RendererInfo("Static", false));
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    // Guards the default: the English name every page had.
    [Fact]
    public void Toggle_WithoutALabel_KeepsTheEnglishName()
    {
        var cut = Render<MenuItem>(p => p.Add(c => c.Text, "Products").Add(c => c.HasSubmenu, true));

        cut.Find("input.osirion-submenu-toggler").GetAttribute("aria-label").ShouldBe("Toggle Products submenu");
    }

    [Fact]
    public void Toggle_WithALabel_PutsTheTextWhereThePlaceholderIs()
    {
        var cut = Render<MenuItem>(p => p
            .Add(c => c.Text, "Производи")
            .Add(c => c.HasSubmenu, true)
            .Add(c => c.ToggleSubmenuLabel, "Отвори подмени {0}"));

        cut.Find("input.osirion-submenu-toggler").GetAttribute("aria-label").ShouldBe("Отвори подмени Производи");
    }

    [Fact]
    public void Toggle_LabelWithOtherBraces_IsWrittenWithoutFormatting()
    {
        var cut = Render<MenuItem>(p => p
            .Add(c => c.Text, "Docs")
            .Add(c => c.HasSubmenu, true)
            .Add(c => c.ToggleSubmenuLabel, "{0} {menu}"));

        cut.Find("input.osirion-submenu-toggler").GetAttribute("aria-label").ShouldBe("Docs {menu}");
    }
}
