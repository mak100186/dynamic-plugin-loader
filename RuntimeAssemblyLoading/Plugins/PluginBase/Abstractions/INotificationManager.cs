using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace PluginBase.Abstractions;
public interface INotificationManager
{
    void Send(Notification notification);
}

public interface INotificationReceiver
{
    void Receive(Notification notification);
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
        var plugins = this._services.GetRequiredService<IPluginsWrapper>();

        var addressedPlugin = plugins.Plugins.FirstOrDefault(x => x.Name == notification.To);

        if (addressedPlugin != null && addressedPlugin.GetType().GetInterfaces().Select(x => x.Name).Contains(nameof(INotificationReceiver)))
        {
            var addressedReceiver = (INotificationReceiver)addressedPlugin;
            addressedReceiver.Receive(notification);
        }

    }
}