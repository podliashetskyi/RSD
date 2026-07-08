using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RSD.Web.Components.Sections.Home;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Tests.Unit.Components;

public sealed class HeroSectionTests
{
    [Fact]
    public void RendersFirstThreeStatsAsTiles()
    {
        using var ctx = new BunitContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        // Deliberately inserted OUT of DisplayOrder so the test pins both OrderBy and Take(3):
        // "Years" (highest order) is first in the list but must be sorted last and dropped;
        // "Partners" (order 3) is last in the list but must be sorted into the top 3.
        // A regression that removed .OrderBy(...) would render "Years" and drop "Partners", failing below.
        var stats = new List<MissionStat>
        {
            new() { Slug = "years",      Label = "Years",      Number = "8",   Symbol = "+", DisplayOrder = 4 },
            new() { Slug = "projects",   Label = "Projects",   Number = "200", Symbol = "+", DisplayOrder = 1 },
            new() { Slug = "developers", Label = "Developers", Number = "60",  Symbol = "+", DisplayOrder = 2 },
            new() { Slug = "partners",   Label = "Partners",   Number = "50",  Symbol = "+", DisplayOrder = 3 },
        };
        ctx.Services.AddSingleton<IMissionStatService>(new FakeMissionStatService(stats));

        var cut = ctx.Render<HeroSection>();

        cut.Markup.Should().Contain("Projects").And.Contain("Developers").And.Contain("Partners");
        cut.Markup.Should().NotContain("Years"); // highest DisplayOrder → sorted last → dropped by Take(3)
    }
}
