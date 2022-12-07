using System.Reflection;
using System.Runtime.Loader;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PluginBase.Abstractions;

namespace RuntimeAssemblyLoading.Services.Dependency;

public class AssemblyLoader : AssemblyLoadContext
{
    private AssemblyDependencyResolver _resolver;

    public Assembly? Assembly { get; }

    public AssemblyLoader(string pluginPath, string pluginAssemblyName)
    {
        var fullPathToPlugin = pluginPath + pluginAssemblyName;

        _resolver = new AssemblyDependencyResolver(fullPathToPlugin);

        Assembly = LoadFromAssemblyName(AssemblyName.GetAssemblyName(fullPathToPlugin));
        if (Assembly == null)
        {
            throw new Exception("Assembly not found");
        }
    }

    public void RegisterDependenciesFromAssembly(IServiceCollection services, IConfiguration configuration)
    {
        if (Assembly == null)
            throw new ArgumentException($"Assembly does not exist");

        foreach (var type in Assembly.GetTypes().Where(x => !x.IsInterface))
        {
            // Register all classes that implement the IIntegration interface
            if (typeof(IPlugin).IsAssignableFrom(type))
            {
                // Add as a singleton as the Worker is a singleton and we'll only have one
                // Instance. If this would be a Controller or something else with clearly defined
                // scope that is not the lifetime of the application, use AddScoped.
                services.AddSingleton(typeof(IPlugin), type);
            } else

            // Register all classes that implement the ISettings interface
            if (typeof(ISettings).IsAssignableFrom(type))
            {
                var settings = Activator.CreateInstance(type);
                // appsettings.json or some other configuration provider should contain
                // a key with the same name as the type
                // e.g. "HttpIntegrationSettings": { ... }
                if (!configuration.GetSection(type.Name).Exists())
                {
                    // If it does not contain the key, throw an error
                    throw new ArgumentException($"Configuration does not contain key [{type.Name}]");
                }
                configuration.Bind(type.Name, settings);

                // Settings can be singleton as we'll only ever read it
                services.AddSingleton(type, settings);
            } else

            if (typeof(IInjectedDependency).IsAssignableFrom(type))
            {
                // Get all of the interfaces the type implements except
                // the IInjectedDependency
                var implementedInterfaces = type.GetInterfaces().Where(i => i != typeof(IInjectedDependency)).ToList();

                // Register as implemented interfaces
                if (implementedInterfaces.Any())
                {
                    foreach (var interfaceType in implementedInterfaces)
                    {
                        services.AddSingleton(interfaceType, type);
                    }
                }
                else
                {
                    services.AddSingleton(type);
                }
            }
            else if (type.IsClass && type.IsPublic) //register as singleton all classes where names match - convention
            {
                var typeName = type.Name;
                var interfaceName = "I" + typeName;

                var interfaces = Assembly.GetTypes().Where(x => x.IsInterface);
                var implementations = Assembly.GetTypes().Where(x => !x.IsInterface);

                if (interfaces.Select(x => x.Name).Contains(interfaceName))
                {
                    var interfaceType = interfaces.Single(x => x.Name == interfaceName);
                    services.AddSingleton(interfaceType, type);

                }
            }
        }
    }

    #region Overrides

    protected override Assembly Load(AssemblyName assemblyName)
    {
        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
        {
            return LoadFromAssemblyPath(assemblyPath);
        }

        return null;
    }

    #endregion
}
