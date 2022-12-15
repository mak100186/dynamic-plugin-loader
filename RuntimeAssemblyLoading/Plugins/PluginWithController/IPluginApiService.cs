namespace PluginWithController;

public interface IPluginApiService
{
    string Print(string input);
}

public class PluginApiService : IPluginApiService
{
    public string Print(string text)
    {
        return text + text;
    }
}
