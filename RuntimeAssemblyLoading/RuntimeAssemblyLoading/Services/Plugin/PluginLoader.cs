using System.Reflection;

using Microsoft.Extensions.Configuration;

using PluginBase.Abstractions;

namespace RuntimeAssemblyLoading.Services.Plugin;

public class PluginLoader
{
    public ICollection<PluginDefinition> PluginDefinitions { get; private set; } = null!;
    public List<PluginContext> Plugins { get; private set; } = null!;

    private readonly string _assemblyPath;
    private readonly IConfiguration _configuration;

    public PluginLoader(IConfiguration configuration)
    {
        Plugins = new List<PluginContext>();
        PluginDefinitions = configuration
            .GetSection("appSettings:plugins")
            .GetChildren()
            .Select(x => new PluginDefinition { Name = x.GetValue<string>("name"), AssemblyName = x.GetValue<string>("assemblyName") })
            .ToList();

        var currentAssembly = Assembly.GetCallingAssembly();
        _assemblyPath = currentAssembly.Location.Replace(currentAssembly.ManifestModule.Name, string.Empty);

        _configuration = configuration;
    }

    public void ValidatePlugins()
    {
        Console.WriteLine("Validating plugin configurations");

        //uniqueness
        if (PluginDefinitions.Count == 0)
        {
            Console.WriteLine("Nothing to validate");
            return;
        }

        if (PluginDefinitions.Count != PluginDefinitions.Select(x => x.Name).Distinct().Count())
        {
            throw new Exception("Plugin type names must be unique");
        }

        if (PluginDefinitions.Count != PluginDefinitions.Select(x => x.AssemblyName).Distinct().Count())
        {
            throw new Exception("Plugin assembly names must be unique");
        }

        //file exists
        foreach (var pluginDefinition in PluginDefinitions)
        {
            var assemblyName = _assemblyPath + pluginDefinition.AssemblyName;

            if (File.Exists(assemblyName))
            {
                Console.WriteLine($"Specified file exists. \n[{assemblyName}]");
            }
            else
            {
                throw new Exception($"Specified file does not exist. \n[{assemblyName}]");
            }
        }

        Console.WriteLine("No validation issues found");
    }

    public void LoadPlugins(IPluginHostApplication pluginHostApplication)
    {
        foreach (var pluginDefinition in PluginDefinitions)
        {
            var pluginContext = new PluginContext(_assemblyPath, pluginDefinition.AssemblyName, pluginDefinition.Name, pluginHostApplication);
            Plugins.Add(pluginContext);

            Console.WriteLine($"Plugin Loaded: \n{pluginContext.InvokeProperty<string>("Name")}");
        }
    }

    public void StartPlugins()
    {
        foreach (var pluginContext in Plugins)
        {
            pluginContext.GetInstance()?.Start(_configuration);
        }
    }

    public void StopPlugins()
    {
        foreach (var pluginContext in Plugins)
        {
            pluginContext.GetInstance()?.Stop();
        }
    }

    public void UnloadPlugin(string pluginName)
    {
        var plugin = Plugins.FirstOrDefault(x => x.GetInstance()?.Name == pluginName);

        if (plugin != null)
        {
            Plugins.Remove(plugin);
        }
    }

    public int LoadedPluginCount()
    {
        return Plugins.Count();
    }

    public int UnloadedPluginCount()
    {
        return PluginDefinitions.Count - Plugins.Count;
    }

    public bool IsEmpty()
    {
        return this.LoadedPluginCount() == 0;
    }

    public void UnloadStoppedPlugins()
    {
        Plugins.RemoveAll(x => x.GetInstance()?.State == PluginBase.Enums.State.Stopped);
    }
}
