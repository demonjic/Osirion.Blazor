using System.Globalization;

namespace Osirion.Blazor.Core.Models;

/// <summary>
/// The validation messages of <c>OsirionContactForm</c>, for a form in another language. Every property starts with
/// the English message the form has always shown; set the ones to translate.
/// </summary>
/// <remarks>
/// The messages are composite format strings: <c>{0}</c> and <c>{1}</c> are replaced as each property says, for example
/// <c>"{0} je obavezno."</c> for <see cref="FieldRequired"/>. The form checks the strings when it renders, so a string
/// with a placeholder that does not exist fails at once instead of when a visitor sends the form.
/// </remarks>
public sealed class ContactFormValidationTexts
{
    /// <summary>Gets the English messages the form shows by default.</summary>
    public static ContactFormValidationTexts Default { get; } = new();

    /// <summary>Gets or sets the message when the name is empty.</summary>
    public string NameRequired { get; init; } = "Name is required";

    /// <summary>Gets or sets the message when the name is too long; <c>{0}</c> is the most characters allowed.</summary>
    public string NameTooLong { get; init; } = "Name cannot exceed {0} characters";

    /// <summary>Gets or sets the message when the email address is empty.</summary>
    public string EmailRequired { get; init; } = "Email is required";

    /// <summary>Gets or sets the message when the email address is not valid.</summary>
    public string EmailInvalid { get; init; } = "Please enter a valid email address";

    /// <summary>Gets or sets the message when the email address is too long; <c>{0}</c> is the most characters allowed.</summary>
    public string EmailTooLong { get; init; } = "Email cannot exceed {0} characters";

    /// <summary>Gets or sets the message when the subject is empty.</summary>
    public string SubjectRequired { get; init; } = "Subject is required";

    /// <summary>Gets or sets the message when the subject is too long; <c>{0}</c> is the most characters allowed.</summary>
    public string SubjectTooLong { get; init; } = "Subject cannot exceed {0} characters";

    /// <summary>Gets or sets the message when the message is empty.</summary>
    public string MessageRequired { get; init; } = "Message is required";

    /// <summary>
    /// Gets or sets the message when the message is too short or too long; <c>{0}</c> is the fewest characters allowed
    /// and <c>{1}</c> the most.
    /// </summary>
    public string MessageLength { get; init; } = "Message must be between {0} and {1} characters";

    /// <summary>Gets or sets the message when the privacy policy box is not ticked.</summary>
    public string PrivacyRequired { get; init; } = "You must agree to the privacy policy";

    /// <summary>Gets or sets the message when an additional field is empty; <c>{0}</c> is its label.</summary>
    public string FieldRequired { get; init; } = "{0} is required.";

    /// <summary>
    /// Gets or sets the message when an additional field is too long; <c>{0}</c> is its label and <c>{1}</c> the most
    /// characters allowed.
    /// </summary>
    public string FieldTooLong { get; init; } = "{0} cannot exceed {1} characters.";

    /// <summary>Gets or sets the message when a single-line field holds a line break; <c>{0}</c> is its label.</summary>
    public string FieldSingleLine { get; init; } = "{0} must be a single line.";

    /// <summary>Gets or sets the message when a multi-line field holds control characters; <c>{0}</c> is its label.</summary>
    public string FieldDisallowedCharacters { get; init; } = "{0} contains characters that are not allowed.";

    /// <summary>Gets or sets the message when an email field is not valid; <c>{0}</c> is its label.</summary>
    public string FieldInvalidEmail { get; init; } = "{0} must be a valid email address.";

    /// <summary>Gets or sets the message when a phone field is not valid; <c>{0}</c> is its label.</summary>
    public string FieldInvalidPhone { get; init; } = "{0} must be a valid phone number.";

    /// <summary>Gets or sets the message when a number field is not a number; <c>{0}</c> is its label.</summary>
    public string FieldNotANumber { get; init; } = "{0} must be a number.";

    /// <summary>Gets or sets the message when a number is too small; <c>{0}</c> is the label and <c>{1}</c> the minimum.</summary>
    public string FieldBelowMinimum { get; init; } = "{0} must be at least {1}.";

    /// <summary>Gets or sets the message when a number is too large; <c>{0}</c> is the label and <c>{1}</c> the maximum.</summary>
    public string FieldAboveMaximum { get; init; } = "{0} must be at most {1}.";

    /// <summary>Gets or sets the message when a choice is not one of the options; <c>{0}</c> is the field's label.</summary>
    public string FieldNotAnOption { get; init; } = "Choose one of the listed options for {0}.";

    /// <summary>
    /// Gets or sets the message when the answers to the additional fields are too long together; <c>{0}</c> is the most
    /// characters allowed.
    /// </summary>
    public string AnswersTooLong { get; init; } = "The answers to the additional questions cannot exceed {0} characters together.";

    /// <summary>
    /// Throws when a message is empty or is not a valid composite format string for the placeholders it is given.
    /// </summary>
    /// <exception cref="ArgumentException">The message's property is named.</exception>
    public void Validate()
    {
        Check(NameRequired, nameof(NameRequired), 0);
        Check(NameTooLong, nameof(NameTooLong), 1);
        Check(EmailRequired, nameof(EmailRequired), 0);
        Check(EmailInvalid, nameof(EmailInvalid), 0);
        Check(EmailTooLong, nameof(EmailTooLong), 1);
        Check(SubjectRequired, nameof(SubjectRequired), 0);
        Check(SubjectTooLong, nameof(SubjectTooLong), 1);
        Check(MessageRequired, nameof(MessageRequired), 0);
        Check(MessageLength, nameof(MessageLength), 2);
        Check(PrivacyRequired, nameof(PrivacyRequired), 0);
        Check(FieldRequired, nameof(FieldRequired), 1);
        Check(FieldTooLong, nameof(FieldTooLong), 2);
        Check(FieldSingleLine, nameof(FieldSingleLine), 1);
        Check(FieldDisallowedCharacters, nameof(FieldDisallowedCharacters), 1);
        Check(FieldInvalidEmail, nameof(FieldInvalidEmail), 1);
        Check(FieldInvalidPhone, nameof(FieldInvalidPhone), 1);
        Check(FieldNotANumber, nameof(FieldNotANumber), 1);
        Check(FieldBelowMinimum, nameof(FieldBelowMinimum), 2);
        Check(FieldAboveMaximum, nameof(FieldAboveMaximum), 2);
        Check(FieldNotAnOption, nameof(FieldNotAnOption), 1);
        Check(AnswersTooLong, nameof(AnswersTooLong), 1);
    }

    /// <summary>Formats <paramref name="message"/> with <paramref name="arguments"/> in the current culture.</summary>
    internal static string Format(string message, params object[] arguments) =>
        string.Format(CultureInfo.CurrentCulture, message, arguments);

    private static void Check(string message, string name, int placeholders)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException($"The contact form message {name} is empty.", name);
        }

        try
        {
            _ = string.Format(CultureInfo.InvariantCulture, message, Enumerable.Repeat<object>("x", placeholders).ToArray());
        }
        catch (FormatException exception)
        {
            throw new ArgumentException(
                $"The contact form message {name} is not a valid format string for {placeholders} placeholder(s): '{message}'.", name, exception);
        }
    }
}
