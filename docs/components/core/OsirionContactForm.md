# OsirionContactForm

Purpose
Accessible contact form with optional contact info panel, privacy and subscription options, SSR-friendly form post pattern.

Key parameters
- Id (required), FormTitle, FormDescription
- NamePlaceholder, EmailPlaceholder, SubjectPlaceholder, MessagePlaceholder, MessageRows
- SubmitButtonText, SubmittingText, ShowResetButton, ResetButtonText
- PrivacyPolicyText (null or empty leaves the checkbox out; the form then submits without it)
- ShowSubscribeOption, SubscribeText
- ShowContactInformation, ContactInfoTitle, ContactInfo
- SideBySideLayout (default true), UseDarkTheme
- UseBuiltInEmailService (requires EmailServiceFactory registration)
- AdditionalFields: up to 20 `ContactFormField` definitions rendered after the message (see below)

Additional fields
- Each `ContactFormField` has `Name` (a letter, then up to 49 letters, digits, `_` or `-`; unique), `Label`, `Type`
  (`Text`, `MultilineText`, `Number`, `Email`, `Phone`, `Select`, `Radio`, `Checkboxes`), `Required`, `MaxLength`,
  `Min`/`Max` (numbers), `Options` (`ContactFormFieldOption(Value, Label)`, for the choice types), `Placeholder` and
  `HelpText`. An invalid definition, a repeated name or more than 20 fields throws an `ArgumentException`.
- Inputs are named `field.{Name}` and carry native `required`, `maxlength`, `min`, `max` (`step="any"` for numbers);
  radio buttons and checkboxes sit in a `fieldset` with a `legend`. Help text and the field's message are linked with
  `aria-describedby`; an invalid field gets `aria-invalid="true"`.
- The server validates every value before anything is sent, together with the standard fields: required, length
  (default 200, 2000 multi-line, 254 email, 40 phone and number), single line, number format and range, email, phone
  characters, option membership, and at most 10,000 characters for all answers. Each failure shows under its field.
  Under static SSR the values are read from the form post of this form (`_handler` equal to `Id`); names that are not
  defined are ignored.
- The answers reach `OnSubmit` as `ContactFormModel.AdditionalFieldValues` (one `ContactFormFieldValue` per definition,
  in order: `Name`, `Label`, `Values`, `Text`) and the built-in email (see `docs/CONTACT_FORM_EMAIL.md`).
- Without `AdditionalFields` the markup, the post and the email are what they were before.

Events
- OnSubmit(ContactFormModel)
- OnReset()
- OnValidationFailed(EditContext)

SSR behavior
- Uses SupplyParameterFromForm and query parameter SubmissionResultValue to display status after navigation

Notes
- Use GetAlertClass styles from OsirionComponentBase
- Add EmailOptions and a service factory to enable server email delivery.
