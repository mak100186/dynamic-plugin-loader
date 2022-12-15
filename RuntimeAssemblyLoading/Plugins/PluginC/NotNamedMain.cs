using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using PluginBase.Abstractions;
using PluginBase.Enums;

namespace PluginC;
public class NotNamedMain : IPlugin, INotificationReceiver
{
    public string Name => "PluginC";

    public IServiceProvider ServiceProvider { get; set; } = null!;

    public State State { get; private set; }

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
        this.State = State.Starting;

        this._logger.LogInformation($"{this.Name} migrating");

        this._notificationManager.Send(new Notification()
        {
            To = "PluginWithApi",
            From = this.Name,
            Action = "Migration Started"
        });

        await OnMigrateComplete();
    }

    public async Task OnMigrateComplete()
    {
        this.State = State.Started;

        this._logger.LogInformation($"{this.Name} has migrated");

        this._notificationManager.Send(new Notification()
        {
            To = "PostGreSQL",
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
    public IServiceCollection Register(IServiceCollection services, IConfiguration config, IMvcBuilder mvcBuilder)
    {
        services.AddSingleton<IPlugin, NotNamedMain>();

        return services;
    }
}