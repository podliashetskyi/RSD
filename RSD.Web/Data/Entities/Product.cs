namespace RSD.Web.Data.Entities;

public record class Product : ContentEntity
{
    public required string Name { get; set; }
    public string Summary { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public string Price { get; set; } = "";
    public string Description { get; set; } = "";
    public List<string> BulletPoints { get; set; } = [];
    public string CoverImagePath { get; set; } = "";
    public string CoverImageAlt { get; set; } = "";
    public string TryForFreeHref { get; set; } = "";
    public string LearnMoreHref { get; set; } = "";
    public ProductDetailFields DetailFields { get; set; } = new();
}
