using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Seo;
using RSD.Web.Tests.Integration.Fixtures;

namespace RSD.Web.Tests.Integration.Seo;

[Collection(nameof(PostgresCollection))]
public sealed class SitemapBuilderTests(PostgresFixture Postgres)
{
    private const string Root = "https://remsoft.dev";

    [Fact]
    public async Task Build_IncludesEstimatePage()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        await factory.CreateAsync();
        var builder = new SitemapBuilder(factory.Provider.GetRequiredService<IDbContextFactory<AppDbContext>>());

        var entries = await builder.BuildAsync(Root, CancellationToken.None);

        entries.Should().Contain(e => e.Loc == $"{Root}/estimate");
    }

    [Fact]
    public async Task Build_BlogAndCasesRoots_UseMostRecentChildUpdatedAt()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        var db = await factory.CreateAsync();
        var future = DateTime.UtcNow.AddDays(30);
        db.BlogPosts.Add(new BlogPost { Slug = $"sitemap-probe-{Guid.NewGuid():N}", Title = "Probe", Status = ContentStatus.Published, UpdatedAt = future });
        db.Cases.Add(new Case { Slug = $"sitemap-probe-{Guid.NewGuid():N}", Name = "Probe", Status = ContentStatus.Published, UpdatedAt = future });
        await db.SaveChangesAsync();

        var builder = new SitemapBuilder(factory.Provider.GetRequiredService<IDbContextFactory<AppDbContext>>());
        var entries = await builder.BuildAsync(Root, CancellationToken.None);

        entries.Single(e => e.Loc == $"{Root}/blog").LastMod.Should().BeCloseTo(future, TimeSpan.FromSeconds(1));
        entries.Single(e => e.Loc == $"{Root}/cases").LastMod.Should().BeCloseTo(future, TimeSpan.FromSeconds(1));
    }
}
