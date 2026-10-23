using System.Collections.Frozen;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Osirion.Blazor.Core.Utils;

namespace Osirion.Blazor.Components;

public abstract partial class OsirionComponentBase : ComponentBase
{
    /// <summary>
    /// Gets or sets extra CSS classes for the component's root element.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets an inline style for the component's root element.
    /// </summary>
    [Parameter]
    public string? Style { get; set; }

    /// <summary>
    /// Gets the CSS class for a default button based on the current CSS framework.
    /// </summary>
    protected virtual string GetButtonClass()
    {
        switch (Framework)
        {
            case CssFramework.Bootstrap:
                return "btn btn-primary";
            case CssFramework.FluentUI:
                return "osirion-fluent-button";
            case CssFramework.MudBlazor:
                return "mud-button mud-button-filled";
            case CssFramework.Radzen:
                return "rz-button rz-button-primary";
            default:
                return "osirion-search-button";
        }
    }

    /// <summary>
    /// Gets the CSS class for a primary button
    /// </summary>
    protected virtual string GetPrimaryButtonClass() => ButtonClasses.Primary(Framework);

    /// <summary>
    /// Gets the CSS class for a secondary button
    /// </summary>
    protected virtual string GetSecondaryButtonClass()
    {
        return Framework switch
        {
            CssFramework.Bootstrap => "btn btn-secondary",
            CssFramework.FluentUI => "osirion-button osirion-button-secondary",
            CssFramework.MudBlazor => "mud-button mud-button-filled mud-button-filled-secondary",
            CssFramework.Radzen => "rz-button rz-button-secondary",
            _ => "osirion-button osirion-button-secondary"
        };
    }

    /// <summary>
    /// Gets the CSS class for an outlined primary button
    /// </summary>
    protected virtual string GetOutlinedPrimaryButtonClass()
    {
        return Framework switch
        {
            CssFramework.Bootstrap => "btn btn-outline-primary",
            CssFramework.FluentUI => "osirion-button osirion-button-outlined osirion-button-outlined-primary",
            CssFramework.MudBlazor => "mud-button mud-button-outlined mud-button-outlined-primary",
            CssFramework.Radzen => "rz-button rz-button-primary rz-button-light",
            _ => "osirion-button osirion-button-outlined osirion-button-outlined-primary"
        };
    }

    /// <summary>
    /// Gets the CSS class for an outlined secondary button
    /// </summary>
    protected virtual string GetOutlinedSecondaryButtonClass()
    {
        return Framework switch
        {
            CssFramework.Bootstrap => "btn btn-outline-secondary",
            CssFramework.FluentUI => "osirion-button osirion-button-outlined osirion-button-outlined-secondary",
            CssFramework.MudBlazor => "mud-button mud-button-outlined mud-button-outlined-secondary",
            CssFramework.Radzen => "rz-button rz-button-secondary rz-button-light",
            _ => "osirion-button osirion-button-outlined osirion-button-outlined-secondary"
        };
    }

    /// <summary>
    /// Gets the CSS class for a text button (no background)
    /// </summary>
    protected virtual string GetTextButtonClass()
    {
        return Framework switch
        {
            CssFramework.Bootstrap => "btn btn-link",
            CssFramework.FluentUI => "osirion-button osirion-button-text",
            CssFramework.MudBlazor => "mud-button mud-button-text",
            CssFramework.Radzen => "rz-button rz-button-text",
            _ => "osirion-button osirion-button-text"
        };
    }

    /// <summary>
    /// Gets the CSS class for a success button
    /// </summary>
    protected virtual string GetSuccessButtonClass()
    {
        return Framework switch
        {
            CssFramework.Bootstrap => "btn btn-success",
            CssFramework.FluentUI => "osirion-button osirion-button-success",
            CssFramework.MudBlazor => "mud-button mud-button-filled mud-button-filled-success",
            CssFramework.Radzen => "rz-button rz-button-success",
            _ => "osirion-button osirion-button-success"
        };
    }

    /// <summary>
    /// Gets the CSS class for a danger button
    /// </summary>
    protected virtual string GetDangerButtonClass()
    {
        return Framework switch
        {
            CssFramework.Bootstrap => "btn btn-danger",
            CssFramework.FluentUI => "osirion-button osirion-button-danger",
            CssFramework.MudBlazor => "mud-button mud-button-filled mud-button-filled-error",
            CssFramework.Radzen => "rz-button rz-button-danger",
            _ => "osirion-button osirion-button-danger"
        };
    }

    /// <summary>
    /// Gets the CSS class for a warning button
    /// </summary>
    protected virtual string GetWarningButtonClass()
    {
        return Framework switch
        {
            CssFramework.Bootstrap => "btn btn-warning",
            CssFramework.FluentUI => "osirion-button osirion-button-warning",
            CssFramework.MudBlazor => "mud-button mud-button-filled mud-button-filled-warning",
            CssFramework.Radzen => "rz-button rz-button-warning",
            _ => "osirion-button osirion-button-warning"
        };
    }

    /// <summary>
    /// Gets the CSS class for an info button
    /// </summary>
    protected virtual string GetInfoButtonClass()
    {
        return Framework switch
        {
            CssFramework.Bootstrap => "btn btn-info",
            CssFramework.FluentUI => "osirion-button osirion-button-info",
            CssFramework.MudBlazor => "mud-button mud-button-filled mud-button-filled-info",
            CssFramework.Radzen => "rz-button rz-button-info",
            _ => "osirion-button osirion-button-info"
        };
    }

    /// <summary>
    /// Gets the CSS class for a small button
    /// </summary>
    protected virtual string GetSmallButtonClass()
    {
        return Framework switch
        {
            CssFramework.Bootstrap => "btn-sm",
            CssFramework.FluentUI => "osirion-button-sm",
            CssFramework.MudBlazor => "mud-button-size-small",
            CssFramework.Radzen => "rz-button-sm",
            _ => "osirion-button-sm"
        };
    }

    /// <summary>
    /// Gets the CSS class for a large button
    /// </summary>
    protected virtual string GetLargeButtonClass()
    {
        return Framework switch
        {
            CssFramework.Bootstrap => "btn-lg",
            CssFramework.FluentUI => "osirion-button-lg",
            CssFramework.MudBlazor => "mud-button-size-large",
            CssFramework.Radzen => "rz-button-lg",
            _ => "osirion-button-lg"
        };
    }

    /// <summary>
    /// Gets the CSS class for a disabled button
    /// </summary>
    protected virtual string GetDisabledButtonClass()
    {
        return Framework switch
        {
            CssFramework.Bootstrap => "disabled",
            CssFramework.FluentUI => "osirion-button-disabled",
            CssFramework.MudBlazor => "mud-disabled",
            CssFramework.Radzen => "rz-state-disabled",
            _ => "osirion-button-disabled"
        };
    }

    /// <summary>
    /// Gets the CSS class for a block/full-width button
    /// </summary>
    protected virtual string GetBlockButtonClass()
    {
        return Framework switch
        {
            CssFramework.Bootstrap => "w-100",
            CssFramework.FluentUI => "osirion-button-block",
            CssFramework.MudBlazor => "mud-width-full",
            CssFramework.Radzen => "rz-button-full-width",
            _ => "osirion-button-block"
        };
    }

    /// <summary>
    /// Gets the CSS class for an icon button
    /// </summary>
    protected virtual string GetIconButtonClass()
    {
        return Framework switch
        {
            CssFramework.Bootstrap => "btn btn-icon",
            CssFramework.FluentUI => "osirion-button osirion-button-icon",
            CssFramework.MudBlazor => "mud-icon-button",
            CssFramework.Radzen => "rz-button rz-button-icon",
            _ => "osirion-button osirion-button-icon"
        };
    }

    /// <summary>
    /// Gets the CSS class for a rounded/pill button
    /// </summary>
    protected virtual string GetRoundedButtonClass()
    {
        return Framework switch
        {
            CssFramework.Bootstrap => "rounded-pill",
            CssFramework.FluentUI => "osirion-button-rounded",
            CssFramework.MudBlazor => "mud-button-round",
            CssFramework.Radzen => "rz-button-rounded",
            _ => "osirion-button-rounded"
        };
    }

    /// <summary>
    /// Gets the CSS class for a button based on variant
    /// </summary>
    /// <param name="variant">The button variant</param>
    /// <returns>CSS class string for the button</returns>
    protected virtual string GetButtonClass(ButtonVariant variant)
    {
        return variant switch
        {
            ButtonVariant.Primary => GetPrimaryButtonClass(),
            ButtonVariant.Secondary => GetSecondaryButtonClass(),
            ButtonVariant.Success => GetSuccessButtonClass(),
            ButtonVariant.Danger => GetDangerButtonClass(),
            ButtonVariant.Warning => GetWarningButtonClass(),
            ButtonVariant.Info => GetInfoButtonClass(),
            ButtonVariant.Text => GetTextButtonClass(),
            ButtonVariant.OutlinedPrimary => GetOutlinedPrimaryButtonClass(),
            ButtonVariant.OutlinedSecondary => GetOutlinedSecondaryButtonClass(),
            _ => GetPrimaryButtonClass()
        };
    }

    /// <summary>
    /// Gets the CSS class for a button size
    /// </summary>
    /// <param name="size">The button size</param>
    /// <returns>CSS class string for the button size</returns>
    protected virtual string GetButtonSizeClass(ButtonSize size)
    {
        return size switch
        {
            ButtonSize.Small => GetSmallButtonClass(),
            ButtonSize.Large => GetLargeButtonClass(),
            ButtonSize.Normal => string.Empty,
            _ => string.Empty
        };
    }

    /// <summary>
    /// Gets the CSS class for a button shape
    /// </summary>
    /// <param name="shape">The button shape</param>
    /// <returns>CSS class string for the button shape</returns>
    protected virtual string GetButtonShapeClass(ButtonShape shape)
    {
        return shape switch
        {
            ButtonShape.Rounded => GetRoundedButtonClass(),
            ButtonShape.Square => GetIconButtonClass(),
            ButtonShape.Default => string.Empty,
            _ => string.Empty
        };
    }

    /// <summary>
    /// Builds a complete button class string with variant, size, and additional options
    /// </summary>
    /// <param name="variant">The button variant</param>
    /// <param name="size">The button size (optional)</param>
    /// <param name="shape">The button shape (optional)</param>
    /// <param name="isBlock">Whether the button should be full width</param>
    /// <param name="isDisabled">Whether the button is disabled</param>
    /// <param name="additionalClasses">Additional CSS classes to append</param>
    /// <returns>Complete CSS class string for the button</returns>
    protected virtual string BuildButtonClass(
        ButtonVariant variant = ButtonVariant.Primary,
        ButtonSize size = ButtonSize.Normal,
        ButtonShape shape = ButtonShape.Default,
        bool isBlock = false,
        bool isDisabled = false,
        string? additionalClasses = null)
    {
        var classes = new List<string> { GetButtonClass(variant) };

        var sizeClass = GetButtonSizeClass(size);
        if (!string.IsNullOrWhiteSpace(sizeClass))
        {
            classes.Add(sizeClass);
        }

        var shapeClass = GetButtonShapeClass(shape);
        if (!string.IsNullOrWhiteSpace(shapeClass))
        {
            classes.Add(shapeClass);
        }

        if (isBlock)
        {
            classes.Add(GetBlockButtonClass());
        }

        if (isDisabled)
        {
            classes.Add(GetDisabledButtonClass());
        }

        if (!string.IsNullOrWhiteSpace(additionalClasses))
        {
            classes.Add(additionalClasses);
        }

        return string.Join(" ", classes.Where(c => !string.IsNullOrWhiteSpace(c)));
    }

    /// <summary>
    /// Gets the CSS class for an input element
    /// </summary>
    /// <remarks>
    /// MudBlazor and Fluent UI get the neutral class their adapter styles: their own input classes only work inside
    /// the framework's input markup, and on a plain input they remove its border and size.
    /// </remarks>
    protected virtual string GetInputClass()
    {
        return Framework switch
        {
            CssFramework.Bootstrap => "form-control",
            CssFramework.FluentUI => "osirion-input",
            CssFramework.MudBlazor => "osirion-input",
            CssFramework.Radzen => "rz-textbox",
            _ => "osirion-input"
        };
    }

    /// <summary>
    /// Gets the CSS class for a select element
    /// </summary>
    protected virtual string GetSelectClass()
    {
        return Framework switch
        {
            CssFramework.Bootstrap => "form-select",
            CssFramework.FluentUI => "osirion-select",
            CssFramework.MudBlazor => "osirion-select",
            CssFramework.Radzen => "rz-dropdown",
            _ => "osirion-select"
        };
    }

    /// <summary>
    /// Gets the CSS class for a checkbox
    /// </summary>
    protected virtual string GetCheckboxClass()
    {
        return Framework switch
        {
            CssFramework.Bootstrap => "form-check-input",
            CssFramework.FluentUI => "osirion-checkbox osirion-fluent-checkbox",
            CssFramework.MudBlazor => "mud-checkbox mud-checkbox-input",
            CssFramework.Radzen => "rz-chkbox rz-chkbox-box",
            _ => "osirion-checkbox"
        };
    }

    /// <summary>
    /// Gets the CSS class for a radio button
    /// </summary>
    protected virtual string GetRadioClass()
    {
        return Framework switch
        {
            CssFramework.Bootstrap => "form-check-input",
            CssFramework.FluentUI => "osirion-radio",
            CssFramework.MudBlazor => "mud-radio",
            CssFramework.Radzen => "rz-radio",
            _ => "osirion-radio"
        };
    }

    /// <summary>
    /// Gets the CSS class for a form label
    /// </summary>
    protected virtual string GetLabelClass()
    {
        return Framework switch
        {
            CssFramework.Bootstrap => "form-label",
            CssFramework.FluentUI => "osirion-label",
            CssFramework.MudBlazor => "mud-input-label",
            CssFramework.Radzen => "rz-text-caption",
            _ => "osirion-label"
        };
    }

    /// <summary>
    /// Gets the CSS class for a card
    /// </summary>
    protected virtual string GetCardClass()
    {
        return Framework switch
        {
            CssFramework.Bootstrap => "card",
            CssFramework.FluentUI => "osirion-card",
            CssFramework.MudBlazor => "mud-paper mud-elevation-1",
            CssFramework.Radzen => "rz-card",
            _ => "osirion-card"
        };
    }

    /// <summary>
    /// Gets the CSS class for a card header
    /// </summary>
    protected virtual string GetCardHeaderClass()
    {
        return Framework switch
        {
            CssFramework.Bootstrap => "card-header",
            CssFramework.FluentUI => "osirion-card-header",
            CssFramework.MudBlazor => "mud-card-header",
            CssFramework.Radzen => "rz-card-header",
            _ => "osirion-card-header"
        };
    }

    /// <summary>
    /// Gets the CSS class for a card body
    /// </summary>
    protected virtual string GetCardBodyClass()
    {
        return Framework switch
        {
            CssFramework.Bootstrap => "card-body",
            CssFramework.FluentUI => "osirion-card-body",
            CssFramework.MudBlazor => "mud-card-content",
            CssFramework.Radzen => "rz-card-content",
            _ => "osirion-card-body"
        };
    }

    /// <summary>
    /// Gets the CSS class for a badge/chip
    /// </summary>
    protected virtual string GetBadgeClass()
    {
        return Framework switch
        {
            CssFramework.Bootstrap => "badge",
            CssFramework.FluentUI => "osirion-badge",
            CssFramework.MudBlazor => "mud-chip",
            CssFramework.Radzen => "rz-badge",
            _ => "osirion-badge"
        };
    }

    /// <summary>
    /// Gets the CSS class for an alert/notification
    /// </summary>
    protected virtual string GetAlertClass()
    {
        return Framework switch
        {
            CssFramework.Bootstrap => "alert",
            CssFramework.FluentUI => "osirion-alert",
            CssFramework.MudBlazor => "mud-alert",
            CssFramework.Radzen => "rz-message",
            _ => "osirion-alert"
        };
    }

    /// <summary>
    /// Gets the CSS class for an alert with a specific variant
    /// </summary>
    /// <param name="variant">The alert variant</param>
    /// <returns>CSS class string for the alert with variant</returns>
    /// <remarks>
    /// An out-of-range severity gets the info class, and an out-of-range framework the neutral Osirion classes.
    /// </remarks>
    protected virtual string GetAlertClass(Severity variant)
    {
        var framework = Enum.IsDefined(Framework) ? Framework : CssFramework.None;
        var severity = Enum.IsDefined(variant) ? variant : Severity.Info;

        return $"{GetAlertClass()} {AlertSeverityClasses[(framework, severity)]}";
    }

    private static readonly FrozenDictionary<(CssFramework Framework, Severity Severity), string> AlertSeverityClasses =
        new Dictionary<(CssFramework Framework, Severity Severity), string>
        {
            [(CssFramework.None, Severity.Success)] = "osirion-alert-success",
            [(CssFramework.None, Severity.Error)] = "osirion-alert-error",
            [(CssFramework.None, Severity.Warning)] = "osirion-alert-warning",
            [(CssFramework.None, Severity.Info)] = "osirion-alert-info",
            [(CssFramework.Bootstrap, Severity.Success)] = "alert-success",
            [(CssFramework.Bootstrap, Severity.Error)] = "alert-danger",
            [(CssFramework.Bootstrap, Severity.Warning)] = "alert-warning",
            [(CssFramework.Bootstrap, Severity.Info)] = "alert-info",
            [(CssFramework.FluentUI, Severity.Success)] = "osirion-alert-success",
            [(CssFramework.FluentUI, Severity.Error)] = "osirion-alert-error",
            [(CssFramework.FluentUI, Severity.Warning)] = "osirion-alert-warning",
            [(CssFramework.FluentUI, Severity.Info)] = "osirion-alert-info",
            [(CssFramework.MudBlazor, Severity.Success)] = "mud-alert-text-success",
            [(CssFramework.MudBlazor, Severity.Error)] = "mud-alert-text-error",
            [(CssFramework.MudBlazor, Severity.Warning)] = "mud-alert-text-warning",
            [(CssFramework.MudBlazor, Severity.Info)] = "mud-alert-text-info",
            [(CssFramework.Radzen, Severity.Success)] = "rz-message-success",
            [(CssFramework.Radzen, Severity.Error)] = "rz-message-error",
            [(CssFramework.Radzen, Severity.Warning)] = "rz-message-warning",
            [(CssFramework.Radzen, Severity.Info)] = "rz-message-info",
        }.ToFrozenDictionary();

    /// <summary>
    /// Gets the CSS class for a table
    /// </summary>
    protected virtual string GetTableClass()
    {
        return Framework switch
        {
            CssFramework.Bootstrap => "table",
            CssFramework.FluentUI => "osirion-table",
            CssFramework.MudBlazor => "mud-table",
            CssFramework.Radzen => "rz-data-grid",
            _ => "osirion-table"
        };
    }

    /// <summary>
    /// Gets the CSS class for a modal/dialog
    /// </summary>
    protected virtual string GetModalClass()
    {
        return Framework switch
        {
            CssFramework.Bootstrap => "modal",
            CssFramework.FluentUI => "osirion-modal",
            CssFramework.MudBlazor => "mud-dialog",
            CssFramework.Radzen => "rz-dialog",
            _ => "osirion-modal"
        };
    }

    /// <summary>
    /// Gets the CSS class for a dropdown menu
    /// </summary>
    protected virtual string GetDropdownClass()
    {
        return Framework switch
        {
            CssFramework.Bootstrap => "dropdown",
            CssFramework.FluentUI => "osirion-dropdown",
            CssFramework.MudBlazor => "mud-menu",
            CssFramework.Radzen => "rz-dropdown",
            _ => "osirion-dropdown"
        };
    }

    /// <summary>
    /// Gets the CSS class for a tooltip
    /// </summary>
    protected virtual string GetTooltipClass()
    {
        return Framework switch
        {
            CssFramework.Bootstrap => "tooltip",
            CssFramework.FluentUI => "osirion-tooltip",
            CssFramework.MudBlazor => "mud-tooltip",
            CssFramework.Radzen => "rz-tooltip",
            _ => "osirion-tooltip"
        };
    }

    /// <summary>
    /// Gets the CSS class for a progress bar
    /// </summary>
    protected virtual string GetProgressClass()
    {
        return Framework switch
        {
            CssFramework.Bootstrap => "progress",
            CssFramework.FluentUI => "osirion-progress",
            CssFramework.MudBlazor => "mud-progress-linear",
            CssFramework.Radzen => "rz-progressbar",
            _ => "osirion-progress"
        };
    }

    /// <summary>
    /// Gets the CSS class for a spinner/loader
    /// </summary>
    protected virtual string GetSpinnerClass()
    {
        return Framework switch
        {
            CssFramework.Bootstrap => "spinner-border",
            CssFramework.FluentUI => "osirion-spinner",
            CssFramework.MudBlazor => "mud-progress-circular",
            CssFramework.Radzen => "rz-spinner",
            _ => "osirion-spinner"
        };
    }

    /// <summary>
    /// Gets the CSS class for tabs
    /// </summary>
    protected virtual string GetTabsClass()
    {
        return Framework switch
        {
            CssFramework.Bootstrap => "nav nav-tabs",
            CssFramework.FluentUI => "osirion-tabs",
            CssFramework.MudBlazor => "mud-tabs",
            CssFramework.Radzen => "rz-tabs",
            _ => "osirion-tabs"
        };
    }

    /// <summary>
    /// Gets the CSS class for an accordion
    /// </summary>
    protected virtual string GetAccordionClass()
    {
        return Framework switch
        {
            CssFramework.Bootstrap => "accordion",
            CssFramework.FluentUI => "osirion-accordion",
            CssFramework.MudBlazor => "mud-expansion-panels",
            CssFramework.Radzen => "rz-accordion",
            _ => "osirion-accordion"
        };
    }

    /// <summary>
    /// Gets the CSS class for a text area
    /// </summary>
    protected virtual string GetTextAreaClass()
    {
        var baseClass = Framework switch
        {
            CssFramework.Bootstrap => "form-control",
            CssFramework.FluentUI => "osirion-textarea osirion-fluent-textarea",
            CssFramework.MudBlazor => "osirion-textarea",
            CssFramework.Radzen => "rz-textbox",
            _ => "osirion-textarea"
        };

        return $"{baseClass} osirion-contact-textarea";
    }


    /// <summary>
    /// Combines the base CSS class with any additional classes provided.
    /// </summary>
    /// <param name="baseClass">The base CSS class for the component</param>
    /// <returns>Combined CSS class string</returns>
    protected string CombineCssClasses(string baseClass)
    {
        return string.IsNullOrWhiteSpace(Class)
            ? baseClass
            : $"{baseClass} {Class}";
    }
}
