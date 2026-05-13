#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Admin.Layout;

public partial class AdminNavbar : ComponentBase
{
    [Parameter] public string DisplayName { get; set; } = "";
}
