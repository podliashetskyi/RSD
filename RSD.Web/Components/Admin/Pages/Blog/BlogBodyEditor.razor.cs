#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using RSD.Web.Components.Admin.Shared.Blocks;
using RSD.Web.Components.Admin.Shared.BlockRows;

namespace RSD.Web.Components.Admin.Pages.Blog;

public partial class BlogBodyEditor : ComponentBase
{
    [Parameter, EditorRequired] public ArticleBodyForm Value { get; set; } = new();

    private void OnIntroChanged(string html) => Value.Intro = html;
    private void OnBlocksChanged(List<IBlockRow> blocks) => Value.Blocks = blocks;
}
