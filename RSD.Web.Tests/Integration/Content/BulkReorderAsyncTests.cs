using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;
using RSD.Web.Tests.Integration.Fixtures;

namespace RSD.Web.Tests.Integration.Content;

[Collection(nameof(PostgresCollection))]
public sealed class BulkReorderAsyncTests(PostgresFixture Postgres)
{
    [Fact]
    public async Task BulkReorderAsync_AppliesDisplayOrder_AndBumpsUpdatedAt()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        var db = await factory.CreateAsync();
        var service = factory.Provider.GetRequiredService<IValueService>();
        var marker = $"reorder-{Guid.NewGuid():N}";

        var a = await service.CreateAsync(new RSD.Web.Data.Entities.Value { Slug = "", Title = $"{marker}-A", Description = "x", DisplayOrder = 10 }, CancellationToken.None);
        var b = await service.CreateAsync(new RSD.Web.Data.Entities.Value { Slug = "", Title = $"{marker}-B", Description = "y", DisplayOrder = 20 }, CancellationToken.None);
        var c = await service.CreateAsync(new RSD.Web.Data.Entities.Value { Slug = "", Title = $"{marker}-C", Description = "z", DisplayOrder = 30 }, CancellationToken.None);

        var originalB = await db.Values.AsNoTracking().FirstAsync(v => v.Id == b.Value);
        await Task.Delay(20);

        // Reverse the order: C=1, B=2, A=3
        var entries = new List<ReorderEntry>
        {
            new(c.Value, 1),
            new(b.Value, 2),
            new(a.Value, 3),
        };
        var result = await service.BulkReorderAsync(entries, CancellationToken.None);
        result.Ok.Should().BeTrue();

        var fetched = await db.Values.AsNoTracking()
            .Where(v => v.Title.StartsWith(marker))
            .OrderBy(v => v.DisplayOrder)
            .ToListAsync();
        fetched.Select(v => v.Title).Should().Equal($"{marker}-C", $"{marker}-B", $"{marker}-A");

        var refreshedB = await db.Values.AsNoTracking().FirstAsync(v => v.Id == b.Value);
        refreshedB.UpdatedAt.Should().BeAfter(originalB.UpdatedAt);
    }

    [Fact]
    public async Task BulkReorderAsync_EmptyList_ReturnsOk()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        await factory.CreateAsync();
        var service = factory.Provider.GetRequiredService<IValueService>();
        var result = await service.BulkReorderAsync([], CancellationToken.None);
        result.Ok.Should().BeTrue();
    }
}
