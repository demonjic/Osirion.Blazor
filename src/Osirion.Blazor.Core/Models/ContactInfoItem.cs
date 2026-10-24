namespace Osirion.Blazor.Core.Models;

/// <summary>
/// One extra row of <see cref="ContactInformation"/>, such as a booking link, a social profile, working hours or a
/// reply time, for details the fixed address, phone, email and website rows do not cover.
/// </summary>
/// <param name="Label">The bold text of the row, such as <c>Working hours:</c>. Plain text; it is encoded.</param>
/// <param name="Text">The text under the label, or the text of the link when <paramref name="Href"/> is set. Plain
/// text; it is encoded.</param>
/// <param name="Href">The link target. Written only when it is a path on the site or an absolute http, https or mailto
/// address (<c>SafeLink.IsSafe</c>); anything else leaves <paramref name="Text"/> as plain text. An absolute http or
/// https address opens in a new tab.</param>
/// <param name="Icon">The icon drawn beside the row.</param>
public sealed record ContactInfoItem(
    string Label,
    string Text,
    string? Href = null,
    ContactInfoIcon Icon = ContactInfoIcon.None);
