using CouchbasePlugin.Configs;
using CouchbasePlugin.Services;

using MediatR;

using Microsoft.Extensions.Logging;

using PluginBase.Abstractions;
using PluginBase.Enums;
using PluginBase.Messages.Commands;

namespace CouchbasePlugin;
public class Main : IPlugin
{
    public string Name => "Couchbase";

    private readonly ILogger _logger;
    private readonly IMediator _mediator;

    public IServiceProvider ServiceProvider { get; set; } = null!;

    public State State { get; private set; }

    private readonly IAnotherDemoService _demoService;

    public Main(ILogger<Main> logger, IAnotherDemoService demoService, CouchbaseSettings settings, IMediator mediator)
    {
        this._logger = logger;
        this._demoService = demoService;
        this._mediator = mediator;
        this._logger.LogInformation($"printing settings received from host: {settings.Url}");
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

        await _mediator.Publish(new MediatorNotification() { Action = "Couchbase Plugin Fnished" });

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

    //listener 3: within plugin (host to plugin comms)
    public class MediatorNotificationHandler3 : INotificationHandler<MediatorNotification>
{
    private readonly ILogger _logger;

    public MediatorNotificationHandler3(ILogger<MediatorNotificationHandler3> logger)
    {
        this._logger = logger;
    }

    public async Task Handle(MediatorNotification notification, CancellationToken cancellationToken)
    {
        this._logger.LogInformation($"Notification 3 received: {notification.Action}");

        await Task.CompletedTask;
    }
}
}
