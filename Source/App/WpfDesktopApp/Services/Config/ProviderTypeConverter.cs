namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.Config;

public static class ProviderTypeConverter
{
    public static string GetProviderStringRepresentation(Type provider)
    {
        return $"{provider.FullName}:{provider.Name}:{provider.Assembly.GetName().Version}";
    }

    public static string GetProviderStringRepresentationWithoutVersion(Type provider)
    {
        return $"{provider.FullName}:{provider.Name}";
    }
}