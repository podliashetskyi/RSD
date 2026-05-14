#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using RSD.Web.Components.Admin.Shared.BodyForms;
using RSD.Web.Data.Entities;

namespace RSD.Web.Components.Admin.Pages.Products;

public partial class ProductBodyEditor : ComponentBase
{
    [Parameter, EditorRequired] public ProductBodyForm Value { get; set; } = new();

    private void OnBadgesChanged(List<BadgeRow> items) => Value.Badges = items;
    private void OnFeaturesChanged(List<string> items) => Value.Features = items;
    private void OnChallengeMetaChanged(List<MetaRow> items) => Value.ChallengeMeta = items;
    private void OnHurdlesChanged(List<HurdleRow> items) => Value.Hurdles = items;
    private void OnResultsChanged(List<string> items) => Value.Results = items;
    private void OnMetricsChanged(List<MetricRow> items) => Value.Metrics = items;
    private void OnTechPillsChanged(List<string> items) => Value.TechPills = items;
}

public sealed record class ProductBodyForm
{
    public List<BadgeRow> Badges { get; set; } = [];
    public List<string> Features { get; set; } = [];
    public List<MetaRow> ChallengeMeta { get; set; } = [];
    public List<HurdleRow> Hurdles { get; set; } = [];
    public List<string> Results { get; set; } = [];
    public List<MetricRow> Metrics { get; set; } = [];
    public List<string> TechPills { get; set; } = [];

    public static ProductBodyForm From(ProductDetailFields d) => new()
    {
        Badges = [.. d.Badges.Select(BadgeRow.From)],
        Features = [.. d.Features],
        ChallengeMeta = [.. d.ChallengeMeta.Select(MetaRow.From)],
        Hurdles = [.. d.Hurdles.Select(HurdleRow.From)],
        Results = [.. d.Results],
        Metrics = [.. d.Metrics.Select(MetricRow.From)],
        TechPills = [.. d.TechPills],
    };

    public ProductDetailFields ToEntity() => new()
    {
        Badges = [.. Badges.Select(r => r.ToEntity())],
        Features = [.. Features],
        ChallengeMeta = [.. ChallengeMeta.Select(r => r.ToEntity())],
        Hurdles = [.. Hurdles.Select(r => r.ToEntity())],
        Results = [.. Results],
        Metrics = [.. Metrics.Select(r => r.ToEntity())],
        TechPills = [.. TechPills],
    };
}
