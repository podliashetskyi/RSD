#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using RSD.Web.Components.Admin.Shared;
using RSD.Web.Data.Entities;

namespace RSD.Web.Components.Admin.Pages._Dev;

public partial class Playground(IToastService Toasts) : ComponentBase
{
    private bool ConfirmOpen { get; set; }
    private bool DestructiveOpen { get; set; }
    private string LastAction { get; set; } = "(none yet)";
    private UploadedFile? UploadedSample { get; set; }

    private static readonly IReadOnlyList<TableRow> DemoRows =
    [
        new("Hello world", ContentStatus.Published, DateTime.UtcNow.AddDays(-1)),
        new("Untitled draft", ContentStatus.Draft, DateTime.UtcNow.AddHours(-2)),
        new("Old marketing post", ContentStatus.Archived, DateTime.UtcNow.AddDays(-30)),
    ];

    private void HandleUploaded(UploadedFile? file) => UploadedSample = file;

    private void ShowInfo() => Toasts.Show("Info toast", ToastKind.Info);
    private void ShowSuccess() => Toasts.Show("Saved successfully", ToastKind.Success);
    private void ShowWarning() => Toasts.Show("Heads up", ToastKind.Warning);
    private void ShowError() => Toasts.Show("Something went wrong", ToastKind.Error);

    private void OpenConfirm() => ConfirmOpen = true;
    private void OpenDestructive() => DestructiveOpen = true;
    private void SetConfirmOpen(bool value) => ConfirmOpen = value;
    private void SetDestructiveOpen(bool value) => DestructiveOpen = value;

    private void MarkConfirmed() => LastAction = "confirmed";
    private void MarkCancelled() => LastAction = "cancelled";
    private void MarkDestructiveConfirmed() => LastAction = "destructive-confirmed";
    private void MarkDestructiveCancelled() => LastAction = "destructive-cancelled";

    public sealed record TableRow(string Title, ContentStatus Status, DateTime Updated);
}
