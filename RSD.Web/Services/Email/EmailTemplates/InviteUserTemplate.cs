using System.Web;

namespace RSD.Web.Services.Email.EmailTemplates;

public static class InviteUserTemplate
{
    public static EmailMessage Render(string to, string displayName, string setupUrl)
    {
        var safeName = HttpUtility.HtmlEncode(string.IsNullOrWhiteSpace(displayName) ? to : displayName);
        var safeUrl = HttpUtility.HtmlEncode(setupUrl);
        var html = $"""
            <p>Hi {safeName},</p>
            <p>You've been invited to the RSD Admin panel. Click the link below to choose a password and sign in:</p>
            <p><a href="{safeUrl}">Set your password</a></p>
            <p>This link is valid for one hour. If you weren't expecting this, you can safely ignore the email.</p>
            """;
        var text = $"""
            Hi {(string.IsNullOrWhiteSpace(displayName) ? to : displayName)},

            You've been invited to the RSD Admin panel. Set your password here:

            {setupUrl}

            This link is valid for one hour. If you weren't expecting this, ignore the email.
            """;
        return new EmailMessage(to, "You've been invited to RSD Admin", html, text);
    }
}
