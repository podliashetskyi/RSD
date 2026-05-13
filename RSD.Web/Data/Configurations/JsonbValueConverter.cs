using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace RSD.Web.Data.Configurations;

/// <summary>
/// Symmetric JSON converter for jsonb-backed value objects. The matching
/// comparer treats two payloads as equal when their serialized form matches,
/// which is the only way EF can detect changes to mutable record graphs.
/// </summary>
public static class JsonbValueConverter
{
    public static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
    };

    public static ValueConverter<T, string> ConverterFor<T>() where T : class, new() => new(
        v => JsonSerializer.Serialize(v, SerializerOptions),
        v => string.IsNullOrEmpty(v) ? new T() : JsonSerializer.Deserialize<T>(v, SerializerOptions) ?? new T());

    public static ValueComparer<T> ComparerFor<T>() where T : class, new() => new(
        (a, b) => JsonSerializer.Serialize(a, SerializerOptions) == JsonSerializer.Serialize(b, SerializerOptions),
        v => JsonSerializer.Serialize(v, SerializerOptions).GetHashCode(),
        v => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(v, SerializerOptions), SerializerOptions) ?? new T());
}
