using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;
using RSD.Web.Services.Slugs;
using RSD.Web.Tests.Integration.Fixtures;

namespace RSD.Web.Tests.Integration.Content;

[Collection(nameof(PostgresCollection))]
public sealed class SluggerUniquenessTests(PostgresFixture Postgres)
{
    [Fact]
    public async Task GenerateUniqueAsync_OnCollision_SuffixesNextAvailable()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        var db = await factory.CreateAsync();
        var slugger = factory.Provider.GetRequiredService<ISlugger>();
        var service = factory.Provider.GetRequiredService<ITestimonialService>();
        var unique = $"collide-{Guid.NewGuid():N}";

        var first = await service.CreateAsync(new Testimonial { Slug = unique, Title = unique, Quote = "x", AuthorName = "Author A" }, CancellationToken.None);
        var firstSlug = (await db.Testimonials.AsNoTracking().FirstAsync(t => t.Id == first.Value)).Slug;
        firstSlug.Should().Be(unique);

        var candidate = await slugger.GenerateUniqueAsync<Testimonial>(unique, currentId: null, CancellationToken.None);
        candidate.Should().Be($"{unique}-2");

        var explicitCollision = await service.CreateAsync(new Testimonial { Slug = unique, Title = unique, Quote = "y", AuthorName = "Author B" }, CancellationToken.None);
        explicitCollision.Ok.Should().BeFalse();
        explicitCollision.Error.Should().Contain("already in use");

        var second = await service.CreateAsync(new Testimonial { Slug = "", Title = unique, Quote = "y", AuthorName = "" }, CancellationToken.None);
        var secondSlug = (await db.Testimonials.AsNoTracking().FirstAsync(t => t.Id == second.Value)).Slug;
        secondSlug.Should().Be($"{unique}-2");
    }

    [Fact]
    public async Task DatabaseEnforcesPartialUniqueIndex()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        var db = await factory.CreateAsync();
        var fixed_ = $"raw-collision-{Guid.NewGuid():N}";

        db.Testimonials.Add(new Testimonial { Slug = fixed_, Title = "A", Quote = "x", AuthorName = "A" });
        await db.SaveChangesAsync();

        db.Testimonials.Add(new Testimonial { Slug = fixed_, Title = "B", Quote = "y", AuthorName = "B" });
        var act = async () => await db.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }
}
