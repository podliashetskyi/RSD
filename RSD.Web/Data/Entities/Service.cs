namespace RSD.Web.Data.Entities;

public record class Service : ContentEntity
{
    public required string Title { get; set; }
    public string Description { get; set; } = "";
    public List<string> BulletPoints { get; set; } = [];
    public string CoverImagePath { get; set; } = "";
    public string DetailsHref { get; set; } = "";
    public string Intro { get; set; } = "";
    public ArticleBody BodyBlocks { get; set; } = new();
}
