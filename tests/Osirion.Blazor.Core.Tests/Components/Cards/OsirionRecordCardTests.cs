using Bunit;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Models;
using Osirion.Blazor.Core.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Components.Cards;

public class OsirionRecordCardTests : OsirionComponentTestBase
{
    private static readonly RecordCardRow[] Rows =
    [
        new("Order number", "A-1042"),
        new("Amount", "120.00"),
    ];

    [Fact]
    public void OsirionRecordCard_AllParameters_RendersAFigureWithHeadingStatusRowsAndCaption()
    {
        var cut = RenderStatic<OsirionRecordCard>(p => p
            .Add(c => c.Title, "Order A-1042")
            .Add(c => c.Status, "Approved")
            .Add(c => c.StatusTone, RecordStatusTone.Positive)
            .Add(c => c.Rows, Rows)
            .Add(c => c.Footnote, "Sample data"));

        var figure = cut.Find("figure.osirion-record-card");
        figure.Children.Select(child => child.LocalName).ShouldBe(["div", "dl", "figcaption"]);
        figure.QuerySelector(".osirion-record-card-header > h3.osirion-record-card-title")!.TextContent.ShouldBe("Order A-1042");

        var status = figure.QuerySelector(".osirion-record-card-status")!;
        status.ClassList.ShouldBe(["osirion-record-card-status", "osirion-record-card-status-positive"]);
        status.TextContent.ShouldBe("Approved");
        status.QuerySelector(".osirion-record-card-status-mark")!.GetAttribute("aria-hidden").ShouldBe("true");

        var rows = figure.QuerySelectorAll("dl.osirion-record-card-rows > div.osirion-record-card-row");
        rows.Length.ShouldBe(2);
        rows[0].QuerySelector("dt.osirion-record-card-label")!.TextContent.ShouldBe("Order number");
        rows[0].QuerySelector("dd.osirion-record-card-value")!.TextContent.ShouldBe("A-1042");
        rows[1].QuerySelector("dt")!.TextContent.ShouldBe("Amount");
        rows[1].QuerySelector("dd")!.TextContent.ShouldBe("120.00");

        figure.QuerySelector("figcaption.osirion-record-card-footnote")!.TextContent.ShouldBe("Sample data");
    }

    [Theory]
    [InlineData(RecordStatusTone.Neutral, "neutral")]
    [InlineData(RecordStatusTone.Positive, "positive")]
    [InlineData(RecordStatusTone.Negative, "negative")]
    [InlineData(RecordStatusTone.Warning, "warning")]
    public void OsirionRecordCard_StatusTone_SetsTheToneClassAndKeepsTheText(RecordStatusTone tone, string name)
    {
        var cut = RenderStatic<OsirionRecordCard>(p => p
            .Add(c => c.Status, "Pending")
            .Add(c => c.StatusTone, tone));

        var status = cut.Find(".osirion-record-card-status");
        status.ClassList.ShouldContain($"osirion-record-card-status-{name}");
        status.TextContent.ShouldBe("Pending");
    }

    [Fact]
    public void OsirionRecordCard_StatusDefaultsToTheNeutralTone()
    {
        var cut = RenderStatic<OsirionRecordCard>(p => p.Add(c => c.Status, "Pending"));

        cut.Find(".osirion-record-card-status").ClassList.ShouldContain("osirion-record-card-status-neutral");
    }

    [Fact]
    public void OsirionRecordCard_OnlyRows_RendersNoHeaderAndNoCaption()
    {
        var cut = RenderStatic<OsirionRecordCard>(p => p.Add(c => c.Rows, Rows));

        cut.FindAll(".osirion-record-card-header").ShouldBeEmpty();
        cut.FindAll("figcaption").ShouldBeEmpty();
        cut.FindAll("dt").Count.ShouldBe(2);
    }

    [Fact]
    public void OsirionRecordCard_TitleWithoutStatus_RendersNoStatus()
    {
        var cut = RenderStatic<OsirionRecordCard>(p => p.Add(c => c.Title, "Ticket 7"));

        cut.Find("h3").TextContent.ShouldBe("Ticket 7");
        cut.FindAll(".osirion-record-card-status").ShouldBeEmpty();
        cut.FindAll("dl").ShouldBeEmpty();
    }

    [Fact]
    public void OsirionRecordCard_NothingToShow_RendersNothing()
    {
        var cut = RenderStatic<OsirionRecordCard>(p => p.Add(c => c.Status, " "));

        cut.Markup.Trim().ShouldBeEmpty();
    }

    [Theory]
    [InlineData(1, "h1")]
    [InlineData(2, "h2")]
    [InlineData(4, "h4")]
    [InlineData(5, "h5")]
    [InlineData(6, "h6")]
    [InlineData(-1, "h1")]
    [InlineData(7, "h6")]
    public void OsirionRecordCard_HeadingLevel_SetsTheTitleElementWithinOneToSix(int level, string element)
    {
        var cut = RenderStatic<OsirionRecordCard>(p => p
            .Add(c => c.Title, "Ticket 7")
            .Add(c => c.HeadingLevel, level));

        cut.Find($"{element}.osirion-record-card-title").TextContent.ShouldBe("Ticket 7");
    }

    [Fact]
    public void OsirionRecordCard_TextIsEncoded()
    {
        var cut = RenderStatic<OsirionRecordCard>(p => p
            .Add(c => c.Title, "<b>t</b>")
            .Add(c => c.Rows, [new RecordCardRow("<i>l</i>", "<u>v</u>")]));

        cut.FindAll("b").ShouldBeEmpty();
        cut.FindAll("i").ShouldBeEmpty();
        cut.FindAll("u").ShouldBeEmpty();
        cut.Find("dd").TextContent.ShouldBe("<u>v</u>");
    }

    [Fact]
    public void OsirionRecordCard_ClassStyleAndAttributes_GoOnTheFigure()
    {
        var cut = RenderStatic<OsirionRecordCard>(p => p
            .Add(c => c.Title, "Ticket 7")
            .Add(c => c.Class, "extra")
            .Add(c => c.Style, "max-width: 24rem")
            .AddUnmatched("aria-label", "Sample ticket"));

        var figure = cut.Find("figure");
        figure.ClassList.ShouldBe(["osirion-record-card", "extra"]);
        figure.GetAttribute("style").ShouldBe("max-width: 24rem");
        figure.GetAttribute("aria-label").ShouldBe("Sample ticket");
    }
}
