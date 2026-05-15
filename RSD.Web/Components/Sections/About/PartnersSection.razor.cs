#pragma warning disable S1144, S4487, S2933

using RSD.Web.Data.Entities;
using RSD.Web.Services.Content;

namespace RSD.Web.Components.Sections.About;

public partial class PartnersSection(IPartnerService Service)
{
    private IReadOnlyList<Partner> Partners { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        var list = await Service.ListAsync(new ContentQuery(Status: ContentStatus.Published, PageSize: 100), CancellationToken.None);
        Partners = [.. list.OrderBy(p => p.DisplayOrder)];
    }

    // Normalize ContactHref to a usable URL or null. Accepts http(s):// URLs,
    // mailto: URIs, and bare emails (auto-prefixed). A bare domain like
    // "google.com" returns null so the button is hidden, preventing a
    // relative link that resolves back to the current host.
    private static string? NormalizeContact(string raw)
    {
        var v = raw?.Trim() ?? "";
        if (v.Length == 0) return null;
        if (v.StartsWith("http://", StringComparison.OrdinalIgnoreCase)) return v;
        if (v.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) return v;
        if (v.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)) return v;
        if (v.Contains('@') && !v.Contains(' ')) return $"mailto:{v}";
        return null;
    }
}
