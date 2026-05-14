using RSD.Web.Data.Entities;

namespace RSD.Web.Services.Content;

public record BlogPostUpsert(
    string Slug,
    string Title,
    string Description,
    string Category,
    Guid? AuthorId,
    string CoverImagePath,
    int ReadTimeMinutes,
    List<string> Tags,
    string Intro,
    ContentStatus Status,
    SeoMetadata Seo,
    ArticleBody Body);

public interface IBlogService : IContentService<BlogPost, BlogPost, BlogPostUpsert> { }
