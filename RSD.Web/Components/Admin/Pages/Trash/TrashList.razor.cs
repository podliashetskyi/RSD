#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using RSD.Web.Components.Admin.Shared;
using RSD.Web.Services.Content.Trash;

namespace RSD.Web.Components.Admin.Pages.Trash;

public partial class TrashList(ITrashService Trash, IToastService Toasts) : ComponentBase
{
    private List<TrashItem> Items { get; set; } = [];
    private TrashItem? Pending { get; set; }
    private bool DialogOpen { get; set; }

    private string DialogBody => Pending is null
        ? ""
        : $"This will permanently remove the {Pending.EntityLabel.ToLowerInvariant()} \"{Pending.Title}\" from the database. This action cannot be undone.";

    protected override Task OnInitializedAsync() => ReloadAsync();

    private async Task ReloadAsync()
    {
        var rows = await Trash.ListAsync(CancellationToken.None);
        Items = [.. rows];
    }

    private async Task RestoreAsync(TrashItem item)
    {
        var result = await Trash.RestoreAsync(item.EntityKey, item.Id, CancellationToken.None);
        Toasts.Show(result.Ok ? $"Restored {item.EntityLabel}." : result.Error, result.Ok ? ToastKind.Success : ToastKind.Error);
        await ReloadAsync();
    }

    private void RequestHardDelete(TrashItem item)
    {
        Pending = item;
        DialogOpen = true;
    }

    private async Task ConfirmHardDeleteAsync()
    {
        if (Pending is null) return;
        var item = Pending;
        Pending = null;
        DialogOpen = false;
        var result = await Trash.HardDeleteAsync(item.EntityKey, item.Id, CancellationToken.None);
        Toasts.Show(result.Ok ? $"Permanently deleted {item.EntityLabel}." : result.Error, result.Ok ? ToastKind.Success : ToastKind.Error);
        await ReloadAsync();
    }

    private void CancelHardDelete()
    {
        Pending = null;
        DialogOpen = false;
    }

    private void OnDialogOpenChanged(bool open)
    {
        DialogOpen = open;
        if (!open) Pending = null;
    }
}
