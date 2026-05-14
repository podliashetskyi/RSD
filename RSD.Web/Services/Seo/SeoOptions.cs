namespace RSD.Web.Services.Seo;

public sealed record class SeoOptions
{
    public const string SectionName = "Seo";

    public RobotsOptions Robots { get; set; } = new();
}

public sealed record class RobotsOptions
{
    /// <summary>
    /// When true, the default robots.txt body disallows all crawlers.
    /// Useful for staging environments.
    /// </summary>
    public bool DisallowAll { get; set; }

    /// <summary>
    /// Optional custom body. When set, replaces the default body entirely.
    /// </summary>
    public string CustomBody { get; set; } = "";
}
