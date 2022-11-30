using Microsoft.Extensions.Configuration;

using PluginBase.Abstractions;
using PluginBase.Enums;

namespace PluginB;
public class Main : IPlugin
{
    public string Name => $"Freebet-B";

    public IPluginHostApplication Application { get; set; } = null!;

    public State State { get; private set; }

    public void OnStarted()
    {
        this.State = State.Started;

        Console.WriteLine($"{this.Name} has started");

        this.Application.PluginStartCompleted(this);
    }

    public void OnStopped()
    {
        this.State = State.Stopped;

        Console.WriteLine($"{this.Name} has stopped");

        this.Application.PluginStopCompleted(this);
    }

    public void Start(IConfiguration configuration)
    {
        this.State = State.Starting;

        Console.WriteLine($"{this.Name} is starting");

        OnStarted();
    }

    public void Stop()
    {
        this.State = State.Stopping;

        Console.WriteLine($"{this.Name} is stopping");

        OnStopped();
    }
}
