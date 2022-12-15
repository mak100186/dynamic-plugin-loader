using PluginBase.Abstractions;

namespace RuntimeAssemblyLoading.Services.Plugin;
public interface IPluginLoader
{
    Task StartPlugins();
    Task StopPlugins();
    Task<int> PluginCount();
    Task<bool> IsEmpty();
}

public class PluginLoader : BasePluginLoader, IPluginLoader
{
    public PluginLoader(ILogger<PluginLoader> logger, IPluginsWrapper plugins)
        : base(logger, plugins) { }

    public override async Task StartPlugins()
    {
        foreach (var pluginContext in _plugins.Plugins)
        {
            await pluginContext.Start();
        }
    }
}
