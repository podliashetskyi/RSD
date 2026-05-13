namespace RSD.Web.Components.Admin.Shared;

public sealed class ToastService : IToastService
{
    private readonly List<ToastModel> Items = [];
    private readonly Lock SyncRoot = new();

    public IReadOnlyList<ToastModel> Current
    {
        get { lock (SyncRoot) return [.. Items]; }
    }

    public event Action? Changed;

    public void Show(string message, ToastKind kind = ToastKind.Info)
    {
        var toast = new ToastModel(Guid.NewGuid(), message, kind, DateTimeOffset.UtcNow);
        lock (SyncRoot) Items.Add(toast);
        Changed?.Invoke();
    }

    public void Dismiss(Guid id)
    {
        lock (SyncRoot) Items.RemoveAll(t => t.Id == id);
        Changed?.Invoke();
    }
}
