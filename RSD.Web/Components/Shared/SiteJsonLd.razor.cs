#pragma warning disable S1144, S4487, S2933
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Common;
using RSD.Web.Services.Content;
using RSD.Web.Services.Seo;

namespace RSD.Web.Components.Shared;

public partial class SiteJsonLd(IOptions<SeoOptions> Seo, IHttpContextAccessor HttpAccessor, ISocialLinkService Socials)
{
    private string Json { get; set; } = "{}";

    // Blazor's SSR pipeline drops literal <script> elements authored inside components,
    // so the whole element is emitted as markup. Safe: Json comes from System.Text.Json,
    // whose encoder escapes angle brackets, so authored content cannot close the script tag.
    private MarkupString ScriptHtml => new($"<script type=\"application/ld+json\">{Json}</script>");

    protected override async Task OnInitializedAsync()
    {
        var links = await Socials.ListAsync(new ContentQuery(Status: ContentStatus.Published, PageSize: 100), CancellationToken.None);
        var sameAs = links.Where(IsCorroboratingProfile)
                          .OrderBy(s => s.DisplayOrder)
                          .Select(s => s.Href)
                          .ToList();
        Json = SiteJsonLdBuilder.Build(OriginValue, sameAs);
    }

    private static bool IsCorroboratingProfile(SocialLink link) =>
        link.Scope == SocialLinkScope.Footer
        && !string.IsNullOrWhiteSpace(link.Href)
        && LinkHrefValidator.IsValidSocialHref(link.Href);

    private string OriginValue
    {
        get
        {
            var request = HttpAccessor.HttpContext?.Request;
            return request is null ? new Origin(Seo.Value.BaseUrl).Value : RequestOrigin.Resolve(Seo.Value, request);
        }
    }
}
