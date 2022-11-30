using System.Reflection;
using System.Runtime.Loader;

using PluginBase;

namespace RuntimeAssemblyLoading;
public class PluginContext : AssemblyLoadContext
{
    private readonly Assembly? assembly;
    private readonly Type? type;

    private readonly IReward? instance;
    private AssemblyDependencyResolver _resolver;

    public PluginContext(string pluginPath, string pluginAssemblyName, string typeName)
    {
        var fullPathToPlugin = pluginPath + pluginAssemblyName;
        _resolver = new AssemblyDependencyResolver(fullPathToPlugin);
        Console.WriteLine($"Loading assembly from: {fullPathToPlugin}");

        assembly = this.LoadFromAssemblyName(AssemblyName.GetAssemblyName(fullPathToPlugin));
        if (assembly == null)
        {
            throw new Exception("Assembly not found");
        }

        type = assembly.GetType(typeName);

        if (type == null)
        {
            throw new Exception("Type not found");
        }

        instance = Activator.CreateInstance(type) as IReward;

        if (instance == null)
        {
            throw new Exception("Instance could not be created not found");
        }
    }

    public IReward? GetInstance()
    {
        return instance;
    }

    public T? InvokeProperty<T>(string propertyName)
    {
        if (type != null && instance != null)
        {
            var method = type.GetProperty(propertyName);
            if (method == null)
            {
                throw new Exception("Method not found");
            }

            var returnValue = method.GetMethod?.Invoke(instance, null);
            if (returnValue == null)
            {
                throw new Exception("Data not found");
            }

            return (T)returnValue;
        }

        return default;
    }

    public T? InvokeMethod<T>(string propertyName)
    {
        if (type != null && instance != null)
        {
            var method = type.GetMethod(propertyName);
            if (method == null)
            {
                throw new Exception("Method not found");
            }

            var returnData = method.Invoke(instance, null);
            if (returnData == null)
            {
                throw new Exception("Data not found");
            }

            return (T)returnData;
        }

        return default;
    }

    #region Overrides
    //https://learn.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support
    protected override Assembly Load(AssemblyName assemblyName)
    {
        string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
        {
            return LoadFromAssemblyPath(assemblyPath);
        }

        return null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        string libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (libraryPath != null)
        {
            return LoadUnmanagedDllFromPath(libraryPath);
        }

        return IntPtr.Zero;
    }

    #endregion
}
