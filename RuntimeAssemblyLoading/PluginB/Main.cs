using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using PluginBase.Abstractions;
using PluginBase.Enums;

namespace PluginB;
public class Main : IPlugin
{
    public string Name => $"Freebet-B";

    public IPluginHostApplication Application { get; set; } = null!;

    public IServiceProvider ServiceProvider { get; set; } = null!;

    public State State { get; private set; }

    public void Migrate(IConfiguration configuration)
    {
        this.State = State.Starting;

        GetLogger()?.LogInformation($"{this.Name} migrating");

        OnMigrateComplete();
    }

    public void OnMigrateComplete()
    {
        this.State = State.Started;

        GetLogger()?.LogInformation($"{this.Name} has migrated");

        this.Application.PluginMigrationCompleted(this);
    }

    public void OnStarted()
    {
        this.State = State.Started;

        GetLogger()?.LogInformation($"{this.Name} has started");

        this.Application.PluginStartCompleted(this);
    }

    public void OnStopped()
    {
        this.State = State.Stopped;

        GetLogger()?.LogInformation($"{this.Name} has stopped");

        this.Application.PluginStopCompleted(this);
    }

    public void Start(IConfiguration configuration)
    {
        this.State = State.Starting;

        GetLogger()?.LogInformation($"{this.Name} is starting");

        OnStarted();
    }

    public void Stop()
    {
        this.State = State.Stopping;

        GetLogger()?.LogInformation($"{this.Name} is stopping");

        OnStopped();
    }

    private ILogger? GetLogger()
    {
        return (ILogger<Main>?)ServiceProvider.GetService(typeof(ILogger<Main>));
    }
}
