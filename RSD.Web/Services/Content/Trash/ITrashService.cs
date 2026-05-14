using RSD.Web.Services.Common;

namespace RSD.Web.Services.Content.Trash;

public interface ITrashService
{
    Task<IReadOnlyList<TrashItem>> ListAsync(CancellationToken ct);
    Task<Result<Unit>> RestoreAsync(string entityKey, Guid id, CancellationToken ct);
    Task<Result<Unit>> HardDeleteAsync(string entityKey, Guid id, CancellationToken ct);
}
