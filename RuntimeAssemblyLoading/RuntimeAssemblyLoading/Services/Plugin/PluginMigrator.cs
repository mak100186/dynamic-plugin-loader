using System.Reflection;

using Microsoft.Extensions.Configuration;

namespace RuntimeAssemblyLoading.Services.Plugin;
public class PluginMigrator
{
	private ICollection<PluginDefinition> PluginDefinitions { get; set; } = null!;
    public List<PluginContext> Plugins { get; private set; } = new List<PluginContext>();

	private readonly string _assemblyPath;
	private readonly IConfiguration _configuration;

	public PluginMigrator(IConfiguration configuration)
	{
		var currentAssembly = Assembly.GetCallingAssembly();

        _assemblyPath = currentAssembly.Location.Replace(currentAssembly.ManifestModule.Name, string.Empty);
		_configuration = configuration;

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
}
