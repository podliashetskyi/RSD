using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Tests.Integration.Fixtures;

namespace RSD.Web.Tests.Integration.Content;

[Collection(nameof(PostgresCollection))]
public sealed class EntityDbSetSmokeTests(PostgresFixture Postgres)
{
    public static TheoryData<string> EntityNames =>
    [
        nameof(AppDbContext.Testimonials),
        nameof(AppDbContext.TeamMembers),
        nameof(AppDbContext.Partners),
        nameof(AppDbContext.Values),
        nameof(AppDbContext.MissionStats),
        nameof(AppDbContext.TechStackItems),
        nameof(AppDbContext.ContactPoints),
        nameof(AppDbContext.MessengerLinks),
        nameof(AppDbContext.SocialLinks),
    ];

    [Theory]
    [MemberData(nameof(EntityNames))]
    public async Task EachEntity_IsQueryable(string entityName)
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        var db = await factory.CreateAsync();
        var query = Select(db, entityName);

        var count = await query.CountAsync();
        count.Should().BeGreaterThanOrEqualTo(0);
    }

    private static IQueryable<ContentEntity> Select(AppDbContext db, string name)
    {
        var map = new Dictionary<string, Func<AppDbContext, IQueryable<ContentEntity>>>(StringComparer.Ordinal)
        {
            [nameof(AppDbContext.Testimonials)] = c => c.Testimonials,
            [nameof(AppDbContext.TeamMembers)] = c => c.TeamMembers,
            [nameof(AppDbContext.Partners)] = c => c.Partners,
            [nameof(AppDbContext.Values)] = c => c.Values,
            [nameof(AppDbContext.MissionStats)] = c => c.MissionStats,
            [nameof(AppDbContext.TechStackItems)] = c => c.TechStackItems,
            [nameof(AppDbContext.ContactPoints)] = c => c.ContactPoints,
            [nameof(AppDbContext.MessengerLinks)] = c => c.MessengerLinks,
            [nameof(AppDbContext.SocialLinks)] = c => c.SocialLinks,
        };
        return map[name](db);
    }
}
