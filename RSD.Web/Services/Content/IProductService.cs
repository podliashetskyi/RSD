using RSD.Web.Data.Entities;

namespace RSD.Web.Services.Content;

public record ProductUpsert(
    string Slug,
    string Name,
    string Subtitle,
    string Price,
    string Description,
    List<string> BulletPoints,
    string CoverImagePath,
    string TryForFreeHref,
    string LearnMoreHref,
    ContentStatus Status,
    SeoMetadata Seo,
    ProductDetailFields DetailFields);

public interface IProductService : IContentService<Product, Product, ProductUpsert> { }
