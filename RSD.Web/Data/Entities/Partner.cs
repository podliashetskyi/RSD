namespace RSD.Web.Data.Entities;

public sealed record class Partner : ContentEntity, IHasDisplayOrder
{
    public required string Name { get; set; }
    public string Role { get; set; } = "";
    public string PhotoPath { get; set; } = "";
    public string ContactHref { get; set; } = "";
    public int DisplayOrder { get; set; }
}
