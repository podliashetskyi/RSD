using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Cache;
using RSD.Web.Services.Slugs;

namespace RSD.Web.Services.Content;

public sealed class TeamMemberService(AppDbContext Db, ISlugger Slugger, IPublicPageCache Cache)
    : SimpleContentService<TeamMember>(Db, Slugger, Cache), ITeamMemberService
{
    protected override string NaturalKeyOf(TeamMember entity) => entity.Name;
}
