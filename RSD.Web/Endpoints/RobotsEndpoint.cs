using Microsoft.Extensions.Options;
using RSD.Web.Services.Seo;

namespace RSD.Web.Endpoints;

public static class RobotsEndpoint
{
    public static IEndpointRouteBuilder MapRobots(this IEndpointRouteBuilder app)
    {
        app.MapGet("/robots.txt", HandleAsync).AllowAnonymous();
        return app;
    }

    internal static IResult HandleAsync(HttpContext http, IRobotsTxtProvider Provider, IOptions<SeoOptions> Seo)
    {
        var baseUrl = RequestOrigin.Resolve(Seo.Value, http.Request);
        return Results.Text(Provider.Build(baseUrl), "text/plain");
    }
}
