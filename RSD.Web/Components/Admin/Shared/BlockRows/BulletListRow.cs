using RSD.Web.Data.Entities;
using RSD.Web.Data.Entities.ArticleBlocks;

namespace RSD.Web.Components.Admin.Shared.BlockRows;

public sealed record class BulletListRow : IBlockRow
{
    public string Id { get; set; } = "";
    public string Heading { get; set; } = "";
    public List<string> Items { get; set; } = [];

    public BlockKind Kind => BlockKind.BulletList;
    public string TypeLabel => "Bullet list";
    public string Preview => BlockPreview.Trim($"{Items.Count} item{(Items.Count == 1 ? "" : "s")}{(string.IsNullOrEmpty(Heading) ? "" : $" — {Heading}")}");

    public static BulletListRow From(BulletListBlock b) => new()
    {
        Id = b.Id,
        Heading = b.Heading,
        Items = [.. b.Items],
    };

    public ArticleBlock ToEntity() => new BulletListBlock
    {
        Id = Id,
        Heading = Heading,
        Items = [.. Items],
    };
}
