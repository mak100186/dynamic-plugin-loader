using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using PluginBase.Abstractions;

namespace RuntimeAssemblyLoading.Services.Plugin;

public abstract class BasePluginLoader
{
    protected ICollection<PluginDefinition> PluginDefinitions { get; set; } = null!;

    protected readonly IServiceProvider _serviceProvider;
    protected readonly ILogger _logger;
    protected readonly string _assemblyPath;
    protected readonly IConfiguration _configuration;

    public List<PluginContext> Plugins { get; private set; } = new List<PluginContext>();

    public BasePluginLoader(IConfiguration configuration, ILogger logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;

        var currentAssembly = Assembly.GetCallingAssembly();

        _assemblyPath = currentAssembly.Location.Replace(currentAssembly.ManifestModule.Name, string.Empty);

        PluginDefinitions = configuration
            .GetSection("appSettings:plugins")
            .GetChildren()
            .Select(x => new PluginDefinition { Name = x.GetValue<string>("name"), AssemblyName = x.GetValue<string>("assemblyName") })
            .ToList();
    }

    public void ValidatePlugins()
    {
        //uniqueness
        if (PluginDefinitions.Count == 0)
        {
            _logger.LogInformation("No plugin definitions found");
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

            if (!File.Exists(assemblyName))
            {
                throw new Exception($"Specified file does not exist. [{assemblyName}]");
            }
        }
    }

    public virtual void LoadPlugins(IPluginHostApplication pluginHostApplication)
    {
        foreach (var pluginDefinition in PluginDefinitions)
        {
            _logger.LogInformation($"Loading plugin:{_assemblyPath + pluginDefinition.AssemblyName}");



            var pluginContext = new PluginContext(_assemblyPath, pluginDefinition.AssemblyName, pluginDefinition.Name);
            pluginContext.PluginHostApplication = pluginHostApplication;
            pluginContext.ServiceProvider = _serviceProvider;

            Plugins.Add(pluginContext);

            _logger.LogInformation($"Plugin Loaded: {pluginContext.Instance.Name}");
        }
    }

    public void StopPlugins()
    {
        foreach (var pluginContext in Plugins)
        {
            pluginContext.Instance?.Stop()
                .GetAwaiter().GetResult();
        }
    }

    public void UnloadPlugin(string pluginName)
    {
        var plugin = Plugins.FirstOrDefault(x => x.Instance?.Name == pluginName);

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
        Plugins.RemoveAll(x => x.Instance?.State == PluginBase.Enums.State.Stopped);
    }

    public abstract void StartPlugins();
}
