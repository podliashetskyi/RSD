using System.Text.Json;
using RSD.Web.Data.Entities;

namespace RSD.Web.Services.Seo;

/// <summary>
/// Per-page schema.org nodes (BlogPosting / Service / Product), each referencing the
/// sitewide #organization anchor. System.Text.Json-serialized only (script-safe escaping).
/// </summary>
internal static class PageJsonLdBuilder
{
    internal static string BlogPosting(string origin, BlogPost post, TeamMember? author)
    {
        var node = WithContext(new Dictionary<string, object>
        {
            ["@type"] = "BlogPosting",
            ["headline"] = post.Title,
            ["mainEntityOfPage"] = $"{origin}/blog/{post.Slug}",
            ["datePublished"] = (post.PublishedAt ?? post.CreatedAt).ToString("yyyy-MM-dd"),
            ["dateModified"] = post.UpdatedAt.ToString("yyyy-MM-dd"),
            ["author"] = AuthorNode(origin, author),
            ["publisher"] = OrganizationRef(origin),
            ["inLanguage"] = "en",
        });
        AddIfPresent(node, "description", post.Summary);
        AddIfPresent(node, "image", AbsoluteUrl.Compose(origin, post.CoverImagePath));
        AddIfPresent(node, "articleSection", post.Category);
        if (post.Tags.Count > 0) node["keywords"] = post.Tags;
        return JsonSerializer.Serialize(node);
    }

    internal static string ServiceNode(string origin, Service service)
    {
        var node = WithContext(new Dictionary<string, object>
        {
            ["@type"] = "Service",
            ["name"] = service.Title,
            ["url"] = $"{origin}/services/{service.Slug}",
            ["provider"] = OrganizationRef(origin),
            ["inLanguage"] = "en",
        });
        AddIfPresent(node, "description", FirstNonEmpty(service.Summary, service.Description));
        return JsonSerializer.Serialize(node);
    }

    internal static string ProductNode(string origin, Product product)
    {
        var node = WithContext(new Dictionary<string, object>
        {
            ["@type"] = "Product",
            ["name"] = product.Name,
            ["url"] = $"{origin}/products/{product.Slug}",
        });
        AddIfPresent(node, "description", FirstNonEmpty(product.Summary, product.Description));
        AddIfPresent(node, "image", AbsoluteUrl.Compose(origin, product.CoverImagePath));
        return JsonSerializer.Serialize(node);
    }

    private static object AuthorNode(string origin, TeamMember? author)
    {
        if (author is null) return OrganizationRef(origin);
        var person = new Dictionary<string, object>
        {
            ["@type"] = "Person",
            ["name"] = author.Name,
        };
        AddIfPresent(person, "jobTitle", author.Role);
        var profiles = Profiles(author);
        if (profiles.Count > 0) person["sameAs"] = profiles;
        return person;
    }

    private static List<string> Profiles(TeamMember author) =>
        new[] { author.LinkedInUrl, author.XUrl, author.GitHubUrl }
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .ToList();

    private static Dictionary<string, object> WithContext(Dictionary<string, object> node)
    {
        node["@context"] = "https://schema.org";
        return node;
    }

    private static Dictionary<string, object> OrganizationRef(string origin) =>
        new() { ["@id"] = $"{origin}#organization" };

    private static void AddIfPresent(Dictionary<string, object> node, string key, string value)
    {
        if (!string.IsNullOrWhiteSpace(value)) node[key] = value;
    }

    private static string FirstNonEmpty(string first, string second) =>
        string.IsNullOrWhiteSpace(first) ? second : first;
}
