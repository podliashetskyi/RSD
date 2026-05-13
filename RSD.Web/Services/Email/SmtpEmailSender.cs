using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace RSD.Web.Services.Email;

public sealed class SmtpEmailSender(IOptions<EmailOptions> Options) : IEmailSender
{
    public async Task SendAsync(EmailMessage message, CancellationToken ct)
    {
        var mime = BuildMimeMessage(message, Options.Value.From);
        using var client = new SmtpClient();
        await ConnectAsync(client, Options.Value.Smtp, ct);
        await AuthenticateIfNeededAsync(client, Options.Value.Smtp, ct);
        await client.SendAsync(mime, ct);
        await client.DisconnectAsync(true, ct);
    }

    private static MimeMessage BuildMimeMessage(EmailMessage message, string from)
    {
        var mime = new MimeMessage
        {
            Subject = message.Subject,
            Body = new BodyBuilder { HtmlBody = message.HtmlBody, TextBody = message.TextBody }.ToMessageBody(),
        };
        mime.From.Add(MailboxAddress.Parse(from));
        mime.To.Add(MailboxAddress.Parse(message.To));
        return mime;
    }

    private static Task ConnectAsync(SmtpClient client, SmtpOptions smtp, CancellationToken ct) =>
        client.ConnectAsync(smtp.Host, smtp.Port, ResolveSecureSocketOption(smtp.EnableSsl), ct);

    private static Task AuthenticateIfNeededAsync(SmtpClient client, SmtpOptions smtp, CancellationToken ct) =>
        string.IsNullOrEmpty(smtp.User) ? Task.CompletedTask : client.AuthenticateAsync(smtp.User, smtp.Password, ct);

    private static SecureSocketOptions ResolveSecureSocketOption(bool enableSsl) =>
        enableSsl ? SecureSocketOptions.StartTlsWhenAvailable : SecureSocketOptions.None;
}
