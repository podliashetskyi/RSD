using System.Text.Json;

namespace RSD.Web.Services.Seo;

/// <summary>
/// Builds the sitewide schema.org @graph (ProfessionalService + WebSite).
/// Serialized exclusively through System.Text.Json — its default encoder escapes
/// angle brackets, so admin-authored strings can never break out of the ld+json script element.
/// </summary>
internal static class SiteJsonLdBuilder
{
    private const string SiteName = "RemSoft.Dev";
    private const string LegalName = "Remote Software Development";
    private const string Description =
        "Remote Software Development (RSD) builds future-ready digital systems: custom software, cloud solutions, and dedicated remote engineering teams.";

    private static readonly string[] KnowsAbout =
    [
        "Remote software development",
        "Software outsourcing",
        "Dedicated development teams",
        ".NET",
        "Blazor",
        "Cloud solutions",
        "Web development",
        "Mobile development",
    ];

    internal static string Build(string origin, IReadOnlyList<string> sameAs) =>
        JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["@context"] = "https://schema.org",
            ["@graph"] = new object[] { OrganizationNode(origin, sameAs), WebSiteNode(origin) },
        });

    private static Dictionary<string, object> OrganizationNode(string origin, IReadOnlyList<string> sameAs)
    {
        var node = new Dictionary<string, object>
        {
            ["@type"] = "ProfessionalService",
            ["@id"] = $"{origin}#organization",
            ["name"] = SiteName,
            ["legalName"] = LegalName,
            ["url"] = $"{origin}/",
            ["logo"] = AbsoluteUrl.Compose(origin, "images/logo.svg"),
            ["description"] = Description,
            ["inLanguage"] = "en",
            ["knowsAbout"] = KnowsAbout,
        };
        if (sameAs.Count > 0) node["sameAs"] = sameAs;
        return node;
    }

    private static Dictionary<string, object> WebSiteNode(string origin) => new()
    {
        ["@type"] = "WebSite",
        ["@id"] = $"{origin}#website",
        ["name"] = SiteName,
        ["url"] = $"{origin}/",
        ["inLanguage"] = "en",
        ["publisher"] = new Dictionary<string, object> { ["@id"] = $"{origin}#organization" },
    };
}
