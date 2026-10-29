using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;
using Osirion.Blazor.Core.Models;

namespace Osirion.Blazor.Components;

/// <summary>
/// The rules of <see cref="ContactFormField"/> that both contact forms share: which definitions are accepted, the
/// attributes each type renders with, and how <c>OsirionContactForm</c> checks a submitted value.
/// </summary>
internal static partial class ContactFormFieldRules
{
    /// <summary>
    /// Put before a field's name to form the name of its input, so an additional field never shares a name with a
    /// standard field or with the form's own data.
    /// </summary>
    internal const string InputNamePrefix = "field.";

    /// <summary>
    /// Checks the definitions a form was given and returns them, or an empty list for null.
    /// </summary>
    /// <exception cref="ArgumentException">A definition the forms cannot render or check safely.</exception>
    internal static IReadOnlyList<ContactFormField> Checked(IReadOnlyList<ContactFormField>? fields)
    {
        if (fields is null || fields.Count == 0)
        {
            return [];
        }

        if (fields.Count > ContactFormField.MaxFields)
        {
            throw new ArgumentException(
                $"A contact form takes at most {ContactFormField.MaxFields} additional fields; {fields.Count} were given.",
                nameof(fields));
        }

        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var field in fields)
        {
            ArgumentNullException.ThrowIfNull(field, nameof(fields));
            CheckDefinition(field);
            if (!names.Add(field.Name))
            {
                throw new ArgumentException($"The additional field name '{field.Name}' is used twice.", nameof(fields));
            }
        }

        return fields;
    }

    /// <summary>
    /// Gets the name of the field's input in the form post.
    /// </summary>
    internal static string InputName(ContactFormField field) => InputNamePrefix + field.Name;

    /// <summary>
    /// Gets whether the field is chosen from its options.
    /// </summary>
    internal static bool IsChoice(ContactFormField field) =>
        field.Type is ContactFormFieldType.Select or ContactFormFieldType.Radio or ContactFormFieldType.Checkboxes;

    /// <summary>
    /// Gets whether the field is a group of inputs, rendered in a <c>fieldset</c> named by its <c>legend</c>.
    /// </summary>
    internal static bool IsGroup(ContactFormField field) =>
        field.Type is ContactFormFieldType.Radio or ContactFormFieldType.Checkboxes;

    /// <summary>
    /// Gets the <c>type</c> attribute of a single <c>input</c>: text, number, email or tel.
    /// </summary>
    internal static string InputType(ContactFormField field) => field.Type switch
    {
        ContactFormFieldType.Number => "number",
        ContactFormFieldType.Email => "email",
        ContactFormFieldType.Phone => "tel",
        _ => "text"
    };

    /// <summary>
    /// Gets the most characters the field's value may hold: <see cref="ContactFormField.MaxLength"/> or the type's default.
    /// </summary>
    internal static int MaxLength(ContactFormField field) => field.MaxLength ?? field.Type switch
    {
        ContactFormFieldType.MultilineText => 2000,
        ContactFormFieldType.Email => 254,
        ContactFormFieldType.Phone or ContactFormFieldType.Number => 40,
        _ => 200
    };

    /// <summary>
    /// Writes a number bound the way the <c>min</c> and <c>max</c> attributes read it, whatever the current culture.
    /// </summary>
    internal static string? FormatNumber(decimal? value) => value?.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Checks the values submitted for one field and turns them into its answer.
    /// </summary>
    /// <param name="field">The definition.</param>
    /// <param name="submitted">What the form post or the inputs held for the field; empty when nothing was sent.</param>
    /// <param name="answer">The answer, set whatever the outcome.</param>
    /// <returns>The message to show next to the field, or null when the value is valid.</returns>
    internal static string? Validate(ContactFormField field, IReadOnlyList<string> submitted, out ContactFormFieldValue answer)
    {
        if (field.Type == ContactFormFieldType.Checkboxes)
        {
            return ValidateChoices(field, submitted, out answer);
        }

        var value = submitted.Count == 0 ? string.Empty : Normalize(submitted[0]);
        IReadOnlyList<string> values = value.Length == 0 ? [] : [value];
        var option = IsChoice(field) ? field.Options.FirstOrDefault(o => o.Value == value) : null;
        answer = new ContactFormFieldValue(field.Name, field.Label, values, option?.DisplayLabel ?? value);

        if (value.Length == 0)
        {
            return field.Required ? $"{field.Label} is required." : null;
        }

        return IsChoice(field)
            ? option is null ? NotAnOption(field) : null
            : ValidateText(field, value);
    }

    private static string? ValidateChoices(ContactFormField field, IReadOnlyList<string> submitted, out ContactFormFieldValue answer)
    {
        var chosen = submitted.Select(Normalize).Where(value => value.Length > 0).ToHashSet(StringComparer.Ordinal);
        var options = field.Options.Where(option => chosen.Contains(option.Value)).ToList();
        answer = new ContactFormFieldValue(
            field.Name,
            field.Label,
            options.Select(option => option.Value).ToList(),
            string.Join(", ", options.Select(option => option.DisplayLabel)));

        if (options.Count != chosen.Count)
        {
            return NotAnOption(field);
        }

        return field.Required && options.Count == 0 ? $"{field.Label} is required." : null;
    }

    private static string? ValidateText(ContactFormField field, string value)
    {
        var maxLength = MaxLength(field);
        if (value.Length > maxLength)
        {
            return $"{field.Label} cannot exceed {maxLength} characters.";
        }

        if (HasDisallowedCharacters(field, value))
        {
            return field.Type == ContactFormFieldType.MultilineText
                ? $"{field.Label} contains characters that are not allowed."
                : $"{field.Label} must be a single line.";
        }

        return field.Type switch
        {
            ContactFormFieldType.Email when !new EmailAddressAttribute().IsValid(value) =>
                $"{field.Label} must be a valid email address.",
            ContactFormFieldType.Phone when !PhoneCharacters().IsMatch(value) || value.Count(char.IsAsciiDigit) < 3 =>
                $"{field.Label} must be a valid phone number.",
            ContactFormFieldType.Number => ValidateNumber(field, value),
            _ => null
        };
    }

    private static string? ValidateNumber(ContactFormField field, string value)
    {
        if (!decimal.TryParse(value, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var number))
        {
            return $"{field.Label} must be a number.";
        }

        if (number < field.Min)
        {
            return $"{field.Label} must be at least {FormatNumber(field.Min)}.";
        }

        return number > field.Max ? $"{field.Label} must be at most {FormatNumber(field.Max)}." : null;
    }

    // A browser posts a line break in a textarea as CRLF but counts it as one character against maxlength, so the
    // server counts the same way.
    private static string Normalize(string? value) => (value ?? string.Empty).Replace("\r\n", "\n", StringComparison.Ordinal).Trim();

    // Only a multi-line field may hold line breaks and tabs; no field holds other control characters.
    private static bool HasDisallowedCharacters(ContactFormField field, string value) =>
        field.Type == ContactFormFieldType.MultilineText
            ? value.Any(c => char.IsControl(c) && c is not '\n' and not '\t')
            : value.Any(char.IsControl);

    private static string NotAnOption(ContactFormField field) => $"Choose one of the listed options for {field.Label}.";

    private static void CheckDefinition(ContactFormField field)
    {
        if (string.IsNullOrEmpty(field.Name) || !SafeName().IsMatch(field.Name))
        {
            throw new ArgumentException(
                $"The additional field name '{field.Name}' is not a letter followed by up to 49 letters, digits, underscores or hyphens.",
                nameof(field));
        }

        if (string.IsNullOrWhiteSpace(field.Label))
        {
            throw new ArgumentException($"The additional field '{field.Name}' has no label.", nameof(field));
        }

        if (field.MaxLength is < 1 or > ContactFormField.MaxLengthLimit)
        {
            throw new ArgumentException(
                $"The MaxLength of the additional field '{field.Name}' is not between 1 and {ContactFormField.MaxLengthLimit}.",
                nameof(field));
        }

        if (field.Min > field.Max)
        {
            throw new ArgumentException($"The Min of the additional field '{field.Name}' is above its Max.", nameof(field));
        }

        if (IsChoice(field))
        {
            CheckOptions(field);
        }
    }

    private static void CheckOptions(ContactFormField field)
    {
        if (field.Options is null || field.Options.Count is 0 or > ContactFormField.MaxOptions)
        {
            throw new ArgumentException(
                $"The choice field '{field.Name}' needs between 1 and {ContactFormField.MaxOptions} options.", nameof(field));
        }

        var values = new HashSet<string>(StringComparer.Ordinal);
        foreach (var option in field.Options)
        {
            if (option is null || string.IsNullOrWhiteSpace(option.Value) || option.Value != option.Value.Trim() || !values.Add(option.Value))
            {
                throw new ArgumentException(
                    $"An option of the choice field '{field.Name}' is blank, starts or ends with white space, or repeats another option's value.",
                    nameof(field));
            }
        }
    }

    [GeneratedRegex("^[A-Za-z][A-Za-z0-9_-]{0,49}$")]
    private static partial Regex SafeName();

    [GeneratedRegex(@"^[0-9+()./ \-]+$")]
    private static partial Regex PhoneCharacters();
}
