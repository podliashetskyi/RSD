namespace RSD.Web.Data.Entities.ArticleBlocks;

public sealed record class QuoteBlock : ArticleBlock
{
    public string Quote { get; init; } = "";
    public string Attribution { get; init; } = "";
}
