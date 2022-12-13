using Microsoft.Extensions.Logging;

namespace CouchbasePlugin.Services;

public interface IAnotherDemoService
{
    string DoWork(string pluginName);
}


public class AnotherDemoService : IAnotherDemoService
{
    public AnotherDemoService(ILogger<AnotherDemoService> logger)
    {
        logger.LogInformation("log from inside another demo service");
    }

    public string DoWork(string pluginName)
    {
        return $"string from dependent service in {pluginName} plugin";
    }
}
