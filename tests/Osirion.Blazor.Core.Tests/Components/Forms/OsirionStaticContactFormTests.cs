using AngleSharp.Dom;
using BlazorJSComponents;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Osirion.Blazor.Components;
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
        LabelOf(cut, name).TextContent.Trim().ShouldBe("Name");

        var email = Field(cut, "email");
        email.GetAttribute("type").ShouldBe("email");
        email.GetAttribute("autocomplete").ShouldBe("email");
        LabelOf(cut, email).TextContent.Trim().ShouldBe("Email");

        var message = Field(cut, "message");
        message.TagName.ShouldBe("TEXTAREA");
        LabelOf(cut, message).TextContent.Trim().ShouldBe("Message");

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
        LabelOf(cut, company).TextContent.Trim().ShouldBe("Organization");
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

        LabelOf(cut, Field(cut, "name")).TextContent.Trim().ShouldBe("<b>Name</b>");
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
}
