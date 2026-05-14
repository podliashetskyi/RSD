using Microsoft.EntityFrameworkCore;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Cache;
using RSD.Web.Services.Slugs;
using RSD.Web.Services.Storage;

namespace RSD.Web.Services.Content;

public sealed class ProductService(
    IDbContextFactory<AppDbContext> DbFactory,
    ISlugger Slugger,
    IPublicPageCache Cache,
    IFileRefCountTracker RefCounts)
    : ContentServiceBase<Product, Product, Product, ProductUpsert>(DbFactory, Slugger, Cache, RefCounts), IProductService
{
    protected override Product NewEntityFrom(ProductUpsert input) => new()
    {
        Slug = input.Slug,
        Name = input.Name,
        Subtitle = input.Subtitle,
        Price = input.Price,
        Description = input.Description,
        BulletPoints = [.. input.BulletPoints],
        CoverImagePath = input.CoverImagePath,
        TryForFreeHref = input.TryForFreeHref,
        LearnMoreHref = input.LearnMoreHref,
        Status = input.Status,
        Seo = input.Seo,
        DetailFields = input.DetailFields
    };

    protected override void ApplyUpdate(Product entity, ProductUpsert input)
    {
        entity.Name = input.Name;
        entity.Subtitle = input.Subtitle;
        entity.Price = input.Price;
        entity.Description = input.Description;
        entity.BulletPoints = [.. input.BulletPoints];
        entity.CoverImagePath = input.CoverImagePath;
        entity.TryForFreeHref = input.TryForFreeHref;
        entity.LearnMoreHref = input.LearnMoreHref;
        entity.Status = input.Status;
        entity.Seo = input.Seo;
        entity.DetailFields = input.DetailFields;
    }

    protected override Product ToListItem(Product entity) => entity;
    protected override Product ToDetail(Product entity) => entity;
    protected override string NaturalKeyOf(ProductUpsert input) => input.Name;
    protected override string SlugOf(ProductUpsert input) => input.Slug;
}
