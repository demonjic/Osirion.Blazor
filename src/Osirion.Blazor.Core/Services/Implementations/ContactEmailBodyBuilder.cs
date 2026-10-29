using System.Net;
using System.Text;
using Osirion.Blazor.Core.Models;

namespace Osirion.Blazor.Core.Services.Implementations;

/// <summary>
/// Builds the HTML body of the contact form notification mail shared by every email provider.
/// </summary>
internal static class ContactEmailBodyBuilder
{
    /// <summary>
    /// Creates the HTML email body from the contact form data. Every user supplied value is HTML encoded.
    /// </summary>
    /// <param name="contactForm">The contact form data.</param>
    /// <param name="submittedAtUtc">The submission time shown in the footer.</param>
    /// <returns>A formatted HTML string for the email body.</returns>
    internal static string Build(ContactFormModel contactForm, DateTime submittedAtUtc)
    {
        var body = new StringBuilder();
        body.AppendLine("<html><body>");
        body.AppendLine("<h2>New Contact Form Submission</h2>");
        body.AppendLine($"<p><strong>Name:</strong> {WebUtility.HtmlEncode(contactForm.Name)}</p>");
        body.AppendLine($"<p><strong>Email:</strong> {WebUtility.HtmlEncode(contactForm.Email)}</p>");
        body.AppendLine($"<p><strong>Subject:</strong> {WebUtility.HtmlEncode(contactForm.Subject)}</p>");
        body.AppendLine("<p><strong>Message:</strong></p>");
        body.AppendLine("<div style='border: 1px solid #ccc; padding: 10px; background-color: #f9f9f9;'>");
        body.AppendLine(WebUtility.HtmlEncode(contactForm.Message).Replace("\n", "<br>"));
        body.AppendLine("</div>");
        AppendAdditionalFields(body, contactForm.AdditionalFieldValues);

        if (contactForm.SubscribeToUpdates)
        {
            body.AppendLine("<p><em>The sender has requested to receive updates and newsletters.</em></p>");
        }

        body.AppendLine($"<p><small>Submitted on: {submittedAtUtc:yyyy-MM-dd HH:mm:ss} UTC</small></p>");
        body.AppendLine("</body></html>");

        return body.ToString();
    }

    // The answers to the form's additional fields, in definition order, each labelled; unanswered fields are left out,
    // and a form without additional fields adds nothing. Labels and answers are encoded like every other value.
    private static void AppendAdditionalFields(StringBuilder body, IReadOnlyList<ContactFormFieldValue> answers)
    {
        var answered = answers.Where(answer => answer.Text.Length > 0).ToList();
        if (answered.Count == 0)
        {
            return;
        }

        body.AppendLine("<h3>Additional details</h3>");
        foreach (var answer in answered)
        {
            var label = WebUtility.HtmlEncode(answer.Label);
            var text = WebUtility.HtmlEncode(answer.Text).Replace("\n", "<br>", StringComparison.Ordinal);
            body.AppendLine($"<p><strong>{label}:</strong> {text}</p>");
        }
    }
}
