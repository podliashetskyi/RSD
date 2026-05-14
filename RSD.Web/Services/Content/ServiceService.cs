using Microsoft.EntityFrameworkCore;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Cache;
using RSD.Web.Services.Slugs;

namespace RSD.Web.Services.Content;

public sealed class ServiceService(
    IDbContextFactory<AppDbContext> DbFactory,
    ISlugger Slugger,
    IPublicPageCache Cache)
    : ContentServiceBase<Service, Service, Service, ServiceUpsert>(DbFactory, Slugger, Cache), IServiceService
{
    protected override Service NewEntityFrom(ServiceUpsert input) => new()
    {
        Slug = input.Slug,
        Title = input.Title,
        Description = input.Description,
        BulletPoints = [.. input.BulletPoints],
        CoverImagePath = input.CoverImagePath,
        DetailsHref = input.DetailsHref,
        Intro = input.Intro,
        Status = input.Status,
        Seo = input.Seo
    };

    protected override void ApplyUpdate(Service entity, ServiceUpsert input)
    {
        entity.Title = input.Title;
        entity.Description = input.Description;
        entity.BulletPoints = [.. input.BulletPoints];
        entity.CoverImagePath = input.CoverImagePath;
        entity.DetailsHref = input.DetailsHref;
        entity.Intro = input.Intro;
        entity.Status = input.Status;
        entity.Seo = input.Seo;
    }

    protected override Service ToListItem(Service entity) => entity;
    protected override Service ToDetail(Service entity) => entity;
    protected override string NaturalKeyOf(ServiceUpsert input) => input.Title;
    protected override string SlugOf(ServiceUpsert input) => input.Slug;
}
