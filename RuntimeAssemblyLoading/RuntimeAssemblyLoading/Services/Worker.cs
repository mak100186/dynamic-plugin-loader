using MediatR;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PluginBase.Messages.Commands;

using RuntimeAssemblyLoading.Abstractions;
using RuntimeAssemblyLoading.Services.Options;

namespace RuntimeAssemblyLoading.Services;
public class Worker : BackgroundService
{
    private readonly IPluginLoader _pluginLoader;
    private readonly IPluginMigrator _pluginMigrator;
    private readonly ILogger _logger;
    private readonly StartUpOptions _options;
    private readonly IMediator _mediator;

    public Worker(IPluginLoader pluginLoader,
        IMediator mediator,
        IPluginMigrator pluginMigrator,
        ILogger<Worker> logger,
        IOptions<StartUpOptions> options)
    {
        _pluginLoader = pluginLoader;
        _pluginMigrator = pluginMigrator;
        _logger = logger;
        _options = options.Value;
        _mediator = mediator;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            if (!stoppingToken.IsCancellationRequested)
            {
                this._logger.LogInformation("Program will start");

                var pluginRunner = (_options.ShouldRunMigrationPathway) ? _pluginMigrator : _pluginLoader;

                await pluginRunner.StartPlugins();

                await pluginRunner.StopPlugins();

                await _mediator.Publish(new MediatorNotification()
                {
                    Subjects = new List<string>() { "PostGreSQLNotificationHandler", "CouchBaseNotificationHandler" },
                    Action = "Worker Fnished",
                    Arguments = new List<object>() { this }
                });

                this._logger.LogInformation("Program will terminate safely");

                Environment.Exit(0);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"{{@ex}}", ex);

            var innerException = ex.InnerException;
            while (innerException != null)
            {
                _logger.LogWarning($"{{@innerException}}", innerException);

                innerException = innerException.InnerException;
            }
        }
    }
}

//listener 1 : within the host 
public class HostNotificationHandler : INotificationHandler<MediatorNotification>
{
    private readonly ILogger _logger;

    public HostNotificationHandler(ILogger<HostNotificationHandler> logger)
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

//listener 2 : within the host
public class HostNotificationHandler2 : INotificationHandler<MediatorNotification>
{
    private readonly ILogger _logger;

    public HostNotificationHandler2(ILogger<HostNotificationHandler2> logger)
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