using System.Reflection;

using PluginBase.Abstractions;

namespace RuntimeAssemblyLoading.Services.Plugin;


public class PluginContext : AssemblyLoader
{
    public IPlugin? Instance { get; }

    public IPluginHostApplication PluginHostApplication { get => Instance.Application; set => Instance.Application = value; }
    public IServiceProvider ServiceProvider { get => Instance.ServiceProvider; set => Instance.ServiceProvider = value; }

    public PluginContext(string pluginPath, string pluginAssemblyName, string typeName) : base(pluginPath, pluginAssemblyName)
    {
        var type = Assembly.GetType(typeName);

        if (type == null)
        {
            throw new Exception("Type not found");
        }

        Instance = Activator.CreateInstance(type) as IPlugin;

        if (Instance == null)
        {
            throw new Exception("Instance could not be created not found");
        }
    }
}

//public void PluginContextSettings
//{

//}