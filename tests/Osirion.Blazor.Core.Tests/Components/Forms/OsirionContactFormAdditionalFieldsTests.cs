using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Configuration;
using Osirion.Blazor.Core.Models;
using Osirion.Blazor.Core.Services;
using Osirion.Blazor.Core.Services.Implementations;
using Osirion.Blazor.Core.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Components.Forms;

/// <summary>
/// The fields a site adds to <see cref="OsirionContactForm"/>: their markup per type, their accessibility attributes,
/// the server check of each rule, the email they reach, and the guard that a form without them is unchanged.
/// </summary>
public class OsirionContactFormAdditionalFieldsTests : OsirionComponentTestBase
{
    private const string FormId = "quote";

    private BunitNavigationManager Navigation => Services.GetRequiredService<BunitNavigationManager>();

    // A guard test: a form without additional fields renders exactly the markup it rendered before they existed.
    [Fact]
    public void ContactForm_WithoutAdditionalFields_RendersTheMarkupItAlwaysDid()
    {
        var cut = RenderStatic<OsirionContactForm>(parameters => parameters.Add(p => p.Id, FormId));

        NormalizedMarkup(cut).ShouldBe(UnchangedMarkup);
    }

    // A guard test: an empty list is the same as none.
    [Fact]
    public void ContactForm_EmptyAdditionalFields_RendersTheMarkupItAlwaysDid()
    {
        var cut = RenderStatic<OsirionContactForm>(parameters => parameters
            .Add(p => p.Id, FormId)
            .Add(p => p.AdditionalFields, Array.Empty<ContactFormField>()));

        NormalizedMarkup(cut).ShouldBe(UnchangedMarkup);
    }

    // A guard test: without additional fields the submitted model carries no answers, and the email has no section for them.
    [Fact]
    public void ContactForm_WithoutAdditionalFields_SubmitsTheSameModelAndEmail()
    {
        var mail = RegisterSendGrid();
        ContactFormModel? submitted = null;
        var cut = RenderStatic<OsirionContactForm>(parameters => parameters
            .Add(p => p.Id, FormId)
            .Add(p => p.UseBuiltInEmailService, true)
            .Add(p => p.OnSubmit, (ContactFormModel model) => submitted = model));
        FillStandardFields(cut);

        cut.Find("form").Submit();

        submitted.ShouldNotBeNull().AdditionalFieldValues.ShouldBeEmpty();
        mail.Body.ShouldNotContain("Additional details");
        Navigation.Uri.ShouldContain("SubmissionResultValue=1");
    }

    [Fact]
    public void ContactForm_AdditionalFields_RenderInOrderAfterTheMessageAndBeforeTheConsent()
    {
        var cut = RenderForm(
            new ContactFormField { Name = "company", Label = "Company" },
            new ContactFormField { Name = "size", Label = "Team size", Type = ContactFormFieldType.Number });

        var ids = cut.FindAll("form input, form textarea, form select").Select(element => element.Id).Where(id => id is not null).ToList();
        ids.IndexOf($"contact-message-{FormId}").ShouldBeLessThan(ids.IndexOf($"contact-field-company-{FormId}"));
        ids.IndexOf($"contact-field-company-{FormId}").ShouldBeLessThan(ids.IndexOf($"contact-field-size-{FormId}"));
        var consent = cut.Find(".osirion-form-checkbox-group");
        consent.CompareDocumentPosition(cut.Find($"#contact-field-size-{FormId}")).HasFlag(DocumentPositions.Preceding).ShouldBeTrue();
    }

    [Theory]
    [InlineData(ContactFormFieldType.Text, "text", "200")]
    [InlineData(ContactFormFieldType.Email, "email", "254")]
    [InlineData(ContactFormFieldType.Phone, "tel", "40")]
    public void ContactForm_SingleLineField_RendersALabelledInputOfItsType(ContactFormFieldType type, string inputType, string maxLength)
    {
        var cut = RenderForm(new ContactFormField { Name = "answer", Label = "Answer", Type = type, Placeholder = "Type here" });

        var input = cut.Find($"#contact-field-answer-{FormId}");
        input.TagName.ShouldBe("INPUT");
        input.GetAttribute("type").ShouldBe(inputType);
        input.GetAttribute("name").ShouldBe("field.answer");
        input.GetAttribute("maxlength").ShouldBe(maxLength);
        input.GetAttribute("placeholder").ShouldBe("Type here");
        input.HasAttribute("required").ShouldBeFalse();
        cut.Find($"label[for='contact-field-answer-{FormId}']").TextContent.Trim().ShouldBe("Answer");
    }

    [Fact]
    public void ContactForm_MultilineField_RendersATextareaWithItsMaxLength()
    {
        var cut = RenderForm(new ContactFormField
        {
            Name = "needs",
            Label = "Needs",
            Type = ContactFormFieldType.MultilineText,
            MaxLength = 800,
            Required = true
        });

        var textarea = cut.Find($"#contact-field-needs-{FormId}");
        textarea.TagName.ShouldBe("TEXTAREA");
        textarea.GetAttribute("name").ShouldBe("field.needs");
        textarea.GetAttribute("maxlength").ShouldBe("800");
        textarea.HasAttribute("required").ShouldBeTrue();
    }

    [Fact]
    public void ContactForm_NumberField_RendersItsRangeInTheInvariantCulture()
    {
        var culture = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("de-DE");
        try
        {
            var cut = RenderForm(new ContactFormField
            {
                Name = "budget",
                Label = "Budget",
                Type = ContactFormFieldType.Number,
                Min = 0.5m,
                Max = 2500
            });

            var input = cut.Find($"#contact-field-budget-{FormId}");
            input.GetAttribute("type").ShouldBe("number");
            input.GetAttribute("min").ShouldBe("0.5");
            input.GetAttribute("max").ShouldBe("2500");
            input.GetAttribute("step").ShouldBe("any");
            input.HasAttribute("maxlength").ShouldBeFalse();
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = culture;
        }
    }

    [Fact]
    public void ContactForm_SelectField_RendersAnEmptyFirstOptionThenTheOptions()
    {
        var cut = RenderForm(SizeSelect(required: true));

        var select = cut.Find($"#contact-field-size-{FormId}");
        select.TagName.ShouldBe("SELECT");
        select.GetAttribute("name").ShouldBe("field.size");
        select.HasAttribute("required").ShouldBeTrue();
        var options = select.QuerySelectorAll("option");
        options.Select(option => option.GetAttribute("value")).ShouldBe(["", "1-10", "11-50", "51+"]);
        options.Select(option => option.TextContent).ShouldBe(["Choose a size", "1 to 10", "11 to 50", "51+"]);
    }

    [Fact]
    public void ContactForm_RadioField_RendersARequiredGroupNamedByItsLegend()
    {
        var cut = RenderForm(new ContactFormField
        {
            Name = "contact_by",
            Label = "Contact me by",
            Type = ContactFormFieldType.Radio,
            Required = true,
            Options = [new("email", "Email"), new("phone", "Phone")]
        });

        var fieldset = cut.Find("fieldset.osirion-form-fieldset");
        fieldset.QuerySelector("legend")!.TextContent.ShouldContain("Contact me by");
        var radios = fieldset.QuerySelectorAll("input[type=radio]");
        radios.Select(radio => radio.GetAttribute("value")).ShouldBe(["email", "phone"]);
        radios.ShouldAllBe(radio => radio.GetAttribute("name") == "field.contact_by" && radio.HasAttribute("required"));
        cut.Find($"label[for='contact-field-contact_by-{FormId}-1']").TextContent.Trim().ShouldBe("Phone");
    }

    [Fact]
    public void ContactForm_CheckboxesField_RendersBoxesWithoutNativeRequired()
    {
        var cut = RenderForm(Topics(required: true));

        var boxes = cut.FindAll("fieldset input[type=checkbox]");
        boxes.Select(box => box.GetAttribute("value")).ShouldBe(["design", "build", "support"]);
        boxes.ShouldAllBe(box => box.GetAttribute("name") == "field.topics" && !box.HasAttribute("required"));
        cut.Find("fieldset legend .osirion-required").GetAttribute("aria-hidden").ShouldBe("true");
    }

    [Fact]
    public void ContactForm_HelpText_IsLinkedWithAriaDescribedBy()
    {
        var cut = RenderForm(
            new ContactFormField { Name = "company", Label = "Company", HelpText = "As registered" },
            Topics(required: false));

        var help = cut.Find($"#field-company-help-{FormId}");
        help.TextContent.ShouldBe("As registered");
        cut.Find($"#contact-field-company-{FormId}").GetAttribute("aria-describedby").ShouldBe($"field-company-help-{FormId}");
        cut.Find("fieldset").HasAttribute("aria-describedby").ShouldBeFalse();
    }

    [Fact]
    public void ContactForm_RequiredMarker_FollowsShowRequiredIndicators()
    {
        var cut = RenderForm(
            parameters => parameters.Add(p => p.ShowRequiredIndicators, false),
            new ContactFormField { Name = "company", Label = "Company", Required = true });

        cut.Find($"label[for='contact-field-company-{FormId}']").QuerySelector(".osirion-required").ShouldBeNull();
        cut.Find($"#contact-field-company-{FormId}").HasAttribute("required").ShouldBeTrue();
    }

    [Fact]
    public void ContactForm_LabelsHelpAndOptions_AreEncoded()
    {
        var cut = RenderForm(new ContactFormField
        {
            Name = "x",
            Label = "<b>Label</b>",
            HelpText = "<i>help</i>",
            Type = ContactFormFieldType.Select,
            Options = [new("<v>", "<script>alert(1)</script>")]
        });

        cut.FindAll("form b, form i, form script").ShouldBeEmpty();
        cut.Find($"label[for='contact-field-x-{FormId}']").TextContent.ShouldContain("<b>Label</b>");
        cut.Find("option[value='<v>']").TextContent.ShouldBe("<script>alert(1)</script>");
    }

    [Fact]
    public void ContactForm_InvalidField_ShowsItsMessageLinkedToTheInput()
    {
        var submitted = false;
        var cut = RenderForm(
            parameters => parameters.Add(p => p.OnSubmit, (ContactFormModel _) => submitted = true),
            new ContactFormField { Name = "company", Label = "Company", Required = true, HelpText = "As registered" });
        FillStandardFields(cut);

        cut.Find("form").Submit();

        submitted.ShouldBeFalse();
        Navigation.Uri.ShouldNotContain("SubmissionResultValue");
        cut.Find($"#field-company-error-{FormId}").TextContent.ShouldBe("Company is required.");
        var input = cut.Find($"#contact-field-company-{FormId}");
        input.GetAttribute("aria-describedby").ShouldBe($"field-company-help-{FormId} field-company-error-{FormId}");
        input.GetAttribute("aria-invalid").ShouldBe("true");
    }

    [Fact]
    public void ContactForm_InvalidGroup_DescribesTheFieldsetAndMarksEachInput()
    {
        var cut = RenderForm(Topics(required: true));
        FillStandardFields(cut);

        cut.Find("form").Submit();

        cut.Find("fieldset").GetAttribute("aria-describedby").ShouldBe($"field-topics-error-{FormId}");
        cut.FindAll("fieldset input").ShouldAllBe(box => box.GetAttribute("aria-invalid") == "true");
        cut.Find($"#field-topics-error-{FormId}").TextContent.ShouldBe("Topics is required.");
    }

    [Fact]
    public void ContactForm_StandardAndAdditionalErrors_AreShownTogether()
    {
        var cut = RenderForm(new ContactFormField { Name = "company", Label = "Company", Required = true });

        cut.Find("form").Submit();

        cut.Find($"#name-error-{FormId}").TextContent.ShouldBe("Name is required");
        cut.Find($"#field-company-error-{FormId}").TextContent.ShouldBe("Company is required.");
    }

    [Theory]
    [MemberData(nameof(InvalidValues))]
    public void ContactForm_InvalidValue_IsRefusedWithItsMessage(ContactFormField field, string value, string message)
    {
        var submitted = false;
        var cut = RenderForm(parameters => parameters.Add(p => p.OnSubmit, (ContactFormModel _) => submitted = true), field);
        FillStandardFields(cut);
        cut.Find($"#contact-field-{field.Name}-{FormId}").Change(value);

        cut.Find("form").Submit();

        submitted.ShouldBeFalse();
        cut.Find($"#field-{field.Name}-error-{FormId}").TextContent.ShouldBe(message);
    }

    public static TheoryData<ContactFormField, string, string> InvalidValues() => new()
    {
        { new ContactFormField { Name = "company", Label = "Company", Required = true }, "   ", "Company is required." },
        { new ContactFormField { Name = "company", Label = "Company", MaxLength = 5 }, "Too long", "Company cannot exceed 5 characters." },
        { new ContactFormField { Name = "company", Label = "Company" }, "Line\r\nbreak", "Company must be a single line." },
        { new ContactFormField { Name = "needs", Label = "Needs", Type = ContactFormFieldType.MultilineText }, "Bell\u0007", "Needs contains characters that are not allowed." },
        { new ContactFormField { Name = "budget", Label = "Budget", Type = ContactFormFieldType.Number }, "1e3", "Budget must be a number." },
        { new ContactFormField { Name = "budget", Label = "Budget", Type = ContactFormFieldType.Number, Min = 100 }, "99.5", "Budget must be at least 100." },
        { new ContactFormField { Name = "budget", Label = "Budget", Type = ContactFormFieldType.Number, Max = 10 }, "10.01", "Budget must be at most 10." },
        { new ContactFormField { Name = "billing", Label = "Billing email", Type = ContactFormFieldType.Email }, "not-an-address", "Billing email must be a valid email address." },
        { new ContactFormField { Name = "phone", Label = "Phone", Type = ContactFormFieldType.Phone }, "call me", "Phone must be a valid phone number." },
        { new ContactFormField { Name = "phone", Label = "Phone", Type = ContactFormFieldType.Phone }, "+1", "Phone must be a valid phone number." },
        { SizeSelect(required: false), "500", "Choose one of the listed options for Team size." }
    };

    [Fact]
    public void ContactForm_RadioValueNotAnOption_IsRefused()
    {
        // A tampered post: the radio's value is not one of the options.
        var cut = RenderPosted(
            [new ContactFormField { Name = "contact_by", Label = "Contact me by", Type = ContactFormFieldType.Radio, Options = [new("email"), new("phone")] }],
            ("field.contact_by", ["fax"]));
        FillStandardFields(cut);

        cut.Find("form").Submit();

        cut.Find($"#field-contact_by-error-{FormId}").TextContent.ShouldBe("Choose one of the listed options for Contact me by.");
    }

    [Fact]
    public void ContactForm_CheckboxValueNotAnOption_IsRefused()
    {
        var cut = RenderPosted([Topics(required: false)], ("field.topics", ["design", "hosting"]));
        FillStandardFields(cut);

        cut.Find("form").Submit();

        cut.Find($"#field-topics-error-{FormId}").TextContent.ShouldBe("Choose one of the listed options for Topics.");
    }

    [Fact]
    public void ContactForm_AnswersLongerThanTheTotalCap_AreRefused()
    {
        var long1 = new string('a', 4000);
        var fields = Enumerable.Range(1, 3)
            .Select(index => new ContactFormField
            {
                Name = $"part{index}",
                Label = $"Part {index}",
                Type = ContactFormFieldType.MultilineText,
                MaxLength = 5000
            })
            .ToArray();
        var submitted = false;
        var cut = RenderPosted(
            fields,
            [("field.part1", [long1]), ("field.part2", [long1]), ("field.part3", [long1])],
            parameters => parameters.Add(p => p.OnSubmit, (ContactFormModel _) => submitted = true));

        cut.Find("form").Submit();

        submitted.ShouldBeFalse();
        cut.Find($"#fields-error-{FormId}").TextContent
            .ShouldBe("The answers to the additional questions cannot exceed 10000 characters together.");
    }

    [Fact]
    public void ContactForm_ChangedField_IsCheckedAgain()
    {
        var cut = RenderForm(new ContactFormField { Name = "company", Label = "Company", Required = true });
        FillStandardFields(cut);
        cut.Find("form").Submit();
        cut.FindAll($"#field-company-error-{FormId}").ShouldNotBeEmpty();

        cut.Find($"#contact-field-company-{FormId}").Change("Example Ltd");

        cut.FindAll($"#field-company-error-{FormId}").ShouldBeEmpty();
        cut.Find($"#contact-field-company-{FormId}").HasAttribute("aria-invalid").ShouldBeFalse();
    }

    [Fact]
    public void ContactForm_ValidAnswers_ReachOnSubmitInDefinitionOrder()
    {
        ContactFormModel? submitted = null;
        var cut = RenderForm(
            parameters => parameters.Add(p => p.OnSubmit, (ContactFormModel model) => submitted = model),
            new ContactFormField { Name = "company", Label = "Company" },
            Topics(required: true),
            SizeSelect(required: true),
            new ContactFormField { Name = "notes", Label = "Notes" });
        FillStandardFields(cut);
        cut.Find($"#contact-field-company-{FormId}").Change("  Example Ltd  ");
        cut.Find($"#contact-field-topics-{FormId}-2").Change(true);
        cut.Find($"#contact-field-topics-{FormId}-0").Change(true);
        cut.Find($"#contact-field-size-{FormId}").Change("11-50");

        cut.Find("form").Submit();

        Navigation.Uri.ShouldContain("SubmissionResultValue=1");
        var answers = submitted.ShouldNotBeNull().AdditionalFieldValues;
        answers.Select(answer => answer.Name).ShouldBe(["company", "topics", "size", "notes"]);
        answers[0].ShouldBe(new ContactFormFieldValue("company", "Company", answers[0].Values, "Example Ltd"));
        answers[0].Values.ShouldBe(["Example Ltd"]);
        answers[1].Values.ShouldBe(["design", "support"]);
        answers[1].Text.ShouldBe("Design, Support");
        answers[2].Values.ShouldBe(["11-50"]);
        answers[2].Text.ShouldBe("11 to 50");
        answers[3].Values.ShouldBeEmpty();
        answers[3].Text.ShouldBeEmpty();
    }

    [Fact]
    public void ContactForm_StaticPost_ReadsTheDefinedFieldsAndIgnoresUnknownNames()
    {
        ContactFormModel? submitted = null;
        var cut = RenderPosted(
            [new ContactFormField { Name = "company", Label = "Company" }, Topics(required: false)],
            [("field.company", ["Example Ltd"]), ("field.topics", ["build", "design"]), ("field.unknown", ["x"]), ("company", ["y"])],
            parameters => parameters.Add(p => p.OnSubmit, (ContactFormModel model) => submitted = model));

        cut.Find($"#contact-field-company-{FormId}").GetAttribute("value").ShouldBe("Example Ltd");
        cut.Find($"#contact-field-topics-{FormId}-0").HasAttribute("checked").ShouldBeTrue();
        cut.Find($"#contact-field-topics-{FormId}-2").HasAttribute("checked").ShouldBeFalse();
        FillStandardFields(cut);

        cut.Find("form").Submit();

        var answers = submitted.ShouldNotBeNull().AdditionalFieldValues;
        answers.Select(answer => answer.Name).ShouldBe(["company", "topics"]);
        answers[0].Text.ShouldBe("Example Ltd");
        answers[1].Values.ShouldBe(["design", "build"]);
    }

    [Fact]
    public void ContactForm_PostOfAnotherForm_LeavesTheFieldsEmpty()
    {
        var cut = RenderPosted(
            [new ContactFormField { Name = "company", Label = "Company" }],
            [("field.company", ["Example Ltd"])],
            handler: "newsletter");

        cut.Find($"#contact-field-company-{FormId}").GetAttribute("value").ShouldBeEmpty();
    }

    [Fact]
    public void ContactForm_StaticPostWithAnError_KeepsTheValuesInTheFields()
    {
        var cut = RenderPosted(
            [new ContactFormField { Name = "needs", Label = "Needs", Type = ContactFormFieldType.MultilineText }, SizeSelect(required: true)],
            [("field.needs", ["Line one\r\nLine two"])]);
        FillStandardFields(cut);

        cut.Find("form").Submit();

        cut.Find($"#field-size-error-{FormId}").TextContent.ShouldBe("Team size is required.");
        cut.Find($"#contact-field-needs-{FormId}").TextContent.ShouldBe("Line one\nLine two");
    }

    [Fact]
    public void ContactForm_Email_ListsTheAnswersInOrderLabelledAndEncoded()
    {
        var mail = RegisterSendGrid();
        var cut = RenderForm(
            parameters => parameters.Add(p => p.UseBuiltInEmailService, true),
            new ContactFormField { Name = "company", Label = "Company <Ltd>" },
            new ContactFormField { Name = "skipped", Label = "Skipped" },
            new ContactFormField { Name = "needs", Label = "Needs", Type = ContactFormFieldType.MultilineText },
            Topics(required: false));
        FillStandardFields(cut);
        cut.Find($"#contact-field-company-{FormId}").Change("<script>alert(1)</script> & co");
        cut.Find($"#contact-field-needs-{FormId}").Change("First\nSecond");
        cut.Find($"#contact-field-topics-{FormId}-1").Change(true);
        cut.Find($"#contact-field-topics-{FormId}-0").Change(true);

        cut.Find("form").Submit();

        Navigation.Uri.ShouldContain("SubmissionResultValue=1");
        var body = mail.Body;
        body.ShouldContain(
            "<h3>Additional details</h3>\r\n".Replace("\r\n", Environment.NewLine, StringComparison.Ordinal)
            + "<p><strong>Company &lt;Ltd&gt;:</strong> &lt;script&gt;alert(1)&lt;/script&gt; &amp; co</p>" + Environment.NewLine
            + "<p><strong>Needs:</strong> First<br>Second</p>" + Environment.NewLine
            + "<p><strong>Topics:</strong> Design, Build</p>");
        body.ShouldNotContain("Skipped");
        body.ShouldNotContain("<script>");
        body.IndexOf("Additional details", StringComparison.Ordinal).ShouldBeGreaterThan(body.IndexOf("Message:", StringComparison.Ordinal));
        mail.Subject.ShouldBe("Contact Form: Hello");
    }

    [Theory]
    [MemberData(nameof(InvalidDefinitions))]
    public void ContactForm_InvalidDefinitions_FailWithTheirReason(ContactFormField[] fields, string reason)
    {
        var exception = Should.Throw<ArgumentException>(() => RenderForm(fields));

        exception.Message.ShouldContain(reason);
    }

    public static TheoryData<ContactFormField[], string> InvalidDefinitions() => new()
    {
        { [new ContactFormField { Name = "1st", Label = "First" }], "is not a letter followed by" },
        { [new ContactFormField { Name = "a b", Label = "Spaced" }], "is not a letter followed by" },
        { [new ContactFormField { Name = "__proto__", Label = "Prototype" }], "is not a letter followed by" },
        { [new ContactFormField { Name = new string('a', 51), Label = "Long" }], "is not a letter followed by" },
        { [new ContactFormField { Name = "a", Label = " " }], "has no label" },
        { [new ContactFormField { Name = "a", Label = "A" }, new ContactFormField { Name = "a", Label = "Again" }], "is used twice" },
        { [new ContactFormField { Name = "a", Label = "A", MaxLength = 0 }], "MaxLength" },
        { [new ContactFormField { Name = "a", Label = "A", MaxLength = 5001 }], "MaxLength" },
        { [new ContactFormField { Name = "a", Label = "A", Type = ContactFormFieldType.Number, Min = 5, Max = 1 }], "Min" },
        { [new ContactFormField { Name = "a", Label = "A", Type = ContactFormFieldType.Select }], "needs between 1 and 50 options" },
        { [new ContactFormField { Name = "a", Label = "A", Type = ContactFormFieldType.Radio, Options = [new("x"), new("x")] }], "repeats" },
        { [new ContactFormField { Name = "a", Label = "A", Type = ContactFormFieldType.Checkboxes, Options = [new(" x")] }], "white space" },
        { Enumerable.Range(0, 21).Select(index => new ContactFormField { Name = $"f{index}", Label = "F" }).ToArray(), "at most 20 additional fields" }
    };

    [Fact]
    public void ContactForm_TwentyFields_AreAccepted()
    {
        var fields = Enumerable.Range(0, ContactFormField.MaxFields)
            .Select(index => new ContactFormField { Name = $"f{index}", Label = $"F{index}" })
            .ToArray();

        var cut = RenderForm(fields);

        cut.FindAll("input[name^='field.']").Count.ShouldBe(20);
    }

    internal static ContactFormField SizeSelect(bool required) => new()
    {
        Name = "size",
        Label = "Team size",
        Type = ContactFormFieldType.Select,
        Required = required,
        Placeholder = "Choose a size",
        Options = [new("1-10", "1 to 10"), new("11-50", "11 to 50"), new("51+")]
    };

    internal static ContactFormField Topics(bool required) => new()
    {
        Name = "topics",
        Label = "Topics",
        Type = ContactFormFieldType.Checkboxes,
        Required = required,
        Options = [new("design", "Design"), new("build", "Build"), new("support", "Support")]
    };

    private IRenderedComponent<OsirionContactForm> RenderForm(params ContactFormField[] fields) =>
        RenderForm(_ => { }, fields);

    private IRenderedComponent<OsirionContactForm> RenderForm(
        Action<ComponentParameterCollectionBuilder<OsirionContactForm>> parameters, params ContactFormField[] fields) =>
        RenderStatic<OsirionContactForm>(builder =>
        {
            builder.Add(p => p.Id, FormId).Add(p => p.AdditionalFields, fields);
            parameters(builder);
        });

    // Renders the form as static rendering does for its own form post: the request carries the posted values.
    private IRenderedComponent<OsirionContactForm> RenderPosted(
        ContactFormField[] fields,
        (string Key, string[] Values)[] posted,
        Action<ComponentParameterCollectionBuilder<OsirionContactForm>>? parameters = null,
        string handler = FormId)
    {
        var form = posted.ToDictionary(entry => entry.Key, entry => new StringValues(entry.Values));
        form["_handler"] = handler;
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.ContentType = "application/x-www-form-urlencoded";
        context.Request.Form = new FormCollection(form);

        return RenderForm(builder =>
        {
            builder.AddCascadingValue<HttpContext>(context);
            parameters?.Invoke(builder);
        }, fields);
    }

    private IRenderedComponent<OsirionContactForm> RenderPosted(ContactFormField[] fields, (string Key, string[] Values) posted) =>
        RenderPosted(fields, [posted]);

    private static void FillStandardFields(IRenderedComponent<OsirionContactForm> cut)
    {
        cut.Find($"#contact-name-{FormId}").Change("Ann");
        cut.Find($"#contact-email-{FormId}").Change("ann@example.com");
        cut.Find($"#contact-subject-{FormId}").Change("Hello");
        cut.Find($"#contact-message-{FormId}").Change("A message that is long enough");
        cut.Find(".osirion-form-checkbox-group input[type=checkbox]").Change(true);
    }

    private CapturingHandler RegisterSendGrid()
    {
        var handler = new CapturingHandler();
        var options = new EmailOptions
        {
            Provider = EmailProviderType.SendGrid,
            ToEmail = "to@example.com",
            SendGrid = { ApiKey = "sg-key", FromEmail = "from@example.com" }
        };
        Services.AddSingleton(Options.Create(options));
        Services.AddSingleton(new SendGridEmailService(options, NullLogger<SendGridEmailService>.Instance, new HttpClient(handler)));
        Services.AddTransient<EmailServiceFactory>();
        return handler;
    }

    // The markup without what changes from one render or checkout to the next: the scope attribute, the test renderer's
    // event and element reference attributes, which static rendering never writes, and the white space of the source.
    private static string NormalizedMarkup(IRenderedComponent<OsirionContactForm> cut)
    {
        var markup = Regex.Replace(cut.Markup, " b-[a-z0-9]{10}", string.Empty);
        markup = Regex.Replace(markup, " blazor:[A-Za-z]+=\"[^\"]*\"", string.Empty);
        markup = Regex.Replace(markup, @"\s+", " ");
        return Regex.Replace(markup, @">\s+<", "><").Trim();
    }

    // The normalized markup of a form with the default parameters, as rendered before additional fields existed.
    private const string UnchangedMarkup =
        "<div class=\"osirion-contact-form-container osirion-contact-form-side-by-side\">"
        + "<div class=\"osirion-contact-form-wrapper\"><div class=\"osirion-contact-form-section\">"
        + "<h2 class=\"osirion-contact-form-title\">Write us</h2>"
        + "<form data-enhance=\"\" class=\"osirion-contact-form\"><div class=\"osirion-honeypot\" aria-hidden=\"true\">"
        + "<input tabindex=\"-1\" autocomplete=\"off\" name=\"FormModel.Website\" class=\"valid\" value=\"\"></input>"
        + "</div><div class=\"osirion-form-group\">"
        + "<label for=\"contact-name-quote\" class=\"osirion-label\"> Name <span class=\"osirion-required\">*</span>"
        + "</label>"
        + "<input id=\"contact-name-quote\" placeholder=\"Your name\" aria-describedby=\"name-error-quote\" name=\"FormModel.Name\" class=\"osirion-input valid\" value=\"\">"
        + "</input></div><div class=\"osirion-form-group\">"
        + "<label for=\"contact-email-quote\" class=\"osirion-label\"> Email <span class=\"osirion-required\">*</span>"
        + "</label>"
        + "<input id=\"contact-email-quote\" type=\"email\" placeholder=\"Your email address\" aria-describedby=\"email-error-quote\" name=\"FormModel.Email\" class=\"osirion-input valid\" value=\"\">"
        + "</input></div><div class=\"osirion-form-group\">"
        + "<label for=\"contact-subject-quote\" class=\"osirion-label\"> Subject <span class=\"osirion-required\">*</span>"
        + "</label>"
        + "<input id=\"contact-subject-quote\" placeholder=\"Subject\" aria-describedby=\"subject-error-quote\" name=\"FormModel.Subject\" class=\"osirion-input valid\" value=\"\">"
        + "</input></div><div class=\"osirion-form-group\">"
        + "<label for=\"contact-message-quote\" class=\"osirion-label\"> Message <span class=\"osirion-required\">*</span>"
        + "</label>"
        + "<textarea id=\"contact-message-quote\" placeholder=\"Your message\" rows=\"6\" aria-describedby=\"message-error-quote\" name=\"FormModel.Message\" class=\"osirion-textarea osirion-contact-textarea valid\" value=\"\">"
        + "</textarea></div><div class=\"osirion-form-group osirion-form-checkbox-group\">"
        + "<label class=\"osirion-form-checkbox-label\">"
        + "<input aria-describedby=\"privacy-error-quote\" type=\"checkbox\" name=\"FormModel.AgreeToPrivacyPolicy\" class=\"osirion-checkbox valid\" value=\"True\">"
        + "</input>"
        + "<span class=\"osirion-checkbox-text\">I agree to the <a href=\"/privacy-policy\" target=\"_blank\">privacy policy</a>"
        + "<span class=\"osirion-required\">*</span></span></label></div>"
        + "<div class=\"osirion-form-group osirion-form-checkbox-group\">"
        + "<label class=\"osirion-form-checkbox-label\">"
        + "<input type=\"checkbox\" name=\"FormModel.SubscribeToUpdates\" class=\"osirion-checkbox valid\" value=\"True\">"
        + "</input><span class=\"osirion-checkbox-text\">I would like to receive updates and newsletters</span>"
        + "</label></div><div class=\"osirion-form-actions\">"
        + "<button type=\"submit\" class=\"osirion-button osirion-button-primary\"><span>Send Message</span>"
        + "</button></div></form></div></div></div>";

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public string Body { get; private set; } = string.Empty;

        public string Subject { get; private set; } = string.Empty;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var json = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken)).RootElement;
            Subject = json.GetProperty("personalizations")[0].GetProperty("subject").GetString()!;
            Body = json.GetProperty("content")[0].GetProperty("value").GetString()!;
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }
    }
}
