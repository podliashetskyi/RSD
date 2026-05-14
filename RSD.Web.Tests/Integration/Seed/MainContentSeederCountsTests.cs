using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RSD.Web.Data;
using RSD.Web.Data.Seed;
using RSD.Web.Services.Slugs;
using RSD.Web.Tests.Integration.Fixtures;

namespace RSD.Web.Tests.Integration.Seed;

[Collection(nameof(PostgresCollection))]
public sealed class MainContentSeederCountsTests(PostgresFixture Postgres)
{
    [Fact]
    public async Task BlogPostSeeder_SeedsNinePosts()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        var db = await factory.CreateAsync();
        await ClearAsync(db);

        var slugger = factory.Provider.GetRequiredService<ISlugger>();
        await new BlogPostSeeder(db, slugger).SeedAsync(CancellationToken.None);

        var rows = await db.BlogPosts.AsNoTracking().ToListAsync();
        rows.Should().HaveCount(9);
        rows.Select(r => r.Slug).Distinct().Should().HaveCount(9);
    }

    [Fact]
    public async Task CaseSeeder_SeedsSixCases_WithHealthcareDetail()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        var db = await factory.CreateAsync();
        await ClearAsync(db);

        var slugger = factory.Provider.GetRequiredService<ISlugger>();
        await new CaseSeeder(db, slugger).SeedAsync(CancellationToken.None);

        var rows = await db.Cases.AsNoTracking().ToListAsync();
        rows.Should().HaveCount(6);
        rows.Should().ContainSingle(c => c.Slug == "healthcare-plus")
            .Which.DetailFields.MetaTags.Should().Contain(["Flutter", "AWS", "IoT", "HIPAA"]);
    }

    [Fact]
    public async Task ProductSeeder_SeedsThreeProducts_WithNexaCrmDetail()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        var db = await factory.CreateAsync();
        await ClearAsync(db);

        var slugger = factory.Provider.GetRequiredService<ISlugger>();
        await new ProductSeeder(db, slugger).SeedAsync(CancellationToken.None);

        var rows = await db.Products.AsNoTracking().ToListAsync();
        rows.Should().HaveCount(3);
        rows.Should().ContainSingle(p => p.Slug == "nexacrm")
            .Which.DetailFields.Features.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ServiceSeeder_SeedsSixServices_WithCloudSolutionsBody()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        var db = await factory.CreateAsync();
        await ClearAsync(db);

        var slugger = factory.Provider.GetRequiredService<ISlugger>();
        await new ServiceSeeder(db, slugger).SeedAsync(CancellationToken.None);

        var rows = await db.Services.AsNoTracking().ToListAsync();
        rows.Should().HaveCount(6);
        rows.Should().ContainSingle(s => s.Slug == "cloud-solutions")
            .Which.BodyBlocks.Blocks.Should().NotBeEmpty();
    }

    private static async Task ClearAsync(AppDbContext db)
    {
        db.BlogPosts.RemoveRange(db.BlogPosts.IgnoreQueryFilters());
        db.Cases.RemoveRange(db.Cases.IgnoreQueryFilters());
        db.Products.RemoveRange(db.Products.IgnoreQueryFilters());
        db.Services.RemoveRange(db.Services.IgnoreQueryFilters());
        await db.SaveChangesAsync();
    }
}
