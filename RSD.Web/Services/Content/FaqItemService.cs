using Microsoft.EntityFrameworkCore;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Cache;
using RSD.Web.Services.Common;
using RSD.Web.Services.Slugs;
using RSD.Web.Services.Storage;

namespace RSD.Web.Services.Content;

public sealed class FaqItemService(
    IDbContextFactory<AppDbContext> DbFactory,
    ISlugger Slugger,
    IPublicPageCache Cache,
    IFileRefCountTracker RefCounts,
    IContentHtmlSanitizer Sanitizer)
    : SimpleContentService<FaqItem>(DbFactory, Slugger, Cache, RefCounts), IFaqItemService
{
    protected override string NaturalKeyOf(FaqItem entity) => entity.Question;

    protected override FaqItem Normalize(FaqItem input) =>
        input with { AnswerHtml = Sanitizer.Sanitize(input.AnswerHtml) };
}
