#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using RSD.Web.Components.Admin.Shared.BlockRows;

namespace RSD.Web.Components.Admin.Shared.Blocks;

public partial class SubsectionBlockEditor : ComponentBase
{
    [Parameter, EditorRequired] public SubsectionRow Row { get; set; } = new();

    private void OnBodyChanged(string html) => Row.Body = html;
    private void OnItemsChanged(List<SubsectionItemRow> items) => Row.Items = items;
}
