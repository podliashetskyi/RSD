#pragma warning disable S1144, S4487, S2933

using System.Text.Json;
using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Admin.Shared;

public partial class AuditDiffViewer : ComponentBase
{
    [Parameter, EditorRequired] public string DiffJson { get; set; } = "{}";

    private List<ChangeRow>? Changes { get; set; }

    private static readonly JsonSerializerOptions PrettyOptions = new() { WriteIndented = true };

    protected override void OnParametersSet() => Changes = TryParse(DiffJson);

    private static List<ChangeRow>? TryParse(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("changes", out var changes)) return [];
            return [.. changes.EnumerateArray().Select(ToRow)];
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static ChangeRow ToRow(JsonElement element) => new(
        element.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "",
        FormatValue(element.TryGetProperty("before", out var b) ? b : default),
        FormatValue(element.TryGetProperty("after", out var a) ? a : default));

    private static string FormatValue(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.Null or JsonValueKind.Undefined => "—",
        JsonValueKind.String => value.GetString() ?? "",
        JsonValueKind.Object or JsonValueKind.Array => JsonSerializer.Serialize(value, PrettyOptions),
        _ => value.GetRawText(),
    };

    private sealed record ChangeRow(string Name, string Before, string After);
}
