using CouchbasePlugin.Configs;
using CouchbasePlugin.Services;
using Microsoft.Extensions.Logging;

using PluginBase.Abstractions;
using PluginBase.Enums;

namespace CouchbasePlugin;
public class Main : IPlugin
{
    public string Name => "Couchbase";

    private readonly ILogger _logger;

    public IServiceProvider ServiceProvider { get; set; } = null!;

    public State State { get; private set; }

    private readonly IAnotherDemoService _demoService;

    public Main(IAnotherDemoService demoService, CouchbaseSettings settings)
    {
        ////this._logger = logger;
        this._demoService = demoService;
        Console.WriteLine($"printing settings received from host: {settings.Url}");
    }

    public async Task Migrate()
    {
        this.State = State.Starting;

        //this._logger.LogInformation($"{this.Name} migrating");
        Console.WriteLine(this._demoService.DoWork(this.Name));

        await OnMigrateComplete();
    }

    public async Task OnMigrateComplete()
    {
        this.State = State.Started;

        //this._logger.LogInformation($"{this.Name} has migrated");

    }

    public async Task OnStarted()
    {
        this.State = State.Started;

        ////this._logger.LogInformation($"{this.Name} has started");
        Console.WriteLine(this._demoService.DoWork(this.Name));
    }

    public async Task OnStopped()
    {
        this.State = State.Stopped;

        //this._logger.LogInformation($"{this.Name} has stopped");

    }

    public async Task Start()
    {
        this.State = State.Starting;

        //this._logger.LogInformation($"{this.Name} is starting");

        await OnStarted();
    }

    public async Task Stop()
    {
        this.State = State.Stopping;

        //this._logger.LogInformation($"{this.Name} is stopping");

        await OnStopped();
    }
}
