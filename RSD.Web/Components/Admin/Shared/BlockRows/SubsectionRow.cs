using RSD.Web.Data.Entities;
using RSD.Web.Data.Entities.ArticleBlocks;

namespace RSD.Web.Components.Admin.Shared.BlockRows;

public sealed record class SubsectionRow : IBlockRow
{
    public string Id { get; set; } = "";
    public string Heading { get; set; } = "";
    public string Subheading { get; set; } = "";
    public string Body { get; set; } = "";
    public List<SubsectionItemRow> Items { get; set; } = [];

    public BlockKind Kind => BlockKind.Subsection;
    public string TypeLabel => "Subsection";
    public string Preview => BlockPreview.Trim(string.IsNullOrEmpty(Heading) ? Subheading : Heading);

    public static SubsectionRow From(SubsectionBlock b) => new()
    {
        Id = b.Id,
        Heading = b.Heading,
        Subheading = b.Subheading,
        Body = b.Body,
        Items = [.. b.Items.Select(SubsectionItemRow.From)],
    };

    public ArticleBlock ToEntity() => new SubsectionBlock
    {
        Id = Id,
        Heading = Heading,
        Subheading = Subheading,
        Body = Body,
        Items = [.. Items.Select(r => r.ToEntity())],
    };
}

public sealed record class SubsectionItemRow
{
    public string Label { get; set; } = "";
    public string Body { get; set; } = "";

    public static SubsectionItemRow From(SubsectionItem i) => new() { Label = i.Label, Body = i.Body };
    public SubsectionItem ToEntity() => new(Label, Body);
}
