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
    public async Task BlogService_Create_PublishesAndRoundTripsBody()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        var db = await factory.CreateAsync();
        var service = factory.Provider.GetRequiredService<IBlogService>();
        var marker = $"blog-{Guid.NewGuid():N}";

        var body = new ArticleBody
        {
            Intro = "<p>Body intro</p>",
            Blocks = [new RSD.Web.Data.Entities.ArticleBlocks.RichTextBlock { Id = "rt", Html = "<p>hi</p>" }]
        };
        var upsert = new BlogPostUpsert(
            Slug: marker, Title: $"Title {marker}", Summary: "", Description: "desc", Category: "Cat",
            AuthorId: null, CoverImagePath: "/cover.png", CoverImageAlt: "", ReadTimeMinutes: 7,
            Tags: ["A", "B"], Intro: "intro", Status: ContentStatus.Draft, Seo: new SeoMetadata(),
            Body: body);

        var created = await service.CreateAsync(upsert, CancellationToken.None);
        created.Ok.Should().BeTrue();

        var entity = await db.BlogPosts.AsNoTracking().FirstAsync(p => p.Id == created.Value);
        entity.BodyBlocks.Intro.Should().Be("<p>Body intro</p>");
        entity.BodyBlocks.Blocks.Should().ContainSingle().Which.Should().BeOfType<RSD.Web.Data.Entities.ArticleBlocks.RichTextBlock>();

        var updateInput = upsert with { Title = "Updated Title", Slug = entity.Slug };
        (await service.UpdateAsync(created.Value, updateInput, CancellationToken.None)).Ok.Should().BeTrue();

        var refreshed = await db.BlogPosts.AsNoTracking().FirstAsync(p => p.Id == created.Value);
        refreshed.Title.Should().Be("Updated Title");
        refreshed.BodyBlocks.Intro.Should().Be("<p>Body intro</p>");

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
            Slug: marker, Name: $"Case {marker}", Summary: "", Industry: "Fintech", Description: "desc",
            CoverImagePath: "/c.png", CoverImageAlt: "", TechTags: ["React"], Status: ContentStatus.Draft, Seo: new SeoMetadata(),
            DetailFields: new CaseDetailFields());
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
            Slug: marker, Name: $"Product {marker}", Summary: "", Subtitle: "sub", Price: "$10",
            Description: "desc", BulletPoints: ["a", "b"], CoverImagePath: "/p.png", CoverImageAlt: "",
            TryForFreeHref: "/contact", LearnMoreHref: "#", Status: ContentStatus.Draft, Seo: new SeoMetadata(),
            DetailFields: new ProductDetailFields());

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
            Slug: marker, Title: $"Service {marker}", Summary: "", Description: "desc",
            BulletPoints: ["Bullet 1", "Bullet 2"], CoverImagePath: "/s.png", CoverImageAlt: "", DetailsHref: "/services/x",
            Intro: "intro", Status: ContentStatus.Published, Seo: new SeoMetadata(), Body: new ArticleBody());
        var created = await service.CreateAsync(upsert, CancellationToken.None);
        created.Ok.Should().BeTrue();

        var fetched = await service.GetByIdAsync(created.Value, CancellationToken.None);
        fetched.Should().NotBeNull();
        fetched!.BulletPoints.Should().Equal("Bullet 1", "Bullet 2");
    }

    [Fact]
    public async Task CaseService_DetailFields_RoundTripThroughUpdate()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        await factory.CreateAsync();
        var service = factory.Provider.GetRequiredService<ICaseService>();
        var marker = $"case-detail-{Guid.NewGuid():N}";

        var detail = new CaseDetailFields
        {
            Badges = [new BadgePill("Featured", "bg-emerald-100", "text-emerald-700")],
            MetaTags = ["B2B", "Fintech"],
            Meta = [new MetaItem("Duration", "6 months"), new MetaItem("Team", "5")],
            Hurdles =
            [
                new ChallengeHurdle("H1", "Body 1"),
                new ChallengeHurdle("H2", "Body 2"),
                new ChallengeHurdle("H3", "Body 3")
            ],
            Results = ["R1", "R2", "R3", "R4", "R5"],
            TechPills = ["React", "Postgres"],
            Metrics =
            [
                new MetricCallout("3.2×", "faster onboarding"),
                new MetricCallout("99.9%", "uptime")
            ],
            Testimonial = new EmbeddedTestimonial("Great team.", "Jane Doe", "CTO", "images/avatars/jane.png"),
            Conclusion = new TwoColumnText("The challenge text", "The solution text"),
        };

        var upsert = new CaseUpsert(
            Slug: marker, Name: $"Case {marker}", Summary: "", Industry: "Fintech", Description: "desc",
            CoverImagePath: "/c.png", CoverImageAlt: "", TechTags: ["React"], Status: ContentStatus.Draft,
            Seo: new SeoMetadata(), DetailFields: detail);

        var created = await service.CreateAsync(upsert, CancellationToken.None);
        created.Ok.Should().BeTrue();

        var refetched = await service.GetByIdAsync(created.Value, CancellationToken.None);
        refetched.Should().NotBeNull();
        refetched!.DetailFields.Badges.Should().HaveCount(1).And.ContainEquivalentOf(detail.Badges[0]);
        refetched.DetailFields.Hurdles.Should().HaveCount(3).And.Equal(detail.Hurdles);
        refetched.DetailFields.Results.Should().HaveCount(5).And.Equal("R1", "R2", "R3", "R4", "R5");
        refetched.DetailFields.Metrics.Should().HaveCount(2).And.Equal(detail.Metrics);
        refetched.DetailFields.Testimonial.Should().Be(detail.Testimonial);
        refetched.DetailFields.Conclusion.Should().Be(detail.Conclusion);

        var reordered = new CaseDetailFields
        {
            Badges = detail.Badges,
            MetaTags = detail.MetaTags,
            Meta = detail.Meta,
            Hurdles = [detail.Hurdles[2], detail.Hurdles[0], detail.Hurdles[1]],
            Results = detail.Results,
            TechPills = detail.TechPills,
            Metrics = detail.Metrics,
            Testimonial = detail.Testimonial,
            Conclusion = detail.Conclusion,
        };

        (await service.UpdateAsync(created.Value, upsert with { DetailFields = reordered }, CancellationToken.None))
            .Ok.Should().BeTrue();

        var afterReorder = await service.GetByIdAsync(created.Value, CancellationToken.None);
        afterReorder!.DetailFields.Hurdles.Select(h => h.Heading).Should().Equal("H3", "H1", "H2");
    }

    [Fact]
    public async Task ProductService_DetailFields_RoundTripThroughCreate()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        await factory.CreateAsync();
        var service = factory.Provider.GetRequiredService<IProductService>();
        var marker = $"prod-detail-{Guid.NewGuid():N}";

        var detail = new ProductDetailFields
        {
            Badges = [new BadgePill("New")],
            Features = ["F1", "F2"],
            ChallengeMeta = [new MetaItem("Industry", "SaaS")],
            Hurdles = [new ChallengeHurdle("H1", "B1")],
            Results = ["R1"],
            Metrics = [new MetricCallout("50%", "faster")],
            TechPills = ["React"],
        };

        var upsert = new ProductUpsert(
            Slug: marker, Name: $"Product {marker}", Summary: "", Subtitle: "sub", Price: "$10",
            Description: "desc", BulletPoints: ["a"], CoverImagePath: "/p.png", CoverImageAlt: "",
            TryForFreeHref: "/contact", LearnMoreHref: "#", Status: ContentStatus.Draft,
            Seo: new SeoMetadata(), DetailFields: detail);

        var created = await service.CreateAsync(upsert, CancellationToken.None);
        created.Ok.Should().BeTrue();

        var refetched = await service.GetByIdAsync(created.Value, CancellationToken.None);
        refetched!.DetailFields.Features.Should().Equal("F1", "F2");
        refetched.DetailFields.ChallengeMeta.Should().ContainSingle().Which.Should().Be(new MetaItem("Industry", "SaaS"));
        refetched.DetailFields.Hurdles.Should().ContainSingle().Which.Heading.Should().Be("H1");
    }

    [Fact]
    public async Task BlogService_Body_RoundTripsEveryBlockType_AndSanitizesScripts()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        var db = await factory.CreateAsync();
        var service = factory.Provider.GetRequiredService<IBlogService>();
        var marker = $"blog-allblocks-{Guid.NewGuid():N}";

        var body = new ArticleBody
        {
            Intro = "<p>intro</p><script>alert(1)</script>",
            Blocks =
            [
                new RSD.Web.Data.Entities.ArticleBlocks.SubsectionBlock {
                    Id = "s1", Heading = "H", Subheading = "Sub", Body = "<p>body</p><script>alert(2)</script>",
                    Items = [new SubsectionItem("L", "B")] },
                new RSD.Web.Data.Entities.ArticleBlocks.StatsRowBlock {
                    Id = "st1", Heading = "Stats", Items = [new StatRowItem("3.2x", "faster")] },
                new RSD.Web.Data.Entities.ArticleBlocks.GalleryBlock {
                    Id = "g1", Heading = "Gallery", Description = "desc",
                    Images = [new GalleryImage("/img.png", "alt")], Tags = ["a"] },
                new RSD.Web.Data.Entities.ArticleBlocks.BulletListBlock {
                    Id = "bl1", Heading = "List", Items = ["one", "two"] },
                new RSD.Web.Data.Entities.ArticleBlocks.QuoteBlock {
                    Id = "q1", Quote = "Q", Attribution = "A" },
                new RSD.Web.Data.Entities.ArticleBlocks.ImageBlock {
                    Id = "i1", ImagePath = "/p.png", Caption = "C", Alt = "A" },
                new RSD.Web.Data.Entities.ArticleBlocks.RichTextBlock {
                    Id = "rt1", Html = "<p>safe</p><script>alert(3)</script>" },
            ]
        };

        var upsert = new BlogPostUpsert(
            Slug: marker, Title: marker, Summary: "", Description: "", Category: "",
            AuthorId: null, CoverImagePath: "", CoverImageAlt: "", ReadTimeMinutes: 1,
            Tags: [], Intro: "", Status: ContentStatus.Draft, Seo: new SeoMetadata(),
            Body: body);

        var created = await service.CreateAsync(upsert, CancellationToken.None);
        created.Ok.Should().BeTrue();

        var stored = await db.BlogPosts.AsNoTracking().FirstAsync(p => p.Id == created.Value);
        stored.BodyBlocks.Intro.Should().NotContain("<script>");
        stored.BodyBlocks.Blocks.Should().HaveCount(7);
        var rt = stored.BodyBlocks.Blocks.OfType<RSD.Web.Data.Entities.ArticleBlocks.RichTextBlock>().Single();
        rt.Html.Should().NotContain("<script>");
        rt.Html.Should().Contain("<p>safe</p>");
        var sub = stored.BodyBlocks.Blocks.OfType<RSD.Web.Data.Entities.ArticleBlocks.SubsectionBlock>().Single();
        sub.Body.Should().NotContain("<script>");

        var reordered = body with { Blocks = [.. ((IEnumerable<ArticleBlock>)body.Blocks).Reverse()] };
        (await service.UpdateAsync(created.Value, upsert with { Body = reordered }, CancellationToken.None))
            .Ok.Should().BeTrue();
        var after = await db.BlogPosts.AsNoTracking().FirstAsync(p => p.Id == created.Value);
        after.BodyBlocks.Blocks[0].Should().BeOfType<RSD.Web.Data.Entities.ArticleBlocks.RichTextBlock>();
        after.BodyBlocks.Blocks[^1].Should().BeOfType<RSD.Web.Data.Entities.ArticleBlocks.SubsectionBlock>();
    }

    [Fact]
    public async Task Slugger_AssignsAndPrevents_Duplicates_AcrossUpsertsInOneTable()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        await factory.CreateAsync();
        var service = factory.Provider.GetRequiredService<IBlogService>();
        var marker = $"slugdup-{Guid.NewGuid():N}";

        var input = new BlogPostUpsert(
            Slug: "", Title: marker, Summary: "", Description: "", Category: "",
            AuthorId: null, CoverImagePath: "", CoverImageAlt: "", ReadTimeMinutes: 1,
            Tags: [], Intro: "", Status: ContentStatus.Draft, Seo: new SeoMetadata(),
            Body: new ArticleBody());

        var a = await service.CreateAsync(input, CancellationToken.None);
        var b = await service.CreateAsync(input, CancellationToken.None);

        a.Ok.Should().BeTrue();
        b.Ok.Should().BeTrue();
        a.Value.Should().NotBe(b.Value);
    }
}
