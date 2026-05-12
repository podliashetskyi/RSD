#pragma warning disable S1144, S4487, S2933

namespace RSD.Web.Components.Sections.Services;

public partial class FeaturesSection
{
    private static readonly IReadOnlyList<ServiceFeature> Features =
    [
        new ServiceFeature(
            Title: "Web Development",
            Description: "Building fast, scalable web applications using modern technologies and frameworks.",
            BulletPoints: ["React, Vue, Angular", "Node.js, Python, Go", "REST API & GraphQL", "Microservices Architecture"],
            ImageSrc: "images/services/features/web-dev.png",
            DetailsHref: "/services/web-development"),
        new ServiceFeature(
            Title: "Mobile Development",
            Description: "Native and cross-platform mobile apps for iOS and Android with best-in-class UX.",
            BulletPoints: ["React Native & Flutter", "Swift & Kotlin", "Offline-first Approach", "Push Notifications"],
            ImageSrc: "images/services/features/mobile-dev.png",
            DetailsHref: "/services/mobile-development"),
        new ServiceFeature(
            Title: "Cloud Solutions",
            Description: "Cloud infrastructure, DevOps, and process automation for optimal performance.",
            BulletPoints: ["AWS, Google Cloud, Azure", "Kubernetes & Docker", "CI/CD Pipelines", "Monitoring & Logging"],
            ImageSrc: "images/services/features/cloud.png",
            DetailsHref: "/services/cloud-solutions"),
        new ServiceFeature(
            Title: "UI/UX Design",
            Description: "Thoughtful interface design that delivers the best user experience.",
            BulletPoints: ["User Research", "Prototyping", "Design Systems", "A/B Testing"],
            ImageSrc: "images/services/features/ui-ux.png",
            DetailsHref: "/services/ui-ux-design"),
        new ServiceFeature(
            Title: "AI & ML Solutions",
            Description: "Implementing artificial intelligence and machine learning into business processes.",
            BulletPoints: ["Predictive Analytics", "NLP & Chatbots", "Computer Vision", "Recommendation Systems"],
            ImageSrc: "images/services/features/ai-ml.png",
            DetailsHref: "/services/ai-ml-solutions"),
        new ServiceFeature(
            Title: "Cybersecurity",
            Description: "Security audits, data protection, and implementation of best practices for your business.",
            BulletPoints: ["Penetration Testing", "Security Audit", "GDPR Compliance", "Data Encryption"],
            ImageSrc: "images/services/features/cybersecurity.png",
            DetailsHref: "/services/cybersecurity"),
    ];

    private static string DirectionClass(int index) =>
        index % 2 == 0 ? "lg:flex-row" : "lg:flex-row-reverse";
}

public record ServiceFeature(
    string Title,
    string Description,
    IReadOnlyList<string> BulletPoints,
    string ImageSrc,
    string DetailsHref);
