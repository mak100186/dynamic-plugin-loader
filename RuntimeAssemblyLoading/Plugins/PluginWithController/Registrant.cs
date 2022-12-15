using Microsoft.Extensions.DependencyInjection.Extensions;

using PluginBase.Abstractions;

namespace PluginWithController;

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

        services.AddSingleton<IPlugin, Main>();
        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();        
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
