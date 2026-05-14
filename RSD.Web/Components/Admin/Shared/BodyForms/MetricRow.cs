using RSD.Web.Data.Entities;

namespace RSD.Web.Components.Admin.Shared.BodyForms;

public sealed record class MetricRow
{
    public string Headline { get; set; } = "";
    public string Description { get; set; } = "";

    public static MetricRow From(MetricCallout m) => new() { Headline = m.Headline, Description = m.Description };
    public MetricCallout ToEntity() => new(Headline, Description);
}
