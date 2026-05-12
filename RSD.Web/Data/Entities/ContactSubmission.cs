namespace RSD.Web.Data.Entities;

public record class ContactSubmission
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Subject { get; set; }
    public required string Message { get; set; }
    public DateTime SubmittedAt { get; init; } = DateTime.UtcNow;
    public bool IsHandled { get; set; }
    public string HandledByUserId { get; set; } = "";
    public DateTime? HandledAt { get; set; }
}
