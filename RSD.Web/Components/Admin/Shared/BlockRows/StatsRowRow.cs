using RSD.Web.Data.Entities;
using RSD.Web.Data.Entities.ArticleBlocks;

namespace RSD.Web.Components.Admin.Shared.BlockRows;

public sealed record class StatsRowRow : IBlockRow
{
    public string Id { get; set; } = "";
    public string Heading { get; set; } = "";
    public List<StatRowItemRow> Items { get; set; } = [];

    public BlockKind Kind => BlockKind.StatsRow;
    public string TypeLabel => "Stats row";
    public string Preview => BlockPreview.Trim($"{Items.Count} stat{(Items.Count == 1 ? "" : "s")}{(string.IsNullOrEmpty(Heading) ? "" : $" — {Heading}")}");

    public static StatsRowRow From(StatsRowBlock b) => new()
    {
        Id = b.Id,
        Heading = b.Heading,
        Items = [.. b.Items.Select(StatRowItemRow.From)],
    };

    public ArticleBlock ToEntity() => new StatsRowBlock
    {
        Id = Id,
        Heading = Heading,
        Items = [.. Items.Select(r => r.ToEntity())],
    };
}

public sealed record class StatRowItemRow
{
    public string Number { get; set; } = "";
    public string Label { get; set; } = "";

    public static StatRowItemRow From(StatRowItem i) => new() { Number = i.Number, Label = i.Label };
    public StatRowItem ToEntity() => new(Number, Label);
}
