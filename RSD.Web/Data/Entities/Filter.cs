namespace RSD.Web.Data.Entities;

public sealed record class Filter : ContentEntity, IHasDisplayOrder
{
    public required FilterType Type { get; set; }
    public required string Label { get; set; }
    public int DisplayOrder { get; set; }
}
