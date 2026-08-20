using System.Security.Claims;

namespace RSD.Web.Services.Mcp;

/// <summary>
/// The synthesized identity carried by gated MCP requests so the audit interceptor
/// attributes every write to "AI Agent" — no user row exists or is needed: the audit
/// UI renders the stored strings directly.
/// </summary>
public static class McpActor
{
    public const string UserId = "mcp-ai-agent";
    public const string Email = "AI Agent";

    public static ClaimsPrincipal BuildPrincipal() => new(new ClaimsIdentity(
    [
        new Claim(ClaimTypes.NameIdentifier, UserId),
        new Claim(ClaimTypes.Email, Email),
    ], authenticationType: "Mcp"));
}
