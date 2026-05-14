using RSD.Web.Data.Entities;

namespace RSD.Web.Services.Content;

public record ServiceUpsert(
    string Slug,
    string Title,
    string Description,
    List<string> BulletPoints,
    string CoverImagePath,
    string DetailsHref,
    string Intro,
    ContentStatus Status,
    SeoMetadata Seo);

public interface IServiceService : IContentService<Service, Service, ServiceUpsert> { }
