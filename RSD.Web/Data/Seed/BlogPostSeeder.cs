using RSD.Web.Data.Entities;
using RSD.Web.Data.Entities.ArticleBlocks;
using RSD.Web.Services.Slugs;

namespace RSD.Web.Data.Seed;

public sealed class BlogPostSeeder(AppDbContext Db, ISlugger Slugger) : SeederBase<BlogPost>(Db, Slugger)
{
    protected override Task<IReadOnlyList<BlogPost>> BuildAsync(CancellationToken ct)
    {
        IReadOnlyList<BlogPost> items =
        [
            BuildWithBody("cloud-infrastructure-scaling-2026",
                  "Cloud Infrastructure Scaling: Strategies for 2026",
                  "Explore the latest trends in cloud-native scaling. We analyze how to maintain high performance under volatile load without sacrificing a stable digital ecosystem.",
                  "Development",
                  16,
                  ["SaaS", "AWS", "Scalability"],
                  "images/blog/post-cloud-infrastructure.png",
                  new DateTime(2025, 3, 15, 12, 0, 0, DateTimeKind.Utc),
                  CloudInfrastructureBody()),
            Build("microservices-vs-monolith",
                  "Microservices vs Monolith: Finding the Perfect Balance.",
                  "Choosing between architectural styles is never easy. This guide details when the pros, cons, and trade-offs between popular paradigms apply to growing engineering teams.",
                  "Architecture",
                  10,
                  ["Architecture", "React", "React"],
                  "images/blog/post-microservices.png",
                  new DateTime(2025, 3, 13, 12, 0, 0, DateTimeKind.Utc)),
            Build("integrating-modern-crm",
                  "How to Integrate Modern CRM Systems into Existing Workflows.",
                  "Integration doesn't have to be painful. Learn our 6-step approach to seamlessly connect a modern CRM with your legacy systems to boost team productivity.",
                  "Development",
                  8,
                  ["Development", "React", "React"],
                  "images/blog/post-modern-crm.png",
                  new DateTime(2025, 3, 9, 12, 0, 0, DateTimeKind.Utc)),
            Build("uptime-business-impact",
                  "The Business Impact of 99.9% Uptime in E-commerce.",
                  "Every minute of downtime costs thousands. We look at the technical investments and financial returns of building world-class infrastructure for retail.",
                  "Business",
                  18,
                  ["Business", "React", "React"],
                  "images/blog/post-uptime.png",
                  new DateTime(2025, 3, 5, 12, 0, 0, DateTimeKind.Utc)),
            Build("security-first-fintech",
                  "Security First: Best Practices for High-Load Financial Platforms.",
                  "Discover how we implement layered security in fintech. From zero-trust architecture to advanced threat detection — what works at scale in 2026.",
                  "Fintech",
                  12,
                  ["React", "React", "React"],
                  "images/blog/post-security-fintech.png",
                  new DateTime(2025, 3, 4, 12, 0, 0, DateTimeKind.Utc)),
            Build("optimizing-project-costs",
                  "Optimizing Project Costs: From MVP to Enterprise.",
                  "Scalability shouldn't drain your budget. Learn how to strategically invest in tech infrastructure at different stages of your company's growth.",
                  "Management",
                  24,
                  ["Management", "React", "React"],
                  "images/blog/post-mvp-costs.png",
                  new DateTime(2025, 3, 1, 12, 0, 0, DateTimeKind.Utc)),
            Build("microservices-vs-monolith-2",
                  "Microservices vs Monolith: Finding the Perfect Balance.",
                  "Choosing between architectural styles is never easy. This guide details when the pros, cons, and trade-offs between popular paradigms apply to growing engineering teams.",
                  "Architecture",
                  18,
                  ["Architecture", "React", "React"],
                  "images/blog/post-microservices.png",
                  new DateTime(2025, 3, 5, 12, 0, 0, DateTimeKind.Utc)),
            Build("ecommerce-uptime-impact",
                  "The Business Impact of 99.9% Uptime in E-commerce.",
                  "Every minute of downtime costs thousands. We look at the technical investments and financial returns of building world-class infrastructure for retail.",
                  "Business",
                  12,
                  ["Business", "React", "React"],
                  "images/blog/post-ecommerce-uptime.png",
                  new DateTime(2025, 3, 4, 12, 0, 0, DateTimeKind.Utc)),
            Build("cloud-infrastructure-scaling-2026-2",
                  "Cloud Infrastructure Scaling: Strategies for 2026",
                  "Explore the latest trends in cloud-native scaling. We analyze how to maintain high performance under volatile load without sacrificing a stable digital ecosystem.",
                  "Development",
                  24,
                  ["React", "React", "React"],
                  "images/blog/post-cloud-infrastructure.png",
                  new DateTime(2025, 3, 1, 12, 0, 0, DateTimeKind.Utc)),
        ];
        return Task.FromResult(items);
    }

    private static BlogPost Build(string slug, string title, string description, string category,
                                  int readTimeMinutes, List<string> tags, string cover, DateTime publishedAt) =>
        new()
        {
            Slug = slug,
            Title = title,
            Description = description,
            Category = category,
            ReadTimeMinutes = readTimeMinutes,
            Tags = tags,
            CoverImagePath = cover,
            Status = ContentStatus.Published,
            PublishedAt = publishedAt,
            CreatedAt = publishedAt,
            UpdatedAt = publishedAt,
            Intro = "",
            BodyBlocks = new ArticleBody()
        };

    private static BlogPost BuildWithBody(string slug, string title, string description, string category,
                                          int readTimeMinutes, List<string> tags, string cover, DateTime publishedAt,
                                          ArticleBody body)
    {
        var p = Build(slug, title, description, category, readTimeMinutes, tags, cover, publishedAt);
        p.Intro = body.Intro;
        p.BodyBlocks = body;
        return p;
    }

    private static ArticleBody CloudInfrastructureBody() => new()
    {
        Intro = "As organisations move beyond first product-market fit, traffic patterns shift faster than infrastructure plans can keep up. The teams that scale gracefully into 2026 will be the ones that pair the right architectural patterns with a relentless focus on cost, reliability, and operational simplicity.",
        Blocks =
        [
            new RichTextBlock
            {
                Id = "intro-prose",
                Html = "<p>In this article we unpack the patterns separating teams that scale gracefully from those that drown in cloud bills and 3 a.m. pages. Drawn from interviews across SaaS, fintech, and e-commerce, these are the strategies our cloud-infrastructure team applies every day.</p>"
            },
            new SubsectionBlock
            {
                Id = "key-takeaways",
                Heading = "Key Takeaways",
                Subheading = "Architectural patterns that matter",
                Body = "The non-negotiables for a 2026-ready stack.",
                Items =
                [
                    new("Horizontal Scaling:",         "Stateless services behind a load balancer, paired with managed datastores that can shard on demand."),
                    new("Multi-region Active-active:", "Run hot in at least two regions so a zone failure becomes a non-event for end users."),
                    new("Event-driven Pipelines:",     "Replace blocking RPC chains with durable message queues; absorb traffic spikes without scaling every service."),
                    new("Edge Caching:",               "Push static and cacheable dynamic content to the CDN edge to cut origin load by an order of magnitude."),
                ]
            },
            new SubsectionBlock
            {
                Id = "ops-disciplines",
                Heading = "",
                Subheading = "Operational disciplines",
                Body = "The behaviours behind the technology.",
                Items =
                [
                    new("FinOps Discipline:",   "Daily cost dashboards owned by engineering, not just finance. Anomalies surface within hours."),
                    new("Game Days:",           "Quarterly chaos drills that prove the active-active failover actually works."),
                    new("SLO-driven Releases:", "Error budgets gate deploys; teams ship faster when they're under budget, pause when they're over."),
                    new("Right-sized Capacity:","Continuous workload right-sizing using historical traces, not static instance types."),
                ]
            },
            new StatsRowBlock
            {
                Id = "benchmarks",
                Heading = "Benchmarks We Saw",
                Items =
                [
                    new("99.99%",  "P99 availability."),
                    new("40%",     "Cloud spend reduction."),
                    new("10×",     "Traffic absorbed at launch."),
                    new("< 5 min", "Mean detection time."),
                ]
            },
            new SubsectionBlock
            {
                Id = "implementation",
                Heading = "A 90-Day Implementation Plan",
                Subheading = "Where to start when you can't pause the business",
                Body = "Each item is a step you can take without a full rewrite.",
                Items =
                [
                    new("Days 1-15 — Observability Baseline:",    "Stand up RED/USE dashboards, traces, and structured logs across the hot path."),
                    new("Days 16-30 — Right-sizing:",             "Apply continuous workload right-sizing and shift bursty traffic to spot capacity."),
                    new("Days 31-60 — Multi-region Read Replica:","Deploy a warm read replica region; redirect read traffic during peak."),
                    new("Days 61-90 — Active-active Cutover:",    "Switch writes to active-active, run a chaos game-day to prove the failover."),
                ]
            },
            new SubsectionBlock
            {
                Id = "pitfalls",
                Heading = "Pitfalls to Avoid",
                Subheading = "",
                Body = "Mistakes the teams we interviewed wished they had skipped.",
                Items =
                [
                    new("Over-provisioning by default:",   "Teams budget for the worst-case spike and pay for headroom they never use."),
                    new("Cost ownership in finance only:", "Without per-service budget signals, engineering and finance end up at odds."),
                    new("Single-region disaster recovery:","DR plans that depend on one provider's status page are not DR plans."),
                    new("Manual scaling rules:",           "Static thresholds drift; predictive auto-scaling beats hand-tuned rules within a quarter."),
                ]
            },
            new GalleryBlock
            {
                Id = "tooling",
                Heading = "Tooling Stack for 2026",
                Description = "The platforms our engineers reach for when scaling SaaS, fintech, and e-commerce workloads. Battle-tested, vendor-neutral, and ready to run.",
                Images =
                [
                    new("images/services/cloud-solutions/gallery-1-big.png", "Mobile dashboards"),
                    new("images/services/cloud-solutions/gallery-1a.png",    "Globe network visualization"),
                    new("images/services/cloud-solutions/gallery-1b.png",    "Connected nodes"),
                    new("images/services/cloud-solutions/gallery-2a.png",    "Cloud network globe"),
                    new("images/services/cloud-solutions/gallery-2b.png",    "Server room"),
                    new("images/services/cloud-solutions/gallery-2-big.png", "Server infrastructure"),
                ],
                Tags = ["Kubernetes", "Observability", "FinOps", "Multi-region"]
            }
        ]
    };
}
