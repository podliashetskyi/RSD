using Microsoft.EntityFrameworkCore;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Cache;
using RSD.Web.Services.Slugs;
using RSD.Web.Services.Storage;

namespace RSD.Web.Services.Content;

public sealed class TechStackItemService(IDbContextFactory<AppDbContext> DbFactory, ISlugger Slugger, IPublicPageCache Cache, IFileRefCountTracker RefCounts)
    : SimpleContentService<TechStackItem>(DbFactory, Slugger, Cache, RefCounts), ITechStackItemService
{
    protected override string NaturalKeyOf(TechStackItem entity) => entity.Label;
}
