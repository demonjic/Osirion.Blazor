using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Utils;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Utils;

// Guards the classes OsirionComponentBase.GetPrimaryButtonClass already returns, now that they are public for markup
// rendered outside the base component.
public class ButtonClassesTests
{
    [Theory]
    [InlineData(CssFramework.None, "osirion-button osirion-button-primary")]
    [InlineData(CssFramework.Bootstrap, "btn btn-primary")]
    [InlineData(CssFramework.FluentUI, "osirion-button osirion-button-primary")]
    [InlineData(CssFramework.MudBlazor, "mud-button mud-button-filled mud-button-filled-primary")]
    [InlineData(CssFramework.Radzen, "rz-button rz-button-primary")]
    public void Primary_EachFramework_ReturnsItsPrimaryButtonClasses(CssFramework framework, string expected)
    {
        ButtonClasses.Primary(framework).ShouldBe(expected);
    }

    // Guards the classes OsirionComponentBase.GetSecondaryButtonClass returned before it read them from here, so every
    // section's secondary button keeps its look.
    [Theory]
    [InlineData(CssFramework.None, "osirion-button osirion-button-secondary")]
    [InlineData(CssFramework.Bootstrap, "btn btn-secondary")]
    [InlineData(CssFramework.FluentUI, "osirion-button osirion-button-secondary")]
    [InlineData(CssFramework.MudBlazor, "mud-button mud-button-filled mud-button-filled-secondary")]
    [InlineData(CssFramework.Radzen, "rz-button rz-button-secondary")]
    public void Secondary_EachFramework_ReturnsItsSecondaryButtonClasses(CssFramework framework, string expected)
    {
        ButtonClasses.Secondary(framework).ShouldBe(expected);
    }

    [Fact]
    public void Secondary_UnknownFramework_ReturnsTheNeutralOsirionClasses()
    {
        ButtonClasses.Secondary((CssFramework)99).ShouldBe("osirion-button osirion-button-secondary");
    }
}
