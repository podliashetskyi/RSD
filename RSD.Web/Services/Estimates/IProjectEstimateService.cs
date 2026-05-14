using RSD.Web.Data.Entities;
using RSD.Web.Services.Common;

namespace RSD.Web.Services.Estimates;

public enum ProjectEstimateFilter
{
    All,
    Open,
    Handled,
}

public record ProjectEstimateQuery(
    ProjectEstimateFilter Filter = ProjectEstimateFilter.All,
    string Search = "",
    int Page = 1,
    int PageSize = 25);

public record ProjectEstimatePage(IReadOnlyList<ProjectEstimate> Items, int TotalCount);

public record ProjectEstimateInput(
    ProjectPlatform Platform,
    ProjectDomain Domain,
    ProjectComplexity Complexity,
    ProjectTimeline Timeline,
    string ContactName,
    string ContactEmail,
    string Company,
    string ProjectDescription);

public interface IProjectEstimateService
{
    Task<Result<Guid>> SubmitAsync(ProjectEstimateInput input, CancellationToken ct);
    Task<ProjectEstimatePage> ListAsync(ProjectEstimateQuery query, CancellationToken ct);
    Task<ProjectEstimate?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Result<Unit>> MarkHandledAsync(Guid id, string userId, CancellationToken ct);
    Task<Result<Unit>> MarkOpenAsync(Guid id, CancellationToken ct);
    Task<Result<Unit>> DeleteAsync(Guid id, CancellationToken ct);
}
