using Microsoft.Extensions.DependencyInjection;

namespace PluginBase.Abstractions;
public interface INotificationManager
{
    void Send(Notification notification);
}

public class Notification
{
    public string To { get; set; }
    public string From { get; set; }
    public string Action { get; set; }
}

public class NotificationManager : INotificationManager
{
    private readonly IServiceProvider _services;
    public NotificationManager(IServiceProvider services)
    {
        this._services = services;
    }
    public void Send(Notification notification)
    {
        var plugins = this._services.GetRequiredService<IPluginCollection>();


        var addressedPlugin = plugins.Plugins.FirstOrDefault(x => x.Name == notification.To);
        addressedPlugin.Receive(notification);
    }
}