using Microsoft.EntityFrameworkCore;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Cache;
using RSD.Web.Services.Slugs;

namespace RSD.Web.Services.Content;

public sealed class MessengerLinkService(IDbContextFactory<AppDbContext> DbFactory, ISlugger Slugger, IPublicPageCache Cache)
    : SimpleContentService<MessengerLink>(DbFactory, Slugger, Cache), IMessengerLinkService
{
    protected override string NaturalKeyOf(MessengerLink entity) => entity.Label;
}
