using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using RSD.Web.Components.Admin.Pages.Stats;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Tests.Unit.Components;

public sealed class MissionStatEditTests
{
    // Mimics static-SSR NavigationManager semantics: NavigateTo throws NavigationException,
    // which the framework (not component code) is supposed to catch and turn into a redirect.
    private sealed class ThrowingNavigationManager : NavigationManager
    {
        public ThrowingNavigationManager() => Initialize("http://localhost/", "http://localhost/admin/stats/x");
        protected override void NavigateToCore(string uri, NavigationOptions options) => throw new NavigationException(uri);
    }

    [Fact]
    public void SuccessfulSave_MustNotSwallowNavigationRedirect()
    {
        using var ctx = new BunitContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var stat = new MissionStat { Slug = "projects", Label = "Projects", Number = "200", Symbol = "+", DisplayOrder = 1 };
        ctx.Services.AddSingleton<IMissionStatService>(new FakeMissionStatService([stat]));
        ctx.Services.AddSingleton<NavigationManager>(new ThrowingNavigationManager());

        var cut = ctx.Render<MissionStatEdit>(p => p.Add(x => x.Id, stat.Id));

        try
        {
            cut.Find("form").Submit();
        }
        catch (Exception ex) when (Unwrap(ex) is NavigationException)
        {
            // Expected: the redirect control-flow exception escapes component code.
        }

        // The bug: a broad catch turned the framework's redirect into "Save failed: ... NavigationException".
        cut.Markup.Should().NotContain("Save failed");
    }

    private static Exception Unwrap(Exception ex) =>
        ex is AggregateException agg ? agg.GetBaseException() : ex;
}
