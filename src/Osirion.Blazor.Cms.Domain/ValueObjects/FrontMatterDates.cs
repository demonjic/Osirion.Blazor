using System.Globalization;

namespace Osirion.Blazor.Cms.Domain.ValueObjects;

/// <summary>
/// How front matter writes and reads <c>date</c> and <c>last_modified</c>: a day as <c>2026-10-29</c>, or a moment as
/// <c>2026-10-29T18:30:00+01:00</c>. A moment without an offset is UTC, so one value means the same moment on every
/// server.
/// </summary>
public static class FrontMatterDates
{
    private const string DayFormat = "yyyy-MM-dd";

    // K writes Z for a UTC value, the offset for a local one and nothing for an unspecified one, which reads back as UTC
    private const string MomentFormat = "yyyy-MM-dd'T'HH:mm:ssK";

    /// <summary>
    /// Writes <paramref name="value"/> for front matter: the day alone when it has no time of day, otherwise the moment
    /// to the second with the offset its <see cref="DateTime.Kind"/> gives.
    /// </summary>
    /// <param name="value">The date or moment.</param>
    /// <returns>The text to write, such as <c>2026-10-29</c> or <c>2026-10-29T17:30:00Z</c>.</returns>
    public static string Format(DateTime value) =>
        value.ToString(value.TimeOfDay == TimeSpan.Zero ? DayFormat : MomentFormat, CultureInfo.InvariantCulture);

    /// <summary>
    /// Reads a front matter date with the invariant culture, a day or a moment, as UTC: a moment with an offset is
    /// moved to UTC, and one without an offset is taken as UTC.
    /// </summary>
    /// <param name="value">The text from the front matter.</param>
    /// <param name="utc">The moment in UTC, with <see cref="DateTimeKind.Utc"/>, when the text is a date.</param>
    /// <returns>Whether the text is a date.</returns>
    public static bool TryParse(string? value, out DateTime utc)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            utc = default;
            return false;
        }

        return DateTime.TryParse(
            value.Trim(),
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out utc);
    }
}
