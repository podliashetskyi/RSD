namespace RSD.Web.Components.Admin.Shared;

public enum ToastKind { Info, Success, Warning, Error }

public record ToastModel(Guid Id, string Message, ToastKind Kind, DateTimeOffset CreatedAt);
