using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Osirion.Blazor.Core.Models;
using Osirion.Blazor.Core.Services;

namespace Osirion.Blazor.Components;

/// <summary>
/// A comprehensive contact form component with validation, accessibility, and framework-agnostic styling.
/// Includes optional contact information display section and follows Osirion design principles.
/// </summary>
public partial class OsirionContactForm : OsirionComponentBase
{
    /// <summary>Performs the OsirionContactForm operation.</summary>
    public OsirionContactForm()
    {
        FormModel = new ContactFormModel();
    }

    #region Form Parameters

    /// <summary>
    /// Gets or sets the form name and the prefix of every field id, such as <c>contact-name-contact</c>. Required; use a
    /// unique value for each form on a page.
    /// </summary>
    [Parameter, EditorRequired]
    public string Id { get; set; } = "contact-form";

    /// <summary>
    /// Gets or sets the heading (<c>h2</c>) above the form. An empty value hides it.
    /// </summary>
    [Parameter]
    public string FormTitle { get; set; } = "Write us";

    /// <summary>
    /// Gets or sets the text under the heading. Empty by default, which renders nothing.
    /// </summary>
    [Parameter]
    public string FormDescription { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the placeholder of the name field.
    /// </summary>
    [Parameter]
    public string NamePlaceholder { get; set; } = "Your name";

    /// <summary>
    /// Gets or sets the placeholder of the email field.
    /// </summary>
    [Parameter]
    public string EmailPlaceholder { get; set; } = "Your email address";

    /// <summary>
    /// Gets or sets the placeholder of the subject field.
    /// </summary>
    [Parameter]
    public string SubjectPlaceholder { get; set; } = "Subject";

    /// <summary>
    /// Gets or sets the placeholder of the message field.
    /// </summary>
    [Parameter]
    public string MessagePlaceholder { get; set; } = "Your message";

    /// <summary>
    /// Gets or sets the number of rows of the message textarea.
    /// </summary>
    [Parameter]
    public int MessageRows { get; set; } = 6;

    /// <summary>
    /// Gets or sets the text of the submit button.
    /// </summary>
    [Parameter]
    public string SubmitButtonText { get; set; } = "Send Message";

    /// <summary>
    /// Gets or sets a text for the submit button while sending. Not used by the current markup.
    /// </summary>
    [Parameter]
    public string SubmittingText { get; set; } = "Sending...";

    /// <summary>
    /// Gets or sets whether a reset button, which clears the fields in the browser, is shown next to the submit button.
    /// </summary>
    [Parameter]
    public bool ShowResetButton { get; set; } = false;

    /// <summary>
    /// Gets or sets the text of the reset button.
    /// </summary>
    [Parameter]
    public string ResetButtonText { get; set; } = "Reset";

    /// <summary>
    /// Gets or sets whether a <c>*</c> follows the label of each required field and of the privacy checkbox.
    /// </summary>
    [Parameter]
    public bool ShowRequiredIndicators { get; set; } = true;

    /// <summary>
    /// Gets or sets fields the site adds after the message, in list order, such as a company size or a budget on a quote
    /// form: at most <see cref="ContactFormField.MaxFields"/>, each with a unique, safe <see cref="ContactFormField.Name"/>.
    /// They render with native <c>required</c>, <c>maxlength</c>, <c>min</c> and <c>max</c>, and the server checks the
    /// submitted values against their definitions before anything is sent: a value that is missing, too long, out of
    /// range, not one of the options or not an email address shows a message under its field, and the answers together
    /// may hold at most <see cref="ContactFormField.MaxTotalLength"/> characters. Values posted under any other name are
    /// ignored. The answers reach <see cref="OnSubmit"/> and the email, in definition order, as
    /// <see cref="ContactFormModel.AdditionalFieldValues"/>. Null or empty, the default, renders the form as before.
    /// </summary>
    /// <exception cref="ArgumentException">A definition is invalid, repeats a name, or there are too many.</exception>
    [Parameter]
    public IReadOnlyList<ContactFormField>? AdditionalFields { get; set; }

    #endregion

    #region Privacy and Subscription Parameters

    /// <summary>
    /// Gets or sets the label of the privacy checkbox, rendered as raw HTML, so pass only text you wrote. The default
    /// text links to <c>/privacy-policy</c>. The checkbox is left out when the value is null or empty, and the form
    /// then submits without it: <c>ContactFormModel.AgreeToPrivacyPolicy</c> is set to true so the model validates, which
    /// means it records no consent and must not be stored as consent.
    /// </summary>
    [Parameter]
    public string? PrivacyPolicyText { get; set; } = "I agree to the <a href=\"/privacy-policy\" target=\"_blank\">privacy policy</a>";

    /// <summary>
    /// Gets or sets whether the newsletter checkbox is shown. Its value reaches your
    /// code as <c>ContactFormModel.SubscribeToUpdates</c>.
    /// </summary>
    [Parameter]
    public bool ShowSubscribeOption { get; set; } = true;

    /// <summary>
    /// Gets or sets the label of the newsletter checkbox.
    /// </summary>
    [Parameter]
    public string SubscribeText { get; set; } = "I would like to receive updates and newsletters";

    #endregion

    #region Contact Information Parameters

    /// <summary>
    /// Gets or sets whether the contact information section is shown when <see cref="ContactInfo"/> is set.
    /// </summary>
    [Parameter]
    public bool ShowContactInformation { get; set; } = true;

    /// <summary>
    /// Gets or sets the title of the contact information section.
    /// </summary>
    [Parameter]
    public string ContactInfoTitle { get; set; } = "Contact information";

    /// <summary>
    /// Gets or sets the address, phone, email, website and message shown beside or below the form. When it is
    /// <see langword="null"/>, which is the default, no contact information section is rendered; the form never invents
    /// placeholder details.
    /// </summary>
    [Parameter]
    public ContactInformation? ContactInfo { get; set; }

    #endregion

    #region Event Parameters

    /// <summary>
    /// Gets or sets the callback that receives the validated message. It runs for a submission that is valid and whose
    /// honeypot field is empty. With <see cref="UseBuiltInEmailService"/> it runs after the email was sent; without it, it is
    /// the only delivery. When it throws, <see cref="ErrorMessage"/> is shown. Without it, and without
    /// <see cref="UseBuiltInEmailService"/>, nothing can deliver the message, so the form reports an error.
    /// </summary>
    [Parameter]
    public EventCallback<ContactFormModel> OnSubmit { get; set; }

    /// <summary>
    /// Gets or sets a callback for a form reset. Not used by the current markup.
    /// </summary>
    [Parameter]
    public EventCallback OnReset { get; set; }

    /// <summary>
    /// Gets or sets a callback for failed validation. Not used by the current markup.
    /// </summary>
    [Parameter]
    public EventCallback<EditContext> OnValidationFailed { get; set; }

    #endregion

    #region Success/Error Message Parameters

    /// <summary>
    /// Gets or sets the message shown after a successful submission.
    /// </summary>
    [Parameter]
    public string SuccessMessage { get; set; } = "Thank you! Your message has been sent successfully. We'll get back to you soon.";

    /// <summary>
    /// Gets or sets the message shown when sending fails, when <see cref="OnSubmit"/> throws, or when nothing can deliver the
    /// message.
    /// </summary>
    [Parameter]
    public string ErrorMessage { get; set; } = "Sorry, there was an error sending your message. Please try again later.";

    /// <summary>
    /// Gets or sets the message shown when the hidden honeypot field was filled. Nothing is sent and <see cref="OnSubmit"/> is
    /// not called.
    /// </summary>
    [Parameter]
    public string SpamMessage { get; set; } = "Your submission appears to be spam and has been blocked.";

    /// <summary>
    /// Gets or sets whether the message is sent through the email service registered with <c>AddOsirion</c> or
    /// <c>AddOsirionEmailServices</c> (SMTP, SendGrid or Infobip). When the service is
    /// not registered, the form logs a warning and shows <see cref="ErrorMessage"/>.
    /// </summary>
    [Parameter]
    public bool UseBuiltInEmailService { get; set; } = false;

    #endregion

    #region Layout Parameters

    /// <summary>
    /// Gets or sets whether the contact information sits beside the form (<see langword="true"/>, the default) or below it.
    /// </summary>
    [Parameter]
    public bool SideBySideLayout { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the form uses its dark palette instead of the theme colors.
    /// </summary>
    [Parameter]
    public bool UseDarkTheme { get; set; } = false;

    [SupplyParameterFromForm]
    private ContactFormModel FormModel { get; set; }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        // The model requires a ticked privacy box, so when the box is not rendered nothing could ever satisfy it.
        // No checkbox means no consent was asked for, and the form must still be submittable.
        if (string.IsNullOrWhiteSpace(PrivacyPolicyText))
        {
            FormModel.AgreeToPrivacyPolicy = true;
        }

        _fields = ContactFormFieldRules.Checked(AdditionalFields);

        // The context EditForm would create from Model, made here so the additional fields can join its validation.
        if (!ReferenceEquals(_editContext?.Model, FormModel))
        {
            _editContext = new EditContext(FormModel);
            _fieldMessages = new ValidationMessageStore(_editContext);
            _editContext.OnValidationRequested += (_, _) => ValidateAdditionalFields();
        }

        ReadPostedFieldValues();
    }

    [SupplyParameterFromQuery]
    private int? SubmissionResultValue { get; set; }

    #endregion

    #region Private Fields and Services

    [Inject]
    private IServiceProvider ServiceProvider { get; set; } = default!;

    [Inject]
    private ILogger<OsirionContactForm> Logger { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    // Set under static rendering only: the request whose form post holds the additional fields' values.
    [CascadingParameter]
    private HttpContext? CascadedHttpContext { get; set; }

    private IReadOnlyList<ContactFormField> _fields = [];

    // What each additional field holds, by name: read from the form post, or kept from the inputs' change events.
    private readonly Dictionary<string, IReadOnlyList<string>> _fieldInput = new(StringComparer.Ordinal);

    private EditContext? _editContext;

    private ValidationMessageStore? _fieldMessages;

    private bool _postedValuesRead;

    private ContactFormSubmissionResult? SubmissionResult =>
        SubmissionResultValue.HasValue && Enum.IsDefined(typeof(ContactFormSubmissionResult), SubmissionResultValue.Value)
            ? (ContactFormSubmissionResult)SubmissionResultValue.Value
            : null;

    #endregion

    #region Event Handlers

    private async Task HandleValidSubmit()
    {
        // Only a filled honeypot field means spam. Anything that stops a genuine submission from being sent
        // (a missing email service, a failed send) must not tell the visitor they were flagged as spam.
        var contactFormSubmissionResult = ContactFormSubmissionResult.ServerError;

        if (!string.IsNullOrWhiteSpace(FormModel.Website))
        {
            contactFormSubmissionResult = ContactFormSubmissionResult.SpamDetected;
        }
        else
        {
            try
            {
                // Use built-in email service if enabled and registered
                if (UseBuiltInEmailService)
                {
                    var emailServiceFactory = ServiceProvider.GetService<EmailServiceFactory>();
                    if (emailServiceFactory != null)
                    {
                        var emailService = emailServiceFactory.CreateEmailService();
                        var result = await emailService.SendEmailAsync(FormModel);

                        if (result.IsSuccess)
                        {
                            await OnSubmit.InvokeAsync(FormModel);
                            contactFormSubmissionResult = ContactFormSubmissionResult.Success;
                        }
                        else
                        {
                            Logger.LogError("Email sending failed: {ErrorMessage}", result.ErrorMessage);
                            contactFormSubmissionResult = ContactFormSubmissionResult.ServerError;
                        }
                    }
                    else
                    {
                        Logger.LogWarning("Email service factory not registered but UseBuiltInEmailService is true");
                    }
                }
                else if (OnSubmit.HasDelegate)
                {
                    // Invoke the submit callback for custom handling
                    await OnSubmit.InvokeAsync(FormModel);

                    // If no explicit navigation happened, assume success
                    contactFormSubmissionResult = ContactFormSubmissionResult.Success;
                }
                else
                {
                    // Nothing can deliver the message; telling the visitor it was sent would lose it silently.
                    Logger.LogWarning("Contact form submitted without OnSubmit or UseBuiltInEmailService; the message was not delivered");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error processing contact form submission");
                contactFormSubmissionResult = ContactFormSubmissionResult.ServerError;
            }
        }

        NavigateWithResult(contactFormSubmissionResult);
    }

    private void NavigateWithResult(ContactFormSubmissionResult result)
    {
        var uri = NavigationManager.GetUriWithQueryParameter("SubmissionResultValue", (int)result);
        NavigationManager.NavigateTo(uri, forceLoad: true);
    }

    #endregion

    #region Additional Fields

    // The validation message of the answers' total length, which belongs to no single field.
    private FieldIdentifier TotalLengthField => new(FormModel, nameof(ContactFormModel.AdditionalFieldValues));

    private string? TotalLengthError => FirstMessage(TotalLengthField);

    // Under static rendering a submission arrives as a new request, so the values are read from this form's post. The
    // _handler entry names the form that was posted; a post of another form on the page leaves the fields empty.
    private void ReadPostedFieldValues()
    {
        if (_postedValuesRead || _fields.Count == 0 || CascadedHttpContext?.Request is not { } request
            || !HttpMethods.IsPost(request.Method) || !request.HasFormContentType)
        {
            return;
        }

        _postedValuesRead = true;
        var form = request.Form;
        if (form["_handler"] != Id)
        {
            return;
        }

        // Only the defined names are read; anything else in the post is ignored.
        foreach (var field in _fields)
        {
            _fieldInput[field.Name] = [.. form[ContactFormFieldRules.InputName(field)].OfType<string>()];
        }
    }

    private IReadOnlyList<string> SubmittedValues(ContactFormField field) =>
        _fieldInput.TryGetValue(field.Name, out var values) ? values : [];

    private void ValidateAdditionalFields()
    {
        _fieldMessages!.Clear();
        FormModel.AdditionalFieldValues = [];
        if (_fields.Count == 0)
        {
            return;
        }

        var answers = new List<ContactFormFieldValue>(_fields.Count);
        var valid = true;
        foreach (var field in _fields)
        {
            if (ContactFormFieldRules.Validate(field, SubmittedValues(field), out var answer) is { } error)
            {
                _fieldMessages.Add(FieldFor(field), error);
                valid = false;
            }

            answers.Add(answer);
        }

        if (valid && answers.Sum(answer => answer.Values.Sum(value => value.Length)) > ContactFormField.MaxTotalLength)
        {
            _fieldMessages.Add(TotalLengthField,
                $"The answers to the additional questions cannot exceed {ContactFormField.MaxTotalLength} characters together.");
            valid = false;
        }

        if (valid)
        {
            FormModel.AdditionalFieldValues = answers;
        }
    }

    // Interactive rendering only: the inputs' change events keep the values, and a changed field is checked again, as
    // DataAnnotationsValidator checks a standard field.
    private void SetFieldValue(ContactFormField field, object? value)
    {
        _fieldInput[field.Name] = value is string text ? [text] : [];
        RevalidateField(field);
    }

    private void SetOptionChosen(ContactFormField field, string optionValue, object? value)
    {
        var chosen = SubmittedValues(field).Where(existing => existing != optionValue);
        _fieldInput[field.Name] = value is true ? [.. chosen, optionValue] : [.. chosen];
        RevalidateField(field);
    }

    private void RevalidateField(ContactFormField field)
    {
        var identifier = FieldFor(field);
        _fieldMessages!.Clear(identifier);
        if (ContactFormFieldRules.Validate(field, SubmittedValues(field), out _) is { } error)
        {
            _fieldMessages.Add(identifier, error);
        }

        _editContext!.NotifyValidationStateChanged();
    }

    private FieldIdentifier FieldFor(ContactFormField field) => new(FormModel, ContactFormFieldRules.InputName(field));

    private string? FieldError(ContactFormField field) => FirstMessage(FieldFor(field));

    private string? FirstMessage(FieldIdentifier identifier) =>
        _editContext?.GetValidationMessages(identifier).FirstOrDefault();

    private string FieldValue(ContactFormField field) => SubmittedValues(field).FirstOrDefault() ?? string.Empty;

    private bool IsChosen(ContactFormField field, ContactFormFieldOption option) => SubmittedValues(field).Contains(option.Value);

    private string FieldInputId(ContactFormField field) => $"contact-field-{field.Name}-{Id}";

    private string FieldHelpId(ContactFormField field) => $"field-{field.Name}-help-{Id}";

    private string FieldErrorId(ContactFormField field) => $"field-{field.Name}-error-{Id}";

    // The hint and the message, whichever are shown, so assistive technology reads both with the field.
    private string? FieldDescribedBy(ContactFormField field, string? error)
    {
        var ids = new List<string>(2);
        if (!string.IsNullOrWhiteSpace(field.HelpText))
        {
            ids.Add(FieldHelpId(field));
        }

        if (error is not null)
        {
            ids.Add(FieldErrorId(field));
        }

        return ids.Count == 0 ? null : string.Join(' ', ids);
    }

    #endregion

    #region CSS Class Methods

    private string GetContactFormContainerClass()
    {
        var classes = new List<string> { "osirion-contact-form-container" };

        if (SideBySideLayout)
            classes.Add("osirion-contact-form-side-by-side");
        else
            classes.Add("osirion-contact-form-stacked");

        if (UseDarkTheme)
            classes.Add("osirion-contact-form-dark");

        if (ShowContactInformation && ContactInfo != null)
            classes.Add("osirion-contact-form-with-info");

        return CombineCssClasses(string.Join(" ", classes));
    }

    private string GetStatusMessageClass()
    {
        var alertVariant = SubmissionResult switch
        {
            ContactFormSubmissionResult.Success => Severity.Success,
            ContactFormSubmissionResult.ValidationError or ContactFormSubmissionResult.ServerError => Severity.Error,
            ContactFormSubmissionResult.SpamDetected => Severity.Warning,
            _ => Severity.Info
        };

        return GetAlertClass(alertVariant);
    }

    private string GetStatusMessage()
    {
        return SubmissionResult switch
        {
            ContactFormSubmissionResult.Success => SuccessMessage,
            ContactFormSubmissionResult.ValidationError => "Please correct the errors below and try again.",
            ContactFormSubmissionResult.ServerError => ErrorMessage,
            ContactFormSubmissionResult.SpamDetected => SpamMessage,
            _ => string.Empty
        };
    }

    private RenderFragment GetStatusIcon()
    {
        return SubmissionResult switch
        {
            ContactFormSubmissionResult.Success => CreateSuccessIcon(),
            ContactFormSubmissionResult.ValidationError => CreateErrorIcon(),
            ContactFormSubmissionResult.ServerError => CreateErrorIcon(),
            ContactFormSubmissionResult.SpamDetected => CreateWarningIcon(),
            _ => CreateInfoIcon()
        };
    }

    private RenderFragment CreateSuccessIcon() => builder =>
    {
        builder.OpenElement(0, "svg");
        builder.AddAttribute(1, "xmlns", "http://www.w3.org/2000/svg");
        builder.AddAttribute(2, "width", "24");
        builder.AddAttribute(3, "height", "24");
        builder.AddAttribute(4, "fill", "currentColor");
        builder.AddAttribute(5, "class", "osirion-status-icon bi bi-check-circle");
        builder.AddAttribute(6, "viewBox", "0 0 20 20");
        builder.AddAttribute(7, "aria-hidden", "true");
        builder.OpenElement(8, "path");
        builder.AddAttribute(9, "d", "M8 15A7 7 0 1 1 8 1a7 7 0 0 1 0 14m0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16");
        builder.CloseElement(); // Close path
        builder.OpenElement(10, "path");
        builder.AddAttribute(11, "d", "m10.97 4.97-.02.022-3.473 4.425-2.093-2.094a.75.75 0 0 0-1.06 1.06L6.97 11.03a.75.75 0 0 0 1.079-.02l3.992-4.99a.75.75 0 0 0-1.071-1.05");
        builder.CloseElement(); // Close path
        builder.CloseElement(); // Close svg
    };

    private RenderFragment CreateErrorIcon() => builder =>
    {
        builder.OpenElement(0, "svg");
        builder.AddAttribute(1, "xmlns", "http://www.w3.org/2000/svg");
        builder.AddAttribute(2, "width", "24");
        builder.AddAttribute(3, "height", "24");
        builder.AddAttribute(4, "fill", "currentColor");
        builder.AddAttribute(5, "class", "osirion-status-icon bi bi-exclamation-circle");
        builder.AddAttribute(6, "viewBox", "0 0 20 20");
        builder.AddAttribute(7, "aria-hidden", "true");
        builder.OpenElement(8, "path");
        builder.AddAttribute(9, "d", "M8 15A7 7 0 1 1 8 1a7 7 0 0 1 0 14m0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16");
        builder.CloseElement(); // Close path
        builder.OpenElement(10, "path");
        builder.AddAttribute(11, "d", "M7.002 11a1 1 0 1 1 2 0 1 1 0 0 1-2 0M7.1 4.995a.905.905 0 1 1 1.8 0l-.35 3.507a.552.552 0 0 1-1.1 0z");
        builder.CloseElement(); // Close path
        builder.CloseElement(); // Close svg
    };

    private RenderFragment CreateWarningIcon() => builder =>
    {
        builder.OpenElement(0, "svg");
        builder.AddAttribute(1, "xmlns", "http://www.w3.org/2000/svg");
        builder.AddAttribute(2, "width", "24");
        builder.AddAttribute(3, "height", "24");
        builder.AddAttribute(4, "fill", "currentColor");
        builder.AddAttribute(5, "class", "osirion-status-icon bi bi-exclamation-triangle");
        builder.AddAttribute(6, "viewBox", "0 0 20 20");
        builder.AddAttribute(7, "aria-hidden", "true");
        builder.OpenElement(8, "path");
        builder.AddAttribute(9, "d", "M7.938 2.016A.13.13 0 0 1 8.002 2a.13.13 0 0 1 .063.016.15.15 0 0 1 .054.057l6.857 11.667c.036.06.035.124.002.183a.2.2 0 0 1-.054.06.1.1 0 0 1-.066.017H1.146a.1.1 0 0 1-.066-.017.2.2 0 0 1-.054-.06.18.18 0 0 1 .002-.183L7.884 2.073a.15.15 0 0 1 .054-.057m1.044-.45a1.13 1.13 0 0 0-1.96 0L.165 13.233c-.457.778.091 1.767.98 1.767h13.713c.889 0 1.438-.99.98-1.767z");
        builder.CloseElement(); // Close path
        builder.OpenElement(10, "path");
        builder.AddAttribute(11, "d", "M7.002 12a1 1 0 1 1 2 0 1 1 0 0 1-2 0M7.1 5.995a.905.905 0 1 1 1.8 0l-.35 3.507a.552.552 0 0 1-1.1 0z");
        builder.CloseElement(); // Close path
        builder.CloseElement(); // Close svg
    };

    private RenderFragment CreateInfoIcon() => builder =>
    {
        builder.OpenElement(0, "svg");
        builder.AddAttribute(1, "xmlns", "http://www.w3.org/2000/svg");
        builder.AddAttribute(2, "width", "24");
        builder.AddAttribute(3, "height", "24");
        builder.AddAttribute(4, "fill", "currentColor");
        builder.AddAttribute(5, "class", "osirion-status-icon bi bi-info-circle");
        builder.AddAttribute(6, "viewBox", "0 0 20 20");
        builder.AddAttribute(7, "aria-hidden", "true");
        builder.OpenElement(8, "path");
        builder.AddAttribute(9, "d", "M8 15A7 7 0 1 1 8 1a7 7 0 0 1 0 14m0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16");
        builder.CloseElement(); // Close path
        builder.OpenElement(10, "path");
        builder.AddAttribute(11, "d", "m8.93 6.588-2.29.287-.082.38.45.083c.294.07.352.176.288.469l-.738 3.468c-.194.897.105 1.319.808 1.319.545 0 1.178-.252 1.465-.598l.088-.416c-.2.176-.492.246-.686.246-.275 0-.375-.193-.304-.533zM9 4.5a1 1 0 1 1-2 0 1 1 0 0 1 2 0");
        builder.CloseElement(); // Close path
        builder.CloseElement(); // Close svg
    };

    #endregion
}
