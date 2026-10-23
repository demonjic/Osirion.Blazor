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
}
