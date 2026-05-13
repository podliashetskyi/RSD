using System.Web;

namespace RSD.Web.Services.Email.EmailTemplates;

public static class ForgotPasswordTemplate
{
    public static EmailMessage Render(string to, string displayName, string resetUrl)
    {
        var safeName = HttpUtility.HtmlEncode(displayName);
        var safeUrl = HttpUtility.HtmlEncode(resetUrl);
        var html = $"""
            <p>Hi {safeName},</p>
            <p>We received a request to reset the password for your RSD Admin account.</p>
            <p><a href="{safeUrl}">Reset your password</a></p>
            <p>This link is valid for one hour. If you did not request this, you can safely ignore this email.</p>
            """;
        var text = $"""
            Hi {displayName},

            We received a request to reset the password for your RSD Admin account.

            Reset your password: {resetUrl}

            This link is valid for one hour. If you did not request this, ignore this email.
            """;
        return new EmailMessage(to, "Reset your RSD Admin password", html, text);
    }
}
