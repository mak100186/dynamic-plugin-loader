using PluginBase.Abstractions;

namespace CouchbasePlugin.Configs;

public class CouchbaseSettings : ISettings
{
    public string Url { get; set; }
}
