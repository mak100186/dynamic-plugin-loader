namespace PluginBase.Abstractions;
public interface IPluginCollection
{
    IEnumerable<IPlugin> Plugins { get; }
}

public class PluginCollection : IPluginCollection
{
    protected readonly List<IPlugin> _plugins;

    public PluginCollection(IEnumerable<IPlugin> plugins)
    {
        _plugins = plugins.ToList();
    }

    public IEnumerable<IPlugin> Plugins => _plugins;
}