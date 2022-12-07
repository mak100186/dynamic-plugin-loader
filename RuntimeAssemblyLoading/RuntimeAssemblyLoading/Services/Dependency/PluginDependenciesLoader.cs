using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using RuntimeAssemblyLoading.Services.Plugin;

namespace RuntimeAssemblyLoading.Services.Dependency;
public static class PluginDependenciesLoader
{
    public static void LoadDependencies(IServiceCollection services, IConfiguration configuration)
    {
        var currentAssembly = Assembly.GetCallingAssembly();

        var assemblyPath = currentAssembly.Location.Replace(currentAssembly.ManifestModule.Name, string.Empty);

        var pluginDefinitions = configuration
            .GetSection("appSettings:plugins")
            .GetChildren()
            .Select(x => new PluginDefinition { Name = x.GetValue<string>("name"), AssemblyName = x.GetValue<string>("assemblyName") })
            .ToList();

        foreach (var pluginDefinition in pluginDefinitions)
        {
            var assemblyLoader = new AssemblyLoader(assemblyPath, pluginDefinition.AssemblyName);
            assemblyLoader.RegisterDependenciesFromAssembly(services, configuration);
        }
    }
}
