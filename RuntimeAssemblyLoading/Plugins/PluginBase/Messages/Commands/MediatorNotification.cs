using MediatR;

using PluginBase.Enums;

namespace PluginBase.Messages.Commands;
public class MediatorNotification : INotification
{
    public ICollection<NotificationSubjects> Subjects { get; set; } = new List<NotificationSubjects>();
    //the message is meant for that subject i.e. Host, CouchbasePlugin, PostGreSQLPlugin
    public NotificationEvents Event { get; set; }
    //the action to perform
    public ICollection<object> Arguments { get; set; } = new List<object>();
    //the parameters associated with the action
}
