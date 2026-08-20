using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RSD.Web.Components.Sections.Article;
using RSD.Web.Components.Sections.Blog;
using RSD.Web.Components.Sections.Detail;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Common;
using RSD.Web.Services.Content;
using CommonUnit = global::RSD.Web.Services.Common.Unit;

namespace RSD.Web.Tests.Unit.Components;

/// <summary>
/// GEO on-page semantics: exactly one h1 owned by the dominant title (IdentityBar),
/// blog cards keep a heading element around linked titles, and dates are machine-readable.
/// </summary>
public sealed class HeadingAndDateSemanticsTests
{
    [Fact]
    public void IdentityBar_RendersTitle_AsH1()
    {
        using var ctx = new BunitContext();

        var cut = ctx.Render<IdentityBar>(ps => ps
            .Add(p => p.BackHref, "/cases")
            .Add(p => p.Title, "Healthcare Plus")
            .Add(p => p.Subtitle, "Case study")
            .Add(p => p.CtaText, "Get Estimate"));

        cut.Find("h1").TextContent.Should().Contain("Healthcare Plus");
    }

    [Fact]
    public void MetaCard_RendersNoH1_SoTheIdentityBarOwnsIt()
    {
        using var ctx = new BunitContext();

        var cut = ctx.Render<MetaCard>(ps => ps
            .Add(p => p.Title, "Healthcare Plus")
            .Add(p => p.Description, "A big project."));

        cut.FindAll("h1").Should().BeEmpty();
        cut.Markup.Should().Contain("Healthcare Plus");
    }

    [Fact]
    public void ArticleHeader_WrapsDate_InTimeElement_WithIsoDatetime()
    {
        using var ctx = new BunitContext();

        var cut = ctx.Render<ArticleHeaderSection>(ps => ps
            .Add(p => p.CategoryText, "AI")
            .Add(p => p.DateText, "May 01, 2026")
            .Add(p => p.PublishedOn, new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc))
            .Add(p => p.ReadTime, "5 min")
            .Add(p => p.Title, "T")
            .Add(p => p.Subtitle, "S")
            .Add(p => p.AuthorName, "A")
            .Add(p => p.AuthorRole, "R")
            .Add(p => p.AuthorAvatarSrc, "images/logo.svg"));

        var time = cut.Find("time");
        time.GetAttribute("datetime").Should().Be("2026-05-01");
        time.TextContent.Should().Contain("May 01, 2026");
    }

    [Fact]
    public void ArticleHeader_TitleComesBeforeMetaRow_InsideAHeaderLandmark()
    {
        using var ctx = new BunitContext();

        var cut = ctx.Render<ArticleHeaderSection>(ps => ps
            .Add(p => p.CategoryText, "AI")
            .Add(p => p.DateText, "May 01, 2026")
            .Add(p => p.PublishedOn, new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc))
            .Add(p => p.ReadTime, "5 min")
            .Add(p => p.Title, "Answer First Title")
            .Add(p => p.Subtitle, "One-sentence summary.")
            .Add(p => p.AuthorName, "A")
            .Add(p => p.AuthorRole, "R")
            .Add(p => p.AuthorAvatarSrc, "images/logo.svg"));

        cut.FindAll("header h1").Should().NotBeEmpty();
        var markup = cut.Markup;
        markup.IndexOf("<h1", StringComparison.Ordinal)
            .Should().BeLessThan(markup.IndexOf("<time", StringComparison.Ordinal));
    }

    [Fact]
    public void Navbar_IsWrappedInAHeaderLandmark()
    {
        using var ctx = new BunitContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = ctx.Render<RSD.Web.Components.Layout.Navbar>();

        cut.FindAll("header nav").Should().NotBeEmpty();
    }

    [Fact]
    public void BlogCards_UseEditorCoverAlt_WhenPresent()
    {
        using var ctx = new BunitContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        var post = new BlogPost
        {
            Slug = "alt-post",
            Title = "Alt Post",
            CoverImagePath = "images/blog/x.png",
            CoverImageAlt = "Editor-written description",
            Status = ContentStatus.Published,
        };
        ctx.Services.AddSingleton<IBlogService>(new FakeBlogService([post]));
        ctx.Services.AddSingleton<ITeamMemberService>(new FakeTeamMemberService([]));
        ctx.Services.AddSingleton<IFilterService>(new FakeFilterService());

        var cut = ctx.Render<PostsGridSection>();

        cut.Markup.Should().Contain("Editor-written description");
    }

    [Fact]
    public void BlogCards_KeepHeadingAroundLinkedTitle_AndUseTimeForDates()
    {
        using var ctx = new BunitContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        var post = new BlogPost
        {
            Slug = "my-post",
            Title = "My Post",
            Summary = "Blurb",
            Status = ContentStatus.Published,
            PublishedAt = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc),
        };
        ctx.Services.AddSingleton<IBlogService>(new FakeBlogService([post]));
        ctx.Services.AddSingleton<ITeamMemberService>(new FakeTeamMemberService([]));
        ctx.Services.AddSingleton<IFilterService>(new FakeFilterService());

        var cut = ctx.Render<PostsGridSection>();

        var heading = cut.Find("article h3");
        heading.QuerySelector("a")!.GetAttribute("href").Should().Be("/blog/my-post");
        heading.TextContent.Should().Contain("My Post");

        cut.Find("article time").GetAttribute("datetime").Should().Be("2026-05-01");
    }
}

internal sealed class FakeBlogService(IReadOnlyList<BlogPost> posts) : IBlogService
{
    public Task<IReadOnlyList<BlogPost>> ListAsync(ContentQuery query, CancellationToken ct) => Task.FromResult(posts);
    public Task<BlogPost?> GetByIdAsync(Guid id, CancellationToken ct) => throw new NotImplementedException();
    public Task<BlogPost?> GetBySlugAsync(string slug, bool includeDrafts, CancellationToken ct) => throw new NotImplementedException();
    public Task<Result<Guid>> CreateAsync(BlogPostUpsert input, CancellationToken ct) => throw new NotImplementedException();
    public Task<Result<CommonUnit>> UpdateAsync(Guid id, BlogPostUpsert input, CancellationToken ct) => throw new NotImplementedException();
    public Task<Result<CommonUnit>> PublishAsync(Guid id, CancellationToken ct) => throw new NotImplementedException();
    public Task<Result<CommonUnit>> UnpublishAsync(Guid id, CancellationToken ct) => throw new NotImplementedException();
    public Task<Result<CommonUnit>> ArchiveAsync(Guid id, CancellationToken ct) => throw new NotImplementedException();
    public Task<Result<CommonUnit>> SoftDeleteAsync(Guid id, CancellationToken ct) => throw new NotImplementedException();
    public Task<Result<CommonUnit>> RestoreAsync(Guid id, CancellationToken ct) => throw new NotImplementedException();
    public Task<Result<CommonUnit>> HardDeleteAsync(Guid id, CancellationToken ct) => throw new NotImplementedException();
}

internal sealed class FakeFilterService() : FakeContentService<Filter>([]), IFilterService
{
    public Task<IReadOnlyList<Filter>> ListByTypeAsync(FilterType type, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<Filter>>([]);
}
