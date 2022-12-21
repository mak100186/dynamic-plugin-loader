using Microsoft.Extensions.Logging;
using PluginBase.Abstractions;

namespace PluginBase.Concrete;

public class PluginsWrapper : IPluginsWrapper
{
    protected readonly List<IPlugin> _plugins;
    protected readonly ILogger<PluginsWrapper> _logger;

    public PluginsWrapper(IEnumerable<IPlugin> plugins, ILogger<PluginsWrapper> logger)
    {
        this._logger = logger;
        this._plugins = plugins.ToList();
        this._logger.LogInformation($"{this._plugins.Count} plugins loaded");
    }

    public IEnumerable<IPlugin> Plugins => this._plugins;
}