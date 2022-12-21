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
        this._logger = logger;
        this._service = service;
        this._notificationManager = notificationManager;
    }

    public string UniqueIdentifier => "PluginWithApi";

    public PluginState State { get; private set; }

    public IServiceProvider ServiceProvider { get; set; } = null!;

    public async Task Migrate()
    {
        this.State = PluginState.Starting;

        this._logger.LogInformation($"{this.UniqueIdentifier} migrating");

        await this.OnMigrateComplete();
    }

    public async Task OnMigrateComplete()
    {
        this.State = PluginState.Started;

        this._logger.LogInformation($"{this.UniqueIdentifier} has migrated");

        this._service.Print(this.UniqueIdentifier);

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
