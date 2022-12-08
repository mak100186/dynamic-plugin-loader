using Microsoft.Extensions.Logging;

using PluginBase.Abstractions;

namespace RuntimeAssemblyLoading.Services.Plugin;

public abstract class BasePluginLoader
{
    protected readonly ILogger _logger;
    protected readonly List<IPlugin> _plugins;

    public BasePluginLoader(ILogger logger, IEnumerable<IPlugin> plugins)
    {
        _logger = logger;
        _plugins = plugins.ToList();
    }

    public void StopPlugins()
    {
        foreach (var plugin in _plugins)
        {
            plugin.Stop()
                .GetAwaiter().GetResult();
        }
    }

    public int PluginCount()
    {
        return _plugins.Count();
    }

    public bool IsEmpty()
    {
        return this.PluginCount() == 0;
    }

    public abstract void StartPlugins();
}
