using Newtonsoft.Json;
using System.Text.Json;
using JsonException = System.Text.Json.JsonException;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector.Extensions;

public static class StringExtensions
{
    public static bool IsValidJson(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        text = text.Trim();

        try
        {
            var doc = JsonDocument.Parse(text);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static string IndentJson(this string text)
    {
        var data = JsonConvert.DeserializeObject(text);
        return JsonConvert.SerializeObject(data, Formatting.Indented);
    }

    public static string CompactJson(this string text)
    {
        var data = JsonConvert.DeserializeObject(text);
        return JsonConvert.SerializeObject(data, Formatting.None);
    }
}