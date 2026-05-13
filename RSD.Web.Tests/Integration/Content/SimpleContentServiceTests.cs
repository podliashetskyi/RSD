using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;
using RSD.Web.Tests.Integration.Fixtures;

namespace RSD.Web.Tests.Integration.Content;

[Collection(nameof(PostgresCollection))]
public sealed class SimpleContentServiceTests(PostgresFixture Postgres)
{
    [Fact]
    public async Task Create_AssignsSlug_AndPersists()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        var db = await factory.CreateAsync();
        var service = factory.Provider.GetRequiredService<ITestimonialService>();
        var quoteSlugBase = $"slugcheck-{Guid.NewGuid():N}";

        var input = new Testimonial { Slug = "", Title = quoteSlugBase, Quote = "Some quote.", AuthorName = "Test Author" };
        var created = await service.CreateAsync(input, CancellationToken.None);

        created.Ok.Should().BeTrue();
        var fetched = await db.Testimonials.AsNoTracking().FirstAsync(t => t.Id == created.Value);
        fetched.Slug.Should().NotBeNullOrEmpty();
        fetched.Title.Should().Be(quoteSlugBase);
    }

    [Fact]
    public async Task List_FiltersByStatus()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        var db = await factory.CreateAsync();
        var service = factory.Provider.GetRequiredService<ITestimonialService>();
        var marker = $"marker-{Guid.NewGuid():N}";

        var draft = new Testimonial { Slug = "", Title = $"{marker}-draft", Quote = "Draft quote", AuthorName = marker, Status = ContentStatus.Draft };
        var published = new Testimonial { Slug = "", Title = $"{marker}-pub", Quote = "Published quote", AuthorName = marker, Status = ContentStatus.Published };
        await service.CreateAsync(draft, CancellationToken.None);
        await service.CreateAsync(published, CancellationToken.None);

        var publishedOnly = await service.ListAsync(new ContentQuery(Status: ContentStatus.Published, Search: marker), CancellationToken.None);
        publishedOnly.Should().OnlyContain(t => t.Status == ContentStatus.Published && t.AuthorName == marker);

        var draftOnly = await service.ListAsync(new ContentQuery(Status: ContentStatus.Draft, Search: marker), CancellationToken.None);
        draftOnly.Should().OnlyContain(t => t.Status == ContentStatus.Draft && t.AuthorName == marker);
    }

    [Fact]
    public async Task Update_AppliesChanges_AndBumpsUpdatedAt()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        var db = await factory.CreateAsync();
        var service = factory.Provider.GetRequiredService<ITestimonialService>();
        var created = await service.CreateAsync(new Testimonial { Slug = "", Title = "T1", Quote = "Q1", AuthorName = "A1" }, CancellationToken.None);
        var original = await db.Testimonials.AsNoTracking().FirstAsync(t => t.Id == created.Value);

        await Task.Delay(20); // ensure UpdatedAt diff is observable
        var update = new Testimonial { Id = created.Value, Slug = original.Slug, Title = "T1-updated", Quote = "Q1-updated", AuthorName = original.AuthorName };
        var result = await service.UpdateAsync(update, CancellationToken.None);

        result.Ok.Should().BeTrue();
        var refreshed = await db.Testimonials.AsNoTracking().FirstAsync(t => t.Id == created.Value);
        refreshed.Title.Should().Be("T1-updated");
        refreshed.Quote.Should().Be("Q1-updated");
        refreshed.UpdatedAt.Should().BeAfter(original.UpdatedAt);
    }

    [Fact]
    public async Task SoftDelete_HidesFromList_Restore_BringsItBack()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        var service = factory.Provider.GetRequiredService<ITestimonialService>();
        var marker = $"softdel-{Guid.NewGuid():N}";

        var created = await service.CreateAsync(new Testimonial { Slug = "", Title = marker, Quote = "x", AuthorName = marker }, CancellationToken.None);
        var listBefore = await service.ListAsync(new ContentQuery(Search: marker), CancellationToken.None);
        listBefore.Should().HaveCount(1);

        (await service.SoftDeleteAsync(created.Value, CancellationToken.None)).Ok.Should().BeTrue();
        var listAfterDelete = await service.ListAsync(new ContentQuery(Search: marker), CancellationToken.None);
        listAfterDelete.Should().BeEmpty();

        (await service.RestoreAsync(created.Value, CancellationToken.None)).Ok.Should().BeTrue();
        var listAfterRestore = await service.ListAsync(new ContentQuery(Search: marker), CancellationToken.None);
        listAfterRestore.Should().HaveCount(1);
    }

    [Fact]
    public async Task SetStatus_TransitionsState()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        var db = await factory.CreateAsync();
        var service = factory.Provider.GetRequiredService<ITestimonialService>();

        var created = await service.CreateAsync(new Testimonial { Slug = "", Title = "TS", Quote = "x", AuthorName = "TS Author", Status = ContentStatus.Draft }, CancellationToken.None);
        (await service.SetStatusAsync(created.Value, ContentStatus.Published, CancellationToken.None)).Ok.Should().BeTrue();

        var fetched = await db.Testimonials.AsNoTracking().FirstAsync(t => t.Id == created.Value);
        fetched.Status.Should().Be(ContentStatus.Published);
        fetched.PublishedAt.Should().NotBeNull();
    }
}
