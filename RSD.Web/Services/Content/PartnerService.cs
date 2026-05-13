using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Cache;
using RSD.Web.Services.Slugs;

namespace RSD.Web.Services.Content;

public sealed class PartnerService(AppDbContext Db, ISlugger Slugger, IPublicPageCache Cache)
    : SimpleContentService<Partner>(Db, Slugger, Cache), IPartnerService
{
    protected override string NaturalKeyOf(Partner entity) => entity.Name;
}
