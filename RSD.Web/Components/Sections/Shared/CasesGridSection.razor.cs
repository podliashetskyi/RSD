#pragma warning disable S1144, S4487, S2933
using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Sections.Shared;

public partial class CasesGridSection
{
    [Parameter] public bool ShowHeader        { get; set; } = true;
    [Parameter] public bool ShowFilters       { get; set; }
    [Parameter] public bool ShowViewAllButton { get; set; } = true;
    [Parameter] public int  MaxItems          { get; set; }

    private static readonly IReadOnlyList<CaseEntry> Cases =
    [
        new CaseEntry(
            Name:        "FinanceHub",
            Industry:    "Fintech",
            Description: "Developed a high-load financial monitoring system for corporate clients. We integrated real-time transaction tracking with AI-based anomaly detection to prevent fraudulent activities.",
            TechTags:    ["React", "Node.js", "AI/ML"],
            ImageSrc:    "images/cases/case-fintech.png",
            Slug:        "financehub"),
        new CaseEntry(
            Name:        "EcoLogistics",
            Industry:    "Logistics",
            Description: "Cloud-based fleet management system. We implemented GPS tracking, fuel consumption analysis, and an automated route optimization algorithm that reduced delivery costs by 22%.",
            TechTags:    ["Python", "ML", "Cloud"],
            ImageSrc:    "images/cases/case-logistics.png",
            Slug:        "ecologistics"),
        new CaseEntry(
            Name:        "HealthCare+",
            Industry:    "Healthcare",
            Description: "A comprehensive telemedicine platform with HIPAA compliance. The solution includes encrypted video calls, automated patient scheduling, and seamless integration with hospital EHR systems.",
            TechTags:    ["WebRTC", "AWS", "IoT"],
            ImageSrc:    "images/cases/case-healthcare.png",
            Slug:        "healthcare-plus"),
        new CaseEntry(
            Name:        "EdTech",
            Industry:    "EdTech",
            Description: "An interactive LMS (Learning Management System) for international universities. Features include live streaming, automated grading, and an AI tutor for personalized student feedback.",
            TechTags:    ["GraphQL", "TypeScript", "Redis"],
            ImageSrc:    "images/cases/case-edtech.png",
            Slug:        "edtech"),
        new CaseEntry(
            Name:        "E-commerce",
            Industry:    "E-Commerce",
            Description: "Omnichannel retail platform for a major fashion brand. We built a scalable backend to handle 50k+ daily users and synchronized online inventory with 120+ physical stores.",
            TechTags:    ["Shopify Plus", "Ruby on Rails"],
            ImageSrc:    "images/cases/case-ecommerce.png",
            Slug:        "e-commerce"),
        new CaseEntry(
            Name:        "Industrial AI",
            Industry:    "Industrial",
            Description: "Computer vision system for manufacturing quality control. The AI automatically detects defects on the production line with 99.8% accuracy, significantly reducing manual inspection time.",
            TechTags:    ["OpenCV", "TensorFlow"],
            ImageSrc:    "images/cases/case-industrial-ai.png",
            Slug:        "industrial-ai"),
    ];

    private IReadOnlyList<CaseEntry> DisplayedCases =>
        MaxItems > 0 ? [.. Cases.Take(MaxItems)] : Cases;

    private static readonly IReadOnlyList<FilterDropdown> Filters =
    [
        new FilterDropdown("Industry",     ["All", "Fintech", "Logistics", "Healthcare", "EdTech", "E-Commerce", "Industrial"]),
        new FilterDropdown("Tech Stack",   ["All", "React", "Python", "TypeScript", "Cloud", "AI/ML"]),
        new FilterDropdown("Project Type", ["All", "Web Platform", "Mobile App", "Cloud System"]),
        new FilterDropdown("Year",         ["All", "2025", "2024", "2023", "2022"]),
    ];
}

public record CaseEntry(
    string Name,
    string Industry,
    string Description,
    IReadOnlyList<string> TechTags,
    string ImageSrc,
    string Slug = "");

public record FilterDropdown(string Label, IReadOnlyList<string> Options);
