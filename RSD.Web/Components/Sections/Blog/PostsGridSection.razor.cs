#pragma warning disable S1144, S4487, S2933

namespace RSD.Web.Components.Sections.Blog;

public partial class PostsGridSection
{
    private static readonly IReadOnlyList<BlogPost> Posts =
    [
        new BlogPost(
            Category:        "Development",
            Title:           "Cloud Infrastructure Scaling: Strategies for 2026",
            Description:     "Explore the latest trends in cloud-native scaling. We analyze how to maintain high performance under volatile load without sacrificing a stable digital ecosystem.",
            AuthorName:      "Joseph McFall",
            AuthorAvatarSrc: "images/avatars/avatar-joseph.png",
            PublishedDate:   "Mar 15, 2025",
            ReadTime:        "16 min read",
            Tags:            ["SaaS", "AWS", "Scalability"],
            ImageSrc:        "images/blog/post-cloud-infrastructure.png",
            Slug:            "cloud-infrastructure-scaling-2026"),
        new BlogPost(
            Category:        "Architecture",
            Title:           "Microservices vs Monolith: Finding the Perfect Balance.",
            Description:     "Choosing between architectural styles is never easy. This guide details when the pros, cons, and trade-offs between popular paradigms apply to growing engineering teams.",
            AuthorName:      "Bonnie Green",
            AuthorAvatarSrc: "images/avatars/avatar-bonnie.png",
            PublishedDate:   "Mar 13, 2025",
            ReadTime:        "10 min watch",
            Tags:            ["Architecture", "React", "React"],
            ImageSrc:        "images/blog/post-microservices.png"),
        new BlogPost(
            Category:        "Development",
            Title:           "How to Integrate Modern CRM Systems into Existing Workflows.",
            Description:     "Integration doesn't have to be painful. Learn our 6-step approach to seamlessly connect a modern CRM with your legacy systems to boost team productivity.",
            AuthorName:      "Lana Byrd",
            AuthorAvatarSrc: "images/avatars/avatar-lana.png",
            PublishedDate:   "Mar 09, 2025",
            ReadTime:        "8 min read",
            Tags:            ["Development", "React", "React"],
            ImageSrc:        "images/blog/post-modern-crm.png"),
        new BlogPost(
            Category:        "Business",
            Title:           "The Business Impact of 99.9% Uptime in E-commerce.",
            Description:     "Every minute of downtime costs thousands. We look at the technical investments and financial returns of building world-class infrastructure for retail.",
            AuthorName:      "Robert Brown",
            AuthorAvatarSrc: "images/avatars/avatar-robert.png",
            PublishedDate:   "Mar 05, 2025",
            ReadTime:        "18 min read",
            Tags:            ["Business", "React", "React"],
            ImageSrc:        "images/blog/post-uptime.png"),
        new BlogPost(
            Category:        "Fintech",
            Title:           "Security First: Best Practices for High-Load Financial Platforms.",
            Description:     "Discover how we implement layered security in fintech. From zero-trust architecture to advanced threat detection — what works at scale in 2026.",
            AuthorName:      "Roberta Casas",
            AuthorAvatarSrc: "images/avatars/avatar-roberta.png",
            PublishedDate:   "Mar 04, 2025",
            ReadTime:        "12 min read",
            Tags:            ["React", "React", "React"],
            ImageSrc:        "images/blog/post-security-fintech.png"),
        new BlogPost(
            Category:        "Management",
            Title:           "Optimizing Project Costs: From MVP to Enterprise.",
            Description:     "Scalability shouldn't drain your budget. Learn how to strategically invest in tech infrastructure at different stages of your company's growth.",
            AuthorName:      "Jese Leos",
            AuthorAvatarSrc: "images/avatars/avatar-jese.png",
            PublishedDate:   "Mar 01, 2025",
            ReadTime:        "24 min read",
            Tags:            ["Management", "React", "React"],
            ImageSrc:        "images/blog/post-mvp-costs.png"),
        new BlogPost(
            Category:        "Architecture",
            Title:           "Microservices vs Monolith: Finding the Perfect Balance.",
            Description:     "Choosing between architectural styles is never easy. This guide details when the pros, cons, and trade-offs between popular paradigms apply to growing engineering teams.",
            AuthorName:      "Robert Brown",
            AuthorAvatarSrc: "images/avatars/avatar-robert.png",
            PublishedDate:   "Mar 05, 2025",
            ReadTime:        "18 min read",
            Tags:            ["Architecture", "React", "React"],
            ImageSrc:        "images/blog/post-microservices.png"),
        new BlogPost(
            Category:        "Business",
            Title:           "The Business Impact of 99.9% Uptime in E-commerce.",
            Description:     "Every minute of downtime costs thousands. We look at the technical investments and financial returns of building world-class infrastructure for retail.",
            AuthorName:      "Roberta Casas",
            AuthorAvatarSrc: "images/avatars/avatar-roberta.png",
            PublishedDate:   "Mar 04, 2025",
            ReadTime:        "12 min read",
            Tags:            ["Business", "React", "React"],
            ImageSrc:        "images/blog/post-ecommerce-uptime.png"),
        new BlogPost(
            Category:        "Development",
            Title:           "Cloud Infrastructure Scaling: Strategies for 2026",
            Description:     "Explore the latest trends in cloud-native scaling. We analyze how to maintain high performance under volatile load without sacrificing a stable digital ecosystem.",
            AuthorName:      "Jese Leos",
            AuthorAvatarSrc: "images/avatars/avatar-jese.png",
            PublishedDate:   "Mar 01, 2025",
            ReadTime:        "24 min read",
            Tags:            ["React", "React", "React"],
            ImageSrc:        "images/blog/post-cloud-infrastructure.png"),
    ];
}

public record BlogPost(
    string Category,
    string Title,
    string Description,
    string AuthorName,
    string AuthorAvatarSrc,
    string PublishedDate,
    string ReadTime,
    IReadOnlyList<string> Tags,
    string ImageSrc,
    string Slug = "");
