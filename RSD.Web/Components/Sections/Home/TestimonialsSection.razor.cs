#pragma warning disable S1144, S4487, S2933
using System.Collections.Generic;

namespace RSD.Web.Components.Sections.Home;

public partial class TestimonialsSection
{
    private static readonly IReadOnlyList<TestimonialEntry> Testimonials =
    [
        new("A 340% conversion lift", "\"RSD reimagined our product, exceeding expectations — conversion up by 340%.\"", "images/avatars/avatar-joseph.png", "Joseph McFall", "Web developer @flowbite"),
        new("A team that understands", "\"A team that understands business needs and proposes better solutions.\"", "images/avatars/avatar-sarah-7be0d3.png", "Sarah Williams", "CTO, HealthPlus"),
        new("Reduced costs by 25%", "\"Thanks to RSD AI solution, we optimized logistics and reduced costs by 25%. Highly recommend!\"", "images/avatars/avatar-michael-7be0d3.png", "Michael Chen", "Founder, LogiTrans"),
        new("On-time, every time", "\"Every milestone was delivered on schedule with zero quality compromises.\"", "images/avatars/avatar-joseph.png", "Joseph McFall", "Web developer @flowbite"),
        new("Better solutions, every time", "\"They consistently surfaced ideas we hadn't considered — a real partner, not a vendor.\"", "images/avatars/avatar-sarah-7be0d3.png", "Sarah Williams", "CTO, HealthPlus"),
        new("25% cost reduction", "\"AI-driven optimisations paid for themselves within the first quarter.\"", "images/avatars/avatar-michael-7be0d3.png", "Michael Chen", "Founder, LogiTrans"),
        new("Clean, scalable code", "\"The codebase they delivered is clean, documented, and easy for our team to extend.\"", "images/avatars/avatar-joseph.png", "Joseph McFall", "Web developer @flowbite"),
        new("Truly senior engineering", "\"Senior-level thinking from day one — architecture, security, performance, all covered.\"", "images/avatars/avatar-sarah-7be0d3.png", "Sarah Williams", "CTO, HealthPlus"),
        new("Logistics ROI in months", "\"Their ML pipeline started saving us money inside the first 90 days.\"", "images/avatars/avatar-michael-7be0d3.png", "Michael Chen", "Founder, LogiTrans"),
    ];
}

public record TestimonialEntry(string Title, string Quote, string AvatarSrc, string Name, string Role);
