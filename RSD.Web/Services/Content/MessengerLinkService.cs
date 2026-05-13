using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Cache;
using RSD.Web.Services.Slugs;

namespace RSD.Web.Services.Content;

public sealed class MessengerLinkService(AppDbContext Db, ISlugger Slugger, IPublicPageCache Cache)
    : SimpleContentService<MessengerLink>(Db, Slugger, Cache), IMessengerLinkService
{
    protected override string NaturalKeyOf(MessengerLink entity) => entity.Label;
}
