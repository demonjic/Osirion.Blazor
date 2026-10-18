---
id: 'osirion-subscription-card'
order: 3
layout: docs
title: OsirionSubscriptionCard
permalink: /components/subscription-card
description: OsirionSubscriptionCard is a newsletter signup form that works under static SSR, with validation, a honeypot and optional topic choices.
author: Dejan Demonjić
date: 2025-09-03
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-subscription-card.jpg'
categories:
- Components
- Core Components
- Cards
- Forms
tags:
- subscription-card
- newsletter
- forms
- validation
- static-ssr
is_featured: true
published: true
slug: subscription-card
lang: en
redirect_from:
- /docs/components/core/cards/subscription-card
custom_fields:
  group: cards
seo_properties:
  title: 'OsirionSubscriptionCard: a newsletter form for Blazor'
  description: 'OsirionSubscriptionCard is a newsletter signup form that works under static SSR, with validation, a honeypot and optional topic choices.'
  image: 'https://media.getosirion.com/og/components-subscription-card.jpg'
  canonical: 'https://getosirion.com/components/subscription-card'
  lang: en
  robots: index, follow
  og_title: 'OsirionSubscriptionCard - Osirion.Blazor'
  og_description: 'A newsletter signup form for static SSR: email validation, privacy consent, a honeypot and topic choices.'
  og_image_url: 'https://media.getosirion.com/og/components-subscription-card.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionSubscriptionCard - Osirion.Blazor'
  twitter_description: 'A newsletter signup form for static SSR with validation and topic choices.'
  twitter_image_url: 'https://media.getosirion.com/og/components-subscription-card.jpg'
  meta_tags: {}
---

# OsirionSubscriptionCard

`OsirionSubscriptionCard` is a newsletter signup form in a card: an email field, an optional name field, a privacy
consent checkbox and optional topics to choose from. It posts as a normal form, so it works on static SSR pages
without a circuit or WebAssembly. Your code receives the validated data in `OnSubscribe` and stores it wherever your
mailing list lives.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Cards/OsirionSubscriptionCard.razor).

```demo
subscription-card
```

## Usage

```razor
@using Osirion.Blazor.Core.Models

<OsirionSubscriptionCard Id="newsletter" OnSubscribe="SaveAsync" />

@code {
    private Task SaveAsync(SubscriptionModel subscription)
    {
        // Send subscription.Email to your mailing list provider here.
        // Throwing an exception shows ErrorMessage instead of SuccessMessage.
        return Task.CompletedTask;
    }
}
```

A card with a name field and topics, where one topic cannot be turned off:

```razor
@using Osirion.Blazor.Core.Models

<OsirionSubscriptionCard Id="blog-signup"
                         Title="Get new articles by email"
                         Description="One email when a new article is published."
                         ShowNameField="true"
                         ShowSubscriptionCategories="true"
                         SubscriptionCategories="topics"
                         PrivacyPolicyText="@PrivacyText"
                         OnSubscribe="SaveAsync" />

@code {
    private const string PrivacyText = "I agree to the <a href=\"/privacy\">privacy policy</a>";

    private readonly SubscriptionCategory[] topics =
    [
        new() { Id = "articles", Name = "New articles", IsRequired = true },
        new() { Id = "releases", Name = "Release notes", Description = "About one email a month", IsDefault = true }
    ];

    private Task SaveAsync(SubscriptionModel subscription)
    {
        // subscription.Name, subscription.Email and subscription.SubscriptionCategories (the chosen ids)
        return Task.CompletedTask;
    }
}
```

## Parameters

<!-- osirion:parameters OsirionSubscriptionCard -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Id` | `string` | `"subscription-card"` | The form name and the prefix of the card's element ids; give each card on a page its own value. Required. |
| `Title` | `string` | `"Stay Updated"` | The heading (`h3`). Rendered as HTML, not encoded, so never pass visitor input. |
| `Description` | `string` | `"Get the latest updates and insights delivered to your inbox."` | The text under the title. Rendered as HTML, not encoded, so never pass visitor input. |
| `ShowNameField` | `bool` | `false` | Whether an optional name field (up to 100 characters) is shown. |
| `NamePlaceholder` | `string` | `"Your name (optional)"` | The name field's placeholder. |
| `EmailPlaceholder` | `string` | `"Enter your email address"` | The email field's placeholder. |
| `SubscribeButtonText` | `string` | `"Subscribe"` | The button's text. |
| `SubscribingText` | `string` | `"Subscribing..."` | Not rendered: the card has no busy state. |
| `ShowRequiredIndicators` | `bool` | `true` | Whether an asterisk follows the labels of required fields. |
| `PrivacyPolicyText` | `string?` | `"I agree to the <a class="osirion-subscribtion-link" href="/privacy-policy" target="_blank">privacy policy</a>"` | The label of the consent checkbox, by default a link to `/privacy-policy`. Rendered as HTML, not encoded. The checkbox is left out when the value is null or empty, and the card then submits without it: `SubscriptionModel.AgreeToPrivacyPolicy` is set to true so the model validates, which means it records no consent and must not be stored as consent. |
| `ShowSubscriptionCategories` | `bool` | `false` | Whether topics to choose from are shown. |
| `SubscriptionCategories` | `IReadOnlyList<SubscriptionCategory>?` | `null` | The topics. With `ShowSubscriptionCategories` on and no list, three sample topics are shown. |
| `CategoriesText` | `string` | `"What would you like to receive?"` | The label above the topics. |
| `ShowCategoriesAsDropdown` | `bool` | `false` | Whether the topics are shown in a `details` drop-down instead of a list of checkboxes. |
| `CategoriesDropdownText` | `string` | `"Select Categories"` | The drop-down button's text. |
| `OnSubscribe` | `EventCallback<SubscriptionModel>` | - | Receives the validated subscription; store the address here. The card shows `SuccessMessage` when it returns and `ErrorMessage` when it throws. Without a handler the card reports an error and logs a warning, because nothing would store the address. |
| `OnValidationFailed` | `EventCallback<EditContext>` | - | Called with the form's edit context when validation fails. |
| `SuccessMessage` | `string` | `"Thank you for subscribing! Please check your email to confirm your subscription."` | Shown after `OnSubscribe` returned. |
| `ErrorMessage` | `string` | `"Sorry, there was an error processing your subscription. Please try again later."` | Shown when `OnSubscribe` throws, or when nothing handles the address. |
| `SpamMessage` | `string` | `"Your submission appears to be spam and has been blocked."` | Shown when the hidden honeypot field was filled in. |
| `AlreadySubscribedMessage` | `string` | `"This email address is already subscribed to our newsletter."` | Shown for the `AlreadySubscribed` result, which the card does not produce itself. |
| `UseBuiltInEmailService` | `bool` | `false` | Not supported for subscriptions: the email service has nothing to send or store for a sign-up, so with this on the card reports an error and logs a warning. Handle `OnSubscribe` instead. |
| `CompactLayout` | `bool` | `false` | Less padding and a smaller title and description. |
| `UseDarkTheme` | `bool` | `false` | Adds `osirion-subscription-card-dark`, which takes input and option colors from the theme tokens. |
| `ShowBorder` | `bool` | `true` | Whether the card has a border. |
| `ShowShadow` | `bool` | `true` | Whether the card has a shadow. |
| `ChildContent` | `RenderFragment?` | `null` | Footer content under the form. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

`SubscriptionCategory` (namespace `Osirion.Blazor.Core.Models`) has `Id`, `Name`, `Description`, `IsDefault` (checked at
first) and `IsRequired` (checked, disabled and always included). `SubscriptionModel` carries `Name`, `Email`,
`AgreeToPrivacyPolicy` and `SubscriptionCategories`, the list of chosen ids.

## Behavior

- The form is an enhanced `EditForm` with `FormName` set to `Id`. Under static SSR it posts to the same page; in an
  interactive page it submits through the circuit. In both cases the card then navigates to the current URL with a
  `SubmissionResultValue` query value and shows the matching message above the form.
- Topic choices are read from the posted form, so they arrive only under static SSR. In an interactive page the topic
  checkboxes are not bound, and `OnSubscribe` receives only the required topics.
- Validation uses data annotations: the email is required, must be a valid address and at most 255 characters.
- The model requires the privacy agreement, so the card shows the checkbox. If `PrivacyPolicyText` is null or empty
  the checkbox is left out and the card submits without it: `AgreeToPrivacyPolicy` is set to true so the model
  validates, which means it records no consent and `OnSubscribe` must not store it as consent.
- The card thanks the visitor only after `OnSubscribe` returns. Without a handler, or with `UseBuiltInEmailService`
  on, nothing stores the address, so the card shows `ErrorMessage` and logs a warning instead. Available from the
  release after 4.1.0; before it, both cases showed `SuccessMessage`.
- A hidden honeypot field catches simple bots. When it is filled in, `OnSubscribe` is not called and `SpamMessage` is
  shown.
- `Title`, `Description` and `PrivacyPolicyText` are written as raw HTML without sanitizing. Use text you wrote; never
  pass text from visitors or unreviewed content.
- The drop-down uses the native `details` element, so it opens without script.
- The labels "Name" and "Email", "Select your preferences:" and "Required" are fixed English text.
- With the `EnhancedNavigation` component on the page, the redirect after posting can scroll to the top; set its
  `ResetScrollOnNavigation` to `false` if that matters for a card low on the page.

## Styling

The root element has `osirion-subscription-card` and, by setting, `osirion-subscription-card-compact`,
`osirion-subscription-card-dark`, `osirion-subscription-card-bordered` and `osirion-subscription-card-shadow`. Inputs,
labels, checkboxes, the button and the result message take the class names of the CSS framework set in the theming
options (for example `form-control`, `btn btn-primary` and `alert alert-success` with Bootstrap). These custom
properties are defined on the card:

| Custom property | Default |
| --- | --- |
| `--osirion-subscription-card-padding` | `2rem` (`1.5rem` compact) |
| `--osirion-subscription-card-border-radius` | `0.75rem` |
| `--osirion-subscription-card-background` | `--osirion-background-secondary` |
| `--osirion-subscription-card-border-color` | `--osirion-card-border-color` |
| `--osirion-subscription-card-shadow` | a small shadow |
| `--osirion-subscription-card-shadow-hover` | a larger shadow on hover |

## Related

- [OsirionContactForm](/components/contact-form)
- [Cards overview](/components/cards)
