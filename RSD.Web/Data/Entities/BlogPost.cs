namespace RSD.Web.Data.Entities;

public record class BlogPost : ContentEntity
{
    public required string Title { get; set; }
    public string Description { get; set; } = "";
    public string Category { get; set; } = "";
    public Guid? AuthorId { get; set; }
    public string CoverImagePath { get; set; } = "";
    public int ReadTimeMinutes { get; set; }
    public List<string> Tags { get; set; } = [];
    public string Intro { get; set; } = "";
    public ArticleBody BodyBlocks { get; set; } = new();
}
