#pragma warning disable S1144, S4487, S2933

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Admin.Pages.Stats;

public partial class MissionStatEdit(IMissionStatService Service, NavigationManager Nav)
{
    [Parameter] public Guid? Id { get; set; }
    [SupplyParameterFromForm] private StatInput Input { get; set; } = new();
    private string ErrorMessage { get; set; } = "";
    private bool IsCreate => Id is null;

    protected override async Task OnInitializedAsync()
    {
        if (Id is { } id) await LoadAsync(id);
    }

    private async Task LoadAsync(Guid id)
    {
        var existing = await Service.GetByIdAsync(id, CancellationToken.None);
        if (existing is null) { Nav.NavigateTo("/admin/stats"); return; }
        Input = StatInput.From(existing);
    }

    private async Task SaveAsync()
    {
        var entity = Input.ToEntity(Id);
        var (ok, error) = await PersistAsync(entity);
        if (!ok) { ErrorMessage = error; return; }
        Nav.NavigateTo("/admin/stats");
    }

    private async Task<(bool Ok, string Error)> PersistAsync(MissionStat entity)
    {
        if (IsCreate)
        {
            var created = await Service.CreateAsync(entity, CancellationToken.None);
            return (created.Ok, created.Error);
        }
        var updated = await Service.UpdateAsync(entity, CancellationToken.None);
        return (updated.Ok, updated.Error);
    }

    public sealed record class StatInput
    {
        [Required]
        [StringLength(FieldLimits.MissionStat.Label)]
        public string Label { get; set; } = "";
        [StringLength(FieldLimits.MissionStat.Number)]
        public string Number { get; set; } = "";
        [StringLength(FieldLimits.MissionStat.Symbol)]
        public string Symbol { get; set; } = "";
        public ContentStatus Status { get; set; } = ContentStatus.Published;
        public int DisplayOrder { get; set; }
        [StringLength(FieldLimits.Slug)]
        public string Slug { get; set; } = "";

        public static StatInput From(MissionStat s) => new()
        {
            Label = s.Label, Number = s.Number, Symbol = s.Symbol,
            Status = s.Status, DisplayOrder = s.DisplayOrder, Slug = s.Slug,
        };

        public MissionStat ToEntity(Guid? id) => new()
        {
            Id = id ?? Guid.NewGuid(), Slug = Slug,
            Label = Label, Number = Number, Symbol = Symbol,
            Status = Status, DisplayOrder = DisplayOrder,
        };
    }
}
