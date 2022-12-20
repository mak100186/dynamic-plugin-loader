using PluginBase.Abstractions;
using PluginBase.Concrete;
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

    public string UniqueIdentifier => "PluginWithApi";

    public PluginState State { get; private set; }

    public IServiceProvider ServiceProvider { get; set; } = null!;

    public async Task Migrate()
    {
        this.State = PluginState.Starting;

        this._logger.LogInformation($"{this.UniqueIdentifier} migrating");

        await OnMigrateComplete();
    }

    public async Task OnMigrateComplete()
    {
        this.State = PluginState.Started;

        this._logger.LogInformation($"{this.UniqueIdentifier} has migrated");

        _service.Print(this.UniqueIdentifier);

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
