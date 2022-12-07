using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using PluginBase.Abstractions;

using RuntimeAssemblyLoading.Abstractions;

namespace RuntimeAssemblyLoading.Services.Plugin;

public class PluginMigrator : BasePluginLoader, IPluginMigrator
{
    public PluginMigrator(ILogger<PluginMigrator> logger, IEnumerable<IPlugin> plugins) 
        : base(logger, plugins) { }

    public override void StartPlugins()
    {
        foreach (var pluginContext in _plugins)
        {
            pluginContext.Migrate()
                .GetAwaiter().GetResult();
        }
    }
}
