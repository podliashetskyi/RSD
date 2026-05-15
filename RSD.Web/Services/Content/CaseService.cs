using Microsoft.EntityFrameworkCore;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Cache;
using RSD.Web.Services.Slugs;
using RSD.Web.Services.Storage;

namespace RSD.Web.Services.Content;

public sealed class CaseService(
    IDbContextFactory<AppDbContext> DbFactory,
    ISlugger Slugger,
    IPublicPageCache Cache,
    IFileRefCountTracker RefCounts)
    : ContentServiceBase<Case, Case, Case, CaseUpsert>(DbFactory, Slugger, Cache, RefCounts), ICaseService
{
    protected override Case NewEntityFrom(CaseUpsert input) => new()
    {
        Slug = input.Slug,
        Name = input.Name,
        Summary = input.Summary,
        Industry = input.Industry,
        Description = input.Description,
        CoverImagePath = input.CoverImagePath,
        CoverImageAlt = input.CoverImageAlt,
        TechTags = [.. input.TechTags],
        Status = input.Status,
        Seo = input.Seo,
        DetailFields = input.DetailFields
    };

    protected override void ApplyUpdate(Case entity, CaseUpsert input)
    {
        entity.Name = input.Name;
        entity.Summary = input.Summary;
        entity.Industry = input.Industry;
        entity.Description = input.Description;
        entity.CoverImagePath = input.CoverImagePath;
        entity.CoverImageAlt = input.CoverImageAlt;
        entity.TechTags = [.. input.TechTags];
        entity.Status = input.Status;
        entity.Seo = input.Seo;
        entity.DetailFields = input.DetailFields;
    }

    protected override Case ToListItem(Case entity) => entity;
    protected override Case ToDetail(Case entity) => entity;
    protected override string NaturalKeyOf(CaseUpsert input) => input.Name;
    protected override string SlugOf(CaseUpsert input) => input.Slug;
}
