using RSD.Web.Data.Entities;
using RSD.Web.Services.Slugs;

namespace RSD.Web.Data.Seed;

public sealed class ProductSeeder(AppDbContext Db, ISlugger Slugger) : SeederBase<Product>(Db, Slugger)
{
    protected override Task<IReadOnlyList<Product>> BuildAsync(CancellationToken ct)
    {
        IReadOnlyList<Product> items =
        [
            Build("nexacrm", "NexaCRM", "Next-Generation CRM", "from $49/mo",
                  "Intelligent CRM system with AI assistant, sales automation, and real-time analytics.",
                  ["AI Sales Forecasting", "Funnel Automation", "50+ Service Integrations", "Real-time Analytics"],
                  "images/products/product-crm.png", "/contact", "/products/nexacrm", NexaCrmDetail()),
            Build("nexahr", "NexaHR", "HR Platform of the Future", "from $99/mo",
                  "Comprehensive HR management: recruiting, onboarding, performance review, and team development.",
                  ["AI Resume Screening", "Automated Onboarding", "360° Evaluation", "People Analytics"],
                  "images/products/product-hr.png", "/contact", "#", new ProductDetailFields()),
            Build("nexaanalytics", "NexaAnalytics", "Business Analytics for Everyone", "from $29/mo",
                  "No-code platform for data visualization and dashboard building with any data source.",
                  ["50+ Data Connectors", "AI Insights", "Custom Dashboards", "Real-time Reporting"],
                  "images/products/product-analytics.png", "/contact", "#", new ProductDetailFields()),
        ];
        return Task.FromResult(items);
    }

    private static Product Build(string slug, string name, string subtitle, string price, string description,
                                 List<string> bullets, string cover, string tryHref, string learnHref,
                                 ProductDetailFields detail) =>
        new()
        {
            Slug = slug,
            Name = name,
            Subtitle = subtitle,
            Price = price,
            Description = description,
            BulletPoints = bullets,
            CoverImagePath = cover,
            TryForFreeHref = tryHref,
            LearnMoreHref = learnHref,
            DetailFields = detail,
            Status = ContentStatus.Published,
            PublishedAt = DateTime.UtcNow
        };

    private static ProductDetailFields NexaCrmDetail() => new()
    {
        Badges =
        [
            new("Available", "bg-success-100", "text-success-600"),
            new("from $49/mo"),
        ],
        Features =
        [
            "AI Sales Forecasting",
            "50+ Service Integrations",
            "Custom Workflows",
            "React, Vue, Angular",
            "Node.js, Python, Go",
            "Mobile App",
        ],
        ChallengeMeta =
        [
            new("Product",   "Digital Service / App"),
            new("Industry",  "Healthcare"),
            new("Timeframe", "2011 – Present"),
        ],
        Hurdles =
        [
            new("Accessibility & Inclusion",
                "Designing an interface that remains fully functional for users with limited motor skills, ensuring they can navigate the app with minimal effort."),
            new("Real-Time Reliability",
                "For a person using a communication aid, the system is their \"voice.\" Any lag or system failure results in an immediate loss of ability to interact with the world, making 99.9% uptime non-negotiable."),
            new("Natural Interaction",
                "Moving beyond simple pre-recorded phrases to integrate advanced Word Prediction (NLP) that learns from the user’s behavior to speed up the communication process."),
            new("Platform Stability",
                "Building a robust architecture capable of supporting continuous updates and new language integrations without disrupting the existing user base."),
        ],
        Results =
        [
            "50,000+ active patients",
            "99.9% uptime achieved",
            "30+ IoT device integrations",
            "HIPAA certified",
        ],
        Metrics =
        [
            new("Increase Sales by 40%",
                "Our AI-powered insights help identify the best opportunities and optimal timing for outreach."),
            new("Save 10+ Hours Weekly",
                "Automate data entry, follow-ups, and reporting to focus on what matters most."),
            new("360° Customer View",
                "All customer interactions, history, and preferences in one unified dashboard."),
        ],
        TechPills = ["Flutter", "Dart", "AWS Lambda", "DynamoDB", "WebRTC", "IoT Core", "FHIR API"],
    };
}
