using PluginBase.Abstractions;

namespace RuntimeAssemblyLoading.Services.Plugin;

public abstract class BasePluginLoader
{
    protected readonly ILogger _logger;
    protected readonly IPluginsWrapper _plugins;

    public BasePluginLoader(ILogger logger, IPluginsWrapper plugins)
    {
        _logger = logger;
        _plugins = plugins;
    }

    public async Task StopPlugins()
    {
        foreach (var plugin in _plugins.Plugins)
        {
            await plugin.Stop();
        }
    }

    public async Task<int> PluginCount()
    {
        await Task.CompletedTask;

        return _plugins.Plugins.Count();
    }

    public async Task<bool> IsEmpty()
    {
        return await this.PluginCount() == 0;
    }

    public abstract Task StartPlugins();
}
