using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Cache;
using RSD.Web.Services.Slugs;

namespace RSD.Web.Services.Content;

public sealed class SocialLinkService(AppDbContext Db, ISlugger Slugger, IPublicPageCache Cache)
    : SimpleContentService<SocialLink>(Db, Slugger, Cache), ISocialLinkService
{
    protected override string NaturalKeyOf(SocialLink entity) => $"{entity.Scope} {entity.Label}";
}
