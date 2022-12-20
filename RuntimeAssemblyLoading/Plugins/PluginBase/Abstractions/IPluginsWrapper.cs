namespace PluginBase.Abstractions;
public interface IPluginsWrapper
{
    IEnumerable<IPlugin> Plugins { get; }
}
