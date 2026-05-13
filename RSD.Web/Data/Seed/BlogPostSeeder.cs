using RSD.Web.Data.Entities;
using RSD.Web.Services.Slugs;

namespace RSD.Web.Data.Seed;

public sealed class BlogPostSeeder(AppDbContext Db, ISlugger Slugger) : SeederBase<BlogPost>(Db, Slugger)
{
    protected override Task<IReadOnlyList<BlogPost>> BuildAsync(CancellationToken ct)
    {
        IReadOnlyList<BlogPost> items =
        [
            Build("cloud-infrastructure-scaling-2026",
                  "Cloud Infrastructure Scaling: Strategies for 2026",
                  "Explore the latest trends in cloud-native scaling. We analyze how to maintain high performance under volatile load without sacrificing a stable digital ecosystem.",
                  "Development",
                  16,
                  ["SaaS", "AWS", "Scalability"],
                  "images/blog/post-cloud-infrastructure.png",
                  new DateTime(2025, 3, 15, 12, 0, 0, DateTimeKind.Utc)),
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
}
