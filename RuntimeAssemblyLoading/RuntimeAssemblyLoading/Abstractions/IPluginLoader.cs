namespace RuntimeAssemblyLoading.Abstractions;

public interface IPluginLoader
{
    Task StartPlugins();
    Task StopPlugins();
    Task<int> PluginCount();
    Task<bool> IsEmpty();
}
