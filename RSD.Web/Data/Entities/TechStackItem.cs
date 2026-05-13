namespace RSD.Web.Data.Entities;

public sealed record class TechStackItem : ContentEntity
{
    public required string Label { get; set; }
    public string LogoPath { get; set; } = "";
    public int DisplayOrder { get; set; }
}
