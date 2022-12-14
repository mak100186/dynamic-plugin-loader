using System.Reflection;

using PluginBase.Abstractions;

namespace RuntimeAssemblyLoading.Services.Dependency;
public static class PluginDependenciesLoader
{
    public static List<Assembly> LoadDependencies(this IServiceCollection services, IConfiguration configuration, IMvcBuilder mvcBuilder)
    {
        var currentAssembly = Assembly.GetCallingAssembly();

        var assemblyPath = currentAssembly.Location.Replace(currentAssembly.ManifestModule.Name, string.Empty);

        var pluginNames = configuration.GetSection("appSettings:plugins").Get<string[]>();

        if (pluginNames == null || !pluginNames.Any())
        {
            throw new Exception("appSettings must define a plugin section listing all plugins to load");
        }

        var assemblies = new List<Assembly>();

        foreach (var pluginName in pluginNames)
        {
            var assemblyLoader = new AssemblyLoader(assemblyPath, pluginName);
            assemblyLoader.RegisterDependenciesFromAssembly(services, configuration);

            assemblies.Add(assemblyLoader.Assembly);

            LoadRegistrants(assemblyLoader.Assembly, services, configuration, mvcBuilder);
        }

        return assemblies;
    }

    public static void LoadRegistrants(Assembly pluginAssembly, IServiceCollection services, IConfiguration config, IMvcBuilder mvcBuilder)
    {
        var pluginRegistrantTypeName = pluginAssembly.GetTypes()
        .Single(t => t.GetInterfaces().Any(i => i.Name == "IRegistrant")).FullName;

        var pluginRegistrant = pluginAssembly.CreateInstance<IRegistrant>(pluginRegistrantTypeName!);

        pluginRegistrant.Register(services, config, mvcBuilder); // create services the host doesn't know about

        // a plugin can contribute more than one class
        foreach (var pluginType in pluginAssembly.GetTypes().Where(t => t.GetInterfaces().Any(i => i.Name == nameof(IPlugin))))
        {
            var pluginTypeCtor = pluginType.GetConstructors().Single(); // exactly one ctor per plugin class
            var pluginTypeCtorParamInfos = pluginTypeCtor
                .GetParameters()
                .OrderBy(pi => pi.Position);
        }
    }

     #region create instance from typeName

    public static object CreateInstance(this Assembly assembly, string typeName, params object[] parmArray)
    {
        if (parmArray.Length > 0)
        {
            var culture = System.Threading.Thread.CurrentThread.CurrentCulture;
            var activationAttrs = new object[] { };
            return assembly.CreateInstance(typeName, false, BindingFlags.CreateInstance, null, parmArray, culture, activationAttrs)!;
        }
        else
        {
            return assembly.CreateInstance(typeName)!;
        }
    }

    public static object CreateInstance(this Assembly assembly, string typeName, IEnumerable<object> parmList) => CreateInstance(assembly, typeName, parmList);

    public static T CreateInstance<T>(this Assembly assembly, string typeName, params object[] parmArray) => (T)CreateInstance(assembly, typeName, parmArray);

    public static T CreateInstance<T>(this Assembly assembly, string typeName, IEnumerable<object> parmList) => (T)CreateInstance(assembly, typeName, parmList.ToArray());

    #endregion



    #region create instance from type

    public static object? CreateInstance(this Type type, params object[] parmArray)
    {
        if (parmArray.Length > 0)
        {
            var culture = System.Threading.Thread.CurrentThread.CurrentCulture;
            var activationAttrs = new object[] { };
            return type.Assembly.CreateInstance(type.Name, false, BindingFlags.CreateInstance, null, parmArray, culture, activationAttrs);
        }
        else
        {
            return type.Assembly.CreateInstance(type.FullName!);
        }
    }

    public static object CreateInstance(this Type type, IEnumerable<object> parmList) => CreateInstance(type, parmList.ToArray())!;

    #endregion


    public static string GetVersionName(this Assembly assembly)
    {
        var x = assembly.GetName();
        return $"{x.Name}.{x.Version?.Major ?? 0}.{x.Version?.Minor ?? 0}";
    }

    public static bool Contains(this Assembly assy, string typeName)
    {
        return assy.GetExportedTypes().Any(t => t.FullName == typeName);
    }
}
