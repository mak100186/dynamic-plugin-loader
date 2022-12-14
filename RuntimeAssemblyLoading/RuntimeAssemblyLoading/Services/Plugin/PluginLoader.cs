using PluginBase.Abstractions;

using RuntimeAssemblyLoading.Abstractions;

namespace RuntimeAssemblyLoading.Services.Plugin;

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
