#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Pages;

public partial class Cases
{
    [SupplyParameterFromQuery(Name = "tech")] public string? Tech { get; set; }
    [SupplyParameterFromQuery(Name = "industry")] public string? Industry { get; set; }
}
