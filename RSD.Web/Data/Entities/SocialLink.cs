namespace RSD.Web.Data.Entities;

public sealed record class SocialLink : ContentEntity, IHasDisplayOrder
{
    public required string Label { get; set; }
    public string IconPath { get; set; } = "";
    public string Href { get; set; } = "";
    public SocialLinkScope Scope { get; set; }
    public int DisplayOrder { get; set; }
}
