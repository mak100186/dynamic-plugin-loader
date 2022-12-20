using Microsoft.Extensions.DependencyInjection;
using PluginBase.Abstractions;

namespace PluginBase.Concrete;

public class NotificationManager : INotificationManager
{
    private readonly IServiceProvider _services;
    public NotificationManager(IServiceProvider services)
    {
        _services = services;
    }
    public void Send(Notification notification)
    {
        var plugins = _services.GetRequiredService<IPluginsWrapper>();

        var addressedPlugin = plugins.Plugins.FirstOrDefault(x => x.UniqueIdentifier == notification.To);

        if (addressedPlugin != null && addressedPlugin.GetType().GetInterfaces().Select(x => x.Name).Contains(nameof(INotificationReceiver)))
        {
            var addressedReceiver = addressedPlugin as INotificationReceiver;
            addressedReceiver!.Receive(notification);
        }

    }
}