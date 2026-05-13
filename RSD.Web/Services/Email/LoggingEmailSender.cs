namespace RSD.Web.Services.Email;

public sealed class LoggingEmailSender(ILogger<LoggingEmailSender> Log) : IEmailSender
{
    public Task SendAsync(EmailMessage message, CancellationToken ct)
    {
        Log.LogInformation(
            "Outbound email (logged, not sent): To={To} Subject={Subject}\n--- HTML ---\n{Html}\n--- TEXT ---\n{Text}",
            message.To, message.Subject, message.HtmlBody, message.TextBody);
        return Task.CompletedTask;
    }
}
