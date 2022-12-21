namespace PluginBase.Concrete;

public abstract class BaseNotification
{
    public string To { get; set; }
    public string From { get; set; }
    public string Action { get; set; }
}

public class Notification : BaseNotification
{
    public object Data { get; set; }
    public Type Type { get; set; }
}
