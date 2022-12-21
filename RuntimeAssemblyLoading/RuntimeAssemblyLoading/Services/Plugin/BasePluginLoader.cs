using PluginBase.Abstractions;

namespace RuntimeAssemblyLoading.Services.Plugin;

public abstract class BasePluginLoader
{
    protected readonly ILogger _logger;
    protected readonly IPluginsWrapper _plugins;

    public BasePluginLoader(ILogger logger, IPluginsWrapper plugins)
    {
        this._logger = logger;
        this._plugins = plugins;
    }

    public async Task StopPlugins()
    {
        foreach (IPlugin plugin in this._plugins.Plugins)
        {
            await plugin.Stop();
        }
    }

    public async Task<int> PluginCount()
    {
        await Task.CompletedTask;

        return this._plugins.Plugins.Count();
    }

    public async Task<bool> IsEmpty()
    {
        return await this.PluginCount() == 0;
    }

    public abstract Task StartPlugins();
}
