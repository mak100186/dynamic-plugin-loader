using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PluginBase.Abstractions;
using PluginBase.Concrete;
using PluginBase.Enums;

using PostGreSQLPlugin.Services;

namespace PostGreSQLPlugin;
public class Main : IPlugin, INotificationReceiver
{
    private readonly ILogger _logger;

    public string UniqueIdentifier => $"PostGreSQL";

    public IServiceProvider ServiceProvider { get; set; } = null!;

    public PluginState State { get; private set; }

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
        this.State = PluginState.Starting;

        this._logger.LogInformation($"{this.UniqueIdentifier} migrating");
        this._logger.LogInformation(this._demoService.DoWork(this.UniqueIdentifier));

        await OnMigrateComplete();
    }

    public async Task OnMigrateComplete()
    {
        this.State = PluginState.Started;

        this._logger.LogInformation($"{this.UniqueIdentifier} has migrated");

        this._notificationManager.Send(new Notification()
        {
            To = "Couchbase",
            From = this.UniqueIdentifier,
            Action = "Migration Completed"
        });

    }

    public async Task OnStarted()
    {
        this.State = PluginState.Started;

        this._logger.LogInformation($"{this.UniqueIdentifier} has started");

    }

    public async Task OnStopped()
    {
        this.State = PluginState.Stopped;

        this._logger.LogInformation($"{this.UniqueIdentifier} has stopped");

    }

    public async Task Start()
    {
        this.State = PluginState.Starting;

        this._logger.LogInformation($"{this.UniqueIdentifier} is starting");
        this._logger.LogInformation(this._demoService.DoWork(this.UniqueIdentifier));

        await OnStarted();
    }

    public async Task Stop()
    {
        this.State = PluginState.Stopping;

        this._logger.LogInformation($"{this.UniqueIdentifier} is stopping");

        await OnStopped();
    }

    public void Receive(Notification notification)
    {
        this._logger.LogInformation($"Notification  intended for {notification.To} received by {this.UniqueIdentifier} for action {notification.Action} sent by {notification.From}");
    }
}


public class Registrant : IRegistrant
{
    public IMvcBuilder Register(IMvcBuilder mvcBuilder, IConfiguration? config)
    {
        mvcBuilder.Services.AddSingleton<IPlugin, Main>();
        mvcBuilder.Services.AddSingleton<IDemoService, DemoService>();

        return mvcBuilder;
    }
}