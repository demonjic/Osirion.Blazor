using BlazorJSComponents;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Models;
using Osirion.Blazor.Core.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Components.Forms;

/// <summary>
/// The texts of the contact forms in another language: the field labels, the contact information labels and the
/// validation messages, each with its English default unchanged.
/// </summary>
public class ContactFormTextsTests : OsirionComponentTestBase
{
    private const string FormId = "contact-form";

    private static readonly ContactFormValidationTexts Serbian = new()
    {
        NameRequired = "Ime je obavezno",
        NameTooLong = "Ime ima najviše {0} znakova",
        EmailRequired = "Email je obavezan",
        EmailInvalid = "Unesite ispravnu email adresu",
        EmailTooLong = "Email ima najviše {0} znakova",
        SubjectRequired = "Naslov je obavezan",
        SubjectTooLong = "Naslov ima najviše {0} znakova",
        MessageRequired = "Poruka je obavezna",
        MessageLength = "Poruka ima od {0} do {1} znakova",
        PrivacyRequired = "Prihvatite politiku privatnosti",
        FieldRequired = "{0}: obavezno.",
        FieldTooLong = "{0}: najviše {1} znakova.",
        FieldNotANumber = "{0}: unesite broj.",
        FieldBelowMinimum = "{0}: najmanje {1}.",
        FieldAboveMaximum = "{0}: najviše {1}.",
        FieldInvalidEmail = "{0}: neispravna adresa.",
        FieldInvalidPhone = "{0}: neispravan broj.",
        FieldSingleLine = "{0}: jedan red.",
        FieldNotAnOption = "{0}: izaberite ponuđenu opciju.",
        AnswersTooLong = "Odgovori imaju najviše {0} znakova.",
    };

    public ContactFormTextsTests()
    {
        Services.AddJSComponents();
    }

    // A guard test: without ValidationTexts the form shows the messages it always showed.
    [Fact]
    public void ContactForm_EmptySubmitWithDefaultTexts_ShowsTheEnglishMessages()
    {
        var cut = RenderStatic<OsirionContactForm>(parameters => { });

        cut.Find("form").Submit();

        Error(cut, "name").ShouldBe("Name is required");
        Error(cut, "email").ShouldBe("Email is required");
        Error(cut, "subject").ShouldBe("Subject is required");
        Error(cut, "message").ShouldBe("Message is required");
        Error(cut, "privacy").ShouldBe("You must agree to the privacy policy");
    }

    // A guard test: the limits of the model's attributes fill the default messages exactly as before.
    [Fact]
    public void ContactForm_TooLongOrInvalidValuesWithDefaultTexts_ShowTheEnglishMessages()
    {
        var cut = RenderStatic<OsirionContactForm>(parameters => { });
        Input(cut, "name").Change(new string('a', 101));
        Input(cut, "email").Change(new string('b', 300));
        Input(cut, "subject").Change(new string('c', 201));
        Input(cut, "message").Change("short");

        cut.Find("form").Submit();

        Error(cut, "name").ShouldBe("Name cannot exceed 100 characters");
        Messages(cut, "email").ShouldBe(["Please enter a valid email address", "Email cannot exceed 255 characters"]);
        Error(cut, "subject").ShouldBe("Subject cannot exceed 200 characters");
        Error(cut, "message").ShouldBe("Message must be between 10 and 5000 characters");
    }

    [Fact]
    public void ContactForm_ValidationTexts_ReplaceEveryStandardMessage()
    {
        var cut = RenderStatic<OsirionContactForm>(parameters => parameters.Add(p => p.ValidationTexts, Serbian));

        cut.Find("form").Submit();

        Error(cut, "name").ShouldBe("Ime je obavezno");
        Error(cut, "email").ShouldBe("Email je obavezan");
        Error(cut, "subject").ShouldBe("Naslov je obavezan");
        Error(cut, "message").ShouldBe("Poruka je obavezna");
        Error(cut, "privacy").ShouldBe("Prihvatite politiku privatnosti");

        Input(cut, "name").Change(new string('a', 101));
        Input(cut, "email").Change(new string('b', 300));
        Input(cut, "subject").Change(new string('c', 201));
        Input(cut, "message").Change("short");

        Error(cut, "name").ShouldBe("Ime ima najviše 100 znakova");
        Messages(cut, "email").ShouldBe(["Unesite ispravnu email adresu", "Email ima najviše 255 znakova"]);
        Error(cut, "subject").ShouldBe("Naslov ima najviše 200 znakova");
        Error(cut, "message").ShouldBe("Poruka ima od 10 do 5000 znakova");
    }

    [Fact]
    public void ContactForm_ChangedFieldThatIsValid_ClearsItsMessage()
    {
        var cut = RenderStatic<OsirionContactForm>(parameters => { });
        cut.Find("form").Submit();

        Input(cut, "name").Change("Ann");

        cut.FindAll($"#name-error-{FormId}").ShouldBeEmpty();
        Error(cut, "email").ShouldBe("Email is required");
    }

    public static TheoryData<ContactFormField, string, string> AdditionalFieldMessages => new()
    {
        { new ContactFormField { Name = "company", Label = "Company", Required = true }, " ", "Company: obavezno." },
        { new ContactFormField { Name = "code", Label = "Code", MaxLength = 3 }, "abcd", "Code: najviše 3 znakova." },
        { new ContactFormField { Name = "size", Label = "Size", Type = ContactFormFieldType.Number }, "many", "Size: unesite broj." },
        { new ContactFormField { Name = "size", Label = "Size", Type = ContactFormFieldType.Number, Min = 5 }, "4", "Size: najmanje 5." },
        { new ContactFormField { Name = "size", Label = "Size", Type = ContactFormFieldType.Number, Max = 5 }, "6", "Size: najviše 5." },
        { new ContactFormField { Name = "mail", Label = "Mail", Type = ContactFormFieldType.Email }, "nope", "Mail: neispravna adresa." },
        { new ContactFormField { Name = "phone", Label = "Phone", Type = ContactFormFieldType.Phone }, "x", "Phone: neispravan broj." },
        { new ContactFormField { Name = "line", Label = "Line" }, "a\u0001b", "Line: jedan red." },
        {
            new ContactFormField { Name = "plan", Label = "Plan", Type = ContactFormFieldType.Select, Options = [new ContactFormFieldOption("a", "A")] },
            "b",
            "Plan: izaberite ponuđenu opciju."
        },
    };

    [Theory]
    [MemberData(nameof(AdditionalFieldMessages))]
    public void ContactForm_ValidationTexts_ReplaceTheAdditionalFieldMessages(ContactFormField field, string value, string expected)
    {
        var cut = RenderStatic<OsirionContactForm>(parameters => parameters
            .Add(p => p.ValidationTexts, Serbian)
            .Add(p => p.AdditionalFields, [field]));

        cut.Find($"#contact-field-{field.Name}-{FormId}").Change(value);

        cut.Find($"#field-{field.Name}-error-{FormId}").TextContent.ShouldBe(expected);
    }

    [Fact]
    public void ContactForm_ValidationTexts_ReplaceTheTotalLengthMessage()
    {
        var fields = Enumerable.Range(1, 3)
            .Select(index => new ContactFormField { Name = $"note{index}", Label = $"Note {index}", Type = ContactFormFieldType.MultilineText, MaxLength = 5000 })
            .ToList();
        var cut = RenderStatic<OsirionContactForm>(parameters => parameters
            .Add(p => p.ValidationTexts, Serbian)
            .Add(p => p.AdditionalFields, fields));
        foreach (var field in fields)
        {
            cut.Find($"#contact-field-{field.Name}-{FormId}").Change(new string('x', 4000));
        }

        cut.Find("form").Submit();

        cut.Find($"#fields-error-{FormId}").TextContent.ShouldBe($"Odgovori imaju najviše {ContactFormField.MaxTotalLength} znakova.");
    }

    [Fact]
    public void ContactForm_Labels_ReplaceTheFieldLabels()
    {
        var cut = RenderStatic<OsirionContactForm>(parameters => parameters
            .Add(p => p.NameLabel, "Ime")
            .Add(p => p.EmailLabel, "E-pošta")
            .Add(p => p.SubjectLabel, "Naslov")
            .Add(p => p.MessageLabel, "Poruka")
            .Add(p => p.ShowRequiredIndicators, false));

        Label(cut, "name").ShouldBe("Ime");
        Label(cut, "email").ShouldBe("E-pošta");
        Label(cut, "subject").ShouldBe("Naslov");
        Label(cut, "message").ShouldBe("Poruka");
    }

    [Fact]
    public void ContactForm_ContactInfoLabels_ReachTheContactInformation()
    {
        var cut = RenderStatic<OsirionContactForm>(parameters => parameters
            .Add(p => p.ContactInfo, FullContact())
            .Add(p => p.ContactInfoAddressLabel, "Adresa:")
            .Add(p => p.ContactInfoPhoneLabel, "Telefon:")
            .Add(p => p.ContactInfoEmailLabel, "Pošta:")
            .Add(p => p.ContactInfoWebsiteLabel, "Sajt:"));

        AssertContactLabels(cut.Markup, "Adresa:", "Telefon:", "Pošta:", "Sajt:");
    }

    [Fact]
    public void StaticContactForm_ContactInfoLabels_ReachTheContactInformation()
    {
        var cut = RenderStatic<OsirionStaticContactForm>(parameters => parameters
            .Add(p => p.Endpoint, "/api/contact")
            .Add(p => p.ContactInfo, FullContact())
            .Add(p => p.ContactInfoAddressLabel, "Adresa:")
            .Add(p => p.ContactInfoPhoneLabel, "Telefon:")
            .Add(p => p.ContactInfoEmailLabel, "Pošta:")
            .Add(p => p.ContactInfoWebsiteLabel, "Sajt:"));

        AssertContactLabels(cut.Markup, "Adresa:", "Telefon:", "Pošta:", "Sajt:");
    }

    // A guard test: without the new parameters both forms label the contact information as before.
    [Fact]
    public void BothForms_DefaultContactInfoLabels_AreTheEnglishOnes()
    {
        var form = RenderStatic<OsirionContactForm>(parameters => parameters.Add(p => p.ContactInfo, FullContact()));
        var staticForm = RenderStatic<OsirionStaticContactForm>(parameters => parameters
            .Add(p => p.Endpoint, "/api/contact")
            .Add(p => p.ContactInfo, FullContact()));

        AssertContactLabels(form.Markup, "Address:", "Phone:", "Email:", "Website:");
        AssertContactLabels(staticForm.Markup, "Address:", "Phone:", "Email:", "Website:");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("Ime ima najviše {1} znakova")]
    [InlineData("Ime ima najviše {0 znakova")]
    public void ValidationTexts_InvalidMessage_FailsNamingTheProperty(string message)
    {
        var texts = new ContactFormValidationTexts { NameTooLong = message };

        var exception = Should.Throw<ArgumentException>(texts.Validate);

        exception.ParamName.ShouldBe(nameof(ContactFormValidationTexts.NameTooLong));
    }

    [Fact]
    public void ContactForm_InvalidValidationTexts_FailsWhenItRenders()
    {
        Should.Throw<ArgumentException>(() => RenderStatic<OsirionContactForm>(parameters => parameters
            .Add(p => p.ValidationTexts, new ContactFormValidationTexts { FieldRequired = "{2} is required" })));
    }

    [Fact]
    public void ValidationTexts_Default_IsValid()
    {
        Should.NotThrow(ContactFormValidationTexts.Default.Validate);
        Should.NotThrow(Serbian.Validate);
    }

    private static ContactInformation FullContact() => new()
    {
        Address = "Main street 1",
        Phone = "+381 11 222 333",
        Email = "hello@example.com",
        Website = "https://example.com",
    };

    private static void AssertContactLabels(string markup, params string[] labels)
    {
        foreach (var label in labels)
        {
            markup.ShouldContain(label);
        }
    }

    private static AngleSharp.Dom.IElement Input(IRenderedComponent<OsirionContactForm> cut, string field) =>
        cut.Find($"#contact-{field}-{FormId}");

    private static string Label(IRenderedComponent<OsirionContactForm> cut, string field) =>
        cut.Find($"label[for='contact-{field}-{FormId}']").TextContent.Trim();

    private static string Error(IRenderedComponent<OsirionContactForm> cut, string field) =>
        cut.Find($"#{field}-error-{FormId}").TextContent;

    private static List<string> Messages(IRenderedComponent<OsirionContactForm> cut, string field) =>
        cut.FindAll($"#{field}-error-{FormId}").Select(message => message.TextContent).ToList();
}
