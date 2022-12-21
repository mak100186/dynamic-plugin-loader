using PluginBase.Concrete;

namespace PluginBase.Abstractions;

public interface INotificationReceiver
{
    void Receive(BaseNotification baseNotification);
}
