namespace RSD.Web.Data.Entities;

public abstract record class ContentEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Slug { get; set; }
    public ContentStatus Status { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
    public bool IsDeleted { get; set; }
    public SeoMetadata Seo { get; set; } = new();
}
