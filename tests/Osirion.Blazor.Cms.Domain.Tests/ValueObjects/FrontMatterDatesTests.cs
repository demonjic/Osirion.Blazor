using System.Globalization;
using Osirion.Blazor.Cms.Domain.ValueObjects;

namespace Osirion.Blazor.Cms.Domain.Tests.ValueObjects;

public class FrontMatterDatesTests
{
    [Theory]
    [InlineData("2026-10-29", 0, 0)]
    [InlineData("2026-10-29T18:30:00+01:00", 17, 30)]
    [InlineData("2026-10-29T18:30+01:00", 17, 30)]
    [InlineData("2026-10-29T18:30:00Z", 18, 30)]
    [InlineData("2026-10-29T18:30:00", 18, 30)]
    [InlineData("2026-10-29 18:30", 18, 30)]
    [InlineData("  2026-10-29T18:30:00Z  ", 18, 30)]
    public void TryParse_DayOrMoment_ReadsItAsUtc(string text, int hour, int minute)
    {
        Assert.True(FrontMatterDates.TryParse(text, out var utc));

        Assert.Equal(new DateTime(2026, 10, 29, hour, minute, 0), utc);
        Assert.Equal(DateTimeKind.Utc, utc.Kind);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("soon")]
    [InlineData("2026-13-01")]
    public void TryParse_NotADate_ReturnsFalse(string? text)
    {
        Assert.False(FrontMatterDates.TryParse(text, out var utc));
        Assert.Equal(default, utc);
    }

    // The invariant culture reads month first, whatever culture the server runs under.
    [Fact]
    public void TryParse_UnderAnotherCulture_ReadsTheInvariantForm()
    {
        var culture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-GB");
        try
        {
            Assert.True(FrontMatterDates.TryParse("02/03/2024", out var utc));

            Assert.Equal(new DateTime(2024, 2, 3), utc);
        }
        finally
        {
            CultureInfo.CurrentCulture = culture;
        }
    }

    [Theory]
    [InlineData(DateTimeKind.Utc)]
    [InlineData(DateTimeKind.Unspecified)]
    [InlineData(DateTimeKind.Local)]
    public void Format_NoTimeOfDay_WritesTheDay(DateTimeKind kind)
    {
        Assert.Equal("2026-10-29", FrontMatterDates.Format(new DateTime(2026, 10, 29, 0, 0, 0, kind)));
    }

    [Fact]
    public void Format_UtcMoment_WritesTheSecondAndZ()
    {
        Assert.Equal("2026-10-29T17:30:05Z", FrontMatterDates.Format(new DateTime(2026, 10, 29, 17, 30, 5, DateTimeKind.Utc)));
    }

    [Fact]
    public void Format_UnspecifiedMoment_WritesNoOffset()
    {
        Assert.Equal("2026-10-29T17:30:00", FrontMatterDates.Format(new DateTime(2026, 10, 29, 17, 30, 0, DateTimeKind.Unspecified)));
    }

    // A local moment carries the server's offset, so it reads back as the same moment on any server.
    [Fact]
    public void Format_LocalMoment_ReadsBackAsTheSameMoment()
    {
        var local = new DateTime(2026, 10, 29, 18, 30, 0, DateTimeKind.Local);

        Assert.True(FrontMatterDates.TryParse(FrontMatterDates.Format(local), out var utc));

        Assert.Equal(local.ToUniversalTime(), utc);
    }

    [Theory]
    [InlineData("2026-10-29")]
    [InlineData("2026-10-29T17:30:05Z")]
    public void FormatOfTryParse_WritesTheSameText(string text)
    {
        Assert.True(FrontMatterDates.TryParse(text, out var utc));

        Assert.Equal(text, FrontMatterDates.Format(utc));
    }
}
