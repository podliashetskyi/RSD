namespace RSD.Web.Data.Entities;

public sealed record class Value : ContentEntity, IHasDisplayOrder
{
    public required string Title { get; set; }
    public string Description { get; set; } = "";
    public string IconPath { get; set; } = "";
    public int DisplayOrder { get; set; }
}
