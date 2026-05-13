using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Data.Entities.ArticleBlocks;
using RSD.Web.Tests.Integration.Fixtures;

namespace RSD.Web.Tests.Integration.Content;

[Collection(nameof(PostgresCollection))]
public sealed class JsonbColumnRoundTripTests(PostgresFixture Postgres)
{
    [Fact]
    public async Task BlogPost_BodyBlocks_RoundTripsThroughDb_PolymorphicBlocksKept()
    {
        await using var factory = new AppDbContextFactory(Postgres.ConnectionString);
        var db = await factory.CreateAsync();
        var marker = $"jsonb-{Guid.NewGuid():N}";

        var post = new BlogPost
        {
            Slug = marker,
            Title = "JSONB probe",
            BodyBlocks = new ArticleBody
            {
                Intro = "intro",
                Blocks =
                [
                    new RichTextBlock { Id = "rt-1", Html = "<p>hi</p>" },
                    new QuoteBlock { Id = "q-1", Quote = "Build fast.", Attribution = "Anon" }
                ]
            }
        };
        db.BlogPosts.Add(post);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        await using var dbConn = new NpgsqlConnection(Postgres.ConnectionString);
        await dbConn.OpenAsync();
        await using var cmd = dbConn.CreateCommand();
        cmd.CommandText = "SELECT \"BodyBlocks\"::text FROM blog_posts WHERE \"Id\" = @id";
        cmd.Parameters.AddWithValue("id", post.Id);
        var raw = (string)(await cmd.ExecuteScalarAsync())!;

        raw.Should().Contain("\"$type\":\"richtext\"");
        raw.Should().Contain("\"$type\":\"quote\"");

        var refreshed = await db.BlogPosts.AsNoTracking().FirstAsync(p => p.Id == post.Id);
        refreshed.BodyBlocks.Intro.Should().Be("intro");
        refreshed.BodyBlocks.Blocks.Should().HaveCount(2);
        refreshed.BodyBlocks.Blocks[0].Should().BeOfType<RichTextBlock>();
        refreshed.BodyBlocks.Blocks[1].Should().BeOfType<QuoteBlock>();
    }
}
