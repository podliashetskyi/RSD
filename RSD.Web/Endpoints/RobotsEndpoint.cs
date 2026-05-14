using RSD.Web.Services.Seo;

namespace RSD.Web.Endpoints;

public static class RobotsEndpoint
{
    public static IEndpointRouteBuilder MapRobots(this IEndpointRouteBuilder app)
    {
        app.MapGet("/robots.txt", HandleAsync).AllowAnonymous();
        return app;
    }

    internal static IResult HandleAsync(HttpContext http, IRobotsTxtProvider Provider)
    {
        var baseUrl = $"{http.Request.Scheme}://{http.Request.Host}";
        return Results.Text(Provider.Build(baseUrl), "text/plain");
    }
}
