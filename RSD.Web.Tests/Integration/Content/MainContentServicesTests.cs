using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;
using RSD.Web.Tests.Integration.Fixtures;

namespace RSD.Web.Tests.Integration.Content;

[Collection(nameof(PostgresCollection))]
public sealed class MainContentServicesTests(PostgresFixture Postgres)
{
    [Fact]
    public async Task BlogService_Create_PublishesAndPreservesBodyOnUpdate()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        var db = await factory.CreateAsync();
        var service = factory.Provider.GetRequiredService<IBlogService>();
        var marker = $"blog-{Guid.NewGuid():N}";

        var upsert = new BlogPostUpsert(
            Slug: marker, Title: $"Title {marker}", Description: "desc", Category: "Cat",
            AuthorId: null, CoverImagePath: "/cover.png", ReadTimeMinutes: 7,
            Tags: ["A", "B"], Intro: "intro", Status: ContentStatus.Draft, Seo: new SeoMetadata());

        var created = await service.CreateAsync(upsert, CancellationToken.None);
        created.Ok.Should().BeTrue();

        var entity = await db.BlogPosts.AsNoTracking().FirstAsync(p => p.Id == created.Value);
        entity.BodyBlocks = new ArticleBody
        {
            Intro = "Body intro",
            Blocks = [new RSD.Web.Data.Entities.ArticleBlocks.RichTextBlock { Id = "rt", Html = "<p>hi</p>" }]
        };
        db.BlogPosts.Update(entity);
        await db.SaveChangesAsync();

        var updateInput = upsert with { Title = "Updated Title", Slug = entity.Slug };
        (await service.UpdateAsync(created.Value, updateInput, CancellationToken.None)).Ok.Should().BeTrue();

        var refreshed = await db.BlogPosts.AsNoTracking().FirstAsync(p => p.Id == created.Value);
        refreshed.Title.Should().Be("Updated Title");
        refreshed.BodyBlocks.Intro.Should().Be("Body intro");
        refreshed.BodyBlocks.Blocks.Should().ContainSingle().Which.Should().BeOfType<RSD.Web.Data.Entities.ArticleBlocks.RichTextBlock>();

        (await service.PublishAsync(created.Value, CancellationToken.None)).Ok.Should().BeTrue();
        (await db.BlogPosts.AsNoTracking().FirstAsync(p => p.Id == created.Value)).Status.Should().Be(ContentStatus.Published);
    }

    [Fact]
    public async Task CaseService_ListBySlug_RespectsDraftVisibility()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        await factory.CreateAsync();
        var service = factory.Provider.GetRequiredService<ICaseService>();
        var marker = $"case-{Guid.NewGuid():N}";

        var upsert = new CaseUpsert(
            Slug: marker, Name: $"Case {marker}", Industry: "Fintech", Description: "desc",
            CoverImagePath: "/c.png", TechTags: ["React"], Status: ContentStatus.Draft, Seo: new SeoMetadata());
        var created = await service.CreateAsync(upsert, CancellationToken.None);
        created.Ok.Should().BeTrue();

        (await service.GetBySlugAsync(marker, includeDrafts: false, CancellationToken.None)).Should().BeNull();
        (await service.GetBySlugAsync(marker, includeDrafts: true, CancellationToken.None)).Should().NotBeNull();
    }

    [Fact]
    public async Task ProductService_LifecyclePublishUnpublishArchiveSoftDelete()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        var db = await factory.CreateAsync();
        var service = factory.Provider.GetRequiredService<IProductService>();
        var marker = $"prod-{Guid.NewGuid():N}";

        var upsert = new ProductUpsert(
            Slug: marker, Name: $"Product {marker}", Subtitle: "sub", Price: "$10",
            Description: "desc", BulletPoints: ["a", "b"], CoverImagePath: "/p.png",
            TryForFreeHref: "/contact", LearnMoreHref: "#", Status: ContentStatus.Draft, Seo: new SeoMetadata());

        var created = await service.CreateAsync(upsert, CancellationToken.None);
        created.Ok.Should().BeTrue();

        (await service.PublishAsync(created.Value, CancellationToken.None)).Ok.Should().BeTrue();
        (await db.Products.AsNoTracking().FirstAsync(p => p.Id == created.Value)).Status.Should().Be(ContentStatus.Published);

        (await service.UnpublishAsync(created.Value, CancellationToken.None)).Ok.Should().BeTrue();
        (await db.Products.AsNoTracking().FirstAsync(p => p.Id == created.Value)).Status.Should().Be(ContentStatus.Draft);

        (await service.ArchiveAsync(created.Value, CancellationToken.None)).Ok.Should().BeTrue();
        (await db.Products.AsNoTracking().FirstAsync(p => p.Id == created.Value)).Status.Should().Be(ContentStatus.Archived);

        (await service.SoftDeleteAsync(created.Value, CancellationToken.None)).Ok.Should().BeTrue();
        (await service.GetByIdAsync(created.Value, CancellationToken.None)).Should().BeNull();

        (await service.RestoreAsync(created.Value, CancellationToken.None)).Ok.Should().BeTrue();
        (await service.GetByIdAsync(created.Value, CancellationToken.None)).Should().NotBeNull();
    }

    [Fact]
    public async Task ServiceService_CreatePersistsBulletsAndSeedDataReadable()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        await factory.CreateAsync();
        var service = factory.Provider.GetRequiredService<IServiceService>();
        var marker = $"svc-{Guid.NewGuid():N}";

        var upsert = new ServiceUpsert(
            Slug: marker, Title: $"Service {marker}", Description: "desc",
            BulletPoints: ["Bullet 1", "Bullet 2"], CoverImagePath: "/s.png", DetailsHref: "/services/x",
            Intro: "intro", Status: ContentStatus.Published, Seo: new SeoMetadata());
        var created = await service.CreateAsync(upsert, CancellationToken.None);
        created.Ok.Should().BeTrue();

        var fetched = await service.GetByIdAsync(created.Value, CancellationToken.None);
        fetched.Should().NotBeNull();
        fetched!.BulletPoints.Should().Equal("Bullet 1", "Bullet 2");
    }

    [Fact]
    public async Task Slugger_AssignsAndPrevents_Duplicates_AcrossUpsertsInOneTable()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        await factory.CreateAsync();
        var service = factory.Provider.GetRequiredService<IBlogService>();
        var marker = $"slugdup-{Guid.NewGuid():N}";

        var input = new BlogPostUpsert(
            Slug: "", Title: marker, Description: "", Category: "",
            AuthorId: null, CoverImagePath: "", ReadTimeMinutes: 1,
            Tags: [], Intro: "", Status: ContentStatus.Draft, Seo: new SeoMetadata());

        var a = await service.CreateAsync(input, CancellationToken.None);
        var b = await service.CreateAsync(input, CancellationToken.None);

        a.Ok.Should().BeTrue();
        b.Ok.Should().BeTrue();
        a.Value.Should().NotBe(b.Value);
    }
}
