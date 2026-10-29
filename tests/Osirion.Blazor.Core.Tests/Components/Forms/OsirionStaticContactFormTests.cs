using AngleSharp.Dom;
using BlazorJSComponents;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Models;
using Osirion.Blazor.Core.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Components.Forms;

public class OsirionStaticContactFormTests : OsirionComponentTestBase
{
    private const string Endpoint = "/api/contact";

    private readonly RecordingLoggerFactory _loggerFactory = new();

    public OsirionStaticContactFormTests()
    {
        Services.AddJSComponents();
        Services.AddSingleton<ILoggerFactory>(_loggerFactory);
    }

    [Fact]
    public void StaticContactForm_SafeEndpoint_RendersAHiddenFormThatPostsToIt()
    {
        var cut = RenderForm();

        var form = cut.Find("form");
        form.GetAttribute("action").ShouldBe(Endpoint);
        form.GetAttribute("method").ShouldBe("post");
        form.HasAttribute("hidden").ShouldBeTrue();
        form.ClassList.ShouldContain("osirion-static-contact-form-form");
    }

    [Fact]
    public void StaticContactForm_Fields_AreLabelledAndNamedForTheEndpoint()
    {
        var cut = RenderForm();

        var name = Field(cut, "name");
        name.GetAttribute("type").ShouldBe("text");
        name.GetAttribute("autocomplete").ShouldBe("name");
        AccessibleLabelOf(cut, name).ShouldBe("Name");

        var email = Field(cut, "email");
        email.GetAttribute("type").ShouldBe("email");
        email.GetAttribute("autocomplete").ShouldBe("email");
        AccessibleLabelOf(cut, email).ShouldBe("Email");

        var message = Field(cut, "message");
        message.TagName.ShouldBe("TEXTAREA");
        AccessibleLabelOf(cut, message).ShouldBe("Message");

        cut.FindAll("[name]").Select(field => field.GetAttribute("name")).ShouldBe(["name", "email", "message", "website"]);
    }

    [Fact]
    public void StaticContactForm_Fields_AreRequiredWithinTheEndpointLimits()
    {
        var cut = RenderForm();

        var name = Field(cut, "name");
        name.HasAttribute("required").ShouldBeTrue();
        name.GetAttribute("maxlength").ShouldBe("100");

        var email = Field(cut, "email");
        email.HasAttribute("required").ShouldBeTrue();
        email.GetAttribute("maxlength").ShouldBe("254");

        var message = Field(cut, "message");
        message.HasAttribute("required").ShouldBeTrue();
        message.GetAttribute("minlength").ShouldBe("10");
        message.GetAttribute("maxlength").ShouldBe("5000");
    }

    [Fact]
    public void StaticContactForm_Defaults_MarkEachRequiredFieldForTheEyeOnly()
    {
        var cut = RenderForm();

        foreach (var fieldName in new[] { "name", "email", "message" })
        {
            var marker = LabelOf(cut, Field(cut, fieldName)).QuerySelector(".osirion-static-contact-form-required")
                .ShouldNotBeNull($"the {fieldName} label has no required marker");
            marker.TextContent.ShouldBe("*");
            marker.GetAttribute("aria-hidden").ShouldBe("true");
        }
    }

    [Fact]
    public void StaticContactForm_ShowCompanyField_LeavesTheOptionalFieldUnmarked()
    {
        var cut = RenderForm(parameters => parameters.Add(p => p.ShowCompanyField, true));

        LabelOf(cut, Field(cut, "company")).QuerySelector(".osirion-static-contact-form-required").ShouldBeNull();
        cut.FindAll(".osirion-static-contact-form-required").Count.ShouldBe(3);
    }

    [Fact]
    public void StaticContactForm_WithoutRequiredIndicators_RendersNoMarkers()
    {
        var cut = RenderForm(parameters => parameters
            .Add(p => p.ShowCompanyField, true)
            .Add(p => p.ShowRequiredIndicators, false));

        cut.FindAll(".osirion-static-contact-form-required").ShouldBeEmpty();
        LabelOf(cut, Field(cut, "name")).TextContent.Trim().ShouldBe("Name");
    }

    [Fact]
    public void StaticContactForm_Honeypot_IsHiddenFromPeopleAndAssistiveTechnology()
    {
        var cut = RenderForm();

        var trap = Field(cut, "website");
        trap.GetAttribute("tabindex").ShouldBe("-1");
        trap.GetAttribute("autocomplete").ShouldBe("off");
        trap.HasAttribute("required").ShouldBeFalse();
        var wrapper = trap.ParentElement.ShouldNotBeNull();
        wrapper.GetAttribute("aria-hidden").ShouldBe("true");
        wrapper.ClassList.ShouldContain("osirion-static-contact-form-trap");
    }

    [Fact]
    public void StaticContactForm_WithoutShowCompanyField_RendersNoCompanyField()
    {
        var cut = RenderForm();

        cut.FindAll("[name=company]").ShouldBeEmpty();
    }

    [Fact]
    public void StaticContactForm_ShowCompanyField_RendersAnOptionalLabelledOrganizationField()
    {
        var cut = RenderForm(parameters => parameters
            .Add(p => p.ShowCompanyField, true)
            .Add(p => p.CompanyLabel, "Organization"));

        var company = Field(cut, "company");
        company.GetAttribute("autocomplete").ShouldBe("organization");
        company.GetAttribute("maxlength").ShouldBe("200");
        company.HasAttribute("required").ShouldBeFalse();
        AccessibleLabelOf(cut, company).ShouldBe("Organization");
        cut.FindAll("[name]").Select(field => field.GetAttribute("name")).ShouldBe(["name", "email", "company", "message", "website"]);
    }

    [Fact]
    public void StaticContactForm_TurnstileSiteKey_RendersTheWidgetContainerWithTheKey()
    {
        var cut = RenderForm(parameters => parameters.Add(p => p.TurnstileSiteKey, " 1x00000000000000000000AA "));

        var container = cut.Find(".osirion-static-contact-form-turnstile");
        container.GetAttribute("data-sitekey").ShouldBe("1x00000000000000000000AA");
        container.ChildElementCount.ShouldBe(0);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void StaticContactForm_WithoutTurnstileSiteKey_RendersNoWidgetContainer(string? siteKey)
    {
        var cut = RenderForm(parameters => parameters.Add(p => p.TurnstileSiteKey, siteKey));

        cut.FindAll(".osirion-static-contact-form-turnstile").ShouldBeEmpty();
        cut.FindAll("[data-sitekey]").ShouldBeEmpty();
    }

    [Fact]
    public void StaticContactForm_StatusRegion_IsAnEmptyLiveRegionBesideTheSubmitButton()
    {
        var cut = RenderForm();

        var button = cut.Find("form button");
        button.GetAttribute("type").ShouldBe("submit");
        button.TextContent.Trim().ShouldBe("Send message");
        var status = cut.Find("form [role=status]");
        status.TextContent.ShouldBeEmpty();
        status.ParentElement.ShouldBe(button.ParentElement);
    }

    [Fact]
    public void StaticContactForm_Messages_ArePassedToTheScriptOnTheForm()
    {
        var cut = RenderForm(parameters => parameters
            .Add(p => p.SubmitButtonText, "Send")
            .Add(p => p.SubmittingText, "Sending now")
            .Add(p => p.SuccessMessage, "Sent.")
            .Add(p => p.FieldsErrorMessage, "Fields.")
            .Add(p => p.CheckErrorMessage, "Check.")
            .Add(p => p.CheckPendingMessage, "Pending.")
            .Add(p => p.ErrorMessage, "Failed."));

        var form = cut.Find("form");
        form.GetAttribute("data-submit-text").ShouldBe("Send");
        form.GetAttribute("data-submitting-text").ShouldBe("Sending now");
        form.GetAttribute("data-success-message").ShouldBe("Sent.");
        form.GetAttribute("data-fields-error-message").ShouldBe("Fields.");
        form.GetAttribute("data-check-error-message").ShouldBe("Check.");
        form.GetAttribute("data-check-pending-message").ShouldBe("Pending.");
        form.GetAttribute("data-error-message").ShouldBe("Failed.");
    }

    [Fact]
    public void StaticContactForm_DefaultMessages_NameTheLimitsTheFieldsUse()
    {
        var cut = RenderForm();

        var form = cut.Find("form");
        form.GetAttribute("data-submitting-text").ShouldBe("Sending...");
        form.GetAttribute("data-success-message").ShouldBe("Thank you. Your message was sent.");
        form.GetAttribute("data-fields-error-message")
            .ShouldBe("Check the fields: your name, a valid email address and a message of at least 10 characters.");
    }

    [Fact]
    public void StaticContactForm_LabelsMessagesAndPrivacyNote_AreEncoded()
    {
        var cut = RenderForm(parameters => parameters
            .Add(p => p.NameLabel, "<b>Name</b>")
            .Add(p => p.SuccessMessage, "\"><img src=x onerror=alert(1)>")
            .Add(p => p.PrivacyNote, "<script>alert(1)</script> & more"));

        AccessibleLabelOf(cut, Field(cut, "name")).ShouldBe("<b>Name</b>");
        cut.Find("form").GetAttribute("data-success-message").ShouldBe("\"><img src=x onerror=alert(1)>");
        cut.Find(".osirion-static-contact-form-privacy").TextContent.ShouldBe("<script>alert(1)</script> & more");
        cut.FindAll("b, img, form script").ShouldBeEmpty();
    }

    [Fact]
    public void StaticContactForm_WithoutPrivacyNote_RendersNoNote()
    {
        var cut = RenderForm();

        cut.FindAll(".osirion-static-contact-form-privacy").ShouldBeEmpty();
    }

    [Fact]
    public void StaticContactForm_NoScriptContent_IsRenderedInsideNoscriptOutsideTheForm()
    {
        var cut = RenderForm(parameters => parameters
            .Add(p => p.NoScriptContent, "<p>Email <a href=\"mailto:hello@example.com\">hello@example.com</a>.</p>"));

        var noscript = cut.Find(".osirion-static-contact-form > noscript");
        noscript.InnerHtml.ShouldContain("mailto:hello@example.com");
        cut.FindAll("form noscript").ShouldBeEmpty();
    }

    // Fails before the fix: the noscript carried the component's CSS isolation attribute (b-...), which no rule uses
    // there and which showed up in every exported page. The elements the scoped rules style keep it.
    [Fact]
    public void StaticContactForm_NoScript_CarriesNoCssIsolationAttribute()
    {
        var cut = RenderForm(parameters => parameters.Add(p => p.NoScriptContent, "<p>Email us.</p>"));

        cut.Find("noscript").Attributes.ShouldBeEmpty();
        cut.Find(".osirion-static-contact-form").Attributes.Select(a => a.Name)
            .ShouldContain(name => name.StartsWith("b-", StringComparison.Ordinal));
        cut.Find(".osirion-static-contact-form-form").Attributes.Select(a => a.Name)
            .ShouldContain(name => name.StartsWith("b-", StringComparison.Ordinal));
    }

    [Fact]
    public void StaticContactForm_WithoutNoScriptContent_RendersNoNoscript()
    {
        var cut = RenderForm();

        cut.FindAll("noscript").ShouldBeEmpty();
    }

    [Theory]
    [InlineData("/api/contact")]
    [InlineData("/")]
    [InlineData("https://forms.example.com/contact")]
    public void StaticContactForm_RootRelativeOrHttpsEndpoint_IsAccepted(string endpoint)
    {
        var cut = RenderForm(endpoint: endpoint);

        cut.Find("form").GetAttribute("action").ShouldBe(endpoint);
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("JavaScript:alert(1)")]
    [InlineData("data:text/html,<p>x</p>")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("//evil.example/api")]
    [InlineData("/\\evil.example/api")]
    [InlineData("http://example.com/api")]
    [InlineData("mailto:hello@example.com")]
    [InlineData("api/contact")]
    public void StaticContactForm_UnsafeEndpoint_RendersNothing(string endpoint)
    {
        var cut = RenderForm(parameters => parameters.Add(p => p.NoScriptContent, "<p>Email us.</p>"), endpoint);

        cut.Markup.Trim().ShouldBeEmpty();
        cut.FindComponents<JS>().ShouldBeEmpty();
    }

    [Fact]
    public void StaticContactForm_UnsafeEndpoint_InDevelopment_LogsAWarning()
    {
        UseEnvironment(Environments.Development);

        RenderForm(endpoint: "javascript:alert(1)");

        var warning = _loggerFactory.Entries.ShouldHaveSingleItem();
        warning.Level.ShouldBe(LogLevel.Warning);
        warning.Message.ShouldBe(
            "OsirionStaticContactForm renders nothing: its Endpoint 'javascript:alert(1)' is not a path on the site or an https URL.");
    }

    [Fact]
    public void StaticContactForm_UnsafeEndpoint_OutsideDevelopment_LogsNothing()
    {
        UseEnvironment(Environments.Production);

        RenderForm(endpoint: "javascript:alert(1)");

        _loggerFactory.Entries.ShouldBeEmpty();
    }

    [Fact]
    public void StaticContactForm_SafeEndpoint_InDevelopment_LogsNothing()
    {
        UseEnvironment(Environments.Development);

        RenderForm();

        _loggerFactory.Entries.ShouldBeEmpty();
    }

    [Fact]
    public void StaticContactForm_Script_LoadsThroughBlazorJSComponentsWithTheFormReferences()
    {
        var cut = RenderForm(parameters => parameters.Add(p => p.TurnstileSiteKey, "1x00000000000000000000AA"));

        var script = cut.FindComponent<JS>().Instance;
        script.Src.ShouldBe("./_content/Osirion.Blazor.Core/Components/Forms/OsirionStaticContactForm.razor.js");
        script.Args.ShouldNotBeNull().Length.ShouldBe(1);
        cut.Find("form").GetAttribute("data-ref").ShouldNotBeNullOrEmpty();
        cut.Find("form [role=status]").GetAttribute("data-ref").ShouldNotBeNullOrEmpty();
        cut.Find("form button").GetAttribute("data-ref").ShouldNotBeNullOrEmpty();
        cut.Find(".osirion-static-contact-form-turnstile").GetAttribute("data-ref").ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void StaticContactForm_Markup_HasNoInlineScriptOrEventHandler()
    {
        var cut = RenderForm(parameters => parameters
            .Add(p => p.TurnstileSiteKey, "1x00000000000000000000AA")
            .Add(p => p.ShowCompanyField, true)
            .Add(p => p.PrivacyNote, "Note."));

        var root = cut.Find(".osirion-static-contact-form");
        root.QuerySelectorAll("script").ShouldBeEmpty();
        root.QuerySelectorAll("*").SelectMany(element => element.Attributes)
            .Select(attribute => attribute.Name)
            .Where(name => name.StartsWith("on", StringComparison.OrdinalIgnoreCase))
            .ShouldBeEmpty();
        root.QuerySelectorAll("[style]").ShouldBeEmpty();
    }

    [Fact]
    public void StaticContactForm_IdsAreUniquePerInstance()
    {
        var first = RenderForm();
        var second = RenderForm();

        Field(first, "name").Id.ShouldNotBe(Field(second, "name").Id);
    }

    [Fact]
    public void StaticContactForm_ClassStyleAndAttributes_GoOnTheRootElement()
    {
        var cut = RenderForm(parameters => parameters
            .Add(p => p.Class, "site-contact")
            .Add(p => p.Style, "max-width: 40rem")
            .AddUnmatched("id", "contact-form"));

        var root = cut.Find("div.osirion-static-contact-form");
        root.ClassList.ShouldContain("site-contact");
        root.GetAttribute("style").ShouldBe("max-width: 40rem");
        root.Id.ShouldBe("contact-form");
    }

    [Fact]
    public void StaticContactForm_SubjectFieldUnused_RendersNoSubject()
    {
        var cut = RenderForm();

        cut.FindAll("[name=subject]").ShouldBeEmpty();
    }

    [Fact]
    public void StaticContactForm_ShowSubjectField_AddsAnOptionalLabelledFieldBeforeTheMessage()
    {
        var cut = RenderForm(parameters => parameters
            .Add(p => p.ShowSubjectField, true)
            .Add(p => p.ShowCompanyField, true));

        var subject = Field(cut, "subject");
        subject.GetAttribute("type").ShouldBe("text");
        subject.GetAttribute("maxlength").ShouldBe("200");
        subject.HasAttribute("required").ShouldBeFalse();
        AccessibleLabelOf(cut, subject).ShouldBe("Subject");
        cut.FindAll("[name]").Select(field => field.GetAttribute("name"))
            .ShouldBe(["name", "email", "company", "subject", "message", "website"]);
    }

    [Fact]
    public void StaticContactForm_SubjectLabel_ReplacesTheDefault()
    {
        var cut = RenderForm(parameters => parameters
            .Add(p => p.ShowSubjectField, true)
            .Add(p => p.SubjectLabel, "Topic"));

        AccessibleLabelOf(cut, Field(cut, "subject")).ShouldBe("Topic");
    }

    [Fact]
    public void StaticContactForm_Placeholders_AreWrittenOnlyWhenSet()
    {
        var plain = RenderForm(parameters => parameters.Add(p => p.ShowSubjectField, true));
        plain.FindAll("[placeholder]").ShouldBeEmpty();

        var cut = RenderForm(parameters => parameters
            .Add(p => p.ShowSubjectField, true)
            .Add(p => p.NamePlaceholder, "Your name")
            .Add(p => p.EmailPlaceholder, "Your email address")
            .Add(p => p.SubjectPlaceholder, "Subject")
            .Add(p => p.MessagePlaceholder, "Your message"));

        Field(cut, "name").GetAttribute("placeholder").ShouldBe("Your name");
        Field(cut, "email").GetAttribute("placeholder").ShouldBe("Your email address");
        Field(cut, "subject").GetAttribute("placeholder").ShouldBe("Subject");
        Field(cut, "message").GetAttribute("placeholder").ShouldBe("Your message");
    }

    [Fact]
    public void StaticContactForm_FormTitleAndDescription_AreRenderedAboveTheForm()
    {
        var cut = RenderForm(parameters => parameters
            .Add(p => p.FormTitle, "Get in touch")
            .Add(p => p.FormDescription, "Tell me <b>what</b> you need."));

        var container = cut.Find("div.osirion-static-contact-form");
        container.Children.Select(child => child.TagName).ShouldBe(["H2", "P", "FORM"]);
        cut.Find("h2").TextContent.ShouldBe("Get in touch");
        cut.Find("p.osirion-static-contact-form-description").TextContent.ShouldBe("Tell me <b>what</b> you need.");
        cut.FindAll("b").ShouldBeEmpty();
    }

    [Fact]
    public void StaticContactForm_WithoutContactInfo_RendersNoPanelAndNoLayoutClass()
    {
        var cut = RenderForm();

        cut.FindAll(".osirion-contact-info-section").ShouldBeEmpty();
        cut.Find("div.osirion-static-contact-form").ClassList
            .ShouldBe(["osirion-static-contact-form"]);
    }

    [Fact]
    public void StaticContactForm_ContactInfo_RendersThePanelAfterTheFormBesideIt()
    {
        var cut = RenderForm(parameters => parameters
            .Add(p => p.FormTitle, "Get in touch")
            .Add(p => p.ContactInfo, new ContactInformation
            {
                Address = "Belgrade, Serbia",
                AdditionalItems = [new ContactInfoItem("Reply time:", "Within one business day", Icon: ContactInfoIcon.Clock)]
            }));

        var container = cut.Find("div.osirion-static-contact-form");
        container.ClassList.ShouldBe(["osirion-static-contact-form", "osirion-static-contact-form-side-by-side"]);
        container.Children.Select(child => child.ClassList.First()).ShouldBe(
            ["osirion-static-contact-form-main", "osirion-contact-info-section"]);
        cut.Find(".osirion-static-contact-form-main > form").ShouldNotBeNull();
        cut.Find(".osirion-static-contact-form-main > h2").TextContent.ShouldBe("Get in touch");

        var panel = cut.Find(".osirion-contact-info-section");
        panel.ClassList.ShouldContain("osirion-static-contact-form-panel");
        panel.QuerySelector("h2")!.TextContent.ShouldBe("Contact information");
        panel.QuerySelectorAll(".osirion-contact-info-label").Select(label => label.TextContent)
            .ShouldBe(["Address:", "Reply time:"]);
    }

    [Fact]
    public void StaticContactForm_ContactInfo_KeepsTheNoScriptContentWithTheForm()
    {
        var cut = RenderForm(parameters => parameters
            .Add(p => p.ContactInfo, new ContactInformation { Address = "Belgrade" })
            .Add(p => p.NoScriptContent, builder => builder.AddContent(0, "Book a call instead.")));

        cut.Find(".osirion-static-contact-form-main > noscript").ShouldNotBeNull();
    }

    [Fact]
    public void StaticContactForm_ContactInfoTitle_ReplacesTheDefault()
    {
        var cut = RenderForm(parameters => parameters
            .Add(p => p.ContactInfo, new ContactInformation { Address = "Belgrade" })
            .Add(p => p.ContactInfoTitle, "Other ways to reach me"));

        cut.Find(".osirion-contact-info-section h2").TextContent.ShouldBe("Other ways to reach me");
    }

    [Fact]
    public void StaticContactForm_SideBySideLayoutOff_StacksThePanelBelowTheForm()
    {
        var cut = RenderForm(parameters => parameters
            .Add(p => p.SideBySideLayout, false)
            .Add(p => p.ContactInfo, new ContactInformation { Address = "Belgrade" }));

        cut.Find("div.osirion-static-contact-form").ClassList
            .ShouldBe(["osirion-static-contact-form", "osirion-static-contact-form-stacked"]);
    }

    [Fact]
    public void StaticContactForm_ShowContactInformationOff_HidesThePanel()
    {
        var cut = RenderForm(parameters => parameters
            .Add(p => p.ShowContactInformation, false)
            .Add(p => p.ContactInfo, new ContactInformation { Address = "Belgrade" }));

        cut.FindAll(".osirion-contact-info-section").ShouldBeEmpty();
        cut.Find("div.osirion-static-contact-form").ClassList.ShouldBe(["osirion-static-contact-form"]);
    }

    [Fact]
    public void StaticContactForm_UnsafeEndpoint_RendersNoPanelEither()
    {
        var cut = RenderForm(
            parameters => parameters.Add(p => p.ContactInfo, new ContactInformation { Address = "Belgrade" }),
            endpoint: "javascript:alert(1)");

        cut.Markup.ShouldNotContain("osirion-contact-info-section");
    }

    // A guard test: a site that uses none of the layout parameters must keep getting the markup it always got.
    [Fact]
    public void StaticContactForm_LayoutParametersUnused_RendersTheMarkupItAlwaysDid()
    {
        var cut = RenderForm(parameters => parameters
            .Add(p => p.ShowCompanyField, true)
            .Add(p => p.PrivacyNote, "Note"));

        NormalizedMarkup(cut).ShouldBe(UnchangedMarkup);
    }

    // A guard test: an empty list of additional fields renders the markup the form always rendered.
    [Fact]
    public void StaticContactForm_EmptyAdditionalFields_RendersTheMarkupItAlwaysDid()
    {
        var cut = RenderForm(parameters => parameters
            .Add(p => p.ShowCompanyField, true)
            .Add(p => p.PrivacyNote, "Note")
            .Add(p => p.AdditionalFields, Array.Empty<ContactFormField>()));

        NormalizedMarkup(cut).ShouldBe(UnchangedMarkup);
    }

    // The markup without what changes from one render or build to the next: the scope attribute, the generated ids and
    // the white space between tags.
    private const string UnchangedMarkup =
        """
        <div class="osirion-static-contact-form"><form class="osirion-static-contact-form-form" action="/api/contact" method="post" hidden data-ref="REF" data-submit-text="Send message" data-submitting-text="Sending..." data-success-message="Thank you. Your message was sent." data-fields-error-message="Check the fields: your name, a valid email address and a message of at least 10 characters." data-check-error-message="The spam check did not pass. Please try again." data-check-pending-message="Wait for the spam check to finish, then send again." data-error-message="The message could not be sent. Please try again later."><div class="osirion-static-contact-form-field"><label for="osirion-static-contact-form-ID-name" class="osirion-label">Name<span class="osirion-static-contact-form-required" aria-hidden="true">*</span></label><input id="osirion-static-contact-form-ID-name" name="name" type="text" class="osirion-input" autocomplete="name" required maxlength="100" /></div><div class="osirion-static-contact-form-field"><label for="osirion-static-contact-form-ID-email" class="osirion-label">Email<span class="osirion-static-contact-form-required" aria-hidden="true">*</span></label><input id="osirion-static-contact-form-ID-email" name="email" type="email" class="osirion-input" autocomplete="email" required maxlength="254" /></div><div class="osirion-static-contact-form-field"><label for="osirion-static-contact-form-ID-company" class="osirion-label">Company</label><input id="osirion-static-contact-form-ID-company" name="company" type="text" class="osirion-input" autocomplete="organization" maxlength="200" /></div><div class="osirion-static-contact-form-field"><label for="osirion-static-contact-form-ID-message" class="osirion-label">Message<span class="osirion-static-contact-form-required" aria-hidden="true">*</span></label><textarea id="osirion-static-contact-form-ID-message" name="message" rows="6" class="osirion-textarea osirion-contact-textarea" required minlength="10" maxlength="5000"></textarea></div><div class="osirion-static-contact-form-trap" aria-hidden="true"><label for="osirion-static-contact-form-ID-website">Website</label><input id="osirion-static-contact-form-ID-website" name="website" type="text" tabindex="-1" autocomplete="off" /></div><div class="osirion-static-contact-form-actions"><button type="submit" class="osirion-button osirion-button-primary" data-ref="REF">Send message</button><p class="osirion-static-contact-form-status" role="status" data-ref="REF"></p></div><p class="osirion-static-contact-form-privacy">Note</p></form></div><script id="bl-args-ID:2" type="application/json">[{"__jsScope":"ID:1"}]</script><bl-script src="./_content/Osirion.Blazor.Core/Components/Forms/OsirionStaticContactForm.razor.js" inst="ID:2" style="display: none; !important"></bl-script>
        """;

    private static string NormalizedMarkup(IRenderedComponent<OsirionStaticContactForm> cut)
    {
        var markup = Regex.Replace(cut.Markup, " b-[a-z0-9]{10}", string.Empty);
        markup = Regex.Replace(markup, "[0-9a-f]{32}", "ID");
        markup = Regex.Replace(markup, "data-ref=\"[^\"]*\"", "data-ref=\"REF\"");
        return Regex.Replace(markup, @">\s+<", "><");
    }

    private IRenderedComponent<OsirionStaticContactForm> RenderForm(
        Action<ComponentParameterCollectionBuilder<OsirionStaticContactForm>>? parameters = null,
        string endpoint = Endpoint) =>
        RenderStatic<OsirionStaticContactForm>(builder =>
        {
            builder.Add(p => p.Endpoint, endpoint);
            parameters?.Invoke(builder);
        });

    private static IElement Field(IRenderedComponent<OsirionStaticContactForm> cut, string name) =>
        cut.Find($"form [name={name}]");

    private static IElement LabelOf(IRenderedComponent<OsirionStaticContactForm> cut, IElement field)
    {
        field.Id.ShouldNotBeNullOrEmpty();
        return cut.Find($"label[for='{field.Id}']");
    }

    // The label as assistive technology reads it, without the aria-hidden required marker.
    private static string AccessibleLabelOf(IRenderedComponent<OsirionStaticContactForm> cut, IElement field) =>
        string.Concat(LabelOf(cut, field).ChildNodes
            .Where(node => node is not IElement element || element.GetAttribute("aria-hidden") != "true")
            .Select(node => node.TextContent)).Trim();
}
