#pragma warning disable S1144, S4487, S2933

using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Sections.Home;

public partial class TestimonialsCarouselSection(ITestimonialService Service)
{
    private IReadOnlyList<Testimonial> AllTestimonials { get; set; } = [];
    private IReadOnlyList<IReadOnlyList<Testimonial>> TestimonialGroups { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        var list = await Service.ListAsync(new ContentQuery(Status: ContentStatus.Published, PageSize: 100), CancellationToken.None);
        AllTestimonials = list
            .Where(t => t.DisplayOnHome)
            .OrderBy(t => t.DisplayOrder)
            .ToList();
        TestimonialGroups = AllTestimonials
            .Select((t, i) => new { t, i })
            .GroupBy(x => x.i / 3)
            .Select(g => (IReadOnlyList<Testimonial>)g.Select(x => x.t).ToList())
            .ToList();
    }

    private static string CarouselItemClass(bool isActive) => isActive ? "block" : "hidden";
}
