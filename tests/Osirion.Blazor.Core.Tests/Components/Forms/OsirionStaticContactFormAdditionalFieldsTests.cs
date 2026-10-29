using System.Diagnostics;
using System.Text.Json;
using AngleSharp.Dom;
using BlazorJSComponents;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Models;
using Osirion.Blazor.Core.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Components.Forms;

/// <summary>
/// The fields a site adds to <see cref="OsirionStaticContactForm"/>: their markup per type, their native constraints,
/// and the JSON its script posts, with and without them.
/// </summary>
public class OsirionStaticContactFormAdditionalFieldsTests : OsirionComponentTestBase
{
    public OsirionStaticContactFormAdditionalFieldsTests()
    {
        Services.AddJSComponents();
    }

    [Fact]
    public void StaticContactForm_AdditionalFields_RenderInOrderAfterTheMessageAndBeforeTheTrap()
    {
        var cut = RenderForm(
            new ContactFormField { Name = "company_size", Label = "Company size", Type = ContactFormFieldType.Number },
            new ContactFormField { Name = "notes", Label = "Notes" });

        cut.FindAll("form [name]").Select(field => field.GetAttribute("name"))
            .ShouldBe(["name", "email", "message", "field.company_size", "field.notes", "website"]);
        cut.FindAll("[data-additional-field]").Select(field => field.GetAttribute("data-additional-field"))
            .ShouldBe(["company_size", "notes"]);
    }

    [Theory]
    [InlineData(ContactFormFieldType.Text, "text", "200")]
    [InlineData(ContactFormFieldType.Email, "email", "254")]
    [InlineData(ContactFormFieldType.Phone, "tel", "40")]
    public void StaticContactForm_SingleLineField_RendersALabelledInputWithItsLimits(
        ContactFormFieldType type, string inputType, string maxLength)
    {
        var cut = RenderForm(new ContactFormField { Name = "answer", Label = "Answer", Type = type, Required = true });

        var input = Field(cut, "answer");
        input.GetAttribute("type").ShouldBe(inputType);
        input.GetAttribute("maxlength").ShouldBe(maxLength);
        input.HasAttribute("required").ShouldBeTrue();
        input.HasAttribute("aria-describedby").ShouldBeFalse();
        var label = cut.Find($"label[for='{input.Id}']");
        label.TextContent.ShouldContain("Answer");
        label.QuerySelector(".osirion-static-contact-form-required")!.GetAttribute("aria-hidden").ShouldBe("true");
        input.ParentElement!.HasAttribute("data-multiple").ShouldBeFalse();
    }

    [Fact]
    public void StaticContactForm_MultilineAndNumberFields_CarryTheirConstraints()
    {
        var cut = RenderForm(
            new ContactFormField { Name = "needs", Label = "Needs", Type = ContactFormFieldType.MultilineText, MaxLength = 1200 },
            new ContactFormField { Name = "budget", Label = "Budget", Type = ContactFormFieldType.Number, Min = 1000, Max = 50000 });

        var needs = Field(cut, "needs");
        needs.TagName.ShouldBe("TEXTAREA");
        needs.GetAttribute("maxlength").ShouldBe("1200");
        needs.HasAttribute("required").ShouldBeFalse();
        var budget = Field(cut, "budget");
        budget.GetAttribute("type").ShouldBe("number");
        budget.GetAttribute("min").ShouldBe("1000");
        budget.GetAttribute("max").ShouldBe("50000");
        budget.GetAttribute("step").ShouldBe("any");
    }

    [Fact]
    public void StaticContactForm_SelectField_RendersAnEmptyFirstOption()
    {
        var cut = RenderForm(OsirionContactFormAdditionalFieldsTests.SizeSelect(required: true));

        var select = Field(cut, "size");
        select.TagName.ShouldBe("SELECT");
        select.HasAttribute("required").ShouldBeTrue();
        select.QuerySelectorAll("option").Select(option => option.GetAttribute("value")).ShouldBe(["", "1-10", "11-50", "51+"]);
        select.QuerySelector("option")!.TextContent.ShouldBe("Choose a size");
    }

    [Fact]
    public void StaticContactForm_RadioField_IsARequiredGroupSentAsOneValue()
    {
        var cut = RenderForm(new ContactFormField
        {
            Name = "contact_by",
            Label = "Contact me by",
            Type = ContactFormFieldType.Radio,
            Required = true,
            Options = [new("email", "Email"), new("phone", "Phone")]
        });

        var group = cut.Find("fieldset[data-additional-field='contact_by']");
        group.HasAttribute("data-multiple").ShouldBeFalse();
        group.QuerySelector("legend")!.TextContent.ShouldContain("Contact me by");
        var radios = group.QuerySelectorAll("input[type=radio]");
        radios.ShouldAllBe(radio => radio.GetAttribute("name") == "field.contact_by" && radio.HasAttribute("required"));
        radios.Select(radio => cut.Find($"label[for='{radio.Id}']").TextContent.Trim()).ShouldBe(["Email", "Phone"]);
    }

    [Fact]
    public void StaticContactForm_CheckboxesField_IsAGroupSentAsAnArrayWithoutNativeRequired()
    {
        var cut = RenderForm(OsirionContactFormAdditionalFieldsTests.Topics(required: true));

        var group = cut.Find("fieldset[data-additional-field='topics']");
        group.HasAttribute("data-multiple").ShouldBeTrue();
        group.QuerySelectorAll("input[type=checkbox]").ShouldAllBe(box => !box.HasAttribute("required"));
    }

    [Fact]
    public void StaticContactForm_HelpText_IsLinkedWithAriaDescribedBy()
    {
        var cut = RenderForm(
            new ContactFormField { Name = "company", Label = "Company", HelpText = "As registered" },
            new ContactFormField
            {
                Name = "contact_by",
                Label = "Contact me by",
                Type = ContactFormFieldType.Radio,
                HelpText = "We answer the same day",
                Options = [new("email")]
            });

        var input = Field(cut, "company");
        cut.Find($"#{input.GetAttribute("aria-describedby")}").TextContent.ShouldBe("As registered");
        var group = cut.Find("fieldset");
        cut.Find($"#{group.GetAttribute("aria-describedby")}").TextContent.ShouldBe("We answer the same day");
    }

    [Fact]
    public void StaticContactForm_InvalidDefinition_FailsWithItsReason()
    {
        var exception = Should.Throw<ArgumentException>(() => RenderForm(new ContactFormField { Name = "bad name", Label = "Bad" }));

        exception.Message.ShouldContain("'bad name'");
    }

    // A guard test: without additional fields the script posts exactly the body it always posted, with no fields key.
    [Fact]
    public void StaticContactForm_ScriptWithoutAdditionalFields_PostsTheBodyItAlwaysDid()
    {
        var json = RunMessageOf(
            [["name", "Ann"], ["email", "ann@example.com"], ["company", "Example Ltd"], ["message", "A long enough message"], ["website", ""]],
            []);

        json.ShouldBe(
            """{"name":"Ann","email":"ann@example.com","company":"Example Ltd","message":"A long enough message","website":"","token":"tok"}""");
    }

    [Fact]
    public void StaticContactForm_ScriptWithAdditionalFields_PostsThemUnderFieldsKeyedByName()
    {
        var json = RunMessageOf(
            [
                ["name", "Ann"], ["email", "ann@example.com"], ["message", "A long enough message"], ["website", ""],
                ["field.size", "11-50"], ["field.topics", "design"], ["field.topics", "support"], ["field.notes", "Line one\nLine two"]
            ],
            [new("size", false), new("topics", true), new("contact_by", false), new("extras", true), new("notes", false)]);

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        root.EnumerateObject().Select(property => property.Name).ShouldBe(["name", "email", "message", "fields", "website", "token"]);
        root.GetProperty("fields").GetRawText().ShouldBe(
            """{"size":"11-50","topics":["design","support"],"contact_by":"","extras":[],"notes":"Line one\nLine two"}""");
    }

    [Fact]
    public void StaticContactForm_Script_ReadsTheFieldsFromTheMarkupAttributes()
    {
        var script = File.ReadAllText(LocateSource("OsirionStaticContactForm.razor.js"));

        script.ShouldContain("const FIELD_INPUT_PREFIX = 'field.';");
        script.ShouldContain("form.querySelectorAll('[data-additional-field]')");
        script.ShouldContain("element.hasAttribute('data-multiple')");
        script.ShouldContain("messageOf(new FormData(form), this.additionalFields, this.token)");
    }

    private IRenderedComponent<OsirionStaticContactForm> RenderForm(params ContactFormField[] fields) =>
        RenderStatic<OsirionStaticContactForm>(builder => builder
            .Add(p => p.Endpoint, "/api/contact")
            .Add(p => p.AdditionalFields, fields));

    private static IElement Field(IRenderedComponent<OsirionStaticContactForm> cut, string name) =>
        cut.Find($"form [name='field.{name}']");

    private sealed record FieldSpec(string Name, bool Multiple);

    // Runs the script's messageOf in Node, with a FormData holding the entries, and returns the JSON it would post.
    private static string RunMessageOf(string[][] entries, FieldSpec[] fields)
    {
        var runner = Path.Combine(Path.GetTempPath(), $"osirion-message-of-{Guid.NewGuid():N}.mjs");
        File.WriteAllText(runner, RunnerScript);
        try
        {
            var start = new ProcessStartInfo("node")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            start.ArgumentList.Add(runner);
            start.ArgumentList.Add(LocateSource("OsirionStaticContactForm.razor.js"));
            start.ArgumentList.Add(JsonSerializer.Serialize(new { entries, fields, token = "tok" }, new JsonSerializerOptions(JsonSerializerDefaults.Web)));

            using var process = Process.Start(start) ?? throw new InvalidOperationException("Node.js could not be started.");
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            process.ExitCode.ShouldBe(0, error);
            return output;
        }
        finally
        {
            File.Delete(runner);
        }
    }

    private const string RunnerScript =
        """
        import { pathToFileURL } from 'node:url';
        globalThis.BlazorJSComponents = { Component: class {} };
        const [modulePath, input] = process.argv.slice(2);
        const { messageOf } = await import(pathToFileURL(modulePath).href);
        const { entries, fields, token } = JSON.parse(input);
        const data = new FormData();
        for (const [name, value] of entries) {
            data.append(name, value);
        }
        process.stdout.write(JSON.stringify(messageOf(data, fields, token)));
        """;

    private static string LocateSource(string file)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "src", "Osirion.Blazor.Core", "Components", "Forms", file);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException(file);
    }
}
