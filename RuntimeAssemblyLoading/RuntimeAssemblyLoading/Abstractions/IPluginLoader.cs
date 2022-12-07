using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PluginBase.Abstractions;

namespace RuntimeAssemblyLoading.Abstractions;

public interface IPluginLoader
{
    void ValidatePlugins();
    void LoadPlugins(IPluginHostApplication pluginHostApplication);
    void StartPlugins();
    void StopPlugins();
    void UnloadPlugin(string pluginName);
    int LoadedPluginCount();
    int UnloadedPluginCount();
    bool IsEmpty();
    void UnloadStoppedPlugins();
}
