namespace Osirion.Blazor.Core.Models;

/// <summary>
/// The kinds of input a <see cref="ContactFormField"/> renders.
/// </summary>
public enum ContactFormFieldType
{
    /// <summary>A single line of text (<c>input type="text"</c>).</summary>
    Text,

    /// <summary>Several lines of text (<c>textarea</c>).</summary>
    MultilineText,

    /// <summary>A number (<c>input type="number"</c>), with optional <see cref="ContactFormField.Min"/> and <see cref="ContactFormField.Max"/>; decimals are accepted.</summary>
    Number,

    /// <summary>An email address (<c>input type="email"</c>).</summary>
    Email,

    /// <summary>A phone number (<c>input type="tel"</c>): digits, spaces and <c>+ ( ) - . /</c>.</summary>
    Phone,

    /// <summary>One of the options, chosen from a <c>select</c> with an empty first option.</summary>
    Select,

    /// <summary>One of the options, chosen from a group of radio buttons.</summary>
    Radio,

    /// <summary>Any number of the options, from a group of checkboxes.</summary>
    Checkboxes
}
