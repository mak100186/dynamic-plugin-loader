using Microsoft.Extensions.Logging;

using PluginBase.Abstractions;

using RuntimeAssemblyLoading.Abstractions;

namespace RuntimeAssemblyLoading.Services.Plugin;

public class PluginMigrator : BasePluginLoader, IPluginMigrator
{
    public PluginMigrator(ILogger<PluginMigrator> logger, IPluginCollection plugins) 
        : base(logger, plugins) { }

    public override async Task StartPlugins()
    {
        foreach (var pluginContext in _plugins.Plugins)
        {
            await pluginContext.Migrate();
        }
    }
}
