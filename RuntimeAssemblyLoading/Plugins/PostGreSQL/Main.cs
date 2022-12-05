using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using PluginBase.Abstractions;
using PluginBase.Enums;

namespace PostGreSQLPlugin;
public class Main : IPlugin
{
    public string Name => $"Freebet-B";

    public IPluginHostApplication Application { get; set; } = null!;

    public IServiceProvider ServiceProvider { get; set; } = null!;

    public State State { get; private set; }

    public async Task Migrate(IServiceProvider serviceProvider)
    {
        this.State = State.Starting;

        GetLogger()?.LogInformation($"{this.Name} migrating");

        await OnMigrateComplete();
    }

    public async Task OnMigrateComplete()
    {
        this.State = State.Started;

        GetLogger()?.LogInformation($"{this.Name} has migrated");

        await this.Application.PluginMigrationCompleted(this);
    }

    public async Task OnStarted()
    {
        this.State = State.Started;

        GetLogger()?.LogInformation($"{this.Name} has started");

        await this.Application.PluginStartCompleted(this);
    }

    public async Task OnStopped()
    {
        this.State = State.Stopped;

        GetLogger()?.LogInformation($"{this.Name} has stopped");

        await this.Application.PluginStopCompleted(this);
    }

    public async Task Start()
    {
        this.State = State.Starting;

        GetLogger()?.LogInformation($"{this.Name} is starting");

        await OnStarted();
    }

    public async Task Stop()
    {
        this.State = State.Stopping;

        GetLogger()?.LogInformation($"{this.Name} is stopping");

        await OnStopped();
    }

    private ILogger? GetLogger()
    {
        return (ILogger<Main>?)ServiceProvider.GetService(typeof(ILogger<Main>));
    }
}
