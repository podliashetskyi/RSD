namespace RSD.Web.Services.Preview;

public sealed record class PreviewOptions
{
    public const string SectionName = "Preview";

    public string SigningKey { get; set; } = "";
    public int TtlMinutes { get; set; } = 60;
}
