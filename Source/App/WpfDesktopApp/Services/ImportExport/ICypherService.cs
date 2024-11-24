namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.ImportExport;

public interface ICypherService
{
    string Encode(string text);
    string Decode(string cypher);
}