using Microsoft.Extensions.Logging;

using PluginBase.Abstractions;

namespace PostGreSQLPlugin.Services;

public interface IDemoService
{
    string DoWork(string pluginName);
}

public class DemoService : IDemoService, IInjectedDependency
{
    private readonly ILogger _logger;

    public DemoService(ILogger<DemoService> logger)
    {
        this._logger = logger;
    }
    public string DoWork(string pluginName)
    {
        this._logger.LogInformation("DoWork from PostGreSQLPlugin.Services.DemoService called");
        return $"string from dependent service in {pluginName} plugin";
    }
}
