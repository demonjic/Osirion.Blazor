using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Osirion.Blazor.Core.Models;

namespace Osirion.Blazor.Components;

/// <summary>
/// Checks the standard fields of <see cref="ContactFormModel"/> (name, email, subject, message, privacy box) against
/// the validation attributes the model declares, the way <c>DataAnnotationsValidator</c> does, and words each broken
/// rule with <see cref="ContactFormValidationTexts"/>.
/// </summary>
/// <remarks>
/// The attributes stay the one place the rules are written: their limits fill the messages' placeholders. As in
/// <see cref="Validator.TryValidateProperty"/>, a failed <see cref="RequiredAttribute"/> is the only message of its field,
/// and otherwise every failed attribute adds one.
/// </remarks>
internal static class ContactFormStandardFields
{
    private static readonly Dictionary<string, ValidationAttribute[]> AttributesByName = new[]
    {
        nameof(ContactFormModel.Name),
        nameof(ContactFormModel.Email),
        nameof(ContactFormModel.Subject),
        nameof(ContactFormModel.Message),
        nameof(ContactFormModel.AgreeToPrivacyPolicy),
    }.ToDictionary(
        name => name,
        name => typeof(ContactFormModel).GetProperty(name)!.GetCustomAttributes<ValidationAttribute>(inherit: true).ToArray(),
        StringComparer.Ordinal);

    /// <summary>Gets the names of the fields checked here.</summary>
    public static IReadOnlySet<string> Names { get; } = AttributesByName.Keys.ToHashSet(StringComparer.Ordinal);

    /// <summary>Returns the messages of the rules <paramref name="fieldName"/> breaks; none when it is valid.</summary>
    public static IEnumerable<string> Validate(ContactFormModel model, string fieldName, ContactFormValidationTexts texts)
    {
        var attributes = AttributesByName[fieldName];
        var value = typeof(ContactFormModel).GetProperty(fieldName)!.GetValue(model);
        var context = new ValidationContext(model) { MemberName = fieldName, DisplayName = fieldName };

        var required = attributes.OfType<RequiredAttribute>().FirstOrDefault();
        if (required is not null && required.GetValidationResult(value, context) != ValidationResult.Success)
        {
            return [Message(fieldName, required, texts)];
        }

        return attributes
            .Where(attribute => attribute is not RequiredAttribute && attribute.GetValidationResult(value, context) != ValidationResult.Success)
            .Select(attribute => Message(fieldName, attribute, texts))
            .ToList();
    }

    private static string Message(string fieldName, ValidationAttribute attribute, ContactFormValidationTexts texts) =>
        (fieldName, attribute) switch
        {
            (nameof(ContactFormModel.Name), RequiredAttribute) => texts.NameRequired,
            (nameof(ContactFormModel.Name), StringLengthAttribute length) => ContactFormValidationTexts.Format(texts.NameTooLong, length.MaximumLength),
            (nameof(ContactFormModel.Email), RequiredAttribute) => texts.EmailRequired,
            (nameof(ContactFormModel.Email), EmailAddressAttribute) => texts.EmailInvalid,
            (nameof(ContactFormModel.Email), StringLengthAttribute length) => ContactFormValidationTexts.Format(texts.EmailTooLong, length.MaximumLength),
            (nameof(ContactFormModel.Subject), RequiredAttribute) => texts.SubjectRequired,
            (nameof(ContactFormModel.Subject), StringLengthAttribute length) => ContactFormValidationTexts.Format(texts.SubjectTooLong, length.MaximumLength),
            (nameof(ContactFormModel.Message), RequiredAttribute) => texts.MessageRequired,
            (nameof(ContactFormModel.Message), StringLengthAttribute length) =>
                ContactFormValidationTexts.Format(texts.MessageLength, length.MinimumLength, length.MaximumLength),
            (nameof(ContactFormModel.AgreeToPrivacyPolicy), RangeAttribute) => texts.PrivacyRequired,
            _ => attribute.FormatErrorMessage(fieldName),
        };
}
