namespace RSD.Web.Data.Entities.ArticleBlocks;

public sealed record class SubsectionBlock : ArticleBlock
{
    public string Heading { get; init; } = "";
    public string Subheading { get; init; } = "";
    public string Body { get; init; } = "";
    public List<SubsectionItem> Items { get; init; } = [];
}
