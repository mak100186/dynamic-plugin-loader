namespace PluginBase.Abstractions;

public interface IPluginHostApplication
{
    void PluginStartCompleted(IPlugin plugin);
    void PluginStopCompleted(IPlugin plugin);
}