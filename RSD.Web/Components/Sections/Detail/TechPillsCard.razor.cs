#pragma warning disable S1144, S4487, S2933
using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Sections.Detail;

public partial class TechPillsCard
{
    [Parameter] public string Heading { get; set; } = "Technologies Used";
    [Parameter, EditorRequired] public IReadOnlyList<string> Pills { get; set; } = [];
}
