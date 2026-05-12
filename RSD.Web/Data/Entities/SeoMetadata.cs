namespace RSD.Web.Data.Entities;

public record class SeoMetadata
{
    public string MetaTitle { get; set; } = "";
    public string MetaDescription { get; set; } = "";
    public string OgImagePath { get; set; } = "";
}
