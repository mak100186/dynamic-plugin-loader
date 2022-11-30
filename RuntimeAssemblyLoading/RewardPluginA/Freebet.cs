using PluginBase;

namespace RewardPluginA;
public class Freebet : IReward
{
    private int invokeCounter = 0;

    public string Name => "Freebet-A";

    public int GetInvokeCounter()
    {
        var cascadeMode = FluentValidation.CascadeMode.Continue;

        return invokeCounter + (int)cascadeMode;
    }

    public Guid GetRn()
    {
        var cascadeMode = FluentValidation.CascadeMode.Stop;

        invokeCounter += (int)cascadeMode - 1;

        return Guid.NewGuid();
    }
}
