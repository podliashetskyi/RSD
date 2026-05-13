using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Common;
using RSD.Web.Services.Email;
using RSD.Web.Services.Email.EmailTemplates;

namespace RSD.Web.Services.Content;

public sealed class ContactSubmissionService(
    IDbContextFactory<AppDbContext> DbFactory,
    IEmailSender Email,
    IOptions<EmailOptions> EmailOptions,
    ILogger<ContactSubmissionService> Log) : IContactSubmissionService
{
    public async Task<Result<Guid>> SubmitAsync(ContactSubmissionInput input, CancellationToken ct)
    {
        var validation = Validate(input);
        if (!validation.Ok) return Result.Fail<Guid>(validation.Error);

        var submission = new ContactSubmission
        {
            Name = input.Name.Trim(),
            Email = input.Email.Trim(),
            Subject = string.IsNullOrWhiteSpace(input.Subject) ? "(No subject)" : input.Subject.Trim(),
            Message = input.Message.Trim()
        };

        await using (var db = await DbFactory.CreateDbContextAsync(ct))
        {
            db.ContactSubmissions.Add(submission);
            await db.SaveChangesAsync(ct);
        }

        _ = NotifyAsync(submission);
        return Result.Ok(submission.Id);
    }

    public async Task<ContactSubmissionPage> ListAsync(ContactSubmissionQuery query, CancellationToken ct)
    {
        await using var db = await DbFactory.CreateDbContextAsync(ct);
        var page = Math.Max(query.Page, 1);
        var size = Math.Clamp(query.PageSize, 1, 200);
        var q = db.ContactSubmissions.AsNoTracking();
        q = ApplyFilter(q, query.Filter);
        q = ApplySearch(q, query.Search);
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(c => c.SubmittedAt)
                           .Skip((page - 1) * size)
                           .Take(size)
                           .ToListAsync(ct);
        return new ContactSubmissionPage(items, total);
    }

    public async Task<ContactSubmission?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        await using var db = await DbFactory.CreateDbContextAsync(ct);
        return await db.ContactSubmissions.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public Task<Result<Unit>> MarkHandledAsync(Guid id, string userId, CancellationToken ct) =>
        MutateAsync(id, s =>
        {
            s.IsHandled = true;
            s.HandledByUserId = userId;
            s.HandledAt = DateTime.UtcNow;
        }, ct);

    public Task<Result<Unit>> MarkOpenAsync(Guid id, CancellationToken ct) =>
        MutateAsync(id, s =>
        {
            s.IsHandled = false;
            s.HandledByUserId = "";
            s.HandledAt = null;
        }, ct);

    public async Task<Result<Unit>> DeleteAsync(Guid id, CancellationToken ct)
    {
        await using var db = await DbFactory.CreateDbContextAsync(ct);
        var entity = await db.ContactSubmissions.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (entity is null) return Result.Fail("Submission not found.");
        db.ContactSubmissions.Remove(entity);
        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }

    private static Result<Unit> Validate(ContactSubmissionInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name) || input.Name.Length > 200) return Result.Fail("Name is required.");
        if (string.IsNullOrWhiteSpace(input.Email) || input.Email.Length > 320 || !input.Email.Contains('@')) return Result.Fail("A valid email is required.");
        if (input.Subject.Length > 500) return Result.Fail("Subject is too long.");
        if (string.IsNullOrWhiteSpace(input.Message) || input.Message.Length > 8000) return Result.Fail("Message is required.");
        return Result.Ok();
    }

    private static IQueryable<ContactSubmission> ApplyFilter(IQueryable<ContactSubmission> q, ContactSubmissionFilter filter) =>
        filter switch
        {
            ContactSubmissionFilter.Open => q.Where(c => !c.IsHandled),
            ContactSubmissionFilter.Handled => q.Where(c => c.IsHandled),
            _ => q
        };

    private static IQueryable<ContactSubmission> ApplySearch(IQueryable<ContactSubmission> q, string search) =>
        string.IsNullOrWhiteSpace(search)
            ? q
            : q.Where(c => EF.Functions.ILike(c.Name, $"%{search}%")
                        || EF.Functions.ILike(c.Email, $"%{search}%")
                        || EF.Functions.ILike(c.Subject, $"%{search}%"));

    private async Task<Result<Unit>> MutateAsync(Guid id, Action<ContactSubmission> mutate, CancellationToken ct)
    {
        await using var db = await DbFactory.CreateDbContextAsync(ct);
        var entity = await db.ContactSubmissions.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (entity is null) return Result.Fail("Submission not found.");
        mutate(entity);
        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }

    private async Task NotifyAsync(ContactSubmission submission)
    {
        try
        {
            var to = EmailOptions.Value.ContactTo;
            if (string.IsNullOrWhiteSpace(to))
            {
                Log.LogWarning("ContactTo is not configured; skipping notification for submission {Id}.", submission.Id);
                return;
            }
            var inboxUrl = $"/admin/inbox?id={submission.Id}";
            var message = ContactSubmissionTemplate.Render(to, submission, inboxUrl);
            await Email.SendAsync(message, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Failed to deliver contact-submission notification for {Id}.", submission.Id);
        }
    }
}
