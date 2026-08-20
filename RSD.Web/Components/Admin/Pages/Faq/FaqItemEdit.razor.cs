#pragma warning disable S1144, S4487, S2933

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Admin.Pages.Faq;

public partial class FaqItemEdit(IFaqItemService Service, NavigationManager Nav)
{
    [Parameter] public Guid? Id { get; set; }
    private FaqItemInput Input { get; set; } = new();
    private string ErrorMessage { get; set; } = "";
    private bool IsCreate => Id is null;

    protected override async Task OnInitializedAsync()
    {
        if (Id is { } id) await LoadAsync(id);
    }

    private async Task LoadAsync(Guid id)
    {
        var existing = await Service.GetByIdAsync(id, CancellationToken.None);
        if (existing is null) { Nav.NavigateTo("/admin/faq"); return; }
        Input = FaqItemInput.From(existing);
    }

    private async Task SaveAsync()
    {
        try
        {
            var entity = Input.ToEntity(Id);
            var (ok, error) = await PersistAsync(entity);
            if (!ok) { ErrorMessage = error; return; }
            Nav.NavigateTo("/admin/faq");
        }
        catch (Exception ex) when (ex is not NavigationException)
        {
            ErrorMessage = $"Save failed: {ex.Message}";
        }
    }

    private async Task<(bool Ok, string Error)> PersistAsync(FaqItem entity)
    {
        if (IsCreate)
        {
            var created = await Service.CreateAsync(entity, CancellationToken.None);
            return (created.Ok, created.Error);
        }
        var updated = await Service.UpdateAsync(entity, CancellationToken.None);
        return (updated.Ok, updated.Error);
    }

    public sealed record class FaqItemInput
    {
        [Required]
        [StringLength(FieldLimits.FaqItem.Question)]
        public string Question { get; set; } = "";
        public string AnswerHtml { get; set; } = "";
        [StringLength(FieldLimits.FaqItem.Category)]
        public string Category { get; set; } = "";
        [StringLength(FieldLimits.FaqItem.OwnerSlug)]
        public string OwnerSlug { get; set; } = "";
        public ContentStatus Status { get; set; } = ContentStatus.Published;
        public int DisplayOrder { get; set; }
        [StringLength(FieldLimits.Slug)]
        public string Slug { get; set; } = "";

        public static FaqItemInput From(FaqItem f) => new()
        {
            Question = f.Question, AnswerHtml = f.AnswerHtml, Category = f.Category,
            OwnerSlug = f.OwnerSlug, Status = f.Status, DisplayOrder = f.DisplayOrder, Slug = f.Slug,
        };

        public FaqItem ToEntity(Guid? id) => new()
        {
            Id = id ?? Guid.NewGuid(), Slug = Slug,
            Question = Question, AnswerHtml = AnswerHtml, Category = Category,
            OwnerSlug = OwnerSlug, Status = Status, DisplayOrder = DisplayOrder,
        };
    }
}
