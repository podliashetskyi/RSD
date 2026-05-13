using RSD.Web.Data.Entities;
using RSD.Web.Services.Common;

namespace RSD.Web.Services.Content;

public interface ISimpleContentService<TEntity> where TEntity : ContentEntity
{
    Task<IReadOnlyList<TEntity>> ListAsync(ContentQuery query, CancellationToken ct);
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Result<Guid>> CreateAsync(TEntity input, CancellationToken ct);
    Task<Result<Unit>> UpdateAsync(TEntity input, CancellationToken ct);
    Task<Result<Unit>> SetStatusAsync(Guid id, ContentStatus status, CancellationToken ct);
    Task<Result<Unit>> SoftDeleteAsync(Guid id, CancellationToken ct);
    Task<Result<Unit>> RestoreAsync(Guid id, CancellationToken ct);
}
