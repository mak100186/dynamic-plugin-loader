using System.Reflection;
using System.Runtime.Loader;

using PluginBase.Abstractions;

namespace RuntimeAssemblyLoading.Dependency;

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

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (libraryPath != null)
        {
            return LoadUnmanagedDllFromPath(libraryPath);
        }

        return IntPtr.Zero;
    }

    #endregion
}
