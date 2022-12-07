using Microsoft.Extensions.Logging;

using PluginBase.Abstractions;
using PluginBase.Enums;
using PostGreSQLPlugin.Services;

namespace PostGreSQLPlugin;
public class Main : IPlugin
{
    private readonly ILogger _logger;

    public string Name => $"PostGreSQL";

    public IServiceProvider ServiceProvider { get; set; } = null!;

    public State State { get; private set; }

    private readonly IDemoService _demoService;

    public Main(/*ILogger<Main> logger,*/ IDemoService demoService)
    {
        ////this._logger = logger;
        this._demoService = demoService;
    }

    public async Task Migrate()
    {
        this.State = State.Starting;

        ////this._logger.LogInformation($"{this.Name} migrating");
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

        //this._logger.LogInformation($"{this.Name} has started");

    }

    public async Task OnStopped()
    {
        this.State = State.Stopped;

        //this._logger.LogInformation($"{this.Name} has stopped");

    }

    public async Task Start()
    {
        this.State = State.Starting;

        ////this._logger.LogInformation($"{this.Name} is starting");
        Console.WriteLine(this._demoService.DoWork(this.Name));

        await OnStarted();
    }

    public async Task Stop()
    {
        this.State = State.Stopping;

        //this._logger.LogInformation($"{this.Name} is stopping");

        await OnStopped();
    }
}
