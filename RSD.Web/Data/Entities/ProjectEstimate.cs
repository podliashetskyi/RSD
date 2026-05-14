namespace RSD.Web.Data.Entities;

public enum ProjectPlatform
{
    MobileApp,
    WebPlatform,
    DesktopApp,
}

public enum ProjectDomain
{
    Ecommerce,
    Healthcare,
    Fintech,
}

public enum ProjectComplexity
{
    SimpleMvp,
    MediumProfessional,
    ComplexEnterprise,
}

public enum ProjectTimeline
{
    Standard,
    Accelerated,
    Urgent,
}

public record class ProjectEstimate
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime SubmittedAt { get; init; } = DateTime.UtcNow;

    public required ProjectPlatform Platform { get; set; }
    public required ProjectDomain Domain { get; set; }
    public required ProjectComplexity Complexity { get; set; }
    public required ProjectTimeline Timeline { get; set; }

    public decimal EstimateMin { get; set; }
    public decimal EstimateMax { get; set; }

    public required string ContactName { get; set; }
    public required string ContactEmail { get; set; }
    public required string Company { get; set; }
    public required string ProjectDescription { get; set; }

    public bool IsHandled { get; set; }
    public string HandledByUserId { get; set; } = "";
    public DateTime? HandledAt { get; set; }
}
