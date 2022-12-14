using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PluginBase.Abstractions;
public interface IRegistrant
{
    public IServiceCollection Register(IServiceCollection services, IConfiguration config, IMvcBuilder mvcBuilder);
}

