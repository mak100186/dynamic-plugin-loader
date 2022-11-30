namespace PluginBase;
public interface IReward
{
    string Name { get; }

    int GetInvokeCounter();

    Guid GetRn();
}
