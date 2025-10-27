using System.Text.Json;
using System.Text.Json.Serialization;

namespace TypeScriptParserBackend.Serialization;

/// <summary>
/// Provides configured System.Text.Json serializer for our DTOs/models.
/// </summary>
public static class TypeScriptSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true
    };

    // PUBLIC_INTERFACE
    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);

    // PUBLIC_INTERFACE
    public static T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, Options);

    // PUBLIC_INTERFACE
    public static JsonSerializerOptions GetOptions() => Options;
}
