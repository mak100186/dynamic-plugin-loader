using Microsoft.Extensions.Logging;
using PluginBase.Abstractions;

namespace PluginBase.Concrete;

public class PluginsWrapper : IPluginsWrapper
{
    protected readonly List<IPlugin> _plugins;
    protected readonly ILogger<PluginsWrapper> _logger;

    public PluginsWrapper(IEnumerable<IPlugin> plugins, ILogger<PluginsWrapper> logger)
    {
        _logger = logger;
        _plugins = plugins.ToList();
        _logger.LogInformation($"{_plugins.Count} plugins loaded");
    }

    public IEnumerable<IPlugin> Plugins => _plugins;
}