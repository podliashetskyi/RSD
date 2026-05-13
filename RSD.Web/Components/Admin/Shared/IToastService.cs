namespace RSD.Web.Components.Admin.Shared;

public interface IToastService
{
    IReadOnlyList<ToastModel> Current { get; }
    event Action? Changed;
    void Show(string message, ToastKind kind = ToastKind.Info);
    void Dismiss(Guid id);
}
