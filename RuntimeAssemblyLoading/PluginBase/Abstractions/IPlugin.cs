using Microsoft.Extensions.Configuration;

using PluginBase.Enums;

namespace PluginBase.Abstractions;

public interface IPlugin
{
    string Name { get; }

    State State { get; }

    void Start(IConfiguration configuration);

    void OnStarted();

    void Stop();

    void OnStopped();

    IPluginHostApplication Application { get; set; }
}
