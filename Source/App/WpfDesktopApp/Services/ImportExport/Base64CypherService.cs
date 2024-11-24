using System.Text;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.ImportExport;

public class Base64CypherService : ICypherService
{
    public string Encode(string text)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
    }

    public string Decode(string cypher)
    {
        return Encoding.UTF8.GetString(Convert.FromBase64String(cypher));
    }
}