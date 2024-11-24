using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.JsonService;

public class NewtonsoftJsonService : IJsonService
{
    public T? Deserialize<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json);
    }

    public string Serialize<T>(T obj, bool indented = false, bool enumsAsString = false)
    {
        var settings = enumsAsString
            ? new JsonSerializerSettings
            {
                Converters =
                {
                    new StringEnumConverter()
                }
            }
            : new JsonSerializerSettings();

        return JsonConvert.SerializeObject(
            obj,
            indented ? Formatting.Indented : Formatting.None,
            settings);
    }
}