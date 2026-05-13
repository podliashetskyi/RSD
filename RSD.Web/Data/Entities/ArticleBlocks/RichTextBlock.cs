namespace RSD.Web.Data.Entities.ArticleBlocks;

public sealed record class RichTextBlock : ArticleBlock
{
    public string Html { get; init; } = "";
}
