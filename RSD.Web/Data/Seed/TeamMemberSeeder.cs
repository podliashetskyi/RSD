using RSD.Web.Data.Entities;
using RSD.Web.Services.Slugs;

namespace RSD.Web.Data.Seed;

public sealed class TeamMemberSeeder(AppDbContext Db, ISlugger Slugger) : SeederBase<TeamMember>(Db, Slugger)
{
    protected override Task<IReadOnlyList<TeamMember>> BuildAsync(CancellationToken ct)
    {
        IReadOnlyList<TeamMember> items =
        [
            Team("Floyd Miles",     "Front-End Engineer", "images/about/team/avatar-01.png", 1),
            Team("Ralph Edwards",   "Front-End Engineer", "images/about/team/avatar-02.png", 2),
            Team("Kathryn Murphy",  "Back-End Engineer",  "images/about/team/avatar-03.png", 3),
            Team("Robert Fox",      "Back-End Engineer",  "images/about/team/avatar-04.png", 4),
            Team("Kathryn Murphy",  "Back-End Engineer",  "images/about/team/avatar-05.png", 5),
            Team("Robert Fox",      "Back-End Engineer",  "images/about/team/avatar-06.png", 6),
            Team("Floyd Miles",     "Front-End Engineer", "images/about/team/avatar-07.png", 7),
            Team("Ralph Edwards",   "Front-End Engineer", "images/about/team/avatar-08.png", 8),
            Team("Kathryn Murphy",  "Back-End Engineer",  "images/about/team/avatar-09.png", 9),
            Team("Robert Fox",      "Back-End Engineer",  "images/about/team/avatar-10.png", 10),
            Team("Kathryn Murphy",  "Back-End Engineer",  "images/about/team/avatar-11.png", 11),
            Management("Bonnie Green",  "Front-end Developer", "images/about/management/portrait-bonnie-green.png",  1),
            Management("Robert Fox",    "Front-end Developer", "images/about/management/portrait-robert-fox.png",    2),
            Management("Eleanor Pena",  "Front-end Developer", "images/about/management/portrait-eleanor-pena.png",  3),
            Management("Esther Howard", "Front-end Developer", "images/about/management/portrait-esther-howard.png", 4),
        ];
        return Task.FromResult(items);
    }

    private static TeamMember Team(string name, string role, string avatar, int order) =>
        Build(name, role, avatar, order, isManagement: false);

    private static TeamMember Management(string name, string role, string photo, int order) =>
        Build(name, role, photo, order, isManagement: true);

    private static TeamMember Build(string name, string role, string avatar, int order, bool isManagement) => new()
    {
        Slug = isManagement ? $"management {name}" : name,
        Status = ContentStatus.Published,
        PublishedAt = DateTime.UtcNow,
        Name = name,
        Role = role,
        AvatarPath = avatar,
        DisplayOrder = order,
        IsManagement = isManagement,
    };
}
