#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Admin.Shared;

public partial class ComingSoonPanel : ComponentBase
{
    [Parameter, EditorRequired] public string Title { get; set; } = "";
    [Parameter] public string Subtitle { get; set; } = "";
    [Parameter] public string Phase { get; set; } = "a later phase";
}
