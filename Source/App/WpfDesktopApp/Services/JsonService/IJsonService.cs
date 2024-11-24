namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.JsonService;

public interface IJsonService
{
    public T? Deserialize<T>(string json);

    public string Serialize<T>(T obj, bool indented = false, bool enumsAsString = false);
}