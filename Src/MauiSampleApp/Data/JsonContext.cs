using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

using MauiSampleApp.Models;

[JsonSerializable(typeof(Project))]
[JsonSerializable(typeof(ProjectTask))]
[JsonSerializable(typeof(ProjectsJson))]
[JsonSerializable(typeof(Category))]
[JsonSerializable(typeof(Tag))]

[JsonSerializable(typeof(ApplicationUser))]
[JsonSerializable(typeof(UserDevice))]
public partial class JsonContext : JsonSerializerContext
{
}

public static class JsonSerializerExtensions
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static string? ToJson<T>(this T? @object, JsonSerializerOptions? jsonSerializerOptions = null)
        => JsonSerializer.Serialize(@object, jsonSerializerOptions ?? _options);

    public static byte[]? ToJsonToUtf8Bytes<T>(this T? @object, JsonSerializerOptions? jsonSerializerOptions = null)
        => JsonSerializer.SerializeToUtf8Bytes(@object, jsonSerializerOptions ?? _options);

    public static T? FromJson<T>(this string? text, JsonSerializerOptions? jsonSerializerOptions = null)
        => string.IsNullOrWhiteSpace(text) ? default : JsonSerializer.Deserialize<T>(text, jsonSerializerOptions ?? _options);

    public static T? FromJson<T>(this Stream stream, JsonTypeInfo<T> jsonTypeInfo)
        => stream == null ? default : JsonSerializer.Deserialize<T>(stream, jsonTypeInfo);

    public static T? FromJson<T>(this byte[]? mByte, JsonSerializerOptions? jsonSerializerOptions = null)
        => mByte is null ? default : JsonSerializer.Deserialize<T>(mByte, jsonSerializerOptions ?? _options);
}