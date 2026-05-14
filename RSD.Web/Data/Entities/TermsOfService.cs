namespace RSD.Web.Data.Entities;

public sealed record class TermsOfService : ContentEntity
{
    public required string Title { get; set; }
    public DateOnly LastUpdatedAt { get; set; }
    public string BodyHtml { get; set; } = "";
}
