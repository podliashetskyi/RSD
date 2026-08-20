using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RSD.Web.Components.Admin.Pages.Faq;
using RSD.Web.Components.Admin.Shared;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Tests.Unit.Components;

/// <summary>/admin/faq list + editor: the editing surface for FAQ content.</summary>
public sealed class FaqAdminTests
{
    private static FaqItem Faq(string q, int order, ContentStatus status = ContentStatus.Published) => new()
    {
        Slug = q.ToLowerInvariant().Replace(' ', '-'),
        Question = q,
        AnswerHtml = "<p>A</p>",
        Category = "Process",
        DisplayOrder = order,
        Status = status,
    };

    [Fact]
    public void List_RendersQuestions_InDisplayOrder_WithReorderControls()
    {
        using var ctx = new BunitContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.Services.AddSingleton<IFaqItemService>(new FakeFaqItemService(
        [
            Faq("Second question", 2),
            Faq("First question", 1),
        ]));
        ctx.Services.AddSingleton<IToastService>(new FakeToastService());

        var cut = ctx.Render<FaqItemList>();

        var cells = cut.FindAll("td").Select(td => td.TextContent).ToList();
        cells.Should().Contain(c => c.Contains("First question"));
        var markup = cut.Markup;
        markup.IndexOf("First question", StringComparison.Ordinal)
            .Should().BeLessThan(markup.IndexOf("Second question", StringComparison.Ordinal));
        cut.FindAll("button[aria-label='Move up']").Should().NotBeEmpty();
    }

    [Fact]
    public void List_MarksHomePinnedItems_WithAStar()
    {
        using var ctx = new BunitContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        var pinned = Faq("Pinned question", 1);
        pinned.ShowOnHome = true;
        ctx.Services.AddSingleton<IFaqItemService>(new FakeFaqItemService([pinned, Faq("Plain question", 2)]));
        ctx.Services.AddSingleton<IToastService>(new FakeToastService());

        var cut = ctx.Render<FaqItemList>();

        var pinnedRow = cut.FindAll("tbody tr").First(r => r.TextContent.Contains("Pinned question"));
        pinnedRow.QuerySelector("[aria-label='Shown on home page']").Should().NotBeNull();
        var plainRow = cut.FindAll("tbody tr").First(r => r.TextContent.Contains("Plain question"));
        plainRow.QuerySelector("[aria-label='Shown on home page']").Should().BeNull();
    }

    [Fact]
    public void Edit_HidesPageScoping_BehindTheAdvancedToggle()
    {
        using var ctx = new BunitContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.Services.AddSingleton<IFaqItemService>(new FakeFaqItemService([]));
        ctx.Services.AddSingleton<IToastService>(new FakeToastService());

        var cut = ctx.Render<FaqItemEdit>();

        // The page-scoping input must live only inside the collapsed advanced block,
        // so it can't be mistaken for the FAQ item's own slug during normal editing.
        var scoping = cut.FindAll("input[maxlength='200']");
        scoping.Should().HaveCount(1);
        cut.Find("details").InnerHtml.Should().Contain("maxlength=\"200\"");
    }

    [Fact]
    public void Edit_LoadsExistingItem_IntoTheForm()
    {
        using var ctx = new BunitContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        var item = Faq("How do we start?", 1);
        ctx.Services.AddSingleton<IFaqItemService>(new FakeFaqItemService([item]));
        ctx.Services.AddSingleton<IToastService>(new FakeToastService());

        var cut = ctx.Render<FaqItemEdit>(p => p.Add(x => x.Id, item.Id));

        cut.Find("input[maxlength='300']").GetAttribute("value").Should().Be("How do we start?");
        cut.Markup.Should().Contain("Answer");     // rich-text editor label
        cut.Markup.Should().Contain("Category");
        cut.FindAll("select").Should().NotBeEmpty(); // status select
    }
}

public sealed class FakeFaqItemService(IReadOnlyList<FaqItem> items)
    : FakeContentService<FaqItem>(items), IFaqItemService;

internal sealed class FakeToastService : IToastService
{
    public IReadOnlyList<ToastModel> Current => [];
    public event Action? Changed { add { } remove { } }
    public void Show(string message, ToastKind kind = ToastKind.Info) { }
    public void Dismiss(Guid id) { }
}
