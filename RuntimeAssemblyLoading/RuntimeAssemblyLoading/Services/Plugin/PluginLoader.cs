using Microsoft.Extensions.Logging;

using PluginBase.Abstractions;

using RuntimeAssemblyLoading.Abstractions;

namespace RuntimeAssemblyLoading.Services.Plugin;

public class PluginLoader : BasePluginLoader, IPluginLoader
{    
    public PluginLoader(ILogger<PluginLoader> logger, IEnumerable<IPlugin> plugins) 
        : base(logger, plugins) { }

    public override async Task StartPlugins()
    {
        foreach (var pluginContext in _plugins)
        {
            await pluginContext.Start();
        }
    }
}
