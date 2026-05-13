#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Admin.Shared;

public partial class FormField : ComponentBase
{
    [Parameter, EditorRequired] public string Label { get; set; } = "";
    [Parameter] public string Hint { get; set; } = "";
    [Parameter] public RenderFragment? ChildContent { get; set; }
}
