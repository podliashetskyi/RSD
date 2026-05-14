using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Estimates;

namespace RSD.Web.Endpoints;

public sealed record EstimateSubmitRequest
{
    [Required]
    public ProjectPlatform Platform { get; init; }

    [Required]
    public ProjectDomain Domain { get; init; }

    [Required]
    public ProjectComplexity Complexity { get; init; }

    [Required]
    public ProjectTimeline Timeline { get; init; }

    [Required, StringLength(200, MinimumLength = 1)]
    public string Name { get; init; } = "";

    [Required, EmailAddress, StringLength(320, MinimumLength = 3)]
    public string Email { get; init; } = "";

    [Required, StringLength(200, MinimumLength = 1)]
    public string Company { get; init; } = "";

    [Required, StringLength(8000, MinimumLength = 1)]
    public string Description { get; init; } = "";

    [StringLength(200)]
    public string Hp { get; init; } = "";
}

public static class EstimateSubmitEndpoint
{
    public const string RateLimitPolicy = "estimate-submit";
    public const string Route = "/api/estimate";

    public static IEndpointRouteBuilder MapEstimateSubmit(this IEndpointRouteBuilder app)
    {
        app.MapPost(Route, HandleAsync)
           .RequireRateLimiting(RateLimitPolicy)
           .DisableAntiforgery()
           .AllowAnonymous();
        return app;
    }

    internal static async Task<IResult> HandleAsync(
        [FromBody] EstimateSubmitRequest request,
        IProjectEstimateService service,
        CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(request.Hp)) return Results.Ok();
        var input = new ProjectEstimateInput(
            request.Platform, request.Domain, request.Complexity, request.Timeline,
            request.Name, request.Email, request.Company, request.Description);
        var result = await service.SubmitAsync(input, ct);
        return result.Ok ? Results.Ok() : Results.BadRequest(new { error = result.Error });
    }
}
