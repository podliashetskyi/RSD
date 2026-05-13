using RSD.Web.Data.Entities;
using RSD.Web.Services.Common;

namespace RSD.Web.Services.Content;

public record ContactSubmissionPage(IReadOnlyList<ContactSubmission> Items, int TotalCount);

public interface IContactSubmissionService
{
    Task<Result<Guid>> SubmitAsync(ContactSubmissionInput input, CancellationToken ct);
    Task<ContactSubmissionPage> ListAsync(ContactSubmissionQuery query, CancellationToken ct);
    Task<ContactSubmission?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Result<Unit>> MarkHandledAsync(Guid id, string userId, CancellationToken ct);
    Task<Result<Unit>> MarkOpenAsync(Guid id, CancellationToken ct);
    Task<Result<Unit>> DeleteAsync(Guid id, CancellationToken ct);
}

public record ContactSubmissionInput(string Name, string Email, string Subject, string Message);
