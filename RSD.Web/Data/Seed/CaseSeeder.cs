using RSD.Web.Data.Entities;
using RSD.Web.Services.Slugs;

namespace RSD.Web.Data.Seed;

public sealed class CaseSeeder(AppDbContext Db, ISlugger Slugger) : SeederBase<Case>(Db, Slugger)
{
    protected override Task<IReadOnlyList<Case>> BuildAsync(CancellationToken ct)
    {
        IReadOnlyList<Case> items =
        [
            Build("financehub", "FinanceHub", "Fintech",
                  "Developed a high-load financial monitoring system for corporate clients. We integrated real-time transaction tracking with AI-based anomaly detection to prevent fraudulent activities.",
                  ["React", "Node.js", "AI/ML"], "images/cases/case-fintech.png", new CaseDetailFields()),
            Build("ecologistics", "EcoLogistics", "Logistics",
                  "Cloud-based fleet management system. We implemented GPS tracking, fuel consumption analysis, and an automated route optimization algorithm that reduced delivery costs by 22%.",
                  ["Python", "ML", "Cloud"], "images/cases/case-logistics.png", new CaseDetailFields()),
            Build("healthcare-plus", "HealthCare+", "Healthcare",
                  "A comprehensive telemedicine platform with HIPAA compliance. The solution includes encrypted video calls, automated patient scheduling, and seamless integration with hospital EHR systems.",
                  ["WebRTC", "AWS", "IoT"], "images/cases/case-healthcare.png", HealthCarePlusDetail()),
            Build("edtech", "EdTech", "EdTech",
                  "An interactive LMS (Learning Management System) for international universities. Features include live streaming, automated grading, and an AI tutor for personalized student feedback.",
                  ["GraphQL", "TypeScript", "Redis"], "images/cases/case-edtech.png", new CaseDetailFields()),
            Build("e-commerce", "E-commerce", "E-Commerce",
                  "Omnichannel retail platform for a major fashion brand. We built a scalable backend to handle 50k+ daily users and synchronized online inventory with 120+ physical stores.",
                  ["Shopify Plus", "Ruby on Rails"], "images/cases/case-ecommerce.png", new CaseDetailFields()),
            Build("industrial-ai", "Industrial AI", "Industrial",
                  "Computer vision system for manufacturing quality control. The AI automatically detects defects on the production line with 99.8% accuracy, significantly reducing manual inspection time.",
                  ["OpenCV", "TensorFlow"], "images/cases/case-industrial-ai.png", new CaseDetailFields()),
        ];
        return Task.FromResult(items);
    }

    private static Case Build(string slug, string name, string industry, string description,
                              List<string> techTags, string cover, CaseDetailFields detail) =>
        new()
        {
            Slug = slug,
            Name = name,
            Industry = industry,
            Description = description,
            TechTags = techTags,
            CoverImagePath = cover,
            DetailFields = detail,
            Status = ContentStatus.Published,
            PublishedAt = DateTime.UtcNow
        };

    private static CaseDetailFields HealthCarePlusDetail() => new()
    {
        Badges =
        [
            new("Healthcare",     "bg-cyan-100", "text-cyan-700"),
            new("23 months",      "bg-cyan-100", "text-cyan-700"),
            new("10 specialists", "bg-cyan-100", "text-cyan-700"),
        ],
        MetaTags = ["Flutter", "AWS", "IoT", "HIPAA"],
        Meta = [],
        Hurdles = [],
        Results =
        [
            "50,000+ active patients",
            "30+ IoT device integrations",
            "99.9% uptime achieved",
            "HIPAA certified",
        ],
        TechPills = ["Flutter", "Dart", "AWS Lambda", "DynamoDB", "WebRTC", "IoT Core", "FHIR API"],
        Metrics = [],
        Conclusion = new(
            "Building a HIPAA-compliant platform that seamlessly integrates with various IoT health monitoring devices while ensuring data security and real-time synchronization.",
            "We developed a Flutter-based cross-platform application with a robust backend on AWS. Custom integrations were built for popular health devices, and all data handling followed strict HIPAA guidelines.")
    };
}
