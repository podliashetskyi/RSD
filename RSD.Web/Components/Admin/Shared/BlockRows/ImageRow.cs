using RSD.Web.Data.Entities;
using RSD.Web.Data.Entities.ArticleBlocks;

namespace RSD.Web.Components.Admin.Shared.BlockRows;

public sealed record class ImageRow : IBlockRow
{
    public string Id { get; set; } = "";
    public string ImagePath { get; set; } = "";
    public string Caption { get; set; } = "";
    public string Alt { get; set; } = "";

    public BlockKind Kind => BlockKind.Image;
    public string TypeLabel => "Image";
    public string Preview => BlockPreview.Trim(string.IsNullOrEmpty(Caption) ? ImagePath : Caption);

    public static ImageRow From(ImageBlock b) => new() { Id = b.Id, ImagePath = b.ImagePath, Caption = b.Caption, Alt = b.Alt };
    public ArticleBlock ToEntity() => new ImageBlock { Id = Id, ImagePath = ImagePath, Caption = Caption, Alt = Alt };
}
