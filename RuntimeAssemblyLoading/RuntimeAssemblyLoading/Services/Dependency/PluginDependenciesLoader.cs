using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RuntimeAssemblyLoading.Services.Dependency;
public static class PluginDependenciesLoader
{
    public static List<Assembly> LoadDependencies(IServiceCollection services, IConfiguration configuration)
    {
        var currentAssembly = Assembly.GetCallingAssembly();

        var assemblyPath = currentAssembly.Location.Replace(currentAssembly.ManifestModule.Name, string.Empty);

        var pluginNames = configuration.GetSection("appSettings:plugins").Get<string[]>();

        var assemblies = new List<Assembly>();

        foreach (var pluginName in pluginNames)
        {
            var assemblyLoader = new AssemblyLoader(assemblyPath, pluginName);
            assemblyLoader.RegisterDependenciesFromAssembly(services, configuration);

            assemblies.Add(assemblyLoader.Assembly);
        }

        return assemblies;
    }
}
