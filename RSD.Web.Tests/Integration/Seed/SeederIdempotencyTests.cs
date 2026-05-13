using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RSD.Web.Data;
using RSD.Web.Data.Seed;
using RSD.Web.Services.Slugs;
using RSD.Web.Tests.Integration.Fixtures;

namespace RSD.Web.Tests.Integration.Seed;

[Collection(nameof(PostgresCollection))]
public sealed class SeederIdempotencyTests(PostgresFixture Postgres)
{
    [Fact]
    public async Task ValueSeeder_RunsOnEmptyTable_SkipsOnFull()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        var db = await factory.CreateAsync();
        await ClearAsync(db);

        var slugger = factory.Provider.GetRequiredService<ISlugger>();
        var seeder = new ValueSeeder(db, slugger);

        await seeder.SeedAsync(CancellationToken.None);
        var firstCount = await db.Values.AsNoTracking().CountAsync();
        firstCount.Should().Be(4);

        await seeder.SeedAsync(CancellationToken.None);
        var secondCount = await db.Values.AsNoTracking().CountAsync();
        secondCount.Should().Be(4); // unchanged
    }

    [Fact]
    public async Task TeamMemberSeeder_HandlesDuplicateNames_WithSlugSuffixing()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        var db = await factory.CreateAsync();
        await ClearAsync(db);

        var slugger = factory.Provider.GetRequiredService<ISlugger>();
        var seeder = new TeamMemberSeeder(db, slugger);
        await seeder.SeedAsync(CancellationToken.None);

        var team = await db.TeamMembers.AsNoTracking().ToListAsync();
        team.Should().HaveCount(15);
        team.Select(t => t.Slug).Distinct().Should().HaveCount(15);
    }

    private static async Task ClearAsync(AppDbContext db)
    {
        // Use IgnoreQueryFilters so soft-deleted leftovers from other tests are wiped too.
        db.Values.RemoveRange(db.Values.IgnoreQueryFilters());
        db.TeamMembers.RemoveRange(db.TeamMembers.IgnoreQueryFilters());
        await db.SaveChangesAsync();
    }
}
