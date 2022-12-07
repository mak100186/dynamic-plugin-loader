using Microsoft.Extensions.DependencyInjection;

using PluginBase.Enums;

namespace PluginBase.Abstractions;

public interface IPlugin
{
    string Name { get; }

    State State { get; }

    Task Migrate();

    Task OnMigrateComplete();

    Task Start();

    Task OnStarted();

    Task Stop();

    Task OnStopped();

    IPluginHostApplication Application { get; set; }

    IServiceProvider ServiceProvider { get; set; }
}
