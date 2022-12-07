using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RuntimeAssemblyLoading.Services.Dependency;
public static class PluginDependenciesLoader
{
    public static void LoadDependencies(IServiceCollection services, IConfiguration configuration)
    {
        var currentAssembly = Assembly.GetCallingAssembly();

        var assemblyPath = currentAssembly.Location.Replace(currentAssembly.ManifestModule.Name, string.Empty);

        var pluginNames = configuration.GetSection("appSettings:plugins").Get<string[]>();

        foreach (var pluginName in pluginNames)
        {
            var assemblyLoader = new AssemblyLoader(assemblyPath, pluginName);
            assemblyLoader.RegisterDependenciesFromAssembly(services, configuration);
        }
    }
}
