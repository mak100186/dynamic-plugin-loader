﻿using PluginBase.Abstractions;
using PluginBase.Enums;

namespace PluginWithController;

public class Main : IPlugin, INotificationReceiver
{
    private readonly ILogger _logger;
    private readonly IPluginApiService _service;
    private readonly INotificationManager _notificationManager;

    public Main(ILogger<Main> logger,
        INotificationManager notificationManager,
        IPluginApiService service)
    {
        _logger = logger;
        _service = service;
        _notificationManager = notificationManager;
    }

    public string Name => "PluginWithApi";

    public State State { get; private set; }

    public IServiceProvider ServiceProvider { get; set; } = null!;

    public async Task Migrate()
    {
        this.State = State.Starting;

        this._logger.LogInformation($"{this.Name} migrating");

        await OnMigrateComplete();
    }

    public async Task OnMigrateComplete()
    {
        this.State = State.Started;

        this._logger.LogInformation($"{this.Name} has migrated");

        _service.Print(this.Name);

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
