using System.Reflection;
using PluginBase.Concrete;

namespace PluginBase.Abstractions;
public interface INotificationManager
{
    void Send(BaseNotification baseNotification);
}
