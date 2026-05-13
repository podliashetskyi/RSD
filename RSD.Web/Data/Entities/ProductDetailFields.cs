namespace RSD.Web.Data.Entities;

public record class ProductDetailFields
{
    public List<BadgePill> Badges { get; set; } = [];
    public List<string> Features { get; set; } = [];
    public List<MetaItem> ChallengeMeta { get; set; } = [];
    public List<ChallengeHurdle> Hurdles { get; set; } = [];
    public List<string> Results { get; set; } = [];
    public List<MetricCallout> Metrics { get; set; } = [];
    public List<string> TechPills { get; set; } = [];
}
