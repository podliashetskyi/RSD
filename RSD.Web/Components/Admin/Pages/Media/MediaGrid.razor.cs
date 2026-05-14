#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using RSD.Web.Components.Admin.Shared;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Storage;

namespace RSD.Web.Components.Admin.Pages.Media;

public partial class MediaGrid(IMediaService Media, RefCountAuditor Auditor, IToastService Toasts) : ComponentBase
{
    private const int PageSize = 24;

    private List<MediaListItem> Items { get; set; } = [];
    private UploadedFile? Selected { get; set; }
    private List<MediaReference> UsedBy { get; set; } = [];
    private string Search { get; set; } = "";
    private int Page { get; set; } = 1;
    private bool HasNextPage { get; set; }
    private bool DialogOpen { get; set; }

    private string DialogBody => Selected is null
        ? ""
        : $"This will permanently remove \"{Selected.OriginalName}\" and all its variants from disk and the database. This action cannot be undone.";

    protected override Task OnInitializedAsync() => ReloadAsync();

    private async Task ReloadAsync()
    {
        var rows = await Media.ListAsync(new MediaQuery(Search, Page, PageSize + 1), CancellationToken.None);
        HasNextPage = rows.Count > PageSize;
        Items = [.. rows.Take(PageSize)];
        if (Selected is not null && !Items.Any(i => i.Id == Selected.Id))
        {
            Selected = null;
            UsedBy = [];
        }
    }

    private async Task SelectAsync(Guid id)
    {
        Selected = await Media.GetByIdAsync(id, CancellationToken.None);
        UsedBy = Selected is null
            ? []
            : [.. await Media.UsedByAsync(Selected.Path, CancellationToken.None)];
    }

    private async Task PrevAsync() { if (Page > 1) { Page--; await ReloadAsync(); } }
    private async Task NextAsync() { if (HasNextPage) { Page++; await ReloadAsync(); } }

    private void RequestHardDelete() { if (Selected is not null) DialogOpen = true; }

    private async Task ConfirmHardDeleteAsync()
    {
        if (Selected is null) return;
        var target = Selected;
        DialogOpen = false;
        var result = await Media.HardDeleteAsync(target.Id, CancellationToken.None);
        Toasts.Show(result.Ok ? "File deleted." : result.Error, result.Ok ? ToastKind.Success : ToastKind.Error);
        if (result.Ok)
        {
            Selected = null;
            UsedBy = [];
            await ReloadAsync();
        }
    }

    private void CancelHardDelete() => DialogOpen = false;
    private void OnDialogOpenChanged(bool open) => DialogOpen = open;

    private async Task RecomputeAsync()
    {
        var scanned = await Auditor.RecomputeAllAsync(CancellationToken.None);
        Toasts.Show($"Recounted {scanned} file{(scanned == 1 ? "" : "s")}.", ToastKind.Success);
        await ReloadAsync();
        if (Selected is { } s)
        {
            await SelectAsync(s.Id);
        }
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:0.#} KB";
        return $"{bytes / (1024.0 * 1024.0):0.##} MB";
    }
}
