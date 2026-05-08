#pragma warning disable S1144, S4487, S2933
using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Shared;

public partial class IconChip
{
    [Parameter] public RenderFragment? ChildContent { get; set; }
}
