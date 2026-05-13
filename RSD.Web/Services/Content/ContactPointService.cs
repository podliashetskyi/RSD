using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Cache;
using RSD.Web.Services.Slugs;

namespace RSD.Web.Services.Content;

public sealed class ContactPointService(AppDbContext Db, ISlugger Slugger, IPublicPageCache Cache)
    : SimpleContentService<ContactPoint>(Db, Slugger, Cache), IContactPointService
{
    protected override string NaturalKeyOf(ContactPoint entity) => entity.Label;
}
