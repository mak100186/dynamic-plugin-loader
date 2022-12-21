using System.Reflection;
using PluginBase.Abstractions;
using PluginBase.Concrete;
using RuntimeAssemblyLoading.Dependency;
using RuntimeAssemblyLoading.Helpers;
using RuntimeAssemblyLoading.Services;
using RuntimeAssemblyLoading.Services.Plugin;

using Serilog;

using Unibet.Infrastructure.Caching.CouchbaseV7;

namespace RuntimeAssemblyLoading;
public static class ServiceRegistrations
{
    public static void ConfigureServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        IMvcBuilder mvcBuilder = services.AddMvc(c =>
        {
            c.SuppressAsyncSuffixInActionNames = false;
        })
        .AddNewtonsoftJson();

        services.AddKspCouchbase7(config).EnableMigrations(config);

        services.AddSingleton<IDateTimeService, DateTimeService>();

        services.AddSingleton<IPluginsWrapper, PluginsWrapper>();
        services.AddSingleton<IPluginLoader, PluginLoader>();
        services.AddSingleton<IPluginMigrator, PluginMigrator>();

        services.AddSingleton<INotificationManager, NotificationManager>();

        services.LoadDependencies(config, mvcBuilder);

        services.AddHostedService<Worker>();
    }

    public static IHostBuilder ConfigureSerilog(this IHostBuilder builder)
    {
        return builder.UseSerilog((ctx, conf) =>
        {
            conf.ReadFrom.Configuration(ctx.Configuration);
            conf.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}{NewLine}");
            conf.WriteTo.File(path: "\\bin\\Debug\\net6.0\\log-.txt", outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}{NewLine}", rollingInterval: RollingInterval.Day);
        });
    }

    public static void LoadDependencies(this IServiceCollection services, IConfiguration configuration, IMvcBuilder mvcBuilder)
    {
        Assembly currentAssembly = Assembly.GetCallingAssembly();

        string assemblyPath = currentAssembly.Location.Replace(currentAssembly.ManifestModule.Name, string.Empty);

        string[] pluginNames = configuration.GetSection("appSettings:plugins").Get<string[]>();

        if (pluginNames == null || !pluginNames.Any())
        {
            throw new("appSettings must define a plugin section listing all plugins to load");
        }

        foreach (string pluginName in pluginNames)
        {
            AssemblyLoader assemblyLoader = new(assemblyPath, pluginName);
            assemblyLoader.RegisterDependenciesFromAssembly(services, configuration);

            string? pluginRegistrantTypeName = assemblyLoader.Assembly!.GetTypes()
                .Single(t => t.GetInterfaces().Any(i => i.Name == nameof(IRegistrant))).FullName;

            IRegistrant pluginRegistrant = assemblyLoader.Assembly.CreateInstance<IRegistrant>(pluginRegistrantTypeName!);

            pluginRegistrant.Register(mvcBuilder, configuration); // create services the host doesn't know about
        }
    }

    #region create instance from typeName

    public static object CreateInstance(this Assembly assembly, string typeName, params object[] parmArray)
    {
        if (parmArray.Length > 0)
        {
            System.Globalization.CultureInfo culture = Thread.CurrentThread.CurrentCulture;
            object[] activationAttrs = Array.Empty<object>();
            return assembly.CreateInstance(typeName, false, BindingFlags.CreateInstance, null, parmArray, culture, activationAttrs)!;
        }
        else
        {
            return assembly.CreateInstance(typeName)!;
        }
    }

    public static T CreateInstance<T>(this Assembly assembly, string typeName, params object[] parmArray) => (T)CreateInstance(assembly, typeName, parmArray);

    #endregion

}