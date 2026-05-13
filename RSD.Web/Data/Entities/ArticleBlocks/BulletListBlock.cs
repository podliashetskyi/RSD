namespace RSD.Web.Data.Entities.ArticleBlocks;

public sealed record class BulletListBlock : ArticleBlock
{
    public string Heading { get; init; } = "";
    public List<string> Items { get; init; } = [];
}
