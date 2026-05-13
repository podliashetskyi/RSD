#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Admin.Shared;

public partial class ToastHost(IToastService Toasts) : ComponentBase, IDisposable
{
    private IReadOnlyList<ToastModel> Items { get; set; } = [];

    protected override void OnInitialized()
    {
        Items = Toasts.Current;
        Toasts.Changed += HandleChanged;
    }

    private void HandleChanged()
    {
        Items = Toasts.Current;
        InvokeAsync(StateHasChanged);
    }

    private static readonly Dictionary<ToastKind, string> KindStyles = new()
    {
        [ToastKind.Info] = "bg-white dark:bg-gray-950 border-gray-200 dark:border-gray-800 text-gray-900 dark:text-white",
        [ToastKind.Success] = "bg-green-50 dark:bg-green-950 border-green-200 dark:border-green-900 text-green-900 dark:text-green-200",
        [ToastKind.Warning] = "bg-yellow-50 dark:bg-yellow-950 border-yellow-200 dark:border-yellow-900 text-yellow-900 dark:text-yellow-200",
        [ToastKind.Error] = "bg-red-50 dark:bg-red-950 border-red-200 dark:border-red-900 text-red-900 dark:text-red-200",
    };

    private static string KindClasses(ToastKind kind) =>
        KindStyles.TryGetValue(kind, out var classes) ? classes : KindStyles[ToastKind.Info];

    public void Dispose() => Toasts.Changed -= HandleChanged;
}
