using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Tests.Integration.Fixtures;

namespace RSD.Web.Tests.Integration;

[Collection(nameof(PostgresCollection))]
public sealed class AuditInterceptorTests(PostgresFixture Postgres)
{
    [Fact]
    public async Task SaveChanges_AddsContactSubmission_WritesAuditRowInSameTransaction()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        var db = await factory.CreateAsync();

        var submission = new ContactSubmission
        {
            Name = "Audit Tester",
            Email = "audit-tester@example.com",
            Subject = "Hello",
            Message = "Auditing the audit log.",
        };

        db.ContactSubmissions.Add(submission);
        await db.SaveChangesAsync();

        var audit = await db.AuditLogEntries
            .AsNoTracking()
            .Where(e => e.EntityType == nameof(ContactSubmission) && e.EntityId == submission.Id)
            .FirstAsync();

        audit.Action.Should().Be(AuditAction.Create);
        audit.UserId.Should().Be("");
        audit.Diff.Should().NotBe("{}");

        var diff = JsonDocument.Parse(audit.Diff);
        diff.RootElement.GetProperty("changes").EnumerateArray()
            .Select(e => e.GetProperty("Name").GetString())
            .Should().Contain("Name", "Email", "Subject", "Message");
    }

    [Fact]
    public async Task SaveChanges_UpdatesIsHandled_RecordsUpdateAction()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        var db = await factory.CreateAsync();

        var submission = new ContactSubmission
        {
            Name = "Handler Tester",
            Email = "handler@example.com",
            Subject = "Update me",
            Message = "Mark me as handled.",
        };
        db.ContactSubmissions.Add(submission);
        await db.SaveChangesAsync();

        submission.IsHandled = true;
        submission.HandledByUserId = "test-handler";
        submission.HandledAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        var updates = await db.AuditLogEntries
            .AsNoTracking()
            .Where(e => e.EntityType == nameof(ContactSubmission) && e.EntityId == submission.Id && e.Action == AuditAction.Update)
            .ToListAsync();

        updates.Should().NotBeEmpty();
        var diff = JsonDocument.Parse(updates.Last().Diff);
        diff.RootElement.GetProperty("changes").EnumerateArray()
            .Select(e => e.GetProperty("Name").GetString())
            .Should().Contain("IsHandled", "HandledByUserId");
    }
}
