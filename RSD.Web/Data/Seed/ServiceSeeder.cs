using RSD.Web.Data.Entities;
using RSD.Web.Data.Entities.ArticleBlocks;
using RSD.Web.Services.Slugs;

namespace RSD.Web.Data.Seed;

public sealed class ServiceSeeder(AppDbContext Db, ISlugger Slugger) : SeederBase<Service>(Db, Slugger)
{
    protected override Task<IReadOnlyList<Service>> BuildAsync(CancellationToken ct)
    {
        IReadOnlyList<Service> items =
        [
            Build("web-development", "Web Development",
                  "Building fast, scalable web applications using modern technologies and frameworks.",
                  ["React, Vue, Angular", "Node.js, Python, Go", "REST API & GraphQL", "Microservices Architecture"],
                  "images/services/features/web-dev.png", "/services/web-development", new ArticleBody()),
            Build("mobile-development", "Mobile Development",
                  "Native and cross-platform mobile apps for iOS and Android with best-in-class UX.",
                  ["React Native & Flutter", "Swift & Kotlin", "Offline-first Approach", "Push Notifications"],
                  "images/services/features/mobile-dev.png", "/services/mobile-development", new ArticleBody()),
            Build("cloud-solutions", "Cloud Solutions",
                  "Cloud infrastructure, DevOps, and process automation for optimal performance.",
                  ["AWS, Google Cloud, Azure", "Kubernetes & Docker", "CI/CD Pipelines", "Monitoring & Logging"],
                  "images/services/features/cloud.png", "/services/cloud-solutions", CloudSolutionsBody()),
            Build("ui-ux-design", "UI/UX Design",
                  "Thoughtful interface design that delivers the best user experience.",
                  ["User Research", "Prototyping", "Design Systems", "A/B Testing"],
                  "images/services/features/ui-ux.png", "/services/ui-ux-design", new ArticleBody()),
            Build("ai-ml-solutions", "AI & ML Solutions",
                  "Implementing artificial intelligence and machine learning into business processes.",
                  ["Predictive Analytics", "NLP & Chatbots", "Computer Vision", "Recommendation Systems"],
                  "images/services/features/ai-ml.png", "/services/ai-ml-solutions", new ArticleBody()),
            Build("cybersecurity", "Cybersecurity",
                  "Security audits, data protection, and implementation of best practices for your business.",
                  ["Penetration Testing", "Security Audit", "GDPR Compliance", "Data Encryption"],
                  "images/services/features/cybersecurity.png", "/services/cybersecurity", new ArticleBody()),
        ];
        return Task.FromResult(items);
    }

    private static Service Build(string slug, string title, string description, List<string> bullets,
                                 string cover, string detailsHref, ArticleBody body) =>
        new()
        {
            Slug = slug,
            Title = title,
            Description = description,
            BulletPoints = bullets,
            CoverImagePath = cover,
            DetailsHref = detailsHref,
            BodyBlocks = body,
            Intro = "",
            Status = ContentStatus.Published,
            PublishedAt = DateTime.UtcNow
        };

    private static ArticleBody CloudSolutionsBody() => new()
    {
        Intro = "",
        Blocks =
        [
            new SubsectionBlock
            {
                Id = "key-features-ux",
                Heading = "Key Features",
                Items =
                [
                    new("High Availability:", "Automated failover systems to ensure zero downtime."),
                    new("Cost Efficiency:",   "Smart resource allocation to reduce cloud infrastructure spending."),
                    new("Security First:",    "Multi-layer encryption and compliance with global standards."),
                    new("Rapid Deployment:",  "CI/CD pipelines that cut time-to-market by 40%."),
                ]
            },
            new SubsectionBlock
            {
                Id = "key-features-design",
                Heading = "Design Practices",
                Items =
                [
                    new("User Research:",  "Deep dive into user behavior and market needs."),
                    new("Prototyping:",    "Creating interactive models to test ideas early."),
                    new("Design Systems:", "Building scalable and consistent visual language."),
                    new("A/B Testing:",    "Data-driven design optimization for better conversions."),
                ]
            },
            new SubsectionBlock
            {
                Id = "scaling-effectively",
                Heading = "Scaling Effectively",
                Items =
                [
                    new("Prioritize Microservices:", "We recommend moving to a microservices architecture to avoid a \"single point of failure\" of the entire system."),
                    new("Automate Deployment:",      "Implementing CI/CD pipelines allows you to reduce time-to-market by 40%."),
                    new("Optimize Cloud Costs:",     "Constant monitoring of resources allows you to save up to 30% of the budget for the infrastructure being contained."),
                    new("Security by Design:",       "Build the system with safety in mind at every stage, ensuring stability during peak loads."),
                ]
            },
            new SubsectionBlock
            {
                Id = "implementation-roadmap",
                Heading = "Implementation Roadmap",
                Items =
                [
                    new("High Availability:", "Automated failover systems to ensure zero downtime."),
                    new("Cost Efficiency:",   "Smart resource allocation to reduce cloud infrastructure spending."),
                    new("Security First:",    "Multi-layer encryption and compliance with global standards."),
                    new("Rapid Deployment:",  "CI/CD pipelines that cut time-to-market by 40%."),
                ]
            },
            new StatsRowBlock
            {
                Id = "recommendation-stats",
                Heading = "Our Recommendation",
                Items =
                [
                    new("99.9%", "Target Uptime."),
                    new("40%",   "Potential Cost Reduction."),
                    new("200+",  "Optimization Checkpoints."),
                    new("4M+",   "Users Supported."),
                ]
            },
            new GalleryBlock
            {
                Id = "gallery",
                Heading = "",
                Description = "",
                Images =
                [
                    new("images/services/cloud-solutions/gallery-1-big.png", "Mobile dashboard mockups"),
                    new("images/services/cloud-solutions/gallery-1a.png",    "Globe network visualization"),
                    new("images/services/cloud-solutions/gallery-1b.png",    "Connected nodes visualization"),
                    new("images/services/cloud-solutions/gallery-2a.png",    "Cloud network globe"),
                    new("images/services/cloud-solutions/gallery-2b.png",    "Server room"),
                    new("images/services/cloud-solutions/gallery-2-big.png", "Server infrastructure"),
                ],
                Tags = []
            },
        ]
    };
}
