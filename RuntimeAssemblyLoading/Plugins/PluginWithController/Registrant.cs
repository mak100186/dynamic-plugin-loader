using Microsoft.Extensions.DependencyInjection.Extensions;

using PluginBase.Abstractions;

namespace PluginWithController;

public class Registrant : IRegistrant
{
    public IMvcBuilder Register(IMvcBuilder mvcBuilder, IConfiguration? config)
    {
        mvcBuilder.Services.Register(mvcBuilder);
        return mvcBuilder;
    }
}

public static class StaticRegistrant
{
    public static IServiceCollection Register(this IServiceCollection services, IMvcBuilder mvcBuilder)
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
