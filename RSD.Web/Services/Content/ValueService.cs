using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Cache;
using RSD.Web.Services.Slugs;

namespace RSD.Web.Services.Content;

public sealed class ValueService(AppDbContext Db, ISlugger Slugger, IPublicPageCache Cache)
    : SimpleContentService<Value>(Db, Slugger, Cache), IValueService
{
    protected override string NaturalKeyOf(Value entity) => entity.Title;
}
