using System.Reflection;
using System.Runtime.Loader;

using PluginBase.Abstractions;

namespace RuntimeAssemblyLoading.Dependency;

public class AssemblyLoader : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public Assembly? Assembly { get; }

    public AssemblyLoader(string pluginPath, string pluginAssemblyName)
    {
        string fullPathToPlugin = pluginPath + pluginAssemblyName;

        this._resolver = new(fullPathToPlugin);

        this.Assembly = this.LoadFromAssemblyName(AssemblyName.GetAssemblyName(fullPathToPlugin));
        if (this.Assembly == null)
        {
            throw new("Assembly not found");
        }
    }

    public void RegisterDependenciesFromAssembly(IServiceCollection services, IConfiguration configuration)
    {
        if (this.Assembly == null)
        {
            throw new ArgumentException($"Assembly does not exist");
        }

        foreach (Type? type in this.Assembly.GetTypes().Where(x => !x.IsInterface))
        {
            // Register all classes that implement the ISettings interface
            if (typeof(ISettings).IsAssignableFrom(type))
            {
                object? settings = Activator.CreateInstance(type);

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
        string? assemblyPath = this._resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
        {
            return this.LoadFromAssemblyPath(assemblyPath);
        }

        return null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        string? libraryPath = this._resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (libraryPath != null)
        {
            return this.LoadUnmanagedDllFromPath(libraryPath);
        }

        return IntPtr.Zero;
    }

    #endregion
}
