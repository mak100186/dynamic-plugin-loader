using PluginBase;

namespace RewardPluginB;
public class Freebet : IReward
{
    private int invokeCounter = 0;

    public string Name => $"Freebet-B";

    public int GetInvokeCounter()
    {
        return invokeCounter;
    }

    public Guid GetRn()
    {
        invokeCounter++;
        return Guid.NewGuid();
    }
}
