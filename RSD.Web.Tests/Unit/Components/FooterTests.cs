using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RSD.Web.Components.Layout;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Tests.Unit.Components;

public sealed class FooterTests
{
    [Fact]
    public void RendersContactPointsWithLinkAndPlainText()
    {
        using var ctx = new BunitContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var points = new List<ContactPoint>
        {
            new() { Slug = "email", Label = "Email", Lines = ["contactus@remsoft.dev"], Href = "mailto:contactus@remsoft.dev", IconPath = "images/icon-email.svg", DisplayOrder = 1 },
            new() { Slug = "addr",  Label = "Address", Lines = ["San Francisco, CA 94102", "Suite 100"], Href = "", IconPath = "", DisplayOrder = 2 },
        };
        ctx.Services.AddSingleton<ISocialLinkService>(new FakeSocialLinkService([]));
        ctx.Services.AddSingleton<IContactPointService>(new FakeContactPointService(points));

        var cut = ctx.Render<Footer>();

        var emailLink = cut.Find("a[href='mailto:contactus@remsoft.dev']");
        emailLink.TextContent.Should().Contain("contactus@remsoft.dev");
        emailLink.QuerySelector("img[src='images/icon-email.svg']").Should().NotBeNull();  // icon iff IconPath
        cut.Markup.Should().Contain("San Francisco, CA 94102").And.Contain("Suite 100");   // all lines render

        var addressItem = cut.FindAll("footer li").First(li => li.TextContent.Contains("San Francisco"));
        addressItem.QuerySelector("a").Should().BeNull();     // no Href -> plain text, not a link
        addressItem.QuerySelector("img").Should().BeNull();   // no IconPath -> no <img src="">
    }

    [Fact]
    public void DrawsPointsFromServiceOrderedByDisplayOrder()
    {
        using var ctx = new BunitContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        // Inserted out of order: rendering must sort by DisplayOrder.
        var points = new List<ContactPoint>
        {
            new() { Slug = "b", Label = "B", Lines = ["second"], DisplayOrder = 2 },
            new() { Slug = "a", Label = "A", Lines = ["first"],  DisplayOrder = 1 },
        };
        ctx.Services.AddSingleton<ISocialLinkService>(new FakeSocialLinkService([]));
        ctx.Services.AddSingleton<IContactPointService>(new FakeContactPointService(points));

        var cut = ctx.Render<Footer>();

        cut.Markup.IndexOf("first").Should().BeLessThan(cut.Markup.IndexOf("second"));
    }
}
