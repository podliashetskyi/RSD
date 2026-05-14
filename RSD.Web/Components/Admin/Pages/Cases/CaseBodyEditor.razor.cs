#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using RSD.Web.Components.Admin.Shared.BodyForms;
using RSD.Web.Data.Entities;

namespace RSD.Web.Components.Admin.Pages.Cases;

public partial class CaseBodyEditor : ComponentBase
{
    [Parameter, EditorRequired] public CaseBodyForm Value { get; set; } = new();

    private void OnBadgesChanged(List<BadgeRow> items) => Value.Badges = items;
    private void OnMetaTagsChanged(List<string> items) => Value.MetaTags = items;
    private void OnMetaChanged(List<MetaRow> items) => Value.Meta = items;
    private void OnHurdlesChanged(List<HurdleRow> items) => Value.Hurdles = items;
    private void OnResultsChanged(List<string> items) => Value.Results = items;
    private void OnTechPillsChanged(List<string> items) => Value.TechPills = items;
    private void OnMetricsChanged(List<MetricRow> items) => Value.Metrics = items;
}

public sealed record class CaseBodyForm
{
    public List<BadgeRow> Badges { get; set; } = [];
    public List<string> MetaTags { get; set; } = [];
    public List<MetaRow> Meta { get; set; } = [];
    public List<HurdleRow> Hurdles { get; set; } = [];
    public List<string> Results { get; set; } = [];
    public List<string> TechPills { get; set; } = [];
    public List<MetricRow> Metrics { get; set; } = [];
    public TestimonialForm Testimonial { get; set; } = new();
    public TwoColForm Conclusion { get; set; } = new();

    public static CaseBodyForm From(CaseDetailFields d) => new()
    {
        Badges = [.. d.Badges.Select(BadgeRow.From)],
        MetaTags = [.. d.MetaTags],
        Meta = [.. d.Meta.Select(MetaRow.From)],
        Hurdles = [.. d.Hurdles.Select(HurdleRow.From)],
        Results = [.. d.Results],
        TechPills = [.. d.TechPills],
        Metrics = [.. d.Metrics.Select(MetricRow.From)],
        Testimonial = TestimonialForm.From(d.Testimonial),
        Conclusion = TwoColForm.From(d.Conclusion),
    };

    public CaseDetailFields ToEntity() => new()
    {
        Badges = [.. Badges.Select(r => r.ToEntity())],
        MetaTags = [.. MetaTags],
        Meta = [.. Meta.Select(r => r.ToEntity())],
        Hurdles = [.. Hurdles.Select(r => r.ToEntity())],
        Results = [.. Results],
        TechPills = [.. TechPills],
        Metrics = [.. Metrics.Select(r => r.ToEntity())],
        Testimonial = Testimonial.ToEntity(),
        Conclusion = Conclusion.ToEntity(),
    };
}
