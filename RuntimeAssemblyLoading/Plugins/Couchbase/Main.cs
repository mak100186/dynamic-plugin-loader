using CouchbasePlugin.Configs;
using CouchbasePlugin.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PluginBase.Abstractions;
using PluginBase.Concrete;
using PluginBase.Enums;

namespace CouchbasePlugin;

public interface IPluginCouchbase : IPlugin { }

public class Main : IPluginCouchbase, INotificationReceiver
{
    public string UniqueIdentifier => "Couchbase";

    private readonly ILogger _logger;
    
    public PluginState State { get; private set; }

    private readonly IAnotherDemoService _demoService;
    private readonly INotificationManager _notificationManager;

    public Main(ILogger<Main> logger, 
        IAnotherDemoService demoService, 
        INotificationManager notificationManager,
        CouchbaseSettings settings)
    {
        this._logger = logger;
        this._demoService = demoService;
        this._notificationManager = notificationManager;
        this._logger.LogInformation($"printing settings received from host: {settings.Url}");
    }

    public async Task Migrate()
    {
        this.State = PluginState.Starting;

        this._logger.LogInformation($"{this.UniqueIdentifier} migrating");
        this._logger.LogInformation(this._demoService.DoWork(this.UniqueIdentifier));

        this._notificationManager.Send(new Notification()
        {
            To = "PluginWithApi",
            From = this.UniqueIdentifier,
            Action = "Migration Started"
        });

        await this.OnMigrateComplete();
    }

    public async Task OnMigrateComplete()
    {
        this.State = PluginState.Started;

        this._logger.LogInformation($"{this.UniqueIdentifier} has migrated");

        this._notificationManager.Send(new Notification()
        {
            To = "PostGreSQL",
            From = this.UniqueIdentifier,
            Action = "Migration Completed"
        });

        await Task.CompletedTask;
    }

    public async Task OnStarted()
    {
        this.State = PluginState.Started;

        this._logger.LogInformation($"{this.UniqueIdentifier} has started");

        await Task.CompletedTask;
    }

    public async Task OnStopped()
    {
        this.State = PluginState.Stopped;

        this._logger.LogInformation($"{this.UniqueIdentifier} has stopped");

        await Task.CompletedTask;
    }

    public async Task Start()
    {
        this.State = PluginState.Starting;

        this._logger.LogInformation(this._demoService.DoWork(this.UniqueIdentifier));
        this._logger.LogInformation($"{this.UniqueIdentifier} is starting");

        await this.OnStarted();
    }

    public async Task Stop()
    {
        this.State = PluginState.Stopping;

        this._logger.LogInformation($"{this.UniqueIdentifier} is stopping");

        await this.OnStopped();
    }

    public void Receive(BaseNotification baseNotification)
    {
        this._logger.LogInformation($"BaseNotification  intended for {baseNotification.To} received by {this.UniqueIdentifier} for action {baseNotification.Action} sent by {baseNotification.From}");
    }
}

public class Registrant : IRegistrant
{
    public IMvcBuilder Register(IMvcBuilder mvcBuilder, IConfiguration? config)
    {
        mvcBuilder.Services.AddSingleton<IPlugin, Main>();
        mvcBuilder.Services.AddSingleton<IAnotherDemoService, AnotherDemoService>();
        mvcBuilder.Services.AddSingleton<IUnusedService, UnusedService>();

        return mvcBuilder;
    }
}