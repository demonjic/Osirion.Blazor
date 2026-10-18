using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace Osirion.Blazor.Components;

/// <summary>Displays author and publication metadata for an article.</summary>
public partial class OsirionArticleMetdata
{
    /// <summary>Gets or sets the author's name, shown with a person icon. Hidden when empty or white space.</summary>
    [Parameter]
    public string? Author { get; set; }

    /// <summary>
    /// Gets or sets the publication date, shown with a calendar icon in the format of <see cref="DateFormat"/>.
    /// Hidden when <see langword="null"/>.
    /// </summary>
    [Parameter]
    public DateTime? PublishDate { get; set; }

    /// <summary>
    /// Gets or sets the .NET date format string for <see cref="PublishDate"/>, applied with <see cref="Culture"/>.
    /// </summary>
    [Parameter]
    public string? DateFormat { get; set; } = "d";

    /// <summary>
    /// Gets or sets the culture <see cref="PublishDate"/> is written in, normally the language of the article. When
    /// <see langword="null"/>, the current culture is used.
    /// </summary>
    [Parameter]
    public CultureInfo? Culture { get; set; }

    /// <summary>
    /// Gets or sets the read time in minutes, shown with a clock icon. The component appends " min", so pass the
    /// number only (<c>5</c>, not <c>5 min</c>). Hidden when empty or white space.
    /// </summary>
    [Parameter]
    public string? ReadTime { get; set; }

    private bool _hasMetadata => !string.IsNullOrWhiteSpace(Author) || PublishDate.HasValue || !string.IsNullOrWhiteSpace(ReadTime);
    /// <summary>
    /// Formats the publish date for display
    /// </summary>
    private string FormatDate(DateTime date) => date.ToString(DateFormat, Culture ?? CultureInfo.CurrentCulture);

    // The datetime attribute needs the Gregorian ISO form whatever culture the visible date is written in.
    private static string MachineReadable(DateTime date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
