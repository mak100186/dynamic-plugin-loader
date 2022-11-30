using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using PluginBase.Abstractions;

using RuntimeAssemblyLoading.Services.Plugin;

namespace RuntimeAssemblyLoading.Services;
public class HostApplication : IHostedService, IPluginHostApplication
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly IConfiguration _configuration;
    private readonly PluginLoader _pluginLoader;

    public HostApplication(IHostApplicationLifetime hostApplicationLifetime, IConfiguration configuration)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _configuration = configuration;
        _pluginLoader = new PluginLoader(_configuration);
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
            _pluginLoader.StartPlugins();
            _pluginLoader.StopPlugins();

            _pluginLoader.UnloadStoppedPlugins();

            if (_pluginLoader.IsEmpty())
            {
                Console.WriteLine("Application is exiting properly");
                Environment.Exit(0);
            }
            else
            {
                throw new Exception("All plugins didnt unload successfully");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FATAL: \n{ex.Message}");

            var innerException = ex.InnerException;
            while (innerException != null)
            {
                Console.WriteLine($"FATAL: \n{innerException.Message}");

                innerException = innerException.InnerException;
            }
        }
    }

    private void OnStopping()
    {
        Console.WriteLine("OnStopping - Application");
    }

    private void OnStopped()
    {
        Console.WriteLine("OnStopped - Application");
    }

    public void PluginStartCompleted(IPlugin plugin)
    {
        Console.WriteLine($"PluginStartCompleted - {plugin.Name}");
    }

    public void PluginStopCompleted(IPlugin plugin)
    {
        Console.WriteLine($"PluginStopCompleted - {plugin.Name}");
    }
}
