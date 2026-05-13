using RSD.Web.Data.Entities;
using RSD.Web.Services.Slugs;

namespace RSD.Web.Data.Seed;

public sealed class TestimonialSeeder(AppDbContext Db, ISlugger Slugger) : SeederBase<Testimonial>(Db, Slugger)
{
    protected override Task<IReadOnlyList<Testimonial>> BuildAsync(CancellationToken ct)
    {
        IReadOnlyList<Testimonial> items =
        [
            Build("A 340% conversion lift",        "\"RSD reimagined our product, exceeding expectations — conversion up by 340%.\"", "Joseph McFall",  "Web developer @flowbite", "images/avatars/avatar-joseph.png",        1),
            Build("A team that understands",       "\"A team that understands business needs and proposes better solutions.\"",         "Sarah Williams", "CTO, HealthPlus",         "images/avatars/avatar-sarah-7be0d3.png",  2),
            Build("Reduced costs by 25%",          "\"Thanks to RSD AI solution, we optimized logistics and reduced costs by 25%. Highly recommend!\"", "Michael Chen", "Founder, LogiTrans", "images/avatars/avatar-michael-7be0d3.png", 3),
            Build("On-time, every time",           "\"Every milestone was delivered on schedule with zero quality compromises.\"",      "Joseph McFall",  "Web developer @flowbite", "images/avatars/avatar-joseph.png",        4),
            Build("Better solutions, every time",  "\"They consistently surfaced ideas we hadn't considered — a real partner, not a vendor.\"", "Sarah Williams", "CTO, HealthPlus", "images/avatars/avatar-sarah-7be0d3.png",  5),
            Build("25% cost reduction",            "\"AI-driven optimisations paid for themselves within the first quarter.\"",         "Michael Chen",   "Founder, LogiTrans",      "images/avatars/avatar-michael-7be0d3.png", 6),
            Build("Clean, scalable code",          "\"The codebase they delivered is clean, documented, and easy for our team to extend.\"", "Joseph McFall", "Web developer @flowbite", "images/avatars/avatar-joseph.png",        7),
            Build("Truly senior engineering",      "\"Senior-level thinking from day one — architecture, security, performance, all covered.\"", "Sarah Williams", "CTO, HealthPlus", "images/avatars/avatar-sarah-7be0d3.png",  8),
            Build("Logistics ROI in months",       "\"Their ML pipeline started saving us money inside the first 90 days.\"",          "Michael Chen",   "Founder, LogiTrans",      "images/avatars/avatar-michael-7be0d3.png", 9),
        ];
        return Task.FromResult(items);
    }

    private static Testimonial Build(string title, string quote, string author, string role, string avatar, int order) => new()
    {
        Slug = $"{author} {title}",
        Status = ContentStatus.Published,
        PublishedAt = DateTime.UtcNow,
        Title = title,
        Quote = quote,
        AuthorName = author,
        AuthorRole = role,
        AvatarPath = avatar,
        DisplayOnHome = true,
        DisplayOrder = order,
    };
}
