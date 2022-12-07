namespace RuntimeAssemblyLoading.Abstractions;

public interface IPluginLoader
{
    void StartPlugins();
    void StopPlugins();
    int PluginCount();
    bool IsEmpty();
}
