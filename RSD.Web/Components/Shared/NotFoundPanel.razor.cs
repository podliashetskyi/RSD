#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Shared;

public partial class NotFoundPanel : ComponentBase
{
    [Parameter] public string Heading { get; set; } = "Page not found";
    [Parameter] public string Description { get; set; } = "The page you were looking for doesn't exist or has been moved.";
    [Parameter] public string BackHref { get; set; } = "/";
    [Parameter] public string BackLabel { get; set; } = "Back home";
}
