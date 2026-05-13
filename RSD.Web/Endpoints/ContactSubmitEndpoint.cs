using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using RSD.Web.Services.Content;

namespace RSD.Web.Endpoints;

public sealed record ContactSubmitRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string Name { get; init; } = "";

    [Required, EmailAddress, StringLength(320, MinimumLength = 3)]
    public string Email { get; init; } = "";

    [StringLength(500)]
    public string Subject { get; init; } = "";

    [Required, StringLength(8000, MinimumLength = 1)]
    public string Message { get; init; } = "";

    [StringLength(200)]
    public string Hp { get; init; } = "";
}

public static class ContactSubmitEndpoint
{
    public const string RateLimitPolicy = "contact-submit";
    public const string Route = "/api/contact";

    public static IEndpointRouteBuilder MapContactSubmit(this IEndpointRouteBuilder app)
    {
        app.MapPost(Route, HandleAsync)
           .RequireRateLimiting(RateLimitPolicy)
           .DisableAntiforgery()
           .AllowAnonymous();
        return app;
    }

    internal static async Task<IResult> HandleAsync(
        [FromBody] ContactSubmitRequest request,
        IContactSubmissionService service,
        CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(request.Hp)) return Results.Ok();
        var input = new ContactSubmissionInput(request.Name, request.Email, request.Subject, request.Message);
        var result = await service.SubmitAsync(input, ct);
        return result.Ok ? Results.Ok() : Results.BadRequest(new { error = result.Error });
    }
}
