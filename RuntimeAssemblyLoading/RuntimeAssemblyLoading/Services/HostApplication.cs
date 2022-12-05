﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PluginBase.Abstractions;

using RuntimeAssemblyLoading.Abstractions;
using RuntimeAssemblyLoading.Services.Plugin;

namespace RuntimeAssemblyLoading.Services;
public class HostApplication : IHostedService, IPluginHostApplication
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly IServiceProvider _serviceProvider;
    private readonly IPluginLoader _pluginLoader;
    private readonly ILogger _logger;
    private readonly StartUpOptions _options;

    public HostApplication(IHostApplicationLifetime hostApplicationLifetime, IServiceProvider serviceProvider, IPluginLoader pluginLoader, ILogger<HostApplication> logger, IOptions<StartUpOptions> options)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _serviceProvider = serviceProvider;
        _pluginLoader = pluginLoader;
        _logger = logger;
        _options = options.Value;
    }


    public Task StartAsync(CancellationToken cancellationToken)
    {
        _hostApplicationLifetime.ApplicationStarted.Register(OnStarted);
        _hostApplicationLifetime.ApplicationStopping.Register(OnStopping);
        _hostApplicationLifetime.ApplicationStopped.Register(OnStopped);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    private void OnStarted()
    {
        try
        {
            _pluginLoader.ValidatePlugins();
            _pluginLoader.LoadPlugins(this);

            if(_options.ShouldRunMigrationPathway)
            {
                _pluginLoader.Migrate();
            }
            else
            {
                _pluginLoader.StartPlugins();
            }
            
            _pluginLoader.StopPlugins();

            _pluginLoader.UnloadStoppedPlugins();

            if (_pluginLoader.IsEmpty())
            {
                _logger.LogInformation("Application is exiting properly");
                Environment.Exit(0);
            }
            else
            {
                throw new Exception("All plugins didnt unload successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"FATAL: {{@ex}}", ex);

            var innerException = ex.InnerException;
            while (innerException != null)
            {
                _logger.LogWarning($"FATAL: {{@innerException}}", innerException);

                innerException = innerException.InnerException;
            }
        }
    }

    private void OnStopping()
    {
        _logger.LogInformation("OnStopping - Application");
    }

    private void OnStopped()
    {
        _logger.LogInformation("OnStopped - Application");
    }

    public async Task PluginStartCompleted(IPlugin plugin)
    {
        _logger.LogInformation($"PluginStartCompleted - {plugin.Name}");

        await Task.CompletedTask;
    }

    public async Task PluginStopCompleted(IPlugin plugin)
    {
        _logger.LogInformation($"PluginStopCompleted - {plugin.Name}");
        
        await Task.CompletedTask;
    }

    public async Task PluginMigrationCompleted(IPlugin plugin)
    {
        _logger.LogInformation($"PluginMigrationCompleted - {plugin.Name}");
        
        await Task.CompletedTask;
    }
}
