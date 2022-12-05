using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using PluginBase.Abstractions;
using RuntimeAssemblyLoading.Abstractions;

namespace RuntimeAssemblyLoading.Services.Plugin;

public class PluginLoader : IPluginLoader
{
    private ICollection<PluginDefinition> PluginDefinitions { get; set; } = null!;
    public List<PluginContext> Plugins { get; private set; } = new List<PluginContext>();

    private readonly string _assemblyPath;
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;

    public PluginLoader(IConfiguration configuration, ILogger<PluginLoader> logger, IServiceProvider serviceProvider)
    {
        var currentAssembly = Assembly.GetCallingAssembly();

        _assemblyPath = currentAssembly.Location.Replace(currentAssembly.ManifestModule.Name, string.Empty);
        _logger = logger;
        _configuration = configuration;
        _serviceProvider = serviceProvider;

        PluginDefinitions = configuration
            .GetSection("appSettings:plugins")
            .GetChildren()
            .Select(x => new PluginDefinition { Name = x.GetValue<string>("name"), AssemblyName = x.GetValue<string>("assemblyName") })
            .ToList();
    }

    public void ValidatePlugins()
    {        
        _logger.LogInformation("Validating plugin configurations");

        //uniqueness
        if (PluginDefinitions.Count == 0)
        {
            _logger.LogInformation("Nothing to validate");
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
                _logger.LogInformation($"Specified file exists. [{assemblyName}]");
            }
            else
            {
                throw new Exception($"Specified file does not exist. [{assemblyName}]");
            }
        }

        _logger.LogInformation("No validation issues found");
    }

    public void LoadPlugins(IPluginHostApplication pluginHostApplication)
    {
        foreach (var pluginDefinition in PluginDefinitions)
        {
            _logger.LogInformation($"Loading plugin:{_assemblyPath + pluginDefinition.AssemblyName}");

            var pluginContext = new PluginContext(_assemblyPath, pluginDefinition.AssemblyName, pluginDefinition.Name, pluginHostApplication, _serviceProvider);
            Plugins.Add(pluginContext);

            _logger.LogInformation($"Plugin Loaded: {pluginContext.InvokeProperty<string>("Name")}");
        }
    }

    public void StartPlugins()
    {
        foreach (var pluginContext in Plugins)
        {
            pluginContext.GetInstance()?.Start()
                .GetAwaiter().GetResult();
        }
    }

    public void Migrate()
    {
        foreach (var pluginContext in Plugins)
        {
            pluginContext.GetInstance()?.Migrate(this._serviceProvider)
                .GetAwaiter().GetResult();
        }
    }

    public void StopPlugins()
    {
        foreach (var pluginContext in Plugins)
        {
            pluginContext.GetInstance()?.Stop()
                .GetAwaiter().GetResult();
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
