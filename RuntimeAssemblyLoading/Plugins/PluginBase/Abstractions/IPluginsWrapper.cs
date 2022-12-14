namespace PluginBase.Abstractions;
public interface IPluginsWrapper
{
    IEnumerable<IPlugin> Plugins { get; }
}

public class PluginsWrapper : IPluginsWrapper
{
    protected readonly List<IPlugin> _plugins;

    public PluginsWrapper(IEnumerable<IPlugin> plugins)
    {
        _plugins = plugins.ToList();
    }

    public IEnumerable<IPlugin> Plugins => _plugins;
}