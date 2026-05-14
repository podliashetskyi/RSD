namespace RSD.Web.Data.Entities.ArticleBlocks;

public sealed record class GalleryBlock : ArticleBlock
{
    public string Heading { get; init; } = "";
    public string Description { get; init; } = "";
    public List<GalleryImage> Images { get; init; } = [];
    public List<string> Tags { get; init; } = [];
}
