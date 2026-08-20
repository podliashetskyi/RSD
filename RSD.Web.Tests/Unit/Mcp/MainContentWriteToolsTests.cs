using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol;
using RSD.Web.Data.Entities;
using RSD.Web.Data.Entities.ArticleBlocks;
using RSD.Web.Services.Common;
using RSD.Web.Services.Content;
using RSD.Web.Services.Mcp.Tools;
using CommonUnit = global::RSD.Web.Services.Common.Unit;

namespace RSD.Web.Tests.Unit.Mcp;

/// <summary>
/// Write guardrails for the four main content types: create always lands as Draft,
/// updates never change status and refuse Published items without allowLiveEdit,
/// service failures surface as MCP errors.
/// </summary>
public sealed class MainContentWriteToolsTests
{
    [Fact]
    public async Task Create_ForcesDraft_EvenWhenPayloadSaysPublished()
    {
        var svc = new RecordingBlogService();
        var tools = Tools(svc);

        await tools.CreateContentAsync("blog", Json("""{"title":"T","status":"Published"}"""), CancellationToken.None);

        svc.LastCreate!.Status.Should().Be(ContentStatus.Draft);
        svc.LastCreate.Title.Should().Be("T");
    }

    [Fact]
    public async Task Create_ParsesArticleBody_WithTypeDiscriminators()
    {
        var svc = new RecordingBlogService();

        await Tools(svc).CreateContentAsync("blog", Json("""
            {"title":"T","body":{"intro":"<p>i</p>","blocks":[{"$type":"richtext","id":"b1","html":"<p>hello</p>"}]}}
            """), CancellationToken.None);

        svc.LastCreate!.Body.Blocks.Should().ContainSingle().Which.Should().BeOfType<RichTextBlock>();
    }

    [Fact]
    public async Task Update_RefusesPublishedItems_WithoutAllowLiveEdit()
    {
        var existing = new BlogPost { Slug = "live", Title = "Live", Status = ContentStatus.Published };
        var svc = new RecordingBlogService(existing);

        var act = () => Tools(svc).UpdateContentAsync("blog", existing.Id.ToString(),
            Json("""{"title":"Changed"}"""), allowLiveEdit: false, CancellationToken.None);

        await act.Should().ThrowAsync<McpException>().WithMessage("*allowLiveEdit*");
        svc.LastUpdate.Should().BeNull();
    }

    [Fact]
    public async Task Update_WithAllowLiveEdit_KeepsThePublishedStatus()
    {
        var existing = new BlogPost { Slug = "live", Title = "Live", Status = ContentStatus.Published };
        var svc = new RecordingBlogService(existing);

        await Tools(svc).UpdateContentAsync("blog", existing.Id.ToString(),
            Json("""{"title":"Changed","status":"Draft"}"""), allowLiveEdit: true, CancellationToken.None);

        svc.LastUpdate!.Status.Should().Be(ContentStatus.Published);
        svc.LastUpdate.Title.Should().Be("Changed");
    }

    [Fact]
    public async Task Update_DraftItem_StaysDraft_WithoutFlag()
    {
        var existing = new BlogPost { Slug = "d", Title = "D", Status = ContentStatus.Draft };
        var svc = new RecordingBlogService(existing);

        await Tools(svc).UpdateContentAsync("blog", existing.Id.ToString(),
            Json("""{"title":"D2","status":"Published"}"""), allowLiveEdit: false, CancellationToken.None);

        svc.LastUpdate!.Status.Should().Be(ContentStatus.Draft);
    }

    [Fact]
    public async Task Create_ServiceFailure_SurfacesTheErrorText()
    {
        var svc = new RecordingBlogService { FailWith = "The slug 'x' is already in use." };

        var act = () => Tools(svc).CreateContentAsync("blog", Json("""{"title":"T"}"""), CancellationToken.None);

        await act.Should().ThrowAsync<McpException>().WithMessage("*already in use*");
    }

    private static ContentTools Tools(IBlogService svc)
    {
        var services = new ServiceCollection();
        services.AddSingleton(svc);
        return new ContentTools(services.BuildServiceProvider());
    }

    private static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement.Clone();
}

internal sealed class RecordingBlogService(params BlogPost[] pool) : IBlogService
{
    public BlogPostUpsert? LastCreate { get; private set; }
    public BlogPostUpsert? LastUpdate { get; private set; }
    public string FailWith { get; init; } = "";

    public Task<IReadOnlyList<BlogPost>> ListAsync(ContentQuery query, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<BlogPost>>([.. pool]);
    public Task<BlogPost?> GetByIdAsync(Guid id, CancellationToken ct) =>
        Task.FromResult(pool.Concat(Created).FirstOrDefault(p => p.Id == id));
    public Task<BlogPost?> GetBySlugAsync(string slug, bool includeDrafts, CancellationToken ct) =>
        Task.FromResult(pool.FirstOrDefault(p => p.Slug == slug));
    private readonly List<BlogPost> Created = [];

    public Task<Result<Guid>> CreateAsync(BlogPostUpsert input, CancellationToken ct)
    {
        if (FailWith.Length > 0) return Task.FromResult(Result.Fail<Guid>(FailWith));
        LastCreate = input;
        var post = new BlogPost { Slug = input.Slug is { Length: > 0 } s ? s : "generated-slug", Title = input.Title, Status = input.Status };
        Created.Add(post);
        return Task.FromResult(Result.Ok(post.Id));
    }
    public Task<Result<CommonUnit>> UpdateAsync(Guid id, BlogPostUpsert input, CancellationToken ct)
    {
        LastUpdate = input;
        return Task.FromResult(Result.Ok());
    }
    public Task<Result<CommonUnit>> PublishAsync(Guid id, CancellationToken ct) => Task.FromResult(Result.Ok());
    public Task<Result<CommonUnit>> UnpublishAsync(Guid id, CancellationToken ct) => throw new NotImplementedException();
    public Task<Result<CommonUnit>> ArchiveAsync(Guid id, CancellationToken ct) => throw new NotImplementedException();
    public Task<Result<CommonUnit>> SoftDeleteAsync(Guid id, CancellationToken ct) => throw new NotImplementedException();
    public Task<Result<CommonUnit>> RestoreAsync(Guid id, CancellationToken ct) => throw new NotImplementedException();
    public Task<Result<CommonUnit>> HardDeleteAsync(Guid id, CancellationToken ct) => throw new NotImplementedException();
}
