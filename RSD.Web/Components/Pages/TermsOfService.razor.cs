#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Pages;

public partial class TermsOfService(ITermsOfServiceService Service) : ComponentBase
{
    private Data.Entities.TermsOfService? Entity { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var rows = await Service.ListAsync(new ContentQuery(Status: ContentStatus.Published, PageSize: 1), CancellationToken.None);
        Entity = rows.FirstOrDefault();
    }
}
