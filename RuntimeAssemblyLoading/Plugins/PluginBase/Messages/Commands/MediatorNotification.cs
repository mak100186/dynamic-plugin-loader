using MediatR;

namespace PluginBase.Messages.Commands;
public class MediatorNotification : INotification
{
    public ICollection<string> Subjects { get; set; } = new List<string>();
    //the message is meant for that subject i.e. Host, CouchbasePlugin, PostGreSQLPlugin
    public string Action { get; set; }
    //the action to perform
    public ICollection<object> Arguments { get; set; } = new List<object>();
    //the parameters associated with the action
}
