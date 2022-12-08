using MediatR;

namespace PluginBase.Messages.Commands;
public class MediatorNotification : INotification
{
    public string Action { get; set; }
}
