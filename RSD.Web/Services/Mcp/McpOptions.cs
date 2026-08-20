namespace RSD.Web.Services.Mcp;

/// <summary>
/// Content MCP server configuration. Disabled by default and absent from appsettings.json:
/// local development enables it in appsettings.Development.json; Docker via the gitignored
/// compose override. Never enable on a public listener — see McpRequestGate.
/// </summary>
public sealed record class McpOptions
{
    public const string SectionName = "Mcp";

    public bool Enabled { get; set; }

    /// <summary>The dedicated listener port the MCP endpoint answers on (0 = never).</summary>
    public int Port { get; set; }

    /// <summary>Origin used to build clickable preview links, e.g. "http://localhost:8082".</summary>
    public string PreviewBaseUrl { get; set; } = "";
}
