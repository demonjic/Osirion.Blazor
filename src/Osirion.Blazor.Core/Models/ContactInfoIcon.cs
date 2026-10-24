namespace Osirion.Blazor.Core.Models;

/// <summary>
/// The icons <c>OsirionContactInfoSection</c> can draw beside a row of contact information.
/// </summary>
public enum ContactInfoIcon
{
    /// <summary>No icon. The row keeps the space of one, so its text lines up with the other rows.</summary>
    None = 0,

    /// <summary>A map pin, the icon of the address.</summary>
    Address,

    /// <summary>A telephone handset, the icon of the phone numbers.</summary>
    Phone,

    /// <summary>An envelope, the icon of the email addresses.</summary>
    Email,

    /// <summary>A globe, the icon of the websites.</summary>
    Website,

    /// <summary>A calendar, for example for a booking link.</summary>
    Calendar,

    /// <summary>A clock, for example for working hours or a reply time.</summary>
    Clock,

    /// <summary>Two chain links, for example for a profile on another site.</summary>
    Link
}
