using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;
using RSD.Web.Services.Mcp;
using RSD.Web.Tests.Integration.Fixtures;

namespace RSD.Web.Tests.Integration.Mcp;

/// <summary>
/// When the MCP principal is on the request, every service write audits as "AI Agent" —
/// the interceptor needs no changes because it reads the HttpContext claims.
/// </summary>
[Collection(nameof(PostgresCollection))]
public sealed class McpAuditAttributionTests(PostgresFixture Postgres)
{
    [Fact]
    public async Task ServiceWrite_UnderMcpPrincipal_AuditsAsAiAgent()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        var db = await factory.CreateAsync();
        var accessor = factory.Provider.GetRequiredService<IHttpContextAccessor>();
        accessor.HttpContext = new DefaultHttpContext { User = McpActor.BuildPrincipal() };

        var service = factory.Provider.GetRequiredService<IFaqItemService>();
        var created = await service.CreateAsync(new FaqItem
        {
            Slug = "",
            Question = $"Audit probe {Guid.NewGuid():N}?",
            AnswerHtml = "<p>A</p>",
        }, CancellationToken.None);
        created.Ok.Should().BeTrue(created.Error);

        var audit = await db.AuditLogEntries.AsNoTracking()
            .Where(a => a.EntityId == created.Value)
            .OrderByDescending(a => a.At)
            .FirstAsync();
        audit.UserId.Should().Be(McpActor.UserId);
        audit.UserEmail.Should().Be(McpActor.Email);
    }
}
