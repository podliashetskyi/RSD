using RSD.Web.Data.Entities;
using RSD.Web.Data.Entities.ArticleBlocks;

namespace RSD.Web.Components.Admin.Shared.BlockRows;

public sealed record class QuoteRow : IBlockRow
{
    public string Id { get; set; } = "";
    public string Quote { get; set; } = "";
    public string Attribution { get; set; } = "";

    public BlockKind Kind => BlockKind.Quote;
    public string TypeLabel => "Quote";
    public string Preview => BlockPreview.Trim(Quote);

    public static QuoteRow From(QuoteBlock b) => new() { Id = b.Id, Quote = b.Quote, Attribution = b.Attribution };
    public ArticleBlock ToEntity() => new QuoteBlock { Id = Id, Quote = Quote, Attribution = Attribution };
}
