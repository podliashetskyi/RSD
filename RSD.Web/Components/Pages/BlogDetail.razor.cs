#pragma warning disable S1144, S4487, S2933
using Microsoft.AspNetCore.Components;
using RSD.Web.Components.Sections.Article;

namespace RSD.Web.Components.Pages;

public partial class BlogDetail(NavigationManager Nav)
{
    [Parameter] public string Slug { get; set; } = "";

    private static readonly IReadOnlyList<TocEntry> TocItems =
    [
        new("introduction",   "Introduction"),
        new("key-takeaways",  "Key Takeaways"),
        new("benchmarks",     "Benchmarks"),
        new("implementation", "Implementation Plan"),
        new("pitfalls",       "Pitfalls to Avoid"),
        new("tooling",        "Tooling Stack"),
    ];

    private static readonly IReadOnlyList<SubsectionItem> KeyTakeawaysArch =
    [
        new("Horizontal Scaling:",     "Stateless services behind a load balancer, paired with managed datastores that can shard on demand."),
        new("Multi-region Active-active:", "Run hot in at least two regions so a zone failure becomes a non-event for end users."),
        new("Event-driven Pipelines:", "Replace blocking RPC chains with durable message queues; absorb traffic spikes without scaling every service."),
        new("Edge Caching:",           "Push static and cacheable dynamic content to the CDN edge to cut origin load by an order of magnitude."),
    ];

    private static readonly IReadOnlyList<SubsectionItem> KeyTakeawaysOps =
    [
        new("FinOps Discipline:",  "Daily cost dashboards owned by engineering, not just finance. Anomalies surface within hours."),
        new("Game Days:",          "Quarterly chaos drills that prove the active-active failover actually works."),
        new("SLO-driven Releases:","Error budgets gate deploys; teams ship faster when they're under budget, pause when they're over."),
        new("Right-sized Capacity:","Continuous workload right-sizing using historical traces, not static instance types."),
    ];

    private static readonly IReadOnlyList<StatRowItem> Benchmarks =
    [
        new("99.99%", "P99 availability."),
        new("40%",    "Cloud spend reduction."),
        new("10×",    "Traffic absorbed at launch."),
        new("< 5 min","Mean detection time."),
    ];

    private static readonly IReadOnlyList<SubsectionItem> ImplementationPlan =
    [
        new("Days 1-15 — Observability Baseline:", "Stand up RED/USE dashboards, traces, and structured logs across the hot path."),
        new("Days 16-30 — Right-sizing:",          "Apply continuous workload right-sizing and shift bursty traffic to spot capacity."),
        new("Days 31-60 — Multi-region Read Replica:", "Deploy a warm read replica region; redirect read traffic during peak."),
        new("Days 61-90 — Active-active Cutover:", "Switch writes to active-active, run a chaos game-day to prove the failover."),
    ];

    private static readonly IReadOnlyList<SubsectionItem> Pitfalls =
    [
        new("Over-provisioning by default:", "Teams budget for the worst-case spike and pay for headroom they never use."),
        new("Cost ownership in finance only:", "Without per-service budget signals, engineering and finance end up at odds."),
        new("Single-region disaster recovery:", "DR plans that depend on one provider's status page are not DR plans."),
        new("Manual scaling rules:",            "Static thresholds drift; predictive auto-scaling beats hand-tuned rules within a quarter."),
    ];

    private static readonly IReadOnlyList<GalleryImage> GalleryImages =
    [
        new("images/services/cloud-solutions/gallery-1-big.png", "Mobile dashboards"),
        new("images/services/cloud-solutions/gallery-1a.png",    "Globe network visualization"),
        new("images/services/cloud-solutions/gallery-1b.png",    "Connected nodes"),
        new("images/services/cloud-solutions/gallery-2a.png",    "Cloud network globe"),
        new("images/services/cloud-solutions/gallery-2b.png",    "Server room"),
        new("images/services/cloud-solutions/gallery-2-big.png", "Server infrastructure"),
    ];

    protected override void OnParametersSet()
    {
        if (!string.Equals(Slug, "cloud-infrastructure-scaling-2026", StringComparison.OrdinalIgnoreCase))
        {
            Nav.NavigateTo("/blog", replace: true);
        }
    }
}
