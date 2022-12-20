﻿using CouchbasePlugin.Configs;
using CouchbasePlugin.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using PluginBase.Abstractions;
using PluginBase.Enums;

namespace CouchbasePlugin;

public interface IPluginCouchbase : IPlugin { }

public class Main : IPluginCouchbase, INotificationReceiver
{
    public string Name => "Couchbase";

    private readonly ILogger _logger;

    public IServiceProvider ServiceProvider { get; set; } = null!;

    public State State { get; private set; }

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
        this.State = State.Starting;

        this._logger.LogInformation($"{this.Name} migrating");
        this._logger.LogInformation(this._demoService.DoWork(this.Name));

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

        this._logger.LogInformation(this._demoService.DoWork(this.Name));
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
    public IMvcBuilder Register(IMvcBuilder mvcBuilder, IConfiguration? config)
    {
        mvcBuilder.Services.AddSingleton<IPlugin, Main>();
        mvcBuilder.Services.AddSingleton<IAnotherDemoService, AnotherDemoService>();
        mvcBuilder.Services.AddSingleton<IUnusedService, UnusedService>();

        return mvcBuilder;
    }
}