using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PluginBase.Abstractions;
using PluginBase.Concrete;
using PluginBase.Enums;

namespace PluginC;
public class NotNamedMain : IPlugin, INotificationReceiver
{
    public string UniqueIdentifier => "PluginC";

    public IServiceProvider ServiceProvider { get; set; } = null!;

    public PluginState State { get; private set; }

    private readonly INotificationManager _notificationManager;
    private readonly ILogger _logger;

    public NotNamedMain(ILogger<NotNamedMain> logger, 
        INotificationManager notificationManager)
    {
        this._logger = logger;
        this._notificationManager = notificationManager;
    }

    public async Task Migrate()
    {
        this.State = PluginState.Starting;

        this._logger.LogInformation($"{this.UniqueIdentifier} migrating");

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
        
        mvcBuilder.Services.AddSingleton<IPlugin, NotNamedMain>();

        return mvcBuilder;
    }
}