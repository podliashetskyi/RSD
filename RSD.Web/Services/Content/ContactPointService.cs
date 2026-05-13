using Microsoft.EntityFrameworkCore;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Cache;
using RSD.Web.Services.Slugs;

namespace RSD.Web.Services.Content;

public sealed class ContactPointService(IDbContextFactory<AppDbContext> DbFactory, ISlugger Slugger, IPublicPageCache Cache)
    : SimpleContentService<ContactPoint>(DbFactory, Slugger, Cache), IContactPointService
{
    protected override string NaturalKeyOf(ContactPoint entity) => entity.Label;
}
