using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Common;
using RSD.Web.Services.Email;
using RSD.Web.Services.Email.EmailTemplates;

namespace RSD.Web.Services.Estimates;

public sealed class ProjectEstimateService(
    IDbContextFactory<AppDbContext> DbFactory,
    IEmailSender Email,
    IOptions<EmailOptions> EmailOptions,
    ILogger<ProjectEstimateService> Log) : IProjectEstimateService
{
    public async Task<Result<Guid>> SubmitAsync(ProjectEstimateInput input, CancellationToken ct)
    {
        var validation = Validate(input);
        if (!validation.Ok) return Result.Fail<Guid>(validation.Error);

        var (min, max) = EstimatePricing.Compute(input.Platform, input.Domain, input.Complexity, input.Timeline);

        var estimate = new ProjectEstimate
        {
            Platform = input.Platform,
            Domain = input.Domain,
            Complexity = input.Complexity,
            Timeline = input.Timeline,
            EstimateMin = min,
            EstimateMax = max,
            ContactName = input.ContactName.Trim(),
            ContactEmail = input.ContactEmail.Trim(),
            Company = input.Company.Trim(),
            ProjectDescription = input.ProjectDescription.Trim(),
        };

        await using (var db = await DbFactory.CreateDbContextAsync(ct))
        {
            db.ProjectEstimates.Add(estimate);
            await db.SaveChangesAsync(ct);
        }

        _ = NotifyAsync(estimate);
        return Result.Ok(estimate.Id);
    }

    public async Task<ProjectEstimatePage> ListAsync(ProjectEstimateQuery query, CancellationToken ct)
    {
        await using var db = await DbFactory.CreateDbContextAsync(ct);
        var page = Math.Max(query.Page, 1);
        var size = Math.Clamp(query.PageSize, 1, 200);
        var q = db.ProjectEstimates.AsNoTracking();
        q = ApplyFilter(q, query.Filter);
        q = ApplySearch(q, query.Search);
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(e => e.SubmittedAt)
                           .Skip((page - 1) * size)
                           .Take(size)
                           .ToListAsync(ct);
        return new ProjectEstimatePage(items, total);
    }

    public async Task<ProjectEstimate?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        await using var db = await DbFactory.CreateDbContextAsync(ct);
        return await db.ProjectEstimates.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public Task<Result<Unit>> MarkHandledAsync(Guid id, string userId, CancellationToken ct) =>
        MutateAsync(id, e =>
        {
            e.IsHandled = true;
            e.HandledByUserId = userId;
            e.HandledAt = DateTime.UtcNow;
        }, ct);

    public Task<Result<Unit>> MarkOpenAsync(Guid id, CancellationToken ct) =>
        MutateAsync(id, e =>
        {
            e.IsHandled = false;
            e.HandledByUserId = "";
            e.HandledAt = null;
        }, ct);

    public async Task<Result<Unit>> DeleteAsync(Guid id, CancellationToken ct)
    {
        await using var db = await DbFactory.CreateDbContextAsync(ct);
        var entity = await db.ProjectEstimates.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (entity is null) return Result.Fail("Estimate not found.");
        db.ProjectEstimates.Remove(entity);
        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }

    private static Result<Unit> Validate(ProjectEstimateInput input)
    {
        if (string.IsNullOrWhiteSpace(input.ContactName) || input.ContactName.Length > 200) return Result.Fail("Name is required.");
        if (string.IsNullOrWhiteSpace(input.ContactEmail) || input.ContactEmail.Length > 320 || !input.ContactEmail.Contains('@')) return Result.Fail("A valid email is required.");
        if (string.IsNullOrWhiteSpace(input.Company) || input.Company.Length > 200) return Result.Fail("Company is required.");
        if (string.IsNullOrWhiteSpace(input.ProjectDescription) || input.ProjectDescription.Length > 8000) return Result.Fail("Project description is required.");
        return Result.Ok();
    }

    private static IQueryable<ProjectEstimate> ApplyFilter(IQueryable<ProjectEstimate> q, ProjectEstimateFilter filter) =>
        filter switch
        {
            ProjectEstimateFilter.Open => q.Where(e => !e.IsHandled),
            ProjectEstimateFilter.Handled => q.Where(e => e.IsHandled),
            _ => q,
        };

    private static IQueryable<ProjectEstimate> ApplySearch(IQueryable<ProjectEstimate> q, string search) =>
        string.IsNullOrWhiteSpace(search)
            ? q
            : q.Where(e => EF.Functions.ILike(e.ContactName, $"%{search}%")
                        || EF.Functions.ILike(e.ContactEmail, $"%{search}%")
                        || EF.Functions.ILike(e.Company, $"%{search}%")
                        || EF.Functions.ILike(e.ProjectDescription, $"%{search}%"));

    private async Task<Result<Unit>> MutateAsync(Guid id, Action<ProjectEstimate> mutate, CancellationToken ct)
    {
        await using var db = await DbFactory.CreateDbContextAsync(ct);
        var entity = await db.ProjectEstimates.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (entity is null) return Result.Fail("Estimate not found.");
        mutate(entity);
        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }

    private async Task NotifyAsync(ProjectEstimate estimate)
    {
        try
        {
            var to = EmailOptions.Value.ContactTo;
            if (string.IsNullOrWhiteSpace(to))
            {
                Log.LogWarning("ContactTo is not configured; skipping notification for estimate {Id}.", estimate.Id);
                return;
            }
            var adminUrl = $"/admin/estimates?id={estimate.Id}";
            var message = ProjectEstimateTemplate.Render(to, estimate, adminUrl);
            await Email.SendAsync(message, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Failed to deliver project-estimate notification for {Id}.", estimate.Id);
        }
    }
}
