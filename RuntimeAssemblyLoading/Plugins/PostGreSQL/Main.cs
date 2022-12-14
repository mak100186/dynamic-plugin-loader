using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
    private readonly INotificationManager _notificationManager;

    public Main(ILogger<Main> logger, 
        INotificationManager notificationManager,
        IDemoService demoService)
    {
        this._logger = logger;
        this._demoService = demoService;
        this._notificationManager = notificationManager;
    }

    public async Task Migrate()
    {
        this.State = State.Starting;

        this._logger.LogInformation($"{this.Name} migrating");
        this._logger.LogInformation(this._demoService.DoWork(this.Name));

        await OnMigrateComplete();
    }

    public async Task OnMigrateComplete()
    {
        this.State = State.Started;

        this._logger.LogInformation($"{this.Name} has migrated");

        this._notificationManager.Send(new Notification()
        {
            To = "Couchbase",
            From = this.Name,
            Action = "Migration Completed"
        });

    }

    public async Task OnStarted()
    {
        this.State = State.Started;

        this._logger.LogInformation($"{this.Name} has started");

    }

    public async Task OnStopped()
    {
        this.State = State.Stopped;

        this._logger.LogInformation($"{this.Name} has stopped");

    }

    public async Task Start()
    {
        this.State = State.Starting;

        this._logger.LogInformation($"{this.Name} is starting");
        this._logger.LogInformation(this._demoService.DoWork(this.Name));

        await OnStarted();
    }

    public async Task Stop()
    {
        this.State = State.Stopping;

        this._logger.LogInformation($"{this.Name} is stopping");

        await OnStopped();
    }

    public void Receive(Notification notification)
    {
        this._logger.LogInformation($"Notification  intended for {notification.To} received by {this.Name} for action {notification.Action} sent by {notification.From}");
    }
}


public class Registrant : IRegistrant
{
    public IServiceCollection Register(IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<IPlugin, Main>();
        services.AddSingleton<IDemoService, DemoService>();

        return services;
    }
}