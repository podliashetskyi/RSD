namespace RSD.Web.Data.Entities;

public record class ArticleBody
{
    public string Intro { get; set; } = "";
    public List<ArticleBlock> Blocks { get; set; } = [];
}
