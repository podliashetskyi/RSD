using RSD.Web.Data.Entities;

namespace RSD.Web.Services.Content;

public interface IFilterService : ISimpleContentService<Filter>
{
    /// <summary>
    /// Published filters of the given <see cref="FilterType"/>, ordered by
    /// <c>DisplayOrder</c> then <c>Label</c>. Used by the Case/Blog admin
    /// editors to populate their dropdowns and by the public list sections.
    /// </summary>
    Task<IReadOnlyList<Filter>> ListByTypeAsync(FilterType type, CancellationToken ct);
}
