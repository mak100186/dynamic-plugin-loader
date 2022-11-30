using System.Reflection;
using System.Runtime.Loader;

using PluginBase.Abstractions;

namespace RuntimeAssemblyLoading.Services.Plugin;

public class PluginContext : AssemblyLoadContext
{
    private readonly Assembly? assembly;
    private readonly Type? type;

    private readonly IPlugin? instance;
    private AssemblyDependencyResolver _resolver;

    public PluginContext(string pluginPath, string pluginAssemblyName, string typeName, IPluginHostApplication pluginHostApplication)
    {
        var fullPathToPlugin = pluginPath + pluginAssemblyName;
        Console.WriteLine($"Loading assembly from: \n{fullPathToPlugin}");

        _resolver = new AssemblyDependencyResolver(fullPathToPlugin);

        assembly = LoadFromAssemblyName(AssemblyName.GetAssemblyName(fullPathToPlugin));
        if (assembly == null)
        {
            throw new Exception("Assembly not found");
        }

        type = assembly.GetType(typeName);

        if (type == null)
        {
            throw new Exception("Type not found");
        }

        instance = Activator.CreateInstance(type) as IPlugin;

        if (instance == null)
        {
            throw new Exception("Instance could not be created not found");
        }

        instance.Application = pluginHostApplication;
    }

    public IPlugin? GetInstance()
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
