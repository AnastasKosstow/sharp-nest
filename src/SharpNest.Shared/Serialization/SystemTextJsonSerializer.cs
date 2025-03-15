using System.Text.Json;

namespace SharpNest.Shared.Serialization;

public sealed class SystemTextJsonSerializer : ISerializer
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    public string Serialize<T>(T value) where T : class
    {
        if (value == null)
        {
            return string.Empty;
        }

        if (typeof(T) == typeof(string))
        {
            return value.ToString();
        }

        var result = JsonSerializer.Serialize(value, jsonSerializerOptions);
        return result;
    }

    public T? Deserialize<T>(string value) where T : class
    {
        try
        {
            var result = JsonSerializer.Deserialize<T>(value, jsonSerializerOptions);
            return result;
        }
        catch
        {
            return default(T);
        }
    }
}
