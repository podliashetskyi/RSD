namespace RSD.Web.Services.Email;

public record EmailMessage(string To, string Subject, string HtmlBody, string TextBody = "");
