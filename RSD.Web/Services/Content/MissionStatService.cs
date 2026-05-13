using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Cache;
using RSD.Web.Services.Slugs;

namespace RSD.Web.Services.Content;

public sealed class MissionStatService(AppDbContext Db, ISlugger Slugger, IPublicPageCache Cache)
    : SimpleContentService<MissionStat>(Db, Slugger, Cache), IMissionStatService
{
    protected override string NaturalKeyOf(MissionStat entity) => entity.Label;
}
