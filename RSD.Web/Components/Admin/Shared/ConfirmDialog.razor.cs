#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Admin.Shared;

public partial class ConfirmDialog : ComponentBase
{
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public string Title { get; set; } = "Confirm";
    [Parameter] public string Body { get; set; } = "";
    [Parameter] public string ConfirmLabel { get; set; } = "Confirm";
    [Parameter] public string CancelLabel { get; set; } = "Cancel";
    [Parameter] public bool IsDestructive { get; set; }
    [Parameter] public EventCallback OnConfirm { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
    [Parameter] public EventCallback<bool> IsOpenChanged { get; set; }

    private string TitleId { get; } = $"dlg-{Guid.NewGuid():N}";

    private string ConfirmClasses => IsDestructive
        ? "bg-red-600 hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500"
        : "bg-gray-900 hover:bg-black dark:bg-white dark:text-gray-900 dark:hover:bg-gray-100 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500";

    private async Task ConfirmAsync()
    {
        await OnConfirm.InvokeAsync();
        await CloseAsync();
    }

    private async Task CancelAsync()
    {
        await OnCancel.InvokeAsync();
        await CloseAsync();
    }

    private async Task CloseAsync()
    {
        IsOpen = false;
        await IsOpenChanged.InvokeAsync(false);
    }
}
