using Microsoft.Extensions.Options;

using RuntimeAssemblyLoading.Abstractions;
using RuntimeAssemblyLoading.Services.Options;

namespace RuntimeAssemblyLoading.Services;
public class Worker : BackgroundService
{
    private readonly IPluginLoader _pluginLoader;
    private readonly IPluginMigrator _pluginMigrator;
    private readonly ILogger _logger;
    private readonly StartUpOptions _options;

    public Worker(IPluginLoader pluginLoader,
        IPluginMigrator pluginMigrator,
        ILogger<Worker> logger,
        IOptions<StartUpOptions> options)
    {
        _pluginLoader = pluginLoader;
        _pluginMigrator = pluginMigrator;
        _logger = logger;
        _options = options.Value;
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

                this._logger.LogInformation("Program will end");
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
