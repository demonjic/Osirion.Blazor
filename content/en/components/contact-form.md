---
id: 'osirion-contact-form'
order: 1
layout: docs
title: OsirionContactForm
permalink: /components/contact-form
description: A contact form that validates on the server, blocks simple bots with a honeypot and sends mail through SMTP, SendGrid or Infobip, without script.
author: Dejan Demonjić
date: 2025-09-03
last_modified: 2026-10-29
featured_image: 'https://media.getosirion.com/og/components-contact-form.jpg'
categories:
- Components
- Core Components
- Forms
tags:
- blazor
- contact-form
- forms
- static-ssr
- email
- validation
is_featured: true
published: true
slug: contact-form
lang: en
redirect_from:
- /docs/components/core/forms/contact-form
custom_fields:
  group: forms-and-feedback
seo_properties:
  title: 'OsirionContactForm: contact form for Blazor'
  description: 'A Blazor contact form for static SSR: server validation, honeypot spam check and email delivery through SMTP, SendGrid or Infobip.'
  image: 'https://media.getosirion.com/og/components-contact-form.jpg'
  canonical: 'https://getosirion.com/components/contact-form'
  lang: en
  robots: index, follow
  og_title: 'OsirionContactForm - Osirion.Blazor'
  og_description: 'Server-validated contact form with a honeypot and built-in SMTP, SendGrid or Infobip delivery.'
  og_image_url: 'https://media.getosirion.com/og/components-contact-form.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionContactForm - Osirion.Blazor'
  twitter_description: 'Server-validated Blazor contact form with email delivery and no script.'
  twitter_image_url: 'https://media.getosirion.com/og/components-contact-form.jpg'
  meta_tags: {}
---

# OsirionContactForm

`OsirionContactForm` renders a name, email, subject and message form with a privacy checkbox and an optional
newsletter checkbox. It validates on the server, posts as a normal HTML form under static SSR, and either sends the
message through the built-in email service or hands it to your own callback.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Forms/OsirionContactForm.razor).

```demo
contact-form
```

The demo validates like the real form, but its `OnSubmit` handler does nothing, so nothing is sent or stored.

## Usage

The smallest form passes the message to your code:

```razor
@page "/contact"
@using Osirion.Blazor.Components
@using Osirion.Blazor.Core.Models

<OsirionContactForm Id="contact" OnSubmit="HandleSubmitAsync" />

@code {
    private async Task HandleSubmitAsync(ContactFormModel message)
    {
        // Save the message, queue it or call your CRM here.
        // An exception shows ErrorMessage instead of SuccessMessage.
        await Task.CompletedTask;
    }
}
```

To have the library send the message by email, register the email service (next section) and set
`UseBuiltInEmailService`:

```razor
<OsirionContactForm Id="contact"
                    UseBuiltInEmailService="true"
                    FormTitle="Get in touch"
                    FormDescription="Questions about the library? Write to us."
                    PrivacyPolicyText="I agree to the <a href='/privacy'>privacy policy</a>"
                    ShowSubscribeOption="false"
                    ContactInfo="contactInfo" />

@code {
    private readonly ContactInformation contactInfo = new()
    {
        Address = "Belgrade, Serbia",
        Email = "hello@example.com"
    };
}
```

When `ContactInfo` is set, an [OsirionContactInfoSection](/components/contact-info-section) is
shown next to the form (or below it with `SideBySideLayout="false"`). Without it, no contact details are rendered.

A form with neither `OnSubmit` nor `UseBuiltInEmailService` has nothing to deliver the message with. It does not
claim success: it logs a warning and shows `ErrorMessage`, so a message is never lost silently.

## Email delivery

`AddOsirion(builder.Configuration)` registers the email service when the `Osirion:Email` section exists.
`Provider` is `Smtp` (the default), `SendGrid` or `Infobip`.

```json
{
  "Osirion": {
    "Email": {
      "Provider": "Smtp",
      "ToEmail": "hello@example.com",
      "FromName": "Website contact form",
      "SubjectTemplate": "Contact form: {subject}",
      "Smtp": {
        "Host": "smtp.example.com",
        "Port": 587,
        "EnableSsl": true,
        "Username": "contact@example.com",
        "FromEmail": "contact@example.com"
      }
    }
  }
}
```

Keep the password or API key out of `appsettings.json`: put `Osirion:Email:Smtp:Password`,
`Osirion:Email:SendGrid:ApiKey` or `Osirion:Email:Infobip:ApiKey` in user secrets locally, and in environment
variables or a key vault in production.

| Key | Default | Meaning |
| --- | --- | --- |
| `Provider` | `Smtp` | `Smtp`, `SendGrid` or `Infobip`. |
| `ToEmail` | empty | Where messages are delivered. |
| `FromName` | `Contact Form` | Sender display name. |
| `SubjectTemplate` | `Contact Form: {subject}` | `{subject}` is replaced with the visitor's subject. |
| `Smtp`: `Host`, `Port`, `EnableSsl`, `Username`, `Password`, `FromEmail` | port `587`, SSL on | SMTP server settings. |
| `SendGrid`: `ApiKey`, `FromEmail` | empty | SendGrid settings. |
| `Infobip`: `BaseUrl`, `ApiKey`, `FromEmail` | `BaseUrl` is `https://api.infobip.com` | Infobip settings. |

You can also configure it in code with `UseEmailServices`:

```csharp
using Osirion.Blazor.Core.Configuration;
using Osirion.Blazor.Extensions;

builder.Services.AddOsirion(osirion => osirion.UseEmailServices(email =>
{
    email.Provider = EmailProviderType.SendGrid;
    email.ToEmail = "hello@example.com";
    email.SendGrid.FromEmail = "noreply@example.com";
    email.SendGrid.ApiKey = builder.Configuration["Osirion:Email:SendGrid:ApiKey"] ?? string.Empty;
}));
```

Without the `Osirion.Blazor` meta-package, `services.AddOsirionEmailServices(configuration)` from
`Osirion.Blazor.Core.Extensions` does the same. The mail is HTML, every visitor value in it is HTML-encoded, and the
visitor's address is set as Reply-To, so you can answer directly. SendGrid and Infobip calls time out after 30
seconds.

## Additional fields

`AdditionalFields` turns the form into a qualification or quote form without a form of your own: up to 20
`ContactFormField` definitions, rendered in list order after the message and before the privacy checkbox. Each has a
`Name` (a letter, then up to 49 letters, digits, underscores or hyphens, unique in the form), a `Label`, a `Type`
(`Text`, `MultilineText`, `Number`, `Email`, `Phone`, `Select`, `Radio` or `Checkboxes`), `Required`, `MaxLength`,
`Min` and `Max` for numbers, `Options` for the three choice types (a `ContactFormFieldOption` with a `Value` and an
optional `Label`), and an optional `Placeholder` and `HelpText`.

```razor
<OsirionContactForm Id="quote"
                    FormTitle="Request a quote"
                    UseBuiltInEmailService="true"
                    AdditionalFields="@questions" />

@code {
    private static readonly ContactFormField[] questions =
    [
        new() { Name = "company", Label = "Company", Required = true },
        new() { Name = "team_size", Label = "Team size", Type = ContactFormFieldType.Select, Required = true,
                Placeholder = "Choose one", Options = [new("1-10", "1 to 10"), new("11-50", "11 to 50"), new("51+")] },
        new() { Name = "services", Label = "What do you need?", Type = ContactFormFieldType.Checkboxes, Required = true,
                Options = [new("design", "Design"), new("build", "Build"), new("support", "Support")] },
        new() { Name = "budget", Label = "Budget in euros", Type = ContactFormFieldType.Number, Min = 1000,
                HelpText = "A rough figure is enough." }
    ];
}
```

The inputs carry the native `required`, `maxlength` (200 for text, 2,000 for multi-line text, 254 for email and 40 for
phone and number unless `MaxLength` says otherwise), `min` and `max`, so the browser helps first. The server then
checks every submitted value against its definition before anything is sent, together with the standard fields: a
required field is answered (at least one box for `Checkboxes`), a value is within its length and on one line (except
`MultilineText`), a number parses and lies between `Min` and `Max`, an email address is valid, a phone number holds
only digits, spaces and `+ ( ) - . /`, and a choice is one of the option values. All answers together may hold at most
10,000 characters. A failure shows its message under the field, linked with `aria-describedby`, and the values the
visitor entered stay in the fields. Values posted under names you did not define are ignored.

The answers reach `OnSubmit` as `ContactFormModel.AdditionalFieldValues`, one `ContactFormFieldValue` per definition
in definition order, with the field's `Name` and `Label`, the submitted `Values` and the `Text` a person reads (the
option labels for choices). The built-in email lists the answered ones under "Additional details", each labelled and
HTML-encoded; they never reach a mail header. The form throws an `ArgumentException` naming the field for a definition
it cannot render safely, such as a name with a space, a name used twice or a choice field without options.

## Parameters

<!-- osirion:parameters OsirionContactForm -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Id` | `string` | `"contact-form"` | The form name and the prefix of every field id, such as `contact-name-contact`. Required; use a unique value for each form on a page. |
| `FormTitle` | `string` | `"Write us"` | The heading (`h2`) above the form. An empty value hides it. |
| `FormDescription` | `string` | `""` | The text under the heading. Empty by default, which renders nothing. |
| `NamePlaceholder` | `string` | `"Your name"` | The placeholder of the name field. |
| `EmailPlaceholder` | `string` | `"Your email address"` | The placeholder of the email field. |
| `SubjectPlaceholder` | `string` | `"Subject"` | The placeholder of the subject field. |
| `MessagePlaceholder` | `string` | `"Your message"` | The placeholder of the message field. |
| `MessageRows` | `int` | `6` | The number of rows of the message textarea. |
| `SubmitButtonText` | `string` | `"Send Message"` | The text of the submit button. |
| `SubmittingText` | `string` | `"Sending..."` | A text for the submit button while sending. Not used by the current markup. |
| `ShowResetButton` | `bool` | `false` | Whether a reset button, which clears the fields in the browser, is shown next to the submit button. |
| `ResetButtonText` | `string` | `"Reset"` | The text of the reset button. |
| `ShowRequiredIndicators` | `bool` | `true` | Whether a `*` follows the label of each required field and of the privacy checkbox. |
| `NameLabel` | `string` | `"Name"` | The label of the name field. |
| `EmailLabel` | `string` | `"Email"` | The label of the email field. |
| `SubjectLabel` | `string` | `"Subject"` | The label of the subject field. |
| `MessageLabel` | `string` | `"Message"` | The label of the message field. |
| `ValidationTexts` | `ContactFormValidationTexts?` | `null` | The validation messages, for a form in another language. Null, the default, shows the English messages of `ContactFormValidationTexts.Default`. |
| `AdditionalFields` | `IReadOnlyList<ContactFormField>?` | `null` | Fields the site adds after the message, in list order, such as a company size or a budget on a quote form: at most `ContactFormField.MaxFields`, each with a unique, safe `ContactFormField.Name`. They render with native `required`, `maxlength`, `min` and `max`, and the server checks the submitted values against their definitions before anything is sent: a value that is missing, too long, out of range, not one of the options or not an email address shows a message under its field, and the answers together may hold at most `ContactFormField.MaxTotalLength` characters. Values posted under any other name are ignored. The answers reach `OnSubmit` and the email, in definition order, as `ContactFormModel.AdditionalFieldValues`. Null or empty, the default, renders the form as before. |
| `PrivacyPolicyText` | `string?` | `"I agree to the <a href="/privacy-policy" target="_blank">privacy policy</a>"` | The label of the privacy checkbox, rendered as raw HTML, so pass only text you wrote. The default text links to `/privacy-policy`. The checkbox is left out when the value is null or empty, and the form then submits without it: `ContactFormModel.AgreeToPrivacyPolicy` is set to true so the model validates, which means it records no consent and must not be stored as consent. |
| `ShowSubscribeOption` | `bool` | `true` | Whether the newsletter checkbox is shown. Its value reaches your code as `ContactFormModel.SubscribeToUpdates`. |
| `SubscribeText` | `string` | `"I would like to receive updates and newsletters"` | The label of the newsletter checkbox. |
| `ShowContactInformation` | `bool` | `true` | Whether the contact information section is shown when `ContactInfo` is set. |
| `ContactInfoTitle` | `string` | `"Contact information"` | The title of the contact information section. |
| `ContactInfoAddressLabel` | `string` | `"Address:"` | The label of the address in the contact information section. |
| `ContactInfoPhoneLabel` | `string` | `"Phone:"` | The label of the phone number in the contact information section. |
| `ContactInfoEmailLabel` | `string` | `"Email:"` | The label of the email address in the contact information section. |
| `ContactInfoWebsiteLabel` | `string` | `"Website:"` | The label of the website in the contact information section. |
| `ContactInfo` | `ContactInformation?` | `null` | The address, phone, email, website and message shown beside or below the form. When it is `null`, which is the default, no contact information section is rendered; the form never invents placeholder details. |
| `OnSubmit` | `EventCallback<ContactFormModel>` | - | The callback that receives the validated message. It runs for a submission that is valid and whose honeypot field is empty. With `UseBuiltInEmailService` it runs after the email was sent; without it, it is the only delivery. When it throws, `ErrorMessage` is shown. Without it, and without `UseBuiltInEmailService`, nothing can deliver the message, so the form reports an error. |
| `OnReset` | `EventCallback` | - | A callback for a form reset. Not used by the current markup. |
| `OnValidationFailed` | `EventCallback<EditContext>` | - | A callback for failed validation. Not used by the current markup. |
| `SuccessMessage` | `string` | `"Thank you! Your message has been sent successfully. We'll get back to you soon."` | The message shown after a successful submission. |
| `ErrorMessage` | `string` | `"Sorry, there was an error sending your message. Please try again later."` | The message shown when sending fails, when `OnSubmit` throws, or when nothing can deliver the message. |
| `SpamMessage` | `string` | `"Your submission appears to be spam and has been blocked."` | The message shown when the hidden honeypot field was filled. Nothing is sent and `OnSubmit` is not called. |
| `UseBuiltInEmailService` | `bool` | `false` | Whether the message is sent through the email service registered with `AddOsirion` or `AddOsirionEmailServices` (SMTP, SendGrid or Infobip). When the service is not registered, the form logs a warning and shows `ErrorMessage`. |
| `SideBySideLayout` | `bool` | `true` | Whether the contact information sits beside the form (`true`, the default) or below it. |
| `UseDarkTheme` | `bool` | `false` | Whether the form uses its dark palette instead of the theme colors. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

`ContactFormModel`, which `OnSubmit` receives, has `Name`, `Email`, `Subject`, `Message`, `AgreeToPrivacyPolicy`,
`SubscribeToUpdates`, the honeypot field `Website`, and `AdditionalFieldValues`, the answers to
[additional fields](#additional-fields).

## Behavior

- **Static SSR.** The form is an `EditForm` with `FormName` set to `Id`, so it posts and validates on the server
  without script. After a valid post the component redirects to the same page with a `SubmissionResultValue` query
  value, and the page shows the success, error or spam message. Invalid fields show their messages in place.
- **Validation** comes from data annotations on `ContactFormModel`: a name up to 100 characters, a valid email up to
  255, a subject up to 200, a message of 10 to 5,000 characters, and a ticked privacy checkbox.
- **Another language.** Every text is a parameter with its English text as the default: `NameLabel`,
  `EmailLabel`, `SubjectLabel` and `MessageLabel` for the labels, the placeholders, buttons and messages, and
  `ContactInfoAddressLabel` and its siblings for the contact information. The validation messages are one
  `ContactFormValidationTexts` object in `ValidationTexts`, a composite format string per rule (`{0}` is a limit or a
  field's label); a property you leave out keeps its English message, and a string with a placeholder it is not
  given throws an `ArgumentException` naming the property when the form renders. From the release after 4.3.0.
- **Spam.** A hidden `Website` field acts as a honeypot. When a bot fills it, nothing is sent, `OnSubmit` is not
  called and `SpamMessage` is shown.
- **When nothing can deliver the message.** The form reports `ErrorMessage`, never success and never the spam
  message, in two cases: neither `OnSubmit` nor `UseBuiltInEmailService` is set, or `UseBuiltInEmailService` is
  `true` and no email service is registered. Both log a warning. A failed send, or an exception from `OnSubmit`,
  shows `ErrorMessage` too and is logged as an error.
- **Both delivery paths.** With `UseBuiltInEmailService="true"`, `OnSubmit` is called only after the email was sent,
  so you can also log or store the message.
- **Accessibility.** Every field has a `label`, and each input points to its validation message with
  `aria-describedby`. Status icons are hidden from assistive technology; the message text carries the meaning. An
  additional field with radio buttons or checkboxes is a `fieldset` named by its `legend`; an additional field points
  to its `HelpText` and, when invalid, to its message, and carries `aria-invalid="true"`.

> [!WARNING]
> `PrivacyPolicyText` is rendered as raw HTML. Use only text you wrote; never pass visitor or CMS input.

> [!NOTE]
> Set `PrivacyPolicyText` to null or empty to leave the checkbox out. The form then submits without it, and
> `AgreeToPrivacyPolicy` on the model `OnSubmit` receives is `true` so the model validates. It then records no
> consent, so do not store it as consent. Keep the checkbox if you need the visitor's consent.

Rate-limit the page that hosts the form: the honeypot stops simple bots, not a determined sender.

## Styling

The outer element has `osirion-contact-form-container` plus `osirion-contact-form-side-by-side` or
`osirion-contact-form-stacked`, and `osirion-contact-form-dark` with `UseDarkTheme`. Override the form's custom
properties on the container:

```css
.osirion-contact-form-container {
    --osirion-contact-accent: #0e7490;
    --osirion-contact-radius: 0;
}
```

The properties are `--osirion-contact-bg`, `--osirion-contact-form-bg`, `--osirion-contact-text`,
`--osirion-contact-text-muted`, `--osirion-contact-border`, `--osirion-contact-accent`, `--osirion-contact-radius` and
`--osirion-contact-spacing`. In the light palette they default to the theme tokens (`--osirion-background-primary`,
`--osirion-text-primary`, `--osirion-action-primary` and so on), so a CSS framework adapter recolors the form. Buttons
and the status message use the active framework's classes.

## Related

- [Forms and feedback](/components/forms-and-feedback)
- [OsirionContactInfoSection](/components/contact-info-section)
- [OsirionSubscriptionCard](/components/subscription-card)
- [OsirionFooter](/components/footer)
