using RSD.Web.Data.Entities;

namespace RSD.Web.Components.Admin.Shared.BlockRows;

public interface IBlockRow
{
    string Id { get; }
    BlockKind Kind { get; }
    string TypeLabel { get; }
    string Preview { get; }
    ArticleBlock ToEntity();
}
