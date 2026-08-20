namespace RSD.Web.Services.Seo;

/// <summary>
/// BreadcrumbList schema for detail pages (Home → Section → Item). Deliberately
/// schema-only: the visible UI keeps its Back link (design decision, 2026-08-20).
/// </summary>
internal static class BreadcrumbJsonLdBuilder
{
    internal static string Build(string origin, string sectionName, string sectionPath, string itemName, string itemPath) =>
        System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "BreadcrumbList",
            ["itemListElement"] = new[]
            {
                ListItem(1, "Home", $"{origin}/"),
                ListItem(2, sectionName, AbsoluteUrl.Compose(origin, sectionPath)),
                ListItem(3, itemName, AbsoluteUrl.Compose(origin, itemPath)),
            },
        });

    private static Dictionary<string, object> ListItem(int position, string name, string url) => new()
    {
        ["@type"] = "ListItem",
        ["position"] = position,
        ["name"] = name,
        ["item"] = url,
    };
}
