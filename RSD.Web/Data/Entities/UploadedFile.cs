namespace RSD.Web.Data.Entities;

public record class UploadedFile
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Path { get; init; }
    public required string OriginalName { get; init; }
    public required string ContentType { get; init; }
    public long Bytes { get; init; }
    public string UploadedByUserId { get; init; } = "";
    public DateTime UploadedAt { get; init; } = DateTime.UtcNow;
    public List<ImageVariant> Variants { get; set; } = [];
    public int RefCount { get; set; }
}
