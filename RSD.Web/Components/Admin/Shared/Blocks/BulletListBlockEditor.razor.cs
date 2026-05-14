#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using RSD.Web.Components.Admin.Shared.BlockRows;

namespace RSD.Web.Components.Admin.Shared.Blocks;

public partial class BulletListBlockEditor : ComponentBase
{
    [Parameter, EditorRequired] public BulletListRow Row { get; set; } = new();

    private void OnItemsChanged(List<string> items) => Row.Items = items;
}
