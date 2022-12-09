using MediatR;

using Microsoft.Extensions.Logging;

using PluginBase.Abstractions;
using PluginBase.Enums;
using PluginBase.Messages.Commands;

using PostGreSQLPlugin.Services;

namespace PostGreSQLPlugin;
public class Main : IPlugin
{
    private readonly ILogger _logger;
    private readonly IMediator _mediator;

    public string Name => $"PostGreSQL";

    public IServiceProvider ServiceProvider { get; set; } = null!;

    public State State { get; private set; }

    private readonly IDemoService _demoService;

    public Main(ILogger<Main> logger, IDemoService demoService, IMediator mediator)
    {
        this._logger = logger;
        this._demoService = demoService;
        this._mediator = mediator;
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
            Subjects = new List<string>() { "PostGreSQLNotificationHandler", "CouchBaseNotificationHandler", "HostNotificationHandler2" },
            Action = "PostGreSQL Plugin Fnished",
            Arguments = new List<object>() { this }
        });

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

    //notification 4 within plugin (host to plugin & pluging to plugin)
    public class PostGreSQLNotificationHandler : INotificationHandler<MediatorNotification>
    {
        private readonly ILogger _logger;

        public PostGreSQLNotificationHandler(ILogger<PostGreSQLNotificationHandler> logger)
        {
            this._logger = logger;
        }

        public async Task Handle(MediatorNotification notification, CancellationToken cancellationToken)
        {
            this._logger.LogInformation($"Notification received by {this.GetType().Name}: {notification.Action} meant for {this.GetType().Name}");

            if (notification.Subjects.Contains(this.GetType().Name))
            {
                this._logger.LogInformation($"Notification accepted with {notification.Arguments.Count} args");
            }

            await Task.CompletedTask;
        }
    }
}
