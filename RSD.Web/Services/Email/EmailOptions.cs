namespace RSD.Web.Services.Email;

public sealed record class EmailOptions
{
    public const string SectionName = "Email";

    public string From { get; set; } = "";
    public string ContactTo { get; set; } = "";
    public SmtpOptions Smtp { get; set; } = new();
}

public sealed record class SmtpOptions
{
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public string User { get; set; } = "";
    public string Password { get; set; } = "";
    public bool EnableSsl { get; set; } = true;
}
