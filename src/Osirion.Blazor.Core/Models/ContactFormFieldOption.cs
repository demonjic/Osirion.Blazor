namespace Osirion.Blazor.Core.Models;

/// <summary>
/// One choice of a <see cref="ContactFormField"/> of type <see cref="ContactFormFieldType.Select"/>,
/// <see cref="ContactFormFieldType.Radio"/> or <see cref="ContactFormFieldType.Checkboxes"/>.
/// </summary>
/// <param name="Value">The value that is posted and sent to the endpoint, such as <c>11-50</c>.</param>
/// <param name="Label">The text shown to the visitor and written in the email; the value when it is null.</param>
public sealed record ContactFormFieldOption(string Value, string? Label = null)
{
    /// <summary>
    /// Gets the text shown for the option: <see cref="Label"/>, or <see cref="Value"/> when no label is set.
    /// </summary>
    public string DisplayLabel => string.IsNullOrWhiteSpace(Label) ? Value : Label;
}
