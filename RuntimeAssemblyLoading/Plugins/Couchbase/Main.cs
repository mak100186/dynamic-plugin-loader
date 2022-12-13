using CouchbasePlugin.Configs;
using CouchbasePlugin.Services;

using MediatR;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        await _mediator.Publish(new MediatorNotification()
        {
            Subjects = new List<NotificationSubjects>() { NotificationSubjects.PostGreSQLNotificationHandler, NotificationSubjects.HostNotificationHandler },
            Event = NotificationEvents.PluginStopped,
            Arguments = new List<object>() { this }
        });

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
    public class CouchBaseNotificationHandler : INotificationHandler<MediatorNotification>
    {
        private readonly ILogger _logger;

        public CouchBaseNotificationHandler(ILogger<CouchBaseNotificationHandler> logger)
        {
            this._logger = logger;
        }

        public async Task Handle(MediatorNotification notification, CancellationToken cancellationToken)
        {
            this._logger.LogInformation($"Notification received by {this.GetType().Name}: {notification.Event} meant for {this.GetType().Name}");

            if (notification.Subjects.Select(x => x.ToString()).Contains(this.GetType().Name))
            {
                this._logger.LogInformation($"Notification accepted with {notification.Arguments.Count} args");
            }

            await Task.CompletedTask;
        }
    }
}

public class Registrant : IRegistrant
{
    public IServiceCollection Register(IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<IPlugin, Main>();
        services.AddSingleton<IAnotherDemoService, AnotherDemoService>();
        services.AddSingleton<IUnusedService, UnusedService>();

        return services;
    }
}