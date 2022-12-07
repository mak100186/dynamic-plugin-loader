using PluginBase.Abstractions;

namespace PostGreSQLPlugin.Services;

public interface IDemoService
{
    string DoWork(string pluginName);
}

public class DemoService : IDemoService, IInjectedDependency
{
    public DemoService(/*ILogger<DemoService> logger*/)
    {

    }
    public string DoWork(string pluginName)
    {
        return $"string from dependent service in {pluginName} plugin";
    }
}
