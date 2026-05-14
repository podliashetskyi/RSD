using RSD.Web.Data.Entities;
using RSD.Web.Data.Entities.ArticleBlocks;

namespace RSD.Web.Components.Admin.Shared.BlockRows;

public sealed record class RichTextRow : IBlockRow
{
    public string Id { get; set; } = "";
    public string Html { get; set; } = "";

    public BlockKind Kind => BlockKind.RichText;
    public string TypeLabel => "Rich text";
    public string Preview => BlockPreview.Trim(BlockPreview.StripTags(Html));

    public static RichTextRow From(RichTextBlock b) => new() { Id = b.Id, Html = b.Html };
    public ArticleBlock ToEntity() => new RichTextBlock { Id = Id, Html = Html };
}
