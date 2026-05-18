#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Admin.Shared;

public partial class DeleteRowButton : ComponentBase
{
    [Parameter, EditorRequired] public string EntityLabel { get; set; } = "";
    [Parameter] public EventCallback OnConfirm { get; set; }

    private bool IsOpen { get; set; }

    private string DialogTitle => string.IsNullOrWhiteSpace(EntityLabel)
        ? "Move this item to Trash?"
        : $"Move “{EntityLabel}” to Trash?";

    private void Open() => IsOpen = true;

    private void OnOpenChanged(bool open) => IsOpen = open;

    private Task HandleConfirmAsync() => OnConfirm.InvokeAsync();
}
