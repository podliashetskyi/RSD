namespace RSD.Web.Services.Seo;

public sealed record class SeoOptions
{
    public const string SectionName = "Seo";

    /// <summary>
    /// Canonical public origin (e.g. "https://remsoft.dev"). When set, sitemap, robots,
    /// canonical URLs, and JSON-LD use it instead of the inbound request host
    /// (Request.Host is untrustworthy behind a proxy without forwarded headers).
    /// </summary>
    public string BaseUrl { get; set; } = "";

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
