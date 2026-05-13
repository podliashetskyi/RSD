using System.Web;

namespace RSD.Web.Services.Email.EmailTemplates;

public static class UserInviteTemplate
{
    public static EmailMessage Render(string to, string invitedBy, string inviteUrl)
    {
        var safeInviter = HttpUtility.HtmlEncode(invitedBy);
        var safeUrl = HttpUtility.HtmlEncode(inviteUrl);
        var html = $"""
            <p>Hello,</p>
            <p>{safeInviter} has invited you to join the RSD Admin team.</p>
            <p><a href="{safeUrl}">Accept the invite and set your password</a></p>
            <p>The link is valid for 7 days. If you weren't expecting this invite, you can ignore this email.</p>
            """;
        var text = $"""
            Hello,

            {invitedBy} has invited you to join the RSD Admin team.

            Accept the invite and set your password: {inviteUrl}

            The link is valid for 7 days. If you weren't expecting this invite, ignore this email.
            """;
        return new EmailMessage(to, "You've been invited to RSD Admin", html, text);
    }
}
