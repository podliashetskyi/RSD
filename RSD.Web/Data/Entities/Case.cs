namespace RSD.Web.Data.Entities;

public record class Case : ContentEntity
{
    public required string Name { get; set; }
    public string Industry { get; set; } = "";
    public string Description { get; set; } = "";
    public string CoverImagePath { get; set; } = "";
    public List<string> TechTags { get; set; } = [];
    public CaseDetailFields DetailFields { get; set; } = new();
}
