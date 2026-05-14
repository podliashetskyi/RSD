namespace RSD.Web.Data.Entities.ArticleBlocks;

public sealed record class StatsRowBlock : ArticleBlock
{
    public string Heading { get; init; } = "";
    public List<StatRowItem> Items { get; init; } = [];
}
