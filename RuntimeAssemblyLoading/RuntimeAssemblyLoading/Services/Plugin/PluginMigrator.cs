using PluginBase.Abstractions;

namespace RuntimeAssemblyLoading.Services.Plugin;

public interface IPluginMigrator : IPluginLoader { }

public class PluginMigrator : BasePluginLoader, IPluginMigrator
{
    public PluginMigrator(ILogger<PluginMigrator> logger, IPluginsWrapper plugins)
        : base(logger, plugins) { }

    public override async Task StartPlugins()
    {
        foreach (var pluginContext in _plugins.Plugins)
        {
            await pluginContext.Migrate();
        }
    }
}
