namespace RSD.Web.Data.Entities;

public sealed record class ContactPoint : ContentEntity, IHasDisplayOrder
{
    public required string Label { get; set; }
    public List<string> Lines { get; set; } = [];
    public bool IsLink { get; set; }
    public string Href { get; set; } = "";
    public string IconPath { get; set; } = "";
    public int DisplayOrder { get; set; }
}
