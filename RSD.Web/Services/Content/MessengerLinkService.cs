using Microsoft.EntityFrameworkCore;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Cache;
using RSD.Web.Services.Slugs;
using RSD.Web.Services.Storage;

namespace RSD.Web.Services.Content;

public sealed class MessengerLinkService(IDbContextFactory<AppDbContext> DbFactory, ISlugger Slugger, IPublicPageCache Cache, IFileRefCountTracker RefCounts)
    : SimpleContentService<MessengerLink>(DbFactory, Slugger, Cache, RefCounts), IMessengerLinkService
{
    protected override string NaturalKeyOf(MessengerLink entity) => entity.Label;
}
