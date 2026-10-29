namespace Osirion.Blazor.Core.Models;

/// <summary>
/// The validated answer to one <see cref="ContactFormField"/> of an <c>OsirionContactForm</c> submission, as
/// <see cref="ContactFormModel.AdditionalFieldValues"/> lists it for <c>OnSubmit</c> and the email services.
/// </summary>
/// <param name="Name">The field's <see cref="ContactFormField.Name"/>.</param>
/// <param name="Label">The field's <see cref="ContactFormField.Label"/>.</param>
/// <param name="Values">
/// The submitted values, trimmed: none for an unanswered field, one for most types, and the chosen option values in
/// option order for <see cref="ContactFormFieldType.Checkboxes"/>.
/// </param>
/// <param name="Text">
/// The answer as a person reads it: the value, or the labels of the chosen options separated by commas. Empty for an
/// unanswered field.
/// </param>
public sealed record ContactFormFieldValue(string Name, string Label, IReadOnlyList<string> Values, string Text);
