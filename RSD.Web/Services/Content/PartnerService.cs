using Microsoft.EntityFrameworkCore;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Cache;
using RSD.Web.Services.Slugs;
using RSD.Web.Services.Storage;

namespace RSD.Web.Services.Content;

public sealed class PartnerService(IDbContextFactory<AppDbContext> DbFactory, ISlugger Slugger, IPublicPageCache Cache, IFileRefCountTracker RefCounts)
    : SimpleContentService<Partner>(DbFactory, Slugger, Cache, RefCounts), IPartnerService
{
    protected override string NaturalKeyOf(Partner entity) => entity.Name;
}
