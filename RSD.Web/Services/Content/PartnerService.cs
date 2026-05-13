using Microsoft.EntityFrameworkCore;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Cache;
using RSD.Web.Services.Slugs;

namespace RSD.Web.Services.Content;

public sealed class PartnerService(IDbContextFactory<AppDbContext> DbFactory, ISlugger Slugger, IPublicPageCache Cache)
    : SimpleContentService<Partner>(DbFactory, Slugger, Cache), IPartnerService
{
    protected override string NaturalKeyOf(Partner entity) => entity.Name;
}
