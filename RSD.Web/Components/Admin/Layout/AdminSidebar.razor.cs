#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Admin.Layout;

public partial class AdminSidebar : ComponentBase
{
    private IEnumerable<IGrouping<string, AdminNavItem>> Groups => AdminNav.Items.GroupBy(i => i.Group);
}
