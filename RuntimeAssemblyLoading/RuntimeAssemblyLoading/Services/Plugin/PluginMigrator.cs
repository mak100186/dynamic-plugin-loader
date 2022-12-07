using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using RuntimeAssemblyLoading.Abstractions;

namespace RuntimeAssemblyLoading.Services.Plugin;

public class PluginMigrator : BasePluginLoader, IPluginMigrator
{
    public PluginMigrator(IConfiguration configuration, ILogger<PluginMigrator> logger, IServiceProvider serviceProvider) : base(configuration, logger, serviceProvider)
    {

    }

    public override void StartPlugins()
    {
        foreach (var pluginContext in Plugins)
        {
            pluginContext.Instance?.Migrate()
                .GetAwaiter().GetResult();
        }
    }
}
