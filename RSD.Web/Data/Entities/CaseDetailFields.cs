namespace RSD.Web.Data.Entities;

public record class CaseDetailFields
{
    public List<BadgePill> Badges { get; set; } = [];
    public List<string> MetaTags { get; set; } = [];
    public List<MetaItem> Meta { get; set; } = [];
    public List<ChallengeHurdle> Hurdles { get; set; } = [];
    public List<string> Results { get; set; } = [];
    public List<string> TechPills { get; set; } = [];
    public List<MetricCallout> Metrics { get; set; } = [];
    public EmbeddedTestimonial? Testimonial { get; set; }
    public TwoColumnText? Conclusion { get; set; }
}
