using System.Text.Json.Serialization;

using FluentValidation.AspNetCore;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using PluginBase.Abstractions;

using RuntimeAssemblyLoading.Abstractions;
using RuntimeAssemblyLoading.Helpers;
using RuntimeAssemblyLoading.Services;
using RuntimeAssemblyLoading.Services.Dependency;
using RuntimeAssemblyLoading.Services.Options;
using RuntimeAssemblyLoading.Services.Plugin;

using Serilog;

using Unibet.Infrastructure.Caching.CouchbaseV7;
using Unibet.Infrastructure.Hosting.WebApi.Health;

namespace RuntimeAssemblyLoading;
public static class ServiceRegistrations
{
    public static void ConfigureServices(this IServiceCollection services, IConfiguration config, bool shouldRunMigrationPathway)
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
        services.AddSingleton<IPluginCollection, PluginCollection>();
        services.AddSingleton<IPluginLoader, PluginLoader>();
        services.AddSingleton<IPluginMigrator, PluginMigrator>();
        services.AddSingleton<INotificationManager, NotificationManager>();

        services.LoadDependencies(config);

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
}