using Microsoft.Extensions.Logging;

namespace PluginBase.Abstractions;
public interface IPluginsWrapper
{
    IEnumerable<IPlugin> Plugins { get; }
}

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