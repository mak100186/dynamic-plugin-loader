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

    public async Task StopPlugins()
    {
        foreach (var plugin in _plugins)
        {
            await plugin.Stop();
        }
    }

    public async Task<int> PluginCount()
    {
        await Task.CompletedTask;

        return _plugins.Count();
    }

    public async Task<bool> IsEmpty()
    {
        return await this.PluginCount() == 0;
    }

    public abstract Task StartPlugins();
}
