using RSD.Web.Data.Entities;

namespace RSD.Web.Services.Slugs;

public interface ISlugger
{
    string Slugify(string source);
    Task<string> GenerateUniqueAsync<TEntity>(string source, Guid? currentId, CancellationToken ct) where TEntity : ContentEntity;
    Task<bool> IsAvailableAsync<TEntity>(string slug, Guid? currentId, CancellationToken ct) where TEntity : ContentEntity;
}
