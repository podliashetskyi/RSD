using System.Text.Json;
using System.Text.Json.Serialization;
using ModelContextProtocol;

namespace RSD.Web.Services.Mcp.Tools;

/// <summary>
/// Payload parsing for MCP tools: web-style (camelCase-insensitive) with enums as strings,
/// matching the app's own JSON conventions. Article body blocks use their $type discriminators.
/// </summary>
internal static class McpJson
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    internal static T Deserialize<T>(JsonElement payload, string what)
    {
        try
        {
            return payload.Deserialize<T>(Options) ?? throw new McpException($"Empty {what} payload.");
        }
        catch (JsonException ex)
        {
            throw new McpException($"Could not parse the {what} payload: {ex.Message}");
        }
    }
}
