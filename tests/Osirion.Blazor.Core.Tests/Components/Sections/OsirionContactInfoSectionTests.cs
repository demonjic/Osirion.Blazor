using System.Text.RegularExpressions;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Models;
using Osirion.Blazor.Core.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Components.Sections;

public class OsirionContactInfoSectionTests : OsirionComponentTestBase
{
    private IRenderedComponent<OsirionContactInfoSection> Render(ContactInformation info) =>
        RenderStatic<OsirionContactInfoSection>(parameters => parameters.Add(p => p.ContactInfo, info));

    [Fact]
    public void ContactInfoSection_Renders()
    {
        var cut = Render(new ContactInformation { Email = "a@b.example" });

        cut.Find("a[href='mailto:a@b.example']").TextContent.ShouldBe("a@b.example");
    }

    // A guard test: a site that adds no extra items must keep getting the markup it always got.
    [Fact]
    public void ContactInfoSection_NoExtraItems_RendersTheMarkupItAlwaysDid()
    {
        var cut = Render(new ContactInformation
        {
            Address = "1 Main Street",
            Phone = "+1 555 0100",
            Email = "a@b.example",
            Website = "https://example.org",
            Message = "Write or call."
        });

        NormalizedMarkup(cut).ShouldBe(UnchangedMarkup);
    }

    private const string UnchangedMarkup =
        """
        <div class="osirion-contact-info-section"><h2 class="osirion-contact-info-title">Contact information</h2><p class="osirion-contact-info-description">Write or call.</p><div class="osirion-contact-info-list"><div class="osirion-contact-info-item"><div class="osirion-contact-info-icon"><svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7zm0 9.5c-1.38 0-2.5-1.12-2.5-2.5s1.12-2.5 2.5-2.5 2.5 1.12 2.5 2.5-1.12 2.5-2.5 2.5z"></path></svg></div><div class="osirion-contact-info-content"><span class="osirion-contact-info-label">Address:</span><span class="osirion-contact-info-value">1 Main Street</span></div></div><div class="osirion-contact-info-item"><div class="osirion-contact-info-icon"><svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M6.62 10.79c1.44 2.83 3.76 5.14 6.59 6.59l2.2-2.2c.27-.27.67-.36 1.02-.24 1.12.37 2.33.57 3.57.57.55 0 1 .45 1 1V20c0 .55-.45 1-1 1-9.39 0-17-7.61-17-17 0-.55.45-1 1-1h3.5c.55 0 1 .45 1 1 0 1.25.2 2.45.57 3.57.11.35.03.74-.25 1.02l-2.2 2.2z"></path></svg></div><div class="osirion-contact-info-content"><span class="osirion-contact-info-label">Phone:</span><a href="tel:+1 555 0100" class="osirion-contact-info-value osirion-contact-link">+1 555 0100</a></div></div><div class="osirion-contact-info-item"><div class="osirion-contact-info-icon"><svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M20 4H4c-1.1 0-1.99.9-1.99 2L2 18c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 4l-8 5-8-5V6l8 5 8-5v2z"></path></svg></div><div class="osirion-contact-info-content"><span class="osirion-contact-info-label">Email:</span><a href="mailto:a@b.example" class="osirion-contact-info-value osirion-contact-link">a@b.example</a></div></div><div class="osirion-contact-info-item"><div class="osirion-contact-info-icon"><svg viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-1 17.93c-3.94-.49-7-3.85-7-7.93 0-.62.08-1.21.21-1.79L9 15v1c0 1.1.9 2 2 2v1.93zm6.9-2.54c-.26-.81-1-1.39-1.9-1.39h-1v-3c0-.55-.45-1-1-1H8v-2h2c.55 0 1-.45 1-1V7h2c1.1 0 2-.9 2-2v-.41c2.93 1.19 5 4.06 5 7.41 0 2.08-.8 3.97-2.1 5.39z"></path></svg></div><div class="osirion-contact-info-content"><span class="osirion-contact-info-label">Website:</span><a href="https://example.org" target="_blank" rel="noopener noreferrer" class="osirion-contact-info-value osirion-contact-link">example.org</a></div></div></div></div>
        """;

    [Fact]
    public void ContactInfoSection_AdditionalItems_AreRenderedAfterTheFixedRows()
    {
        var cut = Render(new ContactInformation
        {
            Website = "https://example.org",
            AdditionalItems =
            [
                new ContactInfoItem("Working hours:", "9 a.m. to noon", Icon: ContactInfoIcon.Clock),
                new ContactInfoItem("Reply time:", "Within one business day")
            ]
        });

        var rows = cut.FindAll(".osirion-contact-info-item");
        rows.Select(row => row.QuerySelector(".osirion-contact-info-label")!.TextContent)
            .ShouldBe(["Website:", "Working hours:", "Reply time:"]);
        rows[1].QuerySelector(".osirion-contact-info-value")!.TextContent.ShouldBe("9 a.m. to noon");
        rows[1].QuerySelector(".osirion-contact-info-value")!.TagName.ShouldBe("SPAN");
        rows[1].QuerySelector(".osirion-contact-info-icon svg")!.GetAttribute("aria-hidden").ShouldBe("true");
    }

    [Fact]
    public void ContactInfoSection_AdditionalItemWithoutIcon_KeepsAnEmptyIconBoxSoRowsLineUp()
    {
        var cut = Render(new ContactInformation
        {
            AdditionalItems = [new ContactInfoItem("Reply time:", "Within one business day")]
        });

        var icon = cut.Find(".osirion-contact-info-icon");
        icon.QuerySelector("svg").ShouldBeNull();
    }

    [Theory]
    [InlineData(ContactInfoIcon.Address)]
    [InlineData(ContactInfoIcon.Phone)]
    [InlineData(ContactInfoIcon.Email)]
    [InlineData(ContactInfoIcon.Website)]
    [InlineData(ContactInfoIcon.Calendar)]
    [InlineData(ContactInfoIcon.Clock)]
    [InlineData(ContactInfoIcon.Link)]
    public void ContactInfoSection_AdditionalItemIcon_DrawsAnSvgPath(ContactInfoIcon icon)
    {
        var cut = Render(new ContactInformation { AdditionalItems = [new ContactInfoItem("Label:", "Text", Icon: icon)] });

        cut.Find(".osirion-contact-info-icon svg path").GetAttribute("d").ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void ContactInfoSection_AdditionalItemIcons_ReuseTheIconsOfTheFixedRows()
    {
        var fixedRows = Render(new ContactInformation { Address = "A", Phone = "1", Email = "a@b.example", Website = "https://example.org" });
        var extraRows = Render(new ContactInformation
        {
            AdditionalItems =
            [
                new ContactInfoItem("a", "A", Icon: ContactInfoIcon.Address),
                new ContactInfoItem("p", "1", Icon: ContactInfoIcon.Phone),
                new ContactInfoItem("e", "E", Icon: ContactInfoIcon.Email),
                new ContactInfoItem("w", "W", Icon: ContactInfoIcon.Website)
            ]
        });

        extraRows.FindAll(".osirion-contact-info-icon path").Select(path => path.GetAttribute("d"))
            .ShouldBe(fixedRows.FindAll(".osirion-contact-info-icon path").Select(path => path.GetAttribute("d")));
    }

    [Fact]
    public void ContactInfoSection_AdditionalItemWithAWebAddress_OpensInANewTabWithoutAReferrer()
    {
        var cut = Render(new ContactInformation
        {
            AdditionalItems = [new ContactInfoItem("LinkedIn:", "Dejan Demonjic", "https://www.linkedin.com/in/example")]
        });

        var link = cut.Find("a.osirion-contact-link");
        link.GetAttribute("href").ShouldBe("https://www.linkedin.com/in/example");
        link.GetAttribute("target").ShouldBe("_blank");
        link.GetAttribute("rel").ShouldBe("noopener noreferrer");
        link.TextContent.ShouldBe("Dejan Demonjic");
    }

    [Fact]
    public void ContactInfoSection_AdditionalItemWithAPathOnTheSite_StaysInTheTab()
    {
        var cut = Render(new ContactInformation
        {
            AdditionalItems = [new ContactInfoItem("Book:", "Book a call", "/book")]
        });

        var link = cut.Find("a.osirion-contact-link");
        link.GetAttribute("href").ShouldBe("/book");
        link.HasAttribute("target").ShouldBeFalse();
        link.HasAttribute("rel").ShouldBeFalse();
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:text/html,x")]
    [InlineData("//evil.example/x")]
    [InlineData("vbscript:x")]
    public void ContactInfoSection_AdditionalItemWithAnUnsafeHref_ShowsPlainText(string href)
    {
        var cut = Render(new ContactInformation
        {
            AdditionalItems = [new ContactInfoItem("Book:", "Book a call", href)]
        });

        cut.FindAll("a").ShouldBeEmpty();
        cut.Find(".osirion-contact-info-value").TextContent.ShouldBe("Book a call");
    }

    [Fact]
    public void ContactInfoSection_AdditionalItem_EncodesLabelAndText()
    {
        var cut = Render(new ContactInformation
        {
            AdditionalItems = [new ContactInfoItem("<b>L</b>", "<script>x()</script>")]
        });

        cut.FindAll("b, script").ShouldBeEmpty();
        cut.Find(".osirion-contact-info-label").TextContent.ShouldBe("<b>L</b>");
        cut.Find(".osirion-contact-info-value").TextContent.ShouldBe("<script>x()</script>");
    }

    [Fact]
    public void ContactInfoSection_AdditionalItemWithoutText_IsLeftOut()
    {
        var cut = Render(new ContactInformation
        {
            AdditionalItems = [new ContactInfoItem("Empty:", " "), new ContactInfoItem("Kept:", "x")]
        });

        cut.FindAll(".osirion-contact-info-item").Count.ShouldBe(1);
    }

    // The markup without the scope attribute and the white space between tags, which change with the build.
    private static string NormalizedMarkup(IRenderedComponent<OsirionContactInfoSection> cut) =>
        Regex.Replace(Regex.Replace(cut.Markup, " b-[a-z0-9]{10}", string.Empty), @">\s+<", "><");

    // Fails before the fix: Class was written twice on the root, once by CombineCssClasses and once by the markup.
    [Fact]
    public void ContactInfoSection_Class_IsWrittenOnceOnTheRoot()
    {
        var cut = RenderStatic<OsirionContactInfoSection>(parameters => parameters
            .Add(p => p.ContactInfo, new ContactInformation { Email = "a@b.example" })
            .Add(p => p.Class, "extra"));

        cut.Find("div.osirion-contact-info-section").GetAttribute("class")!.Trim().ShouldBe("osirion-contact-info-section extra");
    }

    // Fails before the fix: StartsWith made a lookalike host count as the current site.
    [Fact]
    public void ContactInfoSection_LookalikeHostOfTheCurrentSite_IsAnExternalLink()
    {
        var baseUri = Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>().BaseUri.TrimEnd('/');

        var cut = Render(new ContactInformation { Website = baseUri + ".evil.net" });

        var link = cut.Find("a.osirion-contact-link");
        link.GetAttribute("href").ShouldBe(baseUri + ".evil.net");
        link.GetAttribute("target").ShouldBe("_blank");
    }

    // Fails before the fix: every same-site website was rewritten to "/".
    [Fact]
    public void ContactInfoSection_SameSiteWebsiteWithPath_KeepsThePath()
    {
        var baseUri = Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>().BaseUri;

        var cut = Render(new ContactInformation { Website = baseUri + "about?x=1" });

        var link = cut.Find("a.osirion-contact-link");
        link.GetAttribute("href").ShouldBe("/about?x=1");
        link.GetAttribute("target").ShouldBe("_self");
    }

    // Fails before the fix: GetWebsiteDisplayText indexed [0] of an empty array and threw during render.
    [Theory]
    [InlineData("https://")]
    [InlineData("http:///")]
    [InlineData("   ,  ")]
    public void ContactInfoSection_WebsiteWithoutHost_DoesNotThrowAndRendersNoLink(string website)
    {
        var cut = Render(new ContactInformation { Website = website });

        cut.FindAll("a.osirion-contact-link").ShouldBeEmpty();
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:text/html,<script>alert(1)</script>")]
    [InlineData("vbscript:msgbox(1)")]
    public void ContactInfoSection_NonHttpWebsite_RendersNoLink(string website)
    {
        var cut = Render(new ContactInformation { Website = website });

        cut.FindAll("a.osirion-contact-link").ShouldBeEmpty();
        cut.Markup.ShouldNotContain("javascript:");
    }

    [Fact]
    public void ContactInfoSection_SchemelessWebsite_IsLinkedAsHttps()
    {
        var cut = Render(new ContactInformation { Website = "example.org" });

        var link = cut.Find("a.osirion-contact-link");
        link.GetAttribute("href").ShouldBe("https://example.org/");
        link.TextContent.ShouldBe("example.org");
        link.GetAttribute("target").ShouldBe("_blank");
    }

    // Fails before the fix: phone, email and website text were written as raw markup.
    [Fact]
    public void ContactInfoSection_PhoneEmailAndWebsiteText_IsEncoded()
    {
        var cut = Render(new ContactInformation
        {
            Phone = "<img src=x onerror=alert(1)>",
            Email = "<img src=y onerror=alert(2)>"
        });

        cut.FindAll("img").ShouldBeEmpty();
        cut.FindAll("a.osirion-contact-link").Count.ShouldBe(2);
    }

    // Address and message may carry intentional markup (a consumer passes a paragraph), but never script.
    [Fact]
    public void ContactInfoSection_AddressMarkup_IsSanitized()
    {
        var cut = Render(new ContactInformation
        {
            Address = "<p>Main street 1</p><script>alert(1)</script><img src=x onerror=alert(2)>",
            Message = "Hello <b>there</b><script>alert(3)</script>"
        });

        cut.FindAll("script").ShouldBeEmpty();
        cut.Markup.ShouldNotContain("onerror");
        cut.Find(".osirion-contact-info-value").InnerHtml.ShouldContain("<p>Main street 1</p>");
        cut.Find(".osirion-contact-info-description").InnerHtml.ShouldContain("<b>there</b>");
    }
}
