using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Cache;
using RSD.Web.Services.Slugs;

namespace RSD.Web.Services.Content;

public sealed class TestimonialService(AppDbContext Db, ISlugger Slugger, IPublicPageCache Cache)
    : SimpleContentService<Testimonial>(Db, Slugger, Cache), ITestimonialService
{
    protected override string NaturalKeyOf(Testimonial entity) =>
        string.IsNullOrWhiteSpace(entity.AuthorName) ? entity.Title : $"{entity.AuthorName} {entity.Title}";
}
