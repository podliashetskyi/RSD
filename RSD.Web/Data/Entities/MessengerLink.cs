namespace RSD.Web.Data.Entities;

public sealed record class MessengerLink : ContentEntity
{
    public required string Label { get; set; }
    public string LargeIconPath { get; set; } = "";
    public string SmallIconPath { get; set; } = "";
    public string BgColor { get; set; } = "";
    public string Href { get; set; } = "";
    public int DisplayOrder { get; set; }
}
