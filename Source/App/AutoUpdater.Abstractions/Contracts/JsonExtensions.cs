using System.Text.Json;

namespace Rachkov.InspectaQueue.AutoUpdater.Abstractions.Contracts;

public static class JsonExtensions
{
    public static T? ParseJson<T>(this string text)
    {
        return JsonSerializer.Deserialize<T>(text);
    }

    public static string ToJson<T>(this T obj, bool indented = true)
    {
        return JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = indented });
    }
}