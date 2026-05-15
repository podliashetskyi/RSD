#pragma warning disable S1144, S4487, S2933

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Admin.Pages.Filters;

public partial class FilterEdit(IFilterService Service, NavigationManager Nav)
{
    [Parameter] public Guid? Id { get; set; }

    private FilterInput Input { get; set; } = new();
    private string ErrorMessage { get; set; } = "";
    private bool IsCreate => Id is null;

    protected override async Task OnInitializedAsync()
    {
        if (Id is { } id)
        {
            await LoadAsync(id);
            return;
        }
        // Create flow — honour ?type=CaseTechTag from the list page's "+ New" button.
        var uri = Nav.ToAbsoluteUri(Nav.Uri);
        if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("type", out var raw)
            && Enum.TryParse<FilterType>(raw.ToString(), ignoreCase: true, out var type))
        {
            Input.Type = type;
        }
    }

    private async Task LoadAsync(Guid id)
    {
        var existing = await Service.GetByIdAsync(id, CancellationToken.None);
        if (existing is null) { Nav.NavigateTo("/admin/filters"); return; }
        Input = FilterInput.From(existing);
    }

    private async Task SaveAsync()
    {
        var entity = Input.ToEntity(Id);
        var (ok, error) = await PersistAsync(entity);
        if (!ok) { ErrorMessage = error; return; }
        Nav.NavigateTo("/admin/filters");
    }

    private async Task<(bool Ok, string Error)> PersistAsync(Filter entity)
    {
        if (IsCreate)
        {
            var created = await Service.CreateAsync(entity, CancellationToken.None);
            return (created.Ok, created.Error);
        }
        var updated = await Service.UpdateAsync(entity, CancellationToken.None);
        return (updated.Ok, updated.Error);
    }

    private static string TypeLabel(FilterType type) => type switch
    {
        FilterType.CaseIndustry => "Case industry",
        FilterType.CaseTechTag => "Case tech tag",
        FilterType.BlogCategory => "Blog category",
        FilterType.BlogTag => "Blog tag",
        _ => type.ToString(),
    };

    public sealed record class FilterInput
    {
        public FilterType Type { get; set; } = FilterType.CaseIndustry;
        [Required]
        [StringLength(FieldLimits.Filter.Label)]
        public string Label { get; set; } = "";
        public ContentStatus Status { get; set; } = ContentStatus.Published;
        public int DisplayOrder { get; set; }
        [StringLength(FieldLimits.Slug)]
        public string Slug { get; set; } = "";

        public static FilterInput From(Filter f) => new()
        {
            Type = f.Type,
            Label = f.Label,
            Status = f.Status,
            DisplayOrder = f.DisplayOrder,
            Slug = f.Slug,
        };

        public Filter ToEntity(Guid? id) => new()
        {
            Id = id ?? Guid.NewGuid(),
            Slug = Slug,
            Type = Type,
            Label = Label,
            Status = Status,
            DisplayOrder = DisplayOrder,
        };
    }
}
