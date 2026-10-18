using System.Globalization;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Web.Components;
using Osirion.Blazor.Cms.Web.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.Components;

public class ContentDatesTests
{
    // The cache stops growing at its cap; a locale seen after that is still looked up and still falls back.
    [Fact]
    public void CultureOf_MoreLocalesThanTheCacheHolds_StillResolvesEachOne()
    {
        using var scope = new CultureScope("de-DE");

        for (var index = 0; index < 300; index++)
        {
            ContentDates.CultureOf($"zz-cache-{index}").Name.ShouldBe("de-DE");
        }

        ContentDates.CultureOf("fr-FR").Name.ShouldBe("fr-FR");
    }

    private static readonly DateTime Date = new(2026, 10, 17, 0, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData("en", "October 17, 2026")]
    [InlineData("EN", "October 17, 2026")]
    [InlineData(" en-US ", "October 17, 2026")]
    [InlineData("sr-Latn-RS", "oktobar 17, 2026")]
    [InlineData("de", "Oktober 17, 2026")]
    public void Format_KnownLocale_UsesTheContentLanguage(string locale, string expected)
    {
        using var culture = new CultureScope("sr-Latn-RS");

        ContentDates.Format(Date, "MMMM d, yyyy", Item(locale)).ShouldBe(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-culture")]
    [InlineData("xx")]
    public void Format_MissingOrUnknownLocale_UsesTheCurrentCulture(string locale)
    {
        using var culture = new CultureScope("sr-Latn-RS");

        ContentDates.Format(Date, "MMMM d, yyyy", Item(locale)).ShouldBe("oktobar 17, 2026");
    }

    [Fact]
    public void CultureOf_NoItem_IsTheCurrentCulture()
    {
        using var culture = new CultureScope("de-DE");

        ContentDates.CultureOf((ContentItem?)null).ShouldBe(CultureInfo.GetCultureInfo("de-DE"));
    }

    // The fallback must follow the culture of each call, so a cached unknown locale cannot pin the first request's
    // culture for later ones.
    [Fact]
    public void CultureOf_UnknownLocaleAskedTwice_FollowsTheCurrentCultureEachTime()
    {
        using (new CultureScope("de-DE"))
        {
            ContentDates.CultureOf("still-not-a-culture").Name.ShouldBe("de-DE");
        }

        using (new CultureScope("sr-Latn-RS"))
        {
            ContentDates.CultureOf("still-not-a-culture").Name.ShouldBe("sr-Latn-RS");
        }
    }

    // A culture with a non-Gregorian calendar would otherwise write a Buddhist-era year into the attribute.
    [Fact]
    public void MachineReadable_IsTheGregorianIsoDateInEveryCulture()
    {
        using var culture = new CultureScope("th-TH");

        ContentDates.MachineReadable(Date).ShouldBe("2026-10-17");
    }

    private static ContentItem Item(string locale)
    {
        var item = ContentItem.Create("id", "Title", "<p>Body</p>", "blog/title.md", "test");
        item.SetLocale(locale);
        return item;
    }
}
