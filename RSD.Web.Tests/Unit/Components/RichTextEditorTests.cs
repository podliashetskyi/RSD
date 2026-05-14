using Bunit;
using FluentAssertions;
using RSD.Web.Components.Admin.Shared;

namespace RSD.Web.Tests.Unit.Components;

public sealed class RichTextEditorTests
{
    [Fact]
    public void OnHtmlChanged_PropagatesNewValueToParent()
    {
        using var ctx = new BunitContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var emitted = "";
        var cut = ctx.Render<RichTextEditor>(p => p
            .Add(x => x.Value, "<p>initial</p>")
            .Add(x => x.ValueChanged, html => emitted = html));

        cut.InvokeAsync(() => cut.Instance.OnHtmlChangedAsync("<p>updated</p>"));

        emitted.Should().Be("<p>updated</p>");
    }

    [Fact]
    public void RendersContainerAndOptionalLabelHint()
    {
        using var ctx = new BunitContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = ctx.Render<RichTextEditor>(p => p
            .Add(x => x.Label, "Intro")
            .Add(x => x.Hint, "Use H2/H3 for headings."));

        cut.Find("label").TextContent.Should().Contain("Intro");
        cut.Find("div.rich-text-editor").Should().NotBeNull();
        cut.Find("p").TextContent.Should().Contain("Use H2/H3 for headings.");
    }
}
