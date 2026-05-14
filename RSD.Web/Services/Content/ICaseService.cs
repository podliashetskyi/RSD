using RSD.Web.Data.Entities;

namespace RSD.Web.Services.Content;

public record CaseUpsert(
    string Slug,
    string Name,
    string Industry,
    string Description,
    string CoverImagePath,
    List<string> TechTags,
    ContentStatus Status,
    SeoMetadata Seo,
    CaseDetailFields DetailFields);

public interface ICaseService : IContentService<Case, Case, CaseUpsert> { }
