using System.Web;
using RSD.Web.Data.Entities;

namespace RSD.Web.Services.Email.EmailTemplates;

public static class ContactSubmissionTemplate
{
    public static EmailMessage Render(string to, ContactSubmission submission, string adminInboxUrl)
    {
        var name = HttpUtility.HtmlEncode(submission.Name);
        var email = HttpUtility.HtmlEncode(submission.Email);
        var subject = HttpUtility.HtmlEncode(submission.Subject);
        var message = HttpUtility.HtmlEncode(submission.Message);
        var url = HttpUtility.HtmlEncode(adminInboxUrl);

        var html = $"""
            <p>A new contact submission was received on the RSD website.</p>
            <p><strong>From:</strong> {name} &lt;{email}&gt;<br/>
            <strong>Subject:</strong> {subject}<br/>
            <strong>Submitted:</strong> {submission.SubmittedAt:u}</p>
            <hr/>
            <p>{message}</p>
            <hr/>
            <p><a href="{url}">Open admin inbox</a></p>
            """;
        var text = $"""
            New contact submission on RSD website.

            From: {submission.Name} <{submission.Email}>
            Subject: {submission.Subject}
            Submitted: {submission.SubmittedAt:u}

            {submission.Message}

            Inbox: {adminInboxUrl}
            """;
        return new EmailMessage(to, $"[RSD] New contact: {submission.Subject}", html, text);
    }
}
