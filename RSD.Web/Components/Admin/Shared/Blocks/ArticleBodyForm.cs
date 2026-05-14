using RSD.Web.Components.Admin.Shared.BlockRows;
using RSD.Web.Data.Entities;

namespace RSD.Web.Components.Admin.Shared.Blocks;

public sealed record class ArticleBodyForm
{
    public string Intro { get; set; } = "";
    public List<IBlockRow> Blocks { get; set; } = [];

    public static ArticleBodyForm From(ArticleBody body) => new()
    {
        Intro = body.Intro,
        Blocks = [.. body.Blocks.Select(BlockRowFactory.From)],
    };

    public ArticleBody ToEntity() => new()
    {
        Intro = Intro,
        Blocks = [.. Blocks.Select(r => r.ToEntity())],
    };
}
