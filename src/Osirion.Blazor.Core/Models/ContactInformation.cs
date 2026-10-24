namespace Osirion.Blazor.Core.Models;

/// <summary>
/// Model representing contact information to display alongside the contact form.
/// </summary>
public class ContactInformation
{
    /// <summary>
    /// Gets or sets the physical address.
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the phone number(s). Multiple values can be separated by commas.
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address. Multiple values can be separated by commas.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the website URL. Multiple values can be separated by commas.
    /// </summary>
    public string Website { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional message or description.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets extra rows drawn after the address, phone, email and website rows, for details such as a booking
    /// link, a social profile, working hours or a reply time. Empty by default, which adds nothing.
    /// </summary>
    public IList<ContactInfoItem> AdditionalItems { get; set; } = [];
}
