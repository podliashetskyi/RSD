using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Cache;
using RSD.Web.Services.Slugs;

namespace RSD.Web.Services.Content;

public sealed class TechStackItemService(AppDbContext Db, ISlugger Slugger, IPublicPageCache Cache)
    : SimpleContentService<TechStackItem>(Db, Slugger, Cache), ITechStackItemService
{
    protected override string NaturalKeyOf(TechStackItem entity) => entity.Label;
}
