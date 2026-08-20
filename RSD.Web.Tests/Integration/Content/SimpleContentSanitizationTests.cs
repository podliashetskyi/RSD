using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;
using RSD.Web.Tests.Integration.Fixtures;

namespace RSD.Web.Tests.Integration.Content;

/// <summary>
/// Legal-page HTML must be sanitized by the service itself (Normalize hook), not only by
/// the admin page — MCP tools and any future writer go through the same invariant.
/// </summary>
[Collection(nameof(PostgresCollection))]
public sealed class SimpleContentSanitizationTests(PostgresFixture Postgres)
{
    [Fact]
    public async Task TermsOfService_CreateAsync_SanitizesBodyHtml()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        var db = await factory.CreateAsync();
        var service = factory.Provider.GetRequiredService<ITermsOfServiceService>();

        var result = await service.CreateAsync(new TermsOfService
        {
            Slug = $"terms-{Guid.NewGuid():N}",
            Title = "Sanitize check",
            BodyHtml = "<h2>Fine</h2><script>alert(1)</script>",
        }, CancellationToken.None);

        result.Ok.Should().BeTrue(result.Error);
        var stored = (await db.TermsOfService.AsNoTracking().FirstAsync(t => t.Id == result.Value)).BodyHtml;
        stored.Should().Contain("<h2>Fine</h2>").And.NotContain("script");
    }

    [Fact]
    public async Task PrivacyPolicy_UpdateAsync_SanitizesBodyHtml()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        var db = await factory.CreateAsync();
        var service = factory.Provider.GetRequiredService<IPrivacyPolicyService>();
        var created = await service.CreateAsync(new PrivacyPolicy
        {
            Slug = $"privacy-{Guid.NewGuid():N}",
            Title = "Sanitize check",
            BodyHtml = "<p>Original</p>",
        }, CancellationToken.None);
        var entity = await db.PrivacyPolicies.AsNoTracking().FirstAsync(p => p.Id == created.Value);

        entity.BodyHtml = "<p>Edited</p><iframe src=x></iframe>";
        var updated = await service.UpdateAsync(entity, CancellationToken.None);

        updated.Ok.Should().BeTrue(updated.Error);
        var stored = (await db.PrivacyPolicies.AsNoTracking().FirstAsync(p => p.Id == created.Value)).BodyHtml;
        stored.Should().Contain("<p>Edited</p>").And.NotContain("iframe");
    }
}
