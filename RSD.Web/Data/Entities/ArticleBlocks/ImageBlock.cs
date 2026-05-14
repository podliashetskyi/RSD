namespace RSD.Web.Data.Entities.ArticleBlocks;

public sealed record class ImageBlock : ArticleBlock
{
    public string ImagePath { get; init; } = "";
    public string Caption { get; init; } = "";
    public string Alt { get; init; } = "";
}
