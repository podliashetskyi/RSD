using RSD.Web.Data.Entities;
using RSD.Web.Services.Common;

namespace RSD.Web.Services.Storage;

public sealed record MediaListItem(
    Guid Id,
    string Path,
    string OriginalName,
    string ContentType,
    long Bytes,
    DateTime UploadedAt,
    int RefCount);

public sealed record MediaReference(string EntityKey, string EntityLabel, Guid Id, string Title, string Slug);

public sealed record MediaQuery(string Search = "", int Page = 1, int PageSize = 24);

public interface IMediaService
{
    Task<IReadOnlyList<MediaListItem>> ListAsync(MediaQuery query, CancellationToken ct);
    Task<UploadedFile?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<MediaReference>> UsedByAsync(string path, CancellationToken ct);
    Task<Result<Unit>> HardDeleteAsync(Guid id, CancellationToken ct);
}
