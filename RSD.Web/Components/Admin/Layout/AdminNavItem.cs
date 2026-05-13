namespace RSD.Web.Components.Admin.Layout;

public record AdminNavItem(string Label, string Href, string Group);

public static class AdminNav
{
    public const string ContentGroup = "Content";
    public const string OpsGroup = "Operations";

    public static readonly IReadOnlyList<AdminNavItem> Items =
    [
        new("Blog",         "/admin/blog",         ContentGroup),
        new("Cases",        "/admin/cases",        ContentGroup),
        new("Products",     "/admin/products",     ContentGroup),
        new("Services",     "/admin/services",     ContentGroup),
        new("Testimonials", "/admin/testimonials", ContentGroup),
        new("Team",         "/admin/team",         ContentGroup),
        new("Partners",     "/admin/partners",     ContentGroup),
        new("Values",       "/admin/values",       ContentGroup),
        new("Stats",        "/admin/stats",        ContentGroup),
        new("Tech stack",   "/admin/tech",         ContentGroup),
        new("Contact info", "/admin/contact-info", ContentGroup),
        new("Inbox",        "/admin/inbox",        OpsGroup),
        new("Media",        "/admin/media",        OpsGroup),
        new("Audit",        "/admin/audit",        OpsGroup),
        new("Trash",        "/admin/trash",        OpsGroup),
        new("Users",        "/admin/users",        OpsGroup),
    ];
}
