---
id: 'privacy-policy'
order: 4
layout: page
title: 'Privacy policy'
permalink: /privacy-policy
description: 'What getosirion.com does with your data: the cookies it sets, the cookieless page timing Cloudflare measures, the three analytics services that load only after you consent, and how to withdraw consent.'
author: 'Dejan Demonjić'
date: '2026-10-17'
last_modified: 2026-10-18
featured_image: 'https://media.getosirion.com/og/privacy-policy.jpg'
published: true
slug: 'privacy-policy'
lang: 'en'
seo_properties:
  og_image_url: 'https://media.getosirion.com/og/privacy-policy.jpg'
  title: 'Privacy policy: cookies and analytics'
  description: 'The cookies getosirion.com sets, the analytics services that load only after you accept them, and how to withdraw your consent.'
  canonical: 'https://getosirion.com/privacy-policy'
  robots: 'index, follow'
---

Last updated: 18 October 2026.

This page describes what the website getosirion.com does, in plain language. It describes facts about the site; it does
not claim that the site meets any particular law or standard.

## Who runs this site

The site is run by Dejan Demonjić, who also makes and maintains the Osirion.Blazor library (see the
[about page](/about)). To ask a question about this policy, use the [contact page](/contact).

The site has no sign-in, no contact form and no newsletter, so it does not ask you for your name or email address.

## Cookies

The site sets five kinds of cookies. None of them carries your name or any other personal detail.

| Cookie | What it is for | When it is set | Lifetime and flags |
| --- | --- | --- | --- |
| `osirion_cookie_consent` | Remembers your answer to the cookie banner: the date, whether you accepted, declined or chose categories, and which categories. | When you answer the banner. | 365 days. `Path=/`, `HttpOnly`, `Secure`, `SameSite=Lax`. |
| `osirion-preferred-theme` | Remembers whether you chose the light or the dark theme, so the page is drawn in it from the first byte. | When you choose a theme. | One year. `Path=/`, `Secure`, `SameSite=Lax`. Not `HttpOnly`, because the page script reads it. |
| `osirion-preferred-palette` | Remembers the color palette you chose with the palette picker. | When you choose a palette. | One year. `Path=/`, `Secure`, `SameSite=Lax`. Not `HttpOnly`, because the page script reads it. |
| `osirion_announcement_<id>` | Remembers that you closed the announcement bar at the top of the page, so it stays closed. `<id>` names the announcement. | When you close the bar. | 30 days. `Path=/`, `HttpOnly`, `Secure`, `SameSite=Lax`. |
| `.AspNetCore.Antiforgery.<name>` | Protects the forms of the site, such as the cookie banner and the announcement bar, against requests forged by another site. It holds a random token, nothing about you. | On your first page view. | Until you close the browser. `Path=/`, `HttpOnly`, `SameSite=Strict`. |

The first one records a choice you made about cookies; the next three keep a display setting you chose; the last one is a security measure the site needs to accept a form, so it is set without asking. The
[security guide](/docs/security) documents the same cookies and their flags from
the library's side.

## Analytics

### Page timing by Cloudflare

Every page loads Cloudflare Web Analytics, which Cloudflare adds because the site is served through its network. It is
not covered by the cookie banner, because it stores nothing in your browser: no cookies and no local storage (checked on
2026-10-18). Its script, from static.cloudflareinsights.com, measures how fast the page loads and sends those timings
to getosirion.com/cdn-cgi/rum, where Cloudflare counts them. Cloudflare states that Web Analytics does not collect or
use visitors' personal data and does not track people across the sites it serves; its
[privacy policy](https://www.cloudflare.com/privacypolicy/) applies.

### Analytics after consent

The site can also load three analytics services. They are off until you accept analytics cookies in the cookie banner,
and none of them loads on your first page view, while you decline, or while you have not answered.

| Service | Run by | What it is used for |
| --- | --- | --- |
| Google Analytics 4 | Google. [Privacy policy](https://policies.google.com/privacy). | Counting page views and visits. It is also set to record clicks on links that leave this site. |
| Microsoft Clarity | Microsoft. [Privacy statement](https://privacy.microsoft.com/privacystatement). | Showing how visitors use the pages. Microsoft describes Clarity as a tool for heatmaps and session recordings. |
| Matomo | Self-hosted at analytics.tridesetri.com, on a server of Tridesetri, the company the maintainer co-founded. | Counting page views and visits, with the data kept on that server instead of with a third party. |

When you accept, your browser requests each service's script, and the service receives the request, so it sees your
IP address and browser details, and it may set its own cookies. Those cookies belong to the service; read its policy
for their names and lifetimes.

### Withdraw your consent

The banner is shown until you answer, and then it stays away. To change your answer:

1. Choose **Cookie settings** at the bottom of any page. The banner opens with your current choices; clear
   Analytics and save. You can also delete the `osirion_cookie_consent` cookie for getosirion.com in your browser's
   site data, and the banner appears again on the next page.
2. To remove what the analytics services already stored, delete the other cookies for getosirion.com in your
   browser's site data.

Deleting the cookie does not delete data an analytics service already holds. Ask the service about that.

## What other services see

- **Cloudflare.** The site is served through Cloudflare, so every request passes through its network, which sees your
  IP address and the request headers. Read [Cloudflare's privacy policy](https://www.cloudflare.com/privacypolicy/).
- **Images and video.** The link preview cards and screenshots come from media.getosirion.com, which is stored and
  served by Cloudflare. The site menu shows a picture from Unsplash, and the video embed page shows the thumbnail of a
  YouTube video. Your browser requests them from those services when it shows them, and they see that request.
- **GitHub.** The contact channels are on GitHub. What you write in an issue or pull request is public and handled by
  GitHub under [its privacy statement](https://docs.github.com/en/site-policy/privacy-policies/github-general-privacy-statement).
  Links to the maintainer's other sites ([demonjic.com](https://demonjic.com), [Tridesetri](https://tridesetri.com) and
  [Hexavera](https://hexavera.com)) leave this site, and each has its own policy.

## Changes

This page is a file in the [site's repository](https://github.com/demonjic/Osirion.Blazor), so every change to it is in
the Git history. The date at the top changes with it.
