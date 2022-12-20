using PluginBase.Enums;

namespace PluginBase.Abstractions;

public interface IPlugin
{
    string UniqueIdentifier { get; }

    PluginState State { get; }

    Task Migrate();

    Task OnMigrateComplete();

    Task Start();

    Task OnStarted();

    Task Stop();

    Task OnStopped();    

}
