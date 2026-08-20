using Microsoft.EntityFrameworkCore;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Cache;
using RSD.Web.Services.Common;
using RSD.Web.Services.Slugs;
using RSD.Web.Services.Storage;

namespace RSD.Web.Services.Content;

public sealed class TermsOfServiceService(IDbContextFactory<AppDbContext> DbFactory, ISlugger Slugger, IPublicPageCache Cache, IFileRefCountTracker RefCounts, IContentHtmlSanitizer Sanitizer)
    : SimpleContentService<TermsOfService>(DbFactory, Slugger, Cache, RefCounts), ITermsOfServiceService
{
    protected override string NaturalKeyOf(TermsOfService entity) => entity.Title;

    protected override TermsOfService Normalize(TermsOfService input) =>
        input with { BodyHtml = Sanitizer.Sanitize(input.BodyHtml) };
}
