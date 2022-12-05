namespace PluginBase.Abstractions;

public interface IPluginHostApplication
{
    Task PluginStartCompleted(IPlugin plugin);
    Task PluginStopCompleted(IPlugin plugin);
    Task PluginMigrationCompleted(IPlugin plugin);
}