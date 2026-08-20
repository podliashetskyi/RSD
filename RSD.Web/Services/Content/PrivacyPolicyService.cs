using Microsoft.EntityFrameworkCore;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Cache;
using RSD.Web.Services.Common;
using RSD.Web.Services.Slugs;
using RSD.Web.Services.Storage;

namespace RSD.Web.Services.Content;

public sealed class PrivacyPolicyService(IDbContextFactory<AppDbContext> DbFactory, ISlugger Slugger, IPublicPageCache Cache, IFileRefCountTracker RefCounts, IContentHtmlSanitizer Sanitizer)
    : SimpleContentService<PrivacyPolicy>(DbFactory, Slugger, Cache, RefCounts), IPrivacyPolicyService
{
    protected override string NaturalKeyOf(PrivacyPolicy entity) => entity.Title;

    protected override PrivacyPolicy Normalize(PrivacyPolicy input) =>
        input with { BodyHtml = Sanitizer.Sanitize(input.BodyHtml) };
}
