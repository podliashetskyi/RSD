using RSD.Web.Services.Common;

namespace RSD.Web.Services.Content;

public interface IContentService<TListItem, TDetail, TUpsert>
{
    Task<IReadOnlyList<TListItem>> ListAsync(ContentQuery query, CancellationToken ct);
    Task<TDetail?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<TDetail?> GetBySlugAsync(string slug, bool includeDrafts, CancellationToken ct);
    Task<Result<Guid>> CreateAsync(TUpsert input, CancellationToken ct);
    Task<Result<Unit>> UpdateAsync(Guid id, TUpsert input, CancellationToken ct);
    Task<Result<Unit>> PublishAsync(Guid id, CancellationToken ct);
    Task<Result<Unit>> UnpublishAsync(Guid id, CancellationToken ct);
    Task<Result<Unit>> ArchiveAsync(Guid id, CancellationToken ct);
    Task<Result<Unit>> SoftDeleteAsync(Guid id, CancellationToken ct);
    Task<Result<Unit>> RestoreAsync(Guid id, CancellationToken ct);
}
