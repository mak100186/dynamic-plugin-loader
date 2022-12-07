using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using PluginBase.Abstractions;

using RuntimeAssemblyLoading.Abstractions;

namespace RuntimeAssemblyLoading.Services.Plugin;

public class PluginLoader : BasePluginLoader, IPluginLoader
{    
    public PluginLoader(IConfiguration configuration, ILogger<PluginLoader> logger, IServiceProvider serviceProvider) : base(configuration, logger, serviceProvider) 
    {
       
    }

    public override void StartPlugins()
    {
        foreach (var pluginContext in Plugins)
        {
            pluginContext.Instance?.Start()
                .GetAwaiter().GetResult();
        }
    }
}
