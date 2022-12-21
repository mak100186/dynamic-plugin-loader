using Microsoft.Extensions.DependencyInjection;
using PluginBase.Abstractions;

namespace PluginBase.Concrete;

public class NotificationManager : INotificationManager
{
    private readonly IServiceProvider _services;
    public NotificationManager(IServiceProvider services)
    {
        this._services = services;
    }
    public void Send(BaseNotification notification)
    {
        IPluginsWrapper plugins = this._services.GetRequiredService<IPluginsWrapper>();

        IPlugin? addressedPlugin = plugins.Plugins.FirstOrDefault(x => x.UniqueIdentifier == notification.To);

        if (addressedPlugin != null && addressedPlugin.GetType().GetInterfaces().Select(x => x.Name).Contains(nameof(INotificationReceiver)))
        {
            INotificationReceiver? addressedReceiver = addressedPlugin as INotificationReceiver;
            addressedReceiver!.Receive(notification);
        }

    }
}