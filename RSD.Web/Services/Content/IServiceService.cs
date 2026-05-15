using RSD.Web.Data.Entities;

namespace RSD.Web.Services.Content;

public record ServiceUpsert(
    string Slug,
    string Title,
    string Description,
    List<string> BulletPoints,
    string CoverImagePath,
    string CoverImageAlt,
    string DetailsHref,
    string Intro,
    ContentStatus Status,
    SeoMetadata Seo,
    ArticleBody Body);

public interface IServiceService : IContentService<Service, Service, ServiceUpsert> { }
