using System.Reflection;
using System.Text.Json.Serialization;

using FluentValidation.AspNetCore;

using PluginBase.Abstractions;

using RuntimeAssemblyLoading.Dependency;
using RuntimeAssemblyLoading.Helpers;
using RuntimeAssemblyLoading.Services;
using RuntimeAssemblyLoading.Services.Options;
using RuntimeAssemblyLoading.Services.Plugin;

using Serilog;

using Unibet.Infrastructure.Caching.CouchbaseV7;
using Unibet.Infrastructure.Hosting.WebApi.Health;

namespace RuntimeAssemblyLoading;
public static class ServiceRegistrations
{
    public static void ConfigureServices(this IServiceCollection services, IConfiguration config, IMvcBuilder mvcBuilder, bool shouldRunMigrationPathway)
    {
        //No DbContext, No Couchbase, No Kafka - they should be loaded by their respective plugins. 
        services.AddControllers(x => x.AllowEmptyInputInBodyModelBinding = true)
                .AddJsonOptions(options =>
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        var builder = services.AddHealthChecks()
            .AddCheck<InstanceDiagnosticsCheck>(nameof(InstanceDiagnosticsCheck));

        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        services.Configure<StartUpOptions>(options =>
        {
            options.ShouldRunMigrationPathway = shouldRunMigrationPathway;
        });

        services.AddMvc(c =>
        {
            // https://docs.microsoft.com/en-us/dotnet/core/compatibility/aspnetcore#mvc-async-suffix-trimmed-from-controller-action-names
            c.SuppressAsyncSuffixInActionNames = false;
        })
        .AddNewtonsoftJson()
        .AddFluentValidation()
        .AddControllersAsServices();

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
            conf.WriteTo.File(path: "log-.txt", outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}{NewLine}", rollingInterval: RollingInterval.Day);
        });
    }

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
        .Single(t => t.GetInterfaces().Any(i => i.Name == nameof(IRegistrant))).FullName;

        var pluginRegistrant = pluginAssembly.CreateInstance<IRegistrant>(pluginRegistrantTypeName!);

        pluginRegistrant.Register(services, config, mvcBuilder); // create services the host doesn't know about

    }

    #region create instance from typeName

    public static object CreateInstance(this Assembly assembly, string typeName, params object[] parmArray)
    {
        if (parmArray.Length > 0)
        {
            var culture = Thread.CurrentThread.CurrentCulture;
            var activationAttrs = Array.Empty<object>();
            return assembly.CreateInstance(typeName, false, BindingFlags.CreateInstance, null, parmArray, culture, activationAttrs)!;
        }
        else
        {
            return assembly.CreateInstance(typeName)!;
        }
    }

    public static object CreateInstance(this Assembly assembly, string typeName, IEnumerable<object> parmList) => CreateInstance(assembly, typeName, parmList);

    public static T CreateInstance<T>(this Assembly assembly, string typeName, params object[] parmArray) => (T)CreateInstance(assembly, typeName, parmArray);

    #endregion

}