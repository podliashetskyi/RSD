using Microsoft.EntityFrameworkCore;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Cache;
using RSD.Web.Services.Common;
using RSD.Web.Services.Slugs;
using RSD.Web.Services.Storage;

namespace RSD.Web.Services.Content;

public sealed class BlogService(
    IDbContextFactory<AppDbContext> DbFactory,
    ISlugger Slugger,
    IPublicPageCache Cache,
    IFileRefCountTracker RefCounts,
    IContentHtmlSanitizer Html)
    : ContentServiceBase<BlogPost, BlogPost, BlogPost, BlogPostUpsert>(DbFactory, Slugger, Cache, RefCounts), IBlogService
{
    protected override BlogPost NewEntityFrom(BlogPostUpsert input) => new()
    {
        Slug = input.Slug,
        Title = input.Title,
        Summary = input.Summary,
        Description = input.Description,
        Category = input.Category,
        AuthorId = input.AuthorId,
        CoverImagePath = input.CoverImagePath,
        CoverImageAlt = input.CoverImageAlt,
        ReadTimeMinutes = input.ReadTimeMinutes,
        Tags = [.. input.Tags],
        Intro = Html.Sanitize(input.Intro),
        Status = input.Status,
        Seo = input.Seo,
        BodyBlocks = ArticleBodySanitizer.Sanitize(input.Body, Html)
    };

    protected override void ApplyUpdate(BlogPost entity, BlogPostUpsert input)
    {
        entity.Title = input.Title;
        entity.Summary = input.Summary;
        entity.Description = input.Description;
        entity.Category = input.Category;
        entity.AuthorId = input.AuthorId;
        entity.CoverImagePath = input.CoverImagePath;
        entity.CoverImageAlt = input.CoverImageAlt;
        entity.ReadTimeMinutes = input.ReadTimeMinutes;
        entity.Tags = [.. input.Tags];
        entity.Intro = Html.Sanitize(input.Intro);
        entity.Status = input.Status;
        entity.Seo = input.Seo;
        entity.BodyBlocks = ArticleBodySanitizer.Sanitize(input.Body, Html);
    }

    protected override BlogPost ToListItem(BlogPost entity) => entity;
    protected override BlogPost ToDetail(BlogPost entity) => entity;
    protected override string NaturalKeyOf(BlogPostUpsert input) => input.Title;
    protected override string SlugOf(BlogPostUpsert input) => input.Slug;
}
