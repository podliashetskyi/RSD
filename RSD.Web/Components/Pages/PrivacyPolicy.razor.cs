#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;
using RSD.Web.Services.Seo;

namespace RSD.Web.Components.Pages;

public partial class PrivacyPolicy(IPrivacyPolicyService Service) : ComponentBase
{
    private Data.Entities.PrivacyPolicy? Entity { get; set; }

    private string SeoTitle => Entity is null ? "" : SeoFallbacks.Title(Entity.Seo, Entity.Title);
    private string SeoDescription => Entity is null ? "" : SeoFallbacks.Description(Entity.Seo, "", "");

    protected override async Task OnInitializedAsync()
    {
        var rows = await Service.ListAsync(new ContentQuery(Status: ContentStatus.Published, PageSize: 1), CancellationToken.None);
        Entity = rows.FirstOrDefault();
    }
}
