using Microsoft.Extensions.DependencyInjection.Extensions;

using PluginBase.Abstractions;
using PluginBase.Enums;

namespace PluginWithController;

public class PluginWithApi : IPlugin
{
    private readonly ILogger _logger;
    private readonly IPluginApiService _service;

    public PluginWithApi(ILogger<PluginWithApi> logger, IPluginApiService service)
    {
        _service = service;
    }

    public string Name => "PluginWithApi";

    public State State { get; private set; }

    public IServiceProvider ServiceProvider { get; set; } = null!;

    public async Task Migrate()
    {
        this.State = State.Starting;

        this._logger.LogInformation($"{this.Name} migrating");

        await OnMigrateComplete();
    }

    public async Task OnMigrateComplete()
    {
        this.State = State.Started;

        this._logger.LogInformation($"{this.Name} has migrated");

    }

    public async Task OnStarted()
    {
        this.State = State.Started;

        this._logger.LogInformation($"{this.Name} has started");

    }

    public async Task OnStopped()
    {
        this.State = State.Stopped;

        this._logger.LogInformation($"{this.Name} has stopped");


    }

    public async Task Start()
    {
        this.State = State.Starting;

        this._logger.LogInformation($"{this.Name} is starting");

        await OnStarted();
    }

    public async Task Stop()
    {
        this.State = State.Stopping;

        this._logger.LogInformation($"{this.Name} is stopping");

        await OnStopped();
    }

    public void Receive(Notification notification)
    {
        this._logger.LogInformation($"Notification  intended for {notification.To} received by {this.Name} for action {notification.Action} sent by {notification.From}");
    }
}

public class Registrant : IRegistrant
{
    public IServiceCollection Register(IServiceCollection services, IConfiguration config, IMvcBuilder mvcBuilder)
    {
        services = StaticRegistrant.Register(services, config, mvcBuilder);
        return services;
    }
}

public static class StaticRegistrant
{
    public static IServiceCollection Register(this IServiceCollection services, IConfiguration config, IMvcBuilder mvcBuilder)
    {
        mvcBuilder.AddModule(typeof(Registrant));

        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.TryAddSingleton<IPlugin, PluginWithApi>();
        services.TryAddSingleton<IPluginApiService, PluginApiService>();

        return services;
    }

    public static IMvcBuilder AddModule(this IMvcBuilder builder, Type type)
    {
        return builder
            .AddApplicationPart(type.Assembly)
            .AddControllersAsServices();
    }
}

public interface IPluginApiService
{
    string Print(string input);
}
public class PluginApiService : IPluginApiService
{
    public string Print(string text)
    {
        return text + text;
    }
}
