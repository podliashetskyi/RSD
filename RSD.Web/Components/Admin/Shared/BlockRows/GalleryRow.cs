using RSD.Web.Data.Entities;
using RSD.Web.Data.Entities.ArticleBlocks;

namespace RSD.Web.Components.Admin.Shared.BlockRows;

public sealed record class GalleryRow : IBlockRow
{
    public string Id { get; set; } = "";
    public string Heading { get; set; } = "";
    public string Description { get; set; } = "";
    public List<GalleryImageRow> Images { get; set; } = [];
    public List<string> Tags { get; set; } = [];

    public BlockKind Kind => BlockKind.Gallery;
    public string TypeLabel => "Gallery";
    public string Preview => BlockPreview.Trim($"{Images.Count} image{(Images.Count == 1 ? "" : "s")}{(string.IsNullOrEmpty(Heading) ? "" : $" — {Heading}")}");

    public static GalleryRow From(GalleryBlock b) => new()
    {
        Id = b.Id,
        Heading = b.Heading,
        Description = b.Description,
        Images = [.. b.Images.Select(GalleryImageRow.From)],
        Tags = [.. b.Tags],
    };

    public ArticleBlock ToEntity() => new GalleryBlock
    {
        Id = Id,
        Heading = Heading,
        Description = Description,
        Images = [.. Images.Select(r => r.ToEntity())],
        Tags = [.. Tags],
    };
}

public sealed record class GalleryImageRow
{
    public string Src { get; set; } = "";
    public string Alt { get; set; } = "";

    public static GalleryImageRow From(GalleryImage g) => new() { Src = g.Src, Alt = g.Alt };
    public GalleryImage ToEntity() => new(Src, Alt);
}
