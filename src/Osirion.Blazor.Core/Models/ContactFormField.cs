namespace Osirion.Blazor.Core.Models;

/// <summary>
/// Describes one field a site adds to <c>OsirionContactForm</c> or <c>OsirionStaticContactForm</c> after the standard
/// fields, such as the size of a company or a budget on a quote form. The forms render the fields in list order.
/// </summary>
/// <remarks>
/// A form checks its definitions when its parameters are set and throws an <see cref="ArgumentException"/> naming the
/// field for a definition it cannot render safely: see <see cref="Name"/>, <see cref="MaxLength"/>,
/// <see cref="Min"/>, <see cref="Max"/> and <see cref="Options"/>.
/// </remarks>
public sealed class ContactFormField
{
    /// <summary>
    /// The most additional fields one form takes.
    /// </summary>
    public const int MaxFields = 20;

    /// <summary>
    /// The most characters all additional values of one submission may hold together.
    /// </summary>
    public const int MaxTotalLength = 10_000;

    /// <summary>
    /// The highest <see cref="MaxLength"/> a field may set.
    /// </summary>
    public const int MaxLengthLimit = 5_000;

    /// <summary>
    /// The most options a choice field may offer.
    /// </summary>
    public const int MaxOptions = 50;

    /// <summary>
    /// Gets the identifier of the field: the key of its value in the JSON the static form posts, in
    /// <see cref="ContactFormFieldValue.Name"/> and in the form post. A letter, then up to 49 letters, digits,
    /// underscores or hyphens (<c>company_size</c>, <c>budget-range</c>); names are unique within a form and compared
    /// with their letter case.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the visible label of the field, also used to name the field in the email and in validation messages.
    /// Plain text; it is encoded.
    /// </summary>
    public required string Label { get; init; }

    /// <summary>
    /// Gets the kind of input. <see cref="ContactFormFieldType.Text"/> by default.
    /// </summary>
    public ContactFormFieldType Type { get; init; } = ContactFormFieldType.Text;

    /// <summary>
    /// Gets whether a value is required. For <see cref="ContactFormFieldType.Checkboxes"/> it means at least one box.
    /// </summary>
    public bool Required { get; init; }

    /// <summary>
    /// Gets the most characters the value may hold, from 1 to <see cref="MaxLengthLimit"/>. When it is null the type's
    /// default applies: 200 for text, 2000 for multi-line text, 254 for email, 40 for phone and number. Choice fields
    /// ignore it; their values must be one of <see cref="Options"/>.
    /// </summary>
    public int? MaxLength { get; init; }

    /// <summary>
    /// Gets the smallest value a <see cref="ContactFormFieldType.Number"/> field accepts. Ignored by other types.
    /// </summary>
    public decimal? Min { get; init; }

    /// <summary>
    /// Gets the largest value a <see cref="ContactFormFieldType.Number"/> field accepts; it may not be below
    /// <see cref="Min"/>. Ignored by other types.
    /// </summary>
    public decimal? Max { get; init; }

    /// <summary>
    /// Gets the choices of a <see cref="ContactFormFieldType.Select"/>, <see cref="ContactFormFieldType.Radio"/> or
    /// <see cref="ContactFormFieldType.Checkboxes"/> field, in display order: at least one and at most
    /// <see cref="MaxOptions"/>, each with a value that is not blank and that no other option of the field uses. Other
    /// types ignore them.
    /// </summary>
    public IReadOnlyList<ContactFormFieldOption> Options { get; init; } = [];

    /// <summary>
    /// Gets the placeholder of a text, multi-line text, number, email or phone input, or the text of the empty first
    /// option of a <see cref="ContactFormFieldType.Select"/>. Null writes none.
    /// </summary>
    public string? Placeholder { get; init; }

    /// <summary>
    /// Gets a hint rendered under the field and linked to it with <c>aria-describedby</c>. Plain text; it is encoded.
    /// </summary>
    public string? HelpText { get; init; }
}
